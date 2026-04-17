# Configuration - Mock vs HTTP

## Current Setup

Using `MockDeviceRepository` by default. This works without needing the Version2 server running.

## Why Mock?

The app was failing to connect to localhost:44351 when Version2 wasn't running. Using mock data lets me test and demo the code without worrying about the server.

## Switching to HttpDeviceRepository

If you want to use the real Version2 server instead:

1. In Program.cs, comment out:
```csharp
IDeviceRepository repository = new MockDeviceRepository();
```

2. Uncomment:
```csharp
var baseUrl = Environment.GetEnvironmentVariable("VERSION2_URL") ?? "https://localhost:44351/";
var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
const string apiKey = "u007-key";
client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
IDeviceRepository repository = new HttpDeviceRepository(client);
```

3. Run both UglyClient and Version2 as startup projects with HTTPS

## How It Works

The key idea is ConsoleUI only knows about the `IDeviceRepository` interface. It doesn't care if the implementation is Mock or Http.

```csharp
public class ConsoleUI
{
    private readonly IDeviceRepository _repository;

    public ConsoleUI(IDeviceRepository repository)
    {
        _repository = repository;  // Could be Mock or Http
    }
}
```

So you can swap implementations without changing ConsoleUI at all.
