using System;
using System.Threading.Tasks;

/// <summary>
/// Facade pattern - Provides a simplified interface to complex device operations.
/// Combines multiple repository calls into simple, high-level operations.
/// </summary>
public class DeviceManagementFacade
{
    private readonly IDeviceRepository _repository;
    private ITemperatureControlStrategy _temperatureStrategy;

    public DeviceManagementFacade(IDeviceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        // Default to conservative strategy
        _temperatureStrategy = new ConservativeTemperatureStrategy();
    }

    /// <summary>
    /// Set the temperature control strategy (Strategy Pattern).
    /// Allows switching between different control algorithms at runtime.
    /// </summary>
    public void SetTemperatureStrategy(ITemperatureControlStrategy strategy)
    {
        _temperatureStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Simple operation: Read all temperature sensors and return the average.
    /// Facade hides the complexity of reading multiple sensors.
    /// </summary>
    public async Task<double> GetAverageTemperatureAsync()
    {
        return await _repository.GetAverageTemperatureAsync();
    }

    /// <summary>
    /// Simple operation: Cool the environment by turning on fans and reducing heaters.
    /// Facade combines multiple device operations into one logical action.
    /// </summary>
    public async Task CoolEnvironmentAsync()
    {
        await _repository.SetAllFansAsync(true);
        await _repository.SetAllHeatersAsync(0);
        Console.WriteLine("Environment cooling: Fans ON, Heaters OFF");
    }

    /// <summary>
    /// Simple operation: Warm the environment by turning off fans and increasing heaters.
    /// Facade hides the details of multiple device control calls.
    /// </summary>
    public async Task WarmEnvironmentAsync()
    {
        await _repository.SetAllFansAsync(false);
        await _repository.SetAllHeatersAsync(3);
        Console.WriteLine("Environment warming: Fans OFF, Heaters level 3");
    }

    /// <summary>
    /// Simple operation: Achieve and maintain a target temperature.
    /// Facade uses the selected temperature strategy and handles all complexity.
    /// </summary>
    public async Task<double> AchieveTargetTemperatureAsync(double targetTemperature, int durationSeconds)
    {
        double currentTemperature = await _repository.GetAverageTemperatureAsync();
        Console.WriteLine($"Starting temperature: {currentTemperature:F1}°C");
        Console.WriteLine($"Target temperature: {targetTemperature:F1}°C");

        // Adjust using the selected strategy
        double finalTemperature = await _temperatureStrategy.AdjustTemperatureAsync(
            currentTemperature, 
            targetTemperature, 
            durationSeconds
        );

        Console.WriteLine($"Final temperature: {finalTemperature:F1}°C");
        return finalTemperature;
    }

    /// <summary>
    /// Simple operation: Display system status without dealing with individual sensors.
    /// Facade simplifies the display by calling the repository method.
    /// </summary>
    public async Task DisplaySystemStatusAsync()
    {
        await _repository.DisplayAllDevicesAsync();
    }

    /// <summary>
    /// Simple operation: Emergency shutdown - turn off all devices immediately.
    /// Facade ensures all devices are safely shut down in the correct order.
    /// </summary>
    public async Task EmergencyShutdownAsync()
    {
        Console.WriteLine("EMERGENCY SHUTDOWN - Turning off all devices");
        await _repository.SetAllFansAsync(false);
        await _repository.SetAllHeatersAsync(0);
        Console.WriteLine("All devices are OFF");
    }

    /// <summary>
    /// Simple operation: Reset simulation to initial state.
    /// </summary>
    public async Task ResetSimulationAsync()
    {
        await _repository.ResetAsync();
        Console.WriteLine("Simulation reset to initial state");
    }
}
