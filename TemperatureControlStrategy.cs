using System;
using System.Threading.Tasks;

/// <summary>
/// Strategy interface for temperature control algorithms.
/// Allows different temperature control strategies to be used interchangeably.
/// </summary>
public interface ITemperatureControlStrategy
{
    /// <summary>
    /// Adjust temperature toward a target value.
    /// </summary>
    Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);

    /// <summary>
    /// Hold temperature at a specific value.
    /// </summary>
    Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);
}

/// <summary>
/// Aggressive temperature control strategy - makes larger adjustments.
/// </summary>
public class AggressiveTemperatureStrategy : ITemperatureControlStrategy
{
    private const double AdjustmentRate = 0.5; // Larger adjustments per cycle

    public async Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        double adjusted = currentTemperature;
        int cycles = durationSeconds / 2; // Adjust every 2 seconds

        for (int i = 0; i < cycles; i++)
        {
            if (Math.Abs(adjusted - targetTemperature) < 0.1) break;
            adjusted += (targetTemperature > adjusted) ? AdjustmentRate : -AdjustmentRate;
            await Task.Delay(2000);
        }

        return adjusted;
    }

    public async Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        // Hold with aggressive control to maintain temperature
        return await AdjustTemperatureAsync(currentTemperature, targetTemperature, durationSeconds);
    }
}

/// <summary>
/// Conservative temperature control strategy - makes smaller, gradual adjustments.
/// </summary>
public class ConservativeTemperatureStrategy : ITemperatureControlStrategy
{
    private const double AdjustmentRate = 0.1; // Smaller adjustments per cycle

    public async Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        double adjusted = currentTemperature;
        int cycles = durationSeconds / 5; // Adjust every 5 seconds

        for (int i = 0; i < cycles; i++)
        {
            if (Math.Abs(adjusted - targetTemperature) < 0.05) break;
            adjusted += (targetTemperature > adjusted) ? AdjustmentRate : -AdjustmentRate;
            await Task.Delay(5000);
        }

        return adjusted;
    }

    public async Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        // Hold with conservative control for stability
        return await AdjustTemperatureAsync(currentTemperature, targetTemperature, durationSeconds);
    }
}
