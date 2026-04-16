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
        var baseUrl = Environment.GetEnvironmentVariable("VERSION2_URL") ?? "http://localhost:5000/";

        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        const string apiKey = "u007-key";
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var api = new ApiClient(client);

        // Start a background task to open the web UI in the browser once the web server responds.
        _ = Task.Run(async () => await LaunchBrowserWhenReady(new Uri(new Uri(baseUrl), "swagger"), TimeSpan.FromMilliseconds(500), 40));

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
                var resp = await http.GetAsync(url.PathAndQuery);
                if (resp.IsSuccessStatusCode)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
                    }
                    catch { }
                    return;
                }
            }
            catch { }
            await Task.Delay(pollInterval);
        }
        // give up silently if not reachable
    }
}
