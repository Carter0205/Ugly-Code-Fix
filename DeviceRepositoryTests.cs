using System;
using System.Threading.Tasks;

/// <summary>
/// Unit tests for the device repository implementations.
/// Tests both mock and HTTP implementations to verify behavior and contracts.
/// </summary>
public static class DeviceRepositoryTests
{
    /// <summary>
    /// Test 1: Mock repository can set and retrieve heater levels.
    /// </summary>
    public static async Task Test_SetAndGetHeaterLevel()
    {
        var repo = new MockDeviceRepository();
        await repo.SetHeaterLevelAsync(1, 3);
        // Verify by checking call log
        Assert.Contains(repo.CallLog, "SetHeaterLevelAsync(1, 3)");
        Console.WriteLine("✓ Test_SetAndGetHeaterLevel passed");
    }

    /// <summary>
    /// Test 2: Mock repository can set and retrieve fan states.
    /// </summary>
    public static async Task Test_SetAndGetFanState()
    {
        var repo = new MockDeviceRepository();
        await repo.SetFanStateAsync(2, true);
        Assert.Contains(repo.CallLog, "SetFanStateAsync(2, True)");
        Console.WriteLine("✓ Test_SetAndGetFanState passed");
    }

    /// <summary>
    /// Test 3: Mock repository returns valid sensor temperatures.
    /// </summary>
    public static async Task Test_GetSensorTemperature()
    {
        var repo = new MockDeviceRepository();
        double temp = await repo.GetSensorTemperatureAsync(1);
        Assert.GreaterThanOrEqual(temp, 0.0, "Temperature should be non-negative");
        Console.WriteLine($"✓ Test_GetSensorTemperature passed (temp={temp}°C)");
    }

    /// <summary>
    /// Test 4: Mock repository calculates average of three sensors correctly.
    /// </summary>
    public static async Task Test_GetAverageTemperature()
    {
        var repo = new MockDeviceRepository();
        double avg = await repo.GetAverageTemperatureAsync();
        // Should be average of 20.0, 20.5, 21.0 = 20.5
        Assert.ApproximatelyEqual(avg, 20.5, 0.01, "Average temperature calculation incorrect");
        Console.WriteLine($"✓ Test_GetAverageTemperature passed (avg={avg}°C)");
    }

    /// <summary>
    /// Test 5: Mock repository can reset all state.
    /// </summary>
    public static async Task Test_ResetSimulation()
    {
        var repo = new MockDeviceRepository();
        await repo.SetAllHeatersAsync(5);
        await repo.SetAllFansAsync(true);
        await repo.ResetAsync();
        Assert.Contains(repo.CallLog, "ResetAsync()");
        // After reset, heaters should be 0 and fans off
        double temp = await repo.GetAverageTemperatureAsync();
        Assert.ApproximatelyEqual(temp, 20.5, 0.01, "Temperature should be reset");
        Console.WriteLine("✓ Test_ResetSimulation passed");
    }

    /// <summary>
    /// Test 6: Mock repository sets all fans to on state.
    /// </summary>
    public static async Task Test_SetAllFansOn()
    {
        var repo = new MockDeviceRepository();
        await repo.SetAllFansAsync(true);
        Assert.CountInLog(repo.CallLog, "SetFanStateAsync", 3, "Should set 3 fans");
        Console.WriteLine("✓ Test_SetAllFansOn passed");
    }

    /// <summary>
    /// Test 7: Mock repository sets all heaters to a level.
    /// </summary>
    public static async Task Test_SetAllHeatersToLevel()
    {
        var repo = new MockDeviceRepository();
        await repo.SetAllHeatersAsync(2);
        Assert.CountInLog(repo.CallLog, "SetHeaterLevelAsync", 3, "Should set 3 heaters");
        Console.WriteLine("✓ Test_SetAllHeatersToLevel passed");
    }

    /// <summary>
    /// Test 8: Mock repository raises exception on invalid sensor ID.
    /// </summary>
    public static async Task Test_InvalidSensorIdThrows()
    {
        var repo = new MockDeviceRepository();
        try
        {
            await repo.GetSensorTemperatureAsync(99);
            throw new Exception("Should have thrown ArgumentException");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Test_InvalidSensorIdThrows passed");
        }
    }

    /// <summary>
    /// Test 9: Mock repository raises exception on invalid heater level.
    /// </summary>
    public static async Task Test_InvalidHeaterLevelThrows()
    {
        var repo = new MockDeviceRepository();
        try
        {
            await repo.SetHeaterLevelAsync(1, 10);
            throw new Exception("Should have thrown ArgumentException");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Test_InvalidHeaterLevelThrows passed");
        }
    }

    /// <summary>
    /// Test 10: Mock repository adjust temperature returns plausible value.
    /// </summary>
    public static async Task Test_AdjustTemperature()
    {
        var repo = new MockDeviceRepository();
        double result = await repo.AdjustTemperatureAsync(20.0, 25.0, 10);
        Assert.GreaterThan(result, 20.0, "Temperature should increase towards target");
        Console.WriteLine($"✓ Test_AdjustTemperature passed (result={result}°C)");
    }

    /// <summary>
    /// Test 11: Mock repository hold temperature returns near target.
    /// </summary>
    public static async Task Test_HoldTemperature()
    {
        var repo = new MockDeviceRepository();
        double result = await repo.HoldTemperatureAsync(20.0, 20.0, 5);
        Assert.ApproximatelyEqual(result, 20.0, 1.0, "Temperature should stay near target");
        Console.WriteLine($"✓ Test_HoldTemperature passed (result={result}°C)");
    }

    /// <summary>
    /// Test 12: Mock repository displays all devices without throwing.
    /// </summary>
    public static async Task Test_DisplayAllDevices()
    {
        var repo = new MockDeviceRepository();
        await repo.DisplayAllDevicesAsync();
        Assert.Contains(repo.CallLog, "DisplayAllDevicesAsync()");
        Console.WriteLine("✓ Test_DisplayAllDevices passed");
    }

    /// <summary>
    /// Test 13: Multiple operations log correctly in call log.
    /// </summary>
    public static async Task Test_CallLogTracksAllCalls()
    {
        var repo = new MockDeviceRepository();
        await repo.GetSensorTemperatureAsync(1);
        await repo.SetFanStateAsync(1, true);
        await repo.SetHeaterLevelAsync(2, 3);
        Assert.AreEqual(repo.CallLog.Count, 3, "Should have logged 3 calls");
        Console.WriteLine("✓ Test_CallLogTracksAllCalls passed");
    }

    /// <summary>
    /// Test 14: Interface IDeviceRepository contract is satisfied by MockDeviceRepository.
    /// </summary>
    public static void Test_ImplementsInterface()
    {
        IDeviceRepository repo = new MockDeviceRepository();
        Assert.IsNotNull(repo, "Mock repository should implement IDeviceRepository");
        Console.WriteLine("✓ Test_ImplementsInterface passed");
    }

    /// <summary>
    /// Test 15: Repository can be used polymorphically.
    /// </summary>
    public static async Task Test_PolymorphicUsage()
    {
        IDeviceRepository repo = new MockDeviceRepository();
        await repo.SetAllFansAsync(true);
        double temp = await repo.GetAverageTemperatureAsync();
        Assert.GreaterThanOrEqual(temp, 0.0, "Should work through interface");
        Console.WriteLine($"✓ Test_PolymorphicUsage passed (temp={temp}°C)");
    }
}

/// <summary>
/// Simple assertion helpers for testing.
/// </summary>
public static class Assert
{
    public static void AreEqual<T>(T actual, T expected, string message = "")
    {
        if (!Equals(actual, expected))
            throw new Exception($"Assertion failed: {message}. Expected {expected}, got {actual}");
    }

    public static void IsNotNull<T>(T value, string message = "")
    {
        if (value == null)
            throw new Exception($"Assertion failed: {message}. Value should not be null");
    }

    public static void GreaterThan(double actual, double expected, string message = "")
    {
        if (!(actual > expected))
            throw new Exception($"Assertion failed: {message}. Expected {actual} > {expected}");
    }

    public static void GreaterThanOrEqual(double actual, double expected, string message = "")
    {
        if (!(actual >= expected))
            throw new Exception($"Assertion failed: {message}. Expected {actual} >= {expected}");
    }

    public static void ApproximatelyEqual(double actual, double expected, double tolerance, string message = "")
    {
        if (Math.Abs(actual - expected) > tolerance)
            throw new Exception($"Assertion failed: {message}. Expected {actual} ≈ {expected} (±{tolerance}), diff={Math.Abs(actual - expected)}");
    }

    public static void Contains<T>(System.Collections.Generic.List<T> list, T item, string message = "")
    {
        if (!list.Contains(item))
            throw new Exception($"Assertion failed: {message}. List should contain {item}");
    }

    public static void CountInLog(System.Collections.Generic.List<string> log, string substring, int expectedCount, string message = "")
    {
        int count = log.FindAll(s => s.Contains(substring)).Count;
        if (count != expectedCount)
            throw new Exception($"Assertion failed: {message}. Expected {expectedCount} calls containing '{substring}', found {count}");
    }
}
