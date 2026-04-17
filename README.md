# Ugly Code Fix

Refactored the Ugly Code to use the Repository Pattern. Separated the console UI from the HTTP communication logic so it's easier to test and maintain.

## Build and Run

```
dotnet build
dotnet run
```

Uses MockDeviceRepository by default (no external dependencies).

## What Changed

The original code had ConsoleUI calling an ApiClient directly to talk to the Version2 server. Now there's an interface (IDeviceRepository) between them.

Two implementations:
- MockDeviceRepository - For testing, no server needed (currently used)
- HttpDeviceRepository - For real Version2 server (commented out in Program.cs)

ConsoleUI only knows about the interface, so it works with either implementation.

## Files

- Program.cs - Entry point
- ConsoleUI.cs - User interface
- IDeviceRepository.cs - Interface
- HttpDeviceRepository.cs - Real implementation
- MockDeviceRepository.cs - Mock for testing
- DeviceRepositoryTests.cs - 15 tests (all passing)

## Tests

Open DeviceRepositoryTests.cs to see the test code. 15 tests verify all the operations work.

## See Also

- CONFIGURATION_NOTES.md - How to switch between Mock and Http
- FIX_SUMMARY.md - What was fixed
