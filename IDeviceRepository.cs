using System.Threading.Tasks;

/// <summary>
/// Repository interface abstracting device operations and state management.
/// Enables testing with mock implementations and reduces coupling to HTTP transport.
/// </summary>
public interface IDeviceRepository
{
    /// <summary>
    /// Get the current temperature from a single sensor.
    /// </summary>
    /// <param name="sensorId">The sensor ID (1-3)</param>
    /// <returns>Temperature in degrees Celsius</returns>
    Task<double> GetSensorTemperatureAsync(int sensorId);

    /// <summary>
    /// Get the average temperature across all sensors.
    /// </summary>
    /// <returns>Average temperature in degrees Celsius</returns>
    Task<double> GetAverageTemperatureAsync();

    /// <summary>
    /// Set the heater level for a specific heater.
    /// </summary>
    /// <param name="heaterId">The heater ID (1-3)</param>
    /// <param name="level">The heater level (0-5)</param>
    Task SetHeaterLevelAsync(int heaterId, int level);

    /// <summary>
    /// Set the state (on/off) for a specific fan.
    /// </summary>
    /// <param name="fanId">The fan ID (1-3)</param>
    /// <param name="isOn">True to turn on, false to turn off</param>
    Task SetFanStateAsync(int fanId, bool isOn);

    /// <summary>
    /// Set all heaters to the same level.
    /// </summary>
    /// <param name="level">The heater level (0-5)</param>
    Task SetAllHeatersAsync(int level);

    /// <summary>
    /// Set all fans to the same state.
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    Task SetAllFansAsync(bool state);

    /// <summary>
    /// Display the current state of all devices (fans, heaters, sensors).
    /// </summary>
    Task DisplayAllDevicesAsync();

    /// <summary>
    /// Reset the simulation to its initial state.
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Adjust temperature towards a target over a specified duration.
    /// </summary>
    /// <param name="currentTemperature">The current temperature</param>
    /// <param name="targetTemperature">The target temperature</param>
    /// <param name="durationSeconds">Duration in seconds</param>
    /// <returns>The final temperature reached</returns>
    Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);

    /// <summary>
    /// Hold temperature at a target level for a specified duration.
    /// </summary>
    /// <param name="currentTemperature">The current temperature</param>
    /// <param name="targetTemperature">The target temperature to maintain</param>
    /// <param name="durationSeconds">Duration in seconds</param>
    /// <returns>The final temperature</returns>
    Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);
}
