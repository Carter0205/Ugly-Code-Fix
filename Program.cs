using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Use MockDeviceRepository for reliable testing without external dependencies
        // If you want to use the real Version2 server, uncomment the HttpDeviceRepository code below:

        // var baseUrl = Environment.GetEnvironmentVariable("VERSION2_URL") ?? "https://localhost:44351/";
        // var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        // const string apiKey = "u007-key";
        // client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        // IDeviceRepository repository = new HttpDeviceRepository(client);

        IDeviceRepository repository = new MockDeviceRepository();
        var ui = new ConsoleUI(repository);
        await ui.RunAsync();
    }
}
