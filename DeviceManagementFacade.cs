using System;
using System.Threading.Tasks;

/// <summary>
/// Facade for device management operations.
/// </summary>
public class DeviceManagementFacade
{
    private readonly IDeviceRepository _repository;
    private ITemperatureControlStrategy _temperatureStrategy;

    public DeviceManagementFacade(IDeviceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _temperatureStrategy = new ConservativeTemperatureStrategy();
    }

    public void SetTemperatureStrategy(ITemperatureControlStrategy strategy)
    {
        _temperatureStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    public async Task<double> GetAverageTemperatureAsync()
    {
        return await _repository.GetAverageTemperatureAsync();
    }

    public async Task CoolEnvironmentAsync()
    {
        await _repository.SetAllFansAsync(true);
        await _repository.SetAllHeatersAsync(0);
        Console.WriteLine("Environment cooling: Fans ON, Heaters OFF");
    }

    public async Task WarmEnvironmentAsync()
    {
        await _repository.SetAllFansAsync(false);
        await _repository.SetAllHeatersAsync(3);
        Console.WriteLine("Environment warming: Fans OFF, Heaters level 3");
    }

    public async Task<double> AchieveTargetTemperatureAsync(double targetTemperature, int durationSeconds)
    {
        double currentTemperature = await _repository.GetAverageTemperatureAsync();
        Console.WriteLine($"Starting temperature: {currentTemperature:F1}°C");
        Console.WriteLine($"Target temperature: {targetTemperature:F1}°C");

        double finalTemperature = await _temperatureStrategy.AdjustTemperatureAsync(
            currentTemperature, 
            targetTemperature, 
            durationSeconds
        );

        Console.WriteLine($"Final temperature: {finalTemperature:F1}°C");
        return finalTemperature;
    }

    public async Task DisplaySystemStatusAsync()
    {
        await _repository.DisplayAllDevicesAsync();
    }

    public async Task EmergencyShutdownAsync()
    {
        Console.WriteLine("EMERGENCY SHUTDOWN - Turning off all devices");
        await _repository.SetAllFansAsync(false);
        await _repository.SetAllHeatersAsync(0);
        Console.WriteLine("All devices are OFF");
    }

    public async Task ResetSimulationAsync()
    {
        await _repository.ResetAsync();
        Console.WriteLine("Simulation reset to initial state");
    }
}
