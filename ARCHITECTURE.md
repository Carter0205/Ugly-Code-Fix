# System Architecture & Design Pattern Analysis

## Current Architecture

```
┌─────────────────────────────────────────────────────┐
│           UglyClient (Console App)                  │
├─────────────────────────────────────────────────────┤
│  Program.cs                                         │
│    └─> ApiClient (direct HTTP calls)               │
│    └─> ConsoleUI (menu loop)                       │
└─────────────────────────────────────────────────────┘
           ↓ HTTP (API calls)
┌─────────────────────────────────────────────────────┐
│         Version2 (ASP.NET Core Web App)             │
├─────────────────────────────────────────────────────┤
│  Controllers:                                       │
│    ├─ FansController                               │
│    ├─ HeaterController                             │
│    ├─ SensorController                             │
│    ├─ SystemStateController                        │
│    └─ EnviromentController                         │
│  Models:                                            │
│    ├─ FanControl, HeaterControl                    │
│    ├─ TemperatureSensor, Monitor                   │
│    └─ SimulationConfiguration, ClientState         │
└─────────────────────────────────────────────────────┘
```

## Design Weaknesses in Original Code

### 1. **Tight Coupling Between Layers**
   - `ApiClient` directly knows HTTP endpoints and encoding details
   - `ConsoleUI` directly calls `ApiClient` without abstraction
   - No interface-based contracts between components

### 2. **Limited Testability**
   - `ApiClient` requires a real `HttpClient` pointing to running server
   - Cannot mock device operations without network calls
   - ConsoleUI cannot be tested independently from HttpClient behavior

### 3. **Repetitive Code (DRY Violations)**
   - Similar error handling patterns repeated across methods
   - Loop-based operations (fans, heaters) lack unified abstraction
   - Device state retrieval duplicated in multiple locations

### 4. **Poor Separation of Concerns**
   - `ApiClient` mixes HTTP transport details with business logic
   - Parsing and validation scattered across methods
   - Console I/O hardcoded in UI class

### 5. **Scalability Issues**
   - Adding new device types requires modifying multiple classes
   - No mechanism to substitute implementations (e.g., different transport)
   - Limited extensibility for new features

---

## Recommended Design Pattern: Repository Pattern

### Pattern Overview
The **Repository Pattern** abstracts data access logic and device control operations behind a clean interface. This decouples the UI and business logic from HTTP transport details.

### Benefits
- **Testability**: Mock repositories can simulate device behavior without network
- **Maintainability**: Changes to API contracts localized to repositories
- **Extensibility**: New device types add new repositories, no UI changes
- **Flexibility**: Can swap HTTP with other transports (gRPC, local simulation, etc.)

### Implementation

```csharp
// Abstraction layer
public interface IDeviceRepository
{
    Task<double> GetSensorTemperatureAsync(int sensorId);
    Task<double> GetAverageTemperatureAsync();
    Task SetHeaterLevelAsync(int heaterId, int level);
    Task SetFanStateAsync(int fanId, bool isOn);
    Task DisplayAllDevicesAsync();
    Task ResetAsync();
}

// Concrete HTTP implementation
public class HttpDeviceRepository : IDeviceRepository
{
    private readonly HttpClient _client;

    public async Task<double> GetSensorTemperatureAsync(int sensorId)
    {
        // HTTP call + parsing logic
    }

    // ... other methods
}

// Consumer depends on abstraction
public class ConsoleUI
{
    private readonly IDeviceRepository _repository;

    public ConsoleUI(IDeviceRepository repository)
    {
        _repository = repository;
    }

    public async Task RunAsync()
    {
        // Uses repository without knowing it's HTTP
    }
}

// Testing: use mock repository
public class MockDeviceRepository : IDeviceRepository
{
    public Task<double> GetSensorTemperatureAsync(int sensorId)
    {
        return Task.FromResult(20.5);
    }

    // ... other methods return test data
}
```

### Impact on Code
- **Before**: `ConsoleUI → ApiClient → HttpClient → Network`
- **After**: `ConsoleUI → IDeviceRepository (interface) → {HttpDeviceRepository or MockDeviceRepository}`

This achieves:
1. ✅ Reduced coupling (UI depends on abstraction, not implementation)
2. ✅ Improved testability (mock repositories for unit tests)
3. ✅ Better maintainability (repository changes don't affect UI)
4. ✅ Enhanced extensibility (add new repositories without UI changes)

