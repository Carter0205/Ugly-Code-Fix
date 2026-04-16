Analysis of existing system

Summary:
The provided code contains a functional console client and a server simulation project. The console client (Program.cs) originally implemented menu handling, HTTP calls and control loops in a single file. This made the code hard to maintain and test.

Design issues identified:
- Single-file responsibilities: UI, HTTP access, and control logic mixed in Program.cs.
- Repetition: device state display and sensor parsing duplicated.
- Testability: tight coupling to static HttpClient calls hard to mock.

Improvements made:
- Introduced ApiClient to encapsulate HTTP calls and parsing.
- Introduced ConsoleUI to handle user interaction and call ApiClient.
- Centralised sensor parsing with culture-aware parsing.
- Consolidated repeated display logic into one method.

Recommended design pattern:
- Introduce a simple Facade/Wrapper (ApiClient) to hide HTTP complexity from the UI.
- The separation approximates the Single Responsibility Principle and enables testing.

Files changed:
- Program.cs: replaced with bootstrapper that instantiates ApiClient and ConsoleUI.
- ApiClient.cs: new, contains HTTP operations.
- ConsoleUI.cs: new, contains menu and calls to ApiClient.

This addresses the ICA requirements for refactoring and applying a design pattern.
