# Ugly Code Fix

Refactored the Ugly Code to use the Repository Pattern.

## Build and Run

```
dotnet build
dotnet run
```

Uses MockDeviceRepository by default.

## What Changed

Separated ConsoleUI from the HTTP communication logic using an interface (IDeviceRepository). Two implementations: MockDeviceRepository for testing and HttpDeviceRepository for the real server.

## Files

- Program.cs - Entry point
- ConsoleUI.cs - User interface
- IDeviceRepository.cs - Interface
- HttpDeviceRepository.cs - HTTP implementation
- MockDeviceRepository.cs - Mock implementation
- DeviceRepositoryTests.cs - Unit tests
- DeviceManagementFacade.cs - Facade pattern
- TemperatureControlStrategy.cs - Strategy pattern
