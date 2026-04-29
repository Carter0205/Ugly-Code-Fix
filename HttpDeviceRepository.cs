using System;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;

public class HttpDeviceRepository : IDeviceRepository
{
    private const int DeviceCount = 3;
    private const int MinHeaterLevel = 0;
    private const int MaxHeaterLevel = 5;
    private const int FullHeaterLevel = 3;
    private const int LowHeaterLevel = 1;
    private const int OffHeaterLevel = 0;
    private const double TemperatureTolerance = 0.1;
    private const int DelayIntervalMs = 1000;

    private readonly HttpClient _client;

    public HttpDeviceRepository(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<double> GetSensorTemperatureAsync(int sensorId)
    {
        var response = await _client.GetAsync($"api/sensor/{sensorId}");
        if (response.IsSuccessStatusCode)
        {
            var temperatureString = await response.Content.ReadAsStringAsync();
            if (ParseTemperature(temperatureString, out double temperature))
            {
                return temperature;
            }
            throw new Exception($"Failed to parse temperature from sensor {sensorId}: '{temperatureString}'");
        }

        throw new Exception($"Failed to get temperature from sensor {sensorId}: {response.ReasonPhrase}");
    }

    private bool ParseTemperature(string temperatureString, out double temperature)
    {
        if (double.TryParse(temperatureString, NumberStyles.Any, CultureInfo.InvariantCulture, out temperature))
        {
            return true;
        }
        return double.TryParse(temperatureString, out temperature);
    }

    public async Task<double> GetAverageTemperatureAsync()
    {
        double totalTemperature = 0;
        for (int i = 1; i <= DeviceCount; i++)
        {
            totalTemperature += await GetSensorTemperatureAsync(i);
        }

        return totalTemperature / DeviceCount;
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
        for (int i = 1; i <= DeviceCount; i++)
        {
            await SetHeaterLevelAsync(i, level);
        }
    }

    public async Task SetAllFansAsync(bool state)
    {
        for (int i = 1; i <= DeviceCount; i++)
        {
            await SetFanStateAsync(i, state);
        }
    }

    public async Task DisplayAllDevicesAsync()
    {
        Console.WriteLine("Fetching fan states individually...");
        for (int i = 1; i <= DeviceCount; i++)
        {
            await DisplayFanState(i);
        }

        Console.WriteLine("Fetching heater levels individually...");
        for (int i = 1; i <= DeviceCount; i++)
        {
            await DisplayHeaterLevel(i);
        }

        Console.WriteLine("Fetching sensor temperatures individually...");
        try
        {
            for (int i = 1; i <= DeviceCount; i++)
            {
                double temperature = await GetSensorTemperatureAsync(i);
                Console.WriteLine($"  Sensor {i}: Temperature {temperature:F1}°C");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching sensor data: {ex.Message}");
        }
    }

    private async Task DisplayFanState(int fanId)
    {
        try
        {
            var fanResponse = await _client.GetAsync($"api/fans/{fanId}/state");
            if (fanResponse.IsSuccessStatusCode)
            {
                var fanJson = await fanResponse.Content.ReadAsStringAsync();
                var fan = JsonSerializer.Deserialize<FanDTO>(fanJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                Console.WriteLine($"  Fan {fan?.Id}: {(fan?.IsOn ?? false ? "On" : "Off")}");
            }
            else
            {
                Console.WriteLine($"  Fan {fanId}: Failed to fetch state.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Fan {fanId}: Error fetching state - {ex.Message}");
        }
    }

    private async Task DisplayHeaterLevel(int heaterId)
    {
        try
        {
            var heaterResponse = await _client.GetAsync($"api/heat/{heaterId}/level");
            if (heaterResponse.IsSuccessStatusCode)
            {
                var levelString = await heaterResponse.Content.ReadAsStringAsync();
                if (int.TryParse(levelString, out int level))
                {
                    Console.WriteLine($"  Heater {heaterId}: Level {level}");
                }
                else
                {
                    Console.WriteLine($"  Heater {heaterId}: Failed to parse level.");
                }
            }
            else
            {
                Console.WriteLine($"  Heater {heaterId}: Failed to fetch level.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Heater {heaterId}: Error fetching level - {ex.Message}");
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
        int iterations = durationSeconds;
        for (int i = 0; i < iterations; i++)
        {
            if (Math.Abs(currentTemperature - targetTemperature) <= TemperatureTolerance)
            {
                break;
            }

            if (currentTemperature < targetTemperature)
            {
                await SetAllHeatersAsync(FullHeaterLevel);
                await SetAllFansAsync(false);
            }
            else
            {
                await SetAllHeatersAsync(OffHeaterLevel);
                await SetAllFansAsync(true);
            }

            await Task.Delay(DelayIntervalMs);
            currentTemperature = await GetAverageTemperatureAsync();
            Console.WriteLine($"Current Temperature: {currentTemperature:F1}°C");
        }

        return currentTemperature;
    }

    public async Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        for (int i = 0; i < durationSeconds; i++)
        {
            if (currentTemperature < targetTemperature)
            {
                await SetAllHeatersAsync(LowHeaterLevel);
                await SetAllFansAsync(false);
            }
            else if (currentTemperature > targetTemperature)
            {
                await SetAllHeatersAsync(OffHeaterLevel);
                await SetAllFansAsync(true);
            }

            await Task.Delay(DelayIntervalMs);
            currentTemperature = await GetAverageTemperatureAsync();
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
