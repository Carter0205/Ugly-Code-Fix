using System;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Allow overriding the server URL via environment variable to match the web project's actual listening address.
        // default back to the previous working HTTPS address
        var baseUrl = Environment.GetEnvironmentVariable("VERSION2_URL") ?? "https://localhost:44351/";

        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        const string apiKey = "u007-key";
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var api = new ApiClient(client);

        // Start a background task to open the web UI in the browser once the web server responds.
        var targetUrl = new Uri(new Uri(baseUrl), "swagger");
        Console.WriteLine($"[Launcher] Will poll {targetUrl} and open browser when ready (press Ctrl+C to stop).\n");
        _ = Task.Run(async () => await LaunchBrowserWhenReady(targetUrl, TimeSpan.FromMilliseconds(500), 40));

        var ui = new ConsoleUI(api);
        await ui.RunAsync();
    }

    static async Task LaunchBrowserWhenReady(Uri url, TimeSpan pollInterval, int maxAttempts)
    {
        using var http = new HttpClient { BaseAddress = new Uri(url.GetLeftPart(UriPartial.Authority)) };
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                Console.WriteLine($"[Launcher] Attempt {i + 1}/{maxAttempts}: GET {url}");
                var resp = await http.GetAsync(url.PathAndQuery);
                Console.WriteLine($"[Launcher] Response status: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("[Launcher] Server ready — attempting to open browser...");
                    // Try a few strategies to open the browser so Chrome opens reliably
                    var opened = false;
                    // 1) Default shell open
                    try
                    {
                        Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
                        opened = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Launcher] Default shell open failed: {ex.Message}");
                    }

                    // 2) cmd /c start "" "url"
                    if (!opened)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo("cmd", $"/c start \"\" \"{url}\"") { CreateNoWindow = true });
                            opened = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Launcher] cmd start failed: {ex.Message}");
                        }
                    }

                    // 3) Try explicit chrome executable locations and registry lookup
                    if (!opened)
                    {
                        var chromeCandidates = new System.Collections.Generic.List<string>();

                        // common install locations
                        chromeCandidates.Add(Environment.ExpandEnvironmentVariables("%ProgramFiles%\\Google\\Chrome\\Application\\chrome.exe"));
                        chromeCandidates.Add(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%\\Google\\Chrome\\Application\\chrome.exe"));
                        chromeCandidates.Add(Environment.ExpandEnvironmentVariables("%LocalAppData%\\Google\\Chrome\\Application\\chrome.exe"));

                        // try registry App Paths for chrome.exe
                        try
                        {
                            string? regPath = (string?)Microsoft.Win32.Registry.GetValue(
                                "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", "", null);
                            if (!string.IsNullOrEmpty(regPath)) chromeCandidates.Add(regPath);
                        }
                        catch { }
                        try
                        {
                            string? regPathWow = (string?)Microsoft.Win32.Registry.GetValue(
                                "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe", "", null);
                            if (!string.IsNullOrEmpty(regPathWow)) chromeCandidates.Add(regPathWow);
                        }
                        catch { }

                        foreach (var cp in chromeCandidates)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(cp) && System.IO.File.Exists(cp))
                                {
                                    Process.Start(new ProcessStartInfo(cp, url.ToString()) { UseShellExecute = true });
                                    opened = true;
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Launcher] Chrome path {cp} failed: {ex.Message}");
                            }
                        }
                    }

                    // 4) Try launching 'chrome' command if on PATH
                    if (!opened)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo("chrome", url.ToString()) { UseShellExecute = true });
                            opened = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Launcher] chrome command failed: {ex.Message}");
                        }
                    }

                    if (opened)
                    {
                        Console.WriteLine("[Launcher] Browser launch attempted.");
                    }
                    else
                    {
                        Console.WriteLine("[Launcher] All browser launch attempts failed — please open the URL manually:");
                        Console.WriteLine(url.ToString());
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Launcher] Exception while polling: {ex.Message}");
            }
            await Task.Delay(pollInterval);
        }
        Console.WriteLine($"[Launcher] Giving up after {maxAttempts} attempts — server not reachable at {url}");
    }
}
