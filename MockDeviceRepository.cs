using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Mock implementation of the device repository for unit testing.
/// Simulates device behavior without requiring a running web server.
/// </summary>
public class MockDeviceRepository : IDeviceRepository
{
    private const int MinSensorId = 1;
    private const int MaxSensorId = 3;
    private const int MinHeaterLevel = 0;
    private const int MaxHeaterLevel = 5;
    private const double InitialSensor1Temperature = 20.0;
    private const double InitialSensor2Temperature = 20.5;
    private const double InitialSensor3Temperature = 21.0;

    private double _sensor1Temperature = InitialSensor1Temperature;
    private double _sensor2Temperature = InitialSensor2Temperature;
    private double _sensor3Temperature = InitialSensor3Temperature;
    private int[] _heaterLevels = { 0, 0, 0 };
    private bool[] _fanStates = { false, false, false };
    public List<string> CallLog { get; } = new List<string>();

    public Task<double> GetSensorTemperatureAsync(int sensorId)
    {
        CallLog.Add($"GetSensorTemperatureAsync({sensorId})");
        return sensorId switch
        {
            MinSensorId => Task.FromResult(_sensor1Temperature),
            2 => Task.FromResult(_sensor2Temperature),
            MaxSensorId => Task.FromResult(_sensor3Temperature),
            _ => throw new ArgumentException("Invalid sensor ID")
        };
    }

    public async Task<double> GetAverageTemperatureAsync()
    {
        CallLog.Add("GetAverageTemperatureAsync()");
        var sensor1 = await GetSensorTemperatureAsync(MinSensorId);
        var sensor2 = await GetSensorTemperatureAsync(2);
        var sensor3 = await GetSensorTemperatureAsync(MaxSensorId);
        return (sensor1 + sensor2 + sensor3) / 3.0;
    }

    public Task SetHeaterLevelAsync(int heaterId, int level)
    {
        CallLog.Add($"SetHeaterLevelAsync({heaterId}, {level})");
        if (heaterId < MinSensorId || heaterId > MaxSensorId) throw new ArgumentException("Invalid heater ID");
        if (level < MinHeaterLevel || level > MaxHeaterLevel) throw new ArgumentException("Invalid heater level");
        _heaterLevels[heaterId - 1] = level;
        return Task.CompletedTask;
    }

    public Task SetFanStateAsync(int fanId, bool isOn)
    {
        CallLog.Add($"SetFanStateAsync({fanId}, {isOn})");
        if (fanId < MinSensorId || fanId > MaxSensorId) throw new ArgumentException("Invalid fan ID");
        _fanStates[fanId - 1] = isOn;
        return Task.CompletedTask;
    }

    public async Task SetAllHeatersAsync(int level)
    {
        CallLog.Add($"SetAllHeatersAsync({level})");
        for (int i = MinSensorId; i <= MaxSensorId; i++)
        {
            await SetHeaterLevelAsync(i, level);
        }
    }

    public async Task SetAllFansAsync(bool state)
    {
        CallLog.Add($"SetAllFansAsync({state})");
        for (int i = MinSensorId; i <= MaxSensorId; i++)
        {
            await SetFanStateAsync(i, state);
        }
    }

    public Task DisplayAllDevicesAsync()
    {
        CallLog.Add("DisplayAllDevicesAsync()");
        Console.WriteLine($"Fan 1: {(_fanStates[0] ? "On" : "Off")}, Fan 2: {(_fanStates[1] ? "On" : "Off")}, Fan 3: {(_fanStates[2] ? "On" : "Off")}");
        Console.WriteLine($"Heater 1: Level {_heaterLevels[0]}, Heater 2: Level {_heaterLevels[1]}, Heater 3: Level {_heaterLevels[2]}");
        Console.WriteLine($"Sensor 1: {_sensor1Temperature:F1}°C, Sensor 2: {_sensor2Temperature:F1}°C, Sensor 3: {_sensor3Temperature:F1}°C");
        return Task.CompletedTask;
    }

    public Task ResetAsync()
    {
        CallLog.Add("ResetAsync()");
        _sensor1Temperature = InitialSensor1Temperature;
        _sensor2Temperature = InitialSensor2Temperature;
        _sensor3Temperature = InitialSensor3Temperature;
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
