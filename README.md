# Ugly-Code-Fix
Ugly code to fix and make more presentable.
Build and run

Requirements: .NET 8 SDK

Build:

- From the repository root run:
  - dotnet build

Run the web server (Version2):

- In Visual Studio: set Version2 project as startup and run (F5) or use the Project launch profile (https/http).
- Or from terminal:
  - cd "..\..\..\Desktop\Software Engineering Component 2 Hand in\Version2Sim-master\Version2Sim-master"
  - dotnet run

Run the console client (UglyClient):

- Ensure the web server is running and note the listening URL printed (e.g. https://localhost:7021).
- Set environment variable VERSION2_URL to the web server base address if it differs from the default.
  - PowerShell: $env:VERSION2_URL = 'https://localhost:7021/'
- From repo root run:
  - dotnet run --project UglyClient.csproj

Automatic browser opening:

- The console client will poll the web server's /swagger endpoint and open the default browser when reachable. Use VERSION2_URL to match the web project's listening port.

Tests (minimal harness):

- A small test runner is provided at UnitTestRunner. Run:
  - dotnet run --project UnitTestRunner

What I changed (summary):
- Separated Program into ApiClient and ConsoleUI
- Consolidated device display and sensor parsing
- Added XML documentation generation in csproj
- Added a minimal test harness (UnitTestRunner), PlantUML diagram, sprint reviews and documentation files.

Notes for marker:
- The project targets .NET 8. Build and run instructions are above. If HTTPS warnings appear, run dotnet dev-certs https --trust.
