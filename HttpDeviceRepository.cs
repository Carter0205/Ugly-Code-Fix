using System;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;

/// <summary>
/// HTTP-based implementation of the device repository.
/// Communicates with the Version2 ASP.NET Core web API to control devices and read state.
/// </summary>
public class HttpDeviceRepository : IDeviceRepository
{
    private readonly HttpClient _client;

    /// <summary>
    /// Create a new HttpDeviceRepository using the provided HttpClient.
    /// </summary>
    /// <param name="client">HttpClient configured with base address and credentials</param>
    public HttpDeviceRepository(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<double> GetSensorTemperatureAsync(int sensorId)
    {
        var response = await _client.GetAsync($"api/sensor/{sensorId}");
        if (response.IsSuccessStatusCode)
        {
            var tempString = await response.Content.ReadAsStringAsync();
            if (double.TryParse(tempString, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out double temp))
            {
                return temp;
            }
            if (double.TryParse(tempString, out temp))
            {
                return temp;
            }
            throw new Exception($"Failed to parse temperature from sensor {sensorId}: '{tempString}'");
        }

        throw new Exception($"Failed to get temperature from sensor {sensorId}: {response.ReasonPhrase}");
    }

    public async Task<double> GetAverageTemperatureAsync()
    {
        var s1 = await GetSensorTemperatureAsync(1);
        var s2 = await GetSensorTemperatureAsync(2);
        var s3 = await GetSensorTemperatureAsync(3);

        return (s1 + s2 + s3) / 3.0;
    }

    public async Task SetHeaterLevelAsync(int heaterId, int level)
    {
        var response = await _client.PostAsync($"api/heat/{heaterId}",
            new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set heater level {heaterId}: {response.ReasonPhrase}");
        }
    }

    public async Task SetFanStateAsync(int fanId, bool isOn)
    {
        var response = await _client.PostAsync($"api/fans/{fanId}",
            new StringContent(isOn.ToString().ToLower(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
        }
    }

    public async Task SetAllHeatersAsync(int level)
    {
        for (int i = 1; i <= 3; i++)
        {
            await SetHeaterLevelAsync(i, level);
        }
    }

    public async Task SetAllFansAsync(bool state)
    {
        for (int i = 1; i <= 3; i++)
        {
            await SetFanStateAsync(i, state);
        }
    }

    public async Task DisplayAllDevicesAsync()
    {
        Console.WriteLine("Fetching fan states individually...");
        for (int i = 1; i <= 3; i++)
        {
            var fanResponse = await _client.GetAsync($"api/fans/{i}/state");
            if (fanResponse.IsSuccessStatusCode)
            {
                var fanJson = await fanResponse.Content.ReadAsStringAsync();
                var fan = JsonSerializer.Deserialize<FanDTO>(fanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                Console.WriteLine($"  Fan {fan.Id}: {(fan.IsOn ? "On" : "Off")}");
            }
            else
            {
                Console.WriteLine($"  Fan {i}: Failed to fetch state.");
            }
        }

        Console.WriteLine("Fetching heater levels individually...");
        for (int i = 1; i <= 3; i++)
        {
            var heaterResponse = await _client.GetAsync($"api/heat/{i}/level");
            if (heaterResponse.IsSuccessStatusCode)
            {
                var levelString = await heaterResponse.Content.ReadAsStringAsync();
                if (int.TryParse(levelString, out int level))
                {
                    Console.WriteLine($"  Heater {i}: Level {level}");
                }
                else
                {
                    Console.WriteLine($"  Heater {i}: Failed to parse level.");
                }
            }
            else
            {
                Console.WriteLine($"  Heater {i}: Failed to fetch level.");
            }
        }

        Console.WriteLine("Fetching sensor temperatures individually...");
        try
        {
            var s1 = await GetSensorTemperatureAsync(1);
            Console.WriteLine($"  Sensor 1: Temperature {s1} (Deg)");
            var s2 = await GetSensorTemperatureAsync(2);
            Console.WriteLine($"  Sensor 2: Temperature {s2} (Deg)");
            var s3 = await GetSensorTemperatureAsync(3);
            Console.WriteLine($"  Sensor 3: Temperature {s3} (Deg)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching sensor data: {ex.Message}");
        }
    }

    public async Task ResetAsync()
    {
        var response = await _client.PostAsync("api/Envo/reset", null);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to reset client state: {response.ReasonPhrase}");
        }
    }

    public async Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        int intervalMs = 1000;
        int iterations = durationSeconds;
        for (int i = 0; i < iterations; i++)
        {
            if (Math.Abs(currentTemperature - targetTemperature) <= 0.1) break;

            if (currentTemperature < targetTemperature)
            {
                await SetAllHeatersAsync(3);
                await SetAllFansAsync(false);
            }
            else
            {
                await SetAllHeatersAsync(0);
                await SetAllFansAsync(true);
            }

            await Task.Delay(intervalMs);
            currentTemperature = await GetAverageTemperatureAsync();
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    public async Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        int intervalMs = 1000;
        for (int i = 0; i < durationSeconds; i++)
        {
            if (currentTemperature < targetTemperature)
            {
                await SetAllHeatersAsync(1);
                await SetAllFansAsync(false);
            }
            else if (currentTemperature > targetTemperature)
            {
                await SetAllHeatersAsync(0);
                await SetAllFansAsync(true);
            }

            await Task.Delay(intervalMs);
            currentTemperature = await GetAverageTemperatureAsync();
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }
}

/// <summary>
/// DTO for fan state responses from the API.
/// </summary>
public class FanDTO
{
    public int Id { get; set; }
    public bool IsOn { get; set; }
}
