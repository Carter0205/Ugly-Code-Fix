using System;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;

/// <summary>
/// Simple API client wrapping the HTTP calls used by the console UI.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _client;

    /// <summary>
    /// Create a new ApiClient using the provided HttpClient instance.
    /// </summary>
    public ApiClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Get a single sensor temperature as a double.
    /// </summary>
    public async Task<double> GetSensorTemperature(int sensorId)
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

    /// <summary>
    /// Calculate the average temperature across sensors 1..3.
    /// </summary>
    public async Task<double> GetAverageTemperature()
    {
        var s1 = await GetSensorTemperature(1);
        var s2 = await GetSensorTemperature(2);
        var s3 = await GetSensorTemperature(3);

        return (s1 + s2 + s3) / 3.0;
    }

    /// <summary>
    /// Set heater level for a heater.
    /// </summary>
    public async Task SetHeaterLevel(int heaterId, int level)
    {
        var response = await _client.PostAsync($"api/heat/{heaterId}",
            new StringContent(level.ToString(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set heater level {heaterId}: {response.ReasonPhrase}");
        }
    }

    /// <summary>
    /// Set fan state for a fan.
    /// </summary>
    public async Task SetFanState(int fanId, bool isOn)
    {
        var response = await _client.PostAsync($"api/fans/{fanId}",
            new StringContent(isOn.ToString().ToLower(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to set fan state for fan {fanId}: {response.ReasonPhrase}");
        }
    }

    /// <summary>
    /// Set all heaters to a level.
    /// </summary>
    public async Task SetAllHeaters(int level)
    {
        for (int i = 1; i <= 3; i++)
        {
            await SetHeaterLevel(i, level);
        }
    }

    /// <summary>
    /// Set all fans on/off.
    /// </summary>
    public async Task SetAllFans(bool state)
    {
        for (int i = 1; i <= 3; i++)
        {
            await SetFanState(i, state);
        }
    }

    /// <summary>
    /// Display the state of fans, heaters and sensors to the console.
    /// </summary>
    public async Task DisplayAllDevices()
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
            var s1 = await GetSensorTemperature(1);
            Console.WriteLine($"  Sensor 1: Temperature {s1} (Deg)");
            var s2 = await GetSensorTemperature(2);
            Console.WriteLine($"  Sensor 2: Temperature {s2} (Deg)");
            var s3 = await GetSensorTemperature(3);
            Console.WriteLine($"  Sensor 3: Temperature {s3} (Deg)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching sensor data: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset the simulation on the server.
    /// </summary>
    public async Task Reset()
    {
        var response = await _client.PostAsync("api/Envo/reset", null);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to reset client state: {response.ReasonPhrase}");
        }
    }

    /// <summary>
    /// Adjust temperature towards a target over a duration (seconds).
    /// </summary>
    public async Task<double> AdjustTemperature(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        int intervalMs = 1000;
        int iterations = durationSeconds;
        for (int i = 0; i < iterations; i++)
        {
            if (Math.Abs(currentTemperature - targetTemperature) <= 0.1) break;

            if (currentTemperature < targetTemperature)
            {
                await SetAllHeaters(3);
                await SetAllFans(false);
            }
            else
            {
                await SetAllHeaters(0);
                await SetAllFans(true);
            }

            await Task.Delay(intervalMs);
            currentTemperature = await GetAverageTemperature();
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    /// <summary>
    /// Hold temperature at target for duration (seconds).
    /// </summary>
    public async Task<double> HoldTemperature(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        int intervalMs = 1000;
        for (int i = 0; i < durationSeconds; i++)
        {
            if (currentTemperature < targetTemperature)
            {
                await SetAllHeaters(1);
                await SetAllFans(false);
            }
            else if (currentTemperature > targetTemperature)
            {
                await SetAllHeaters(0);
                await SetAllFans(true);
            }

            await Task.Delay(intervalMs);
            currentTemperature = await GetAverageTemperature();
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }
}

public class FanDTO
{
    public int Id { get; set; }
    public bool IsOn { get; set; }
}
