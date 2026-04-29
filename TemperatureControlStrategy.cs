using System;
using System.Threading.Tasks;

public interface ITemperatureControlStrategy
{
    Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);
    Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);
}

public class AggressiveTemperatureStrategy : ITemperatureControlStrategy
{
    private const double AdjustmentRate = 0.5;

    public async Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        double adjusted = currentTemperature;
        int cycles = durationSeconds / 2;

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
        return await AdjustTemperatureAsync(currentTemperature, targetTemperature, durationSeconds);
    }
}

public class ConservativeTemperatureStrategy : ITemperatureControlStrategy
{
    private const double AdjustmentRate = 0.1;

    public async Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        double adjusted = currentTemperature;
        int cycles = durationSeconds / 5;

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
        return await AdjustTemperatureAsync(currentTemperature, targetTemperature, durationSeconds);
    }
}
