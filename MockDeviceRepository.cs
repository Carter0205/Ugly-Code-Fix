using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Mock implementation of the device repository for unit testing.
/// Simulates device behavior without requiring a running web server.
/// </summary>
public class MockDeviceRepository : IDeviceRepository
{
    private double _temp1 = 20.0;
    private double _temp2 = 20.5;
    private double _temp3 = 21.0;
    private int[] _heaterLevels = { 0, 0, 0 };
    private bool[] _fanStates = { false, false, false };
    public List<string> CallLog { get; } = new List<string>();

    public Task<double> GetSensorTemperatureAsync(int sensorId)
    {
        CallLog.Add($"GetSensorTemperatureAsync({sensorId})");
        return sensorId switch
        {
            1 => Task.FromResult(_temp1),
            2 => Task.FromResult(_temp2),
            3 => Task.FromResult(_temp3),
            _ => throw new ArgumentException("Invalid sensor ID")
        };
    }

    public async Task<double> GetAverageTemperatureAsync()
    {
        CallLog.Add("GetAverageTemperatureAsync()");
        var s1 = await GetSensorTemperatureAsync(1);
        var s2 = await GetSensorTemperatureAsync(2);
        var s3 = await GetSensorTemperatureAsync(3);
        return (s1 + s2 + s3) / 3.0;
    }

    public Task SetHeaterLevelAsync(int heaterId, int level)
    {
        CallLog.Add($"SetHeaterLevelAsync({heaterId}, {level})");
        if (heaterId < 1 || heaterId > 3) throw new ArgumentException("Invalid heater ID");
        if (level < 0 || level > 5) throw new ArgumentException("Invalid heater level");
        _heaterLevels[heaterId - 1] = level;
        return Task.CompletedTask;
    }

    public Task SetFanStateAsync(int fanId, bool isOn)
    {
        CallLog.Add($"SetFanStateAsync({fanId}, {isOn})");
        if (fanId < 1 || fanId > 3) throw new ArgumentException("Invalid fan ID");
        _fanStates[fanId - 1] = isOn;
        return Task.CompletedTask;
    }

    public async Task SetAllHeatersAsync(int level)
    {
        CallLog.Add($"SetAllHeatersAsync({level})");
        for (int i = 1; i <= 3; i++)
        {
            await SetHeaterLevelAsync(i, level);
        }
    }

    public async Task SetAllFansAsync(bool state)
    {
        CallLog.Add($"SetAllFansAsync({state})");
        for (int i = 1; i <= 3; i++)
        {
            await SetFanStateAsync(i, state);
        }
    }

    public Task DisplayAllDevicesAsync()
    {
        CallLog.Add("DisplayAllDevicesAsync()");
        Console.WriteLine($"Fan 1: {(_fanStates[0] ? "On" : "Off")}, Fan 2: {(_fanStates[1] ? "On" : "Off")}, Fan 3: {(_fanStates[2] ? "On" : "Off")}");
        Console.WriteLine($"Heater 1: Level {_heaterLevels[0]}, Heater 2: Level {_heaterLevels[1]}, Heater 3: Level {_heaterLevels[2]}");
        Console.WriteLine($"Sensor 1: {_temp1:F1}°C, Sensor 2: {_temp2:F1}°C, Sensor 3: {_temp3:F1}°C");
        return Task.CompletedTask;
    }

    public Task ResetAsync()
    {
        CallLog.Add("ResetAsync()");
        _temp1 = 20.0;
        _temp2 = 20.5;
        _temp3 = 21.0;
        _heaterLevels = new int[] { 0, 0, 0 };
        _fanStates = new bool[] { false, false, false };
        return Task.CompletedTask;
    }

    public async Task<double> AdjustTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        CallLog.Add($"AdjustTemperatureAsync({currentTemperature}, {targetTemperature}, {durationSeconds})");
        // Simulate temperature change towards target
        double direction = targetTemperature > currentTemperature ? 0.5 : -0.5;
        return currentTemperature + (direction * durationSeconds / 10.0);
    }

    public async Task<double> HoldTemperatureAsync(double currentTemperature, double targetTemperature, int durationSeconds)
    {
        CallLog.Add($"HoldTemperatureAsync({currentTemperature}, {targetTemperature}, {durationSeconds})");
        // Simulate small fluctuations around target
        return currentTemperature + (new Random().NextDouble() - 0.5);
    }
}
