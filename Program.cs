using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var baseUrl = Environment.GetEnvironmentVariable("VERSION2_URL") ?? "https://localhost:44351/";
        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        const string apiKey = "u007-key";
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        IDeviceRepository repository = new HttpDeviceRepository(client);
        var ui = new ConsoleUI(repository);
        await ui.RunAsync();
    }
}
