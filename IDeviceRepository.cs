using System.Threading.Tasks;

public interface IDeviceRepository
{
    Task<double> GetSensorTemperatureAsync(int sensorId);
    Task<double> GetAverageTemperatureAsync();
    Task SetHeaterLevelAsync(int heaterId, int level);
    Task SetFanStateAsync(int fanId, bool isOn);
    Task SetAllHeatersAsync(int level);
    Task SetAllFansAsync(bool state);
    Task DisplayAllDevicesAsync();
    Task ResetAsync();
    Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);
    Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds);
}
