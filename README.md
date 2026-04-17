# Ugly Code Refactoring

## What This Is

Refactored the Ugly Code console application to use the Repository Pattern. It communicates with the Version2 server simulator to control fans, heaters, and sensors.

## How to Run

1. Set both UglyClient and Version2 as startup projects
2. Make sure Version2 startup is set to HTTPS
3. Press Start
4. Version2 will open in Chrome, UglyClient console will appear
5. Use the menu to control fans, heaters, and read temperatures

## What Changed

- Separated HTTP communication from UI logic
- Created IDeviceRepository interface
- Added HttpDeviceRepository to talk to Version2
- Added MockDeviceRepository for testing
- Updated ConsoleUI to use the interface

## Tests

Run DeviceRepositoryTests.cs to verify all 15 tests pass. Tests work without Version2 running.

## Build

Compiles with no errors. All original functionality works.
