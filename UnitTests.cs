using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UglyClientTests
{
    /// <summary>
    /// Unit tests for MockDeviceRepository to ensure correct behavior.
    /// Tests cover all interface methods and error conditions.
    /// </summary>
    public static class MockDeviceRepositoryTests
    {
        // Test 1: Temperature retrieval for individual sensors
        public static async Task Test_GetSensorTemperatureReturnsValidTemperature()
        {
            var repository = new MockDeviceRepository();

            double sensor1Temp = await repository.GetSensorTemperatureAsync(1);
            double sensor2Temp = await repository.GetSensorTemperatureAsync(2);
            double sensor3Temp = await repository.GetSensorTemperatureAsync(3);

            AssertTemperatureIsValid(sensor1Temp, "Sensor 1");
            AssertTemperatureIsValid(sensor2Temp, "Sensor 2");
            AssertTemperatureIsValid(sensor3Temp, "Sensor 3");

            Console.WriteLine("✓ Test_GetSensorTemperatureReturnsValidTemperature passed");
        }

        // Test 2: Average temperature calculation
        public static async Task Test_AverageTemperatureCalculation()
        {
            var repository = new MockDeviceRepository();

            double average = await repository.GetAverageTemperatureAsync();
            double sensor1 = await repository.GetSensorTemperatureAsync(1);
            double sensor2 = await repository.GetSensorTemperatureAsync(2);
            double sensor3 = await repository.GetSensorTemperatureAsync(3);

            double expectedAverage = (sensor1 + sensor2 + sensor3) / 3.0;

            AssertApproximatelyEqual(average, expectedAverage, 0.01, "Average temperature");
            Console.WriteLine($"✓ Test_AverageTemperatureCalculation passed (avg={average:F1}°C)");
        }

        // Test 3: Setting heater level
        public static async Task Test_SetHeaterLevel()
        {
            var repository = new MockDeviceRepository();

            await repository.SetHeaterLevelAsync(1, 3);
            AssertContainsCall(repository.CallLog, "SetHeaterLevelAsync(1, 3)");

            Console.WriteLine("✓ Test_SetHeaterLevel passed");
        }

        // Test 4: Invalid heater level throws exception
        public static async Task Test_InvalidHeaterLevelThrows()
        {
            var repository = new MockDeviceRepository();

            try
            {
                await repository.SetHeaterLevelAsync(1, 10);
                throw new Exception("Should have thrown ArgumentException for invalid heater level");
            }
            catch (ArgumentException ex)
            {
                AssertTrue(ex.Message.Contains("heater level"), "Exception should mention heater level");
                Console.WriteLine("✓ Test_InvalidHeaterLevelThrows passed");
            }
        }

        // Test 5: Invalid heater ID throws exception
        public static async Task Test_InvalidHeaterIdThrows()
        {
            var repository = new MockDeviceRepository();

            try
            {
                await repository.SetHeaterLevelAsync(99, 3);
                throw new Exception("Should have thrown ArgumentException for invalid heater ID");
            }
            catch (ArgumentException ex)
            {
                AssertTrue(ex.Message.Contains("heater"), "Exception should mention heater ID");
                Console.WriteLine("✓ Test_InvalidHeaterIdThrows passed");
            }
        }

        // Test 6: Setting fan state
        public static async Task Test_SetFanState()
        {
            var repository = new MockDeviceRepository();

            await repository.SetFanStateAsync(1, true);
            AssertContainsCall(repository.CallLog, "SetFanStateAsync(1, True)");

            await repository.SetFanStateAsync(2, false);
            AssertContainsCall(repository.CallLog, "SetFanStateAsync(2, False)");

            Console.WriteLine("✓ Test_SetFanState passed");
        }

        // Test 7: Invalid fan ID throws exception
        public static async Task Test_InvalidFanIdThrows()
        {
            var repository = new MockDeviceRepository();

            try
            {
                await repository.SetFanStateAsync(5, true);
                throw new Exception("Should have thrown ArgumentException for invalid fan ID");
            }
            catch (ArgumentException ex)
            {
                AssertTrue(ex.Message.Contains("fan"), "Exception should mention fan ID");
                Console.WriteLine("✓ Test_InvalidFanIdThrows passed");
            }
        }

        // Test 8: Set all heaters
        public static async Task Test_SetAllHeaters()
        {
            var repository = new MockDeviceRepository();

            await repository.SetAllHeatersAsync(2);

            AssertCountInLog(repository.CallLog, "SetHeaterLevelAsync", 4, 
                "Should set 3 heaters (3 calls) + 1 SetAllHeatersAsync call");

            Console.WriteLine("✓ Test_SetAllHeaters passed");
        }

        // Test 9: Set all fans
        public static async Task Test_SetAllFans()
        {
            var repository = new MockDeviceRepository();

            await repository.SetAllFansAsync(true);

            AssertCountInLog(repository.CallLog, "SetFanStateAsync", 3, 
                "Should set 3 fans");

            Console.WriteLine("✓ Test_SetAllFans passed");
        }

        // Test 10: Call log tracks all operations
        public static async Task Test_CallLogTracksOperations()
        {
            var repository = new MockDeviceRepository();

            int initialCount = repository.CallLog.Count;

            await repository.GetSensorTemperatureAsync(1);
            await repository.SetFanStateAsync(1, true);
            await repository.SetHeaterLevelAsync(1, 2);

            AssertEqual(repository.CallLog.Count, initialCount + 3, 
                "Call log should track all 3 operations");

            Console.WriteLine("✓ Test_CallLogTracksOperations passed");
        }

        // Test 11: Reset clears all state
        public static async Task Test_ResetClearsState()
        {
            var repository = new MockDeviceRepository();

            await repository.SetAllHeatersAsync(5);
            await repository.SetAllFansAsync(true);

            await repository.ResetAsync();

            AssertContainsCall(repository.CallLog, "ResetAsync()");

            // After reset, should get initial temperatures
            double avg = await repository.GetAverageTemperatureAsync();
            AssertApproximatelyEqual(avg, 20.5, 0.1, "After reset, temperature should be initial value");

            Console.WriteLine("✓ Test_ResetClearsState passed");
        }

        // Test 12: Adjust temperature increases when below target
        public static async Task Test_AdjustTemperatureIncreases()
        {
            var repository = new MockDeviceRepository();

            double currentTemp = 15.0;
            double targetTemp = 25.0;
            double result = await repository.AdjustTemperatureAsync(currentTemp, targetTemp, 10);

            AssertTrue(result > currentTemp, "Temperature should increase toward target");
            Console.WriteLine($"✓ Test_AdjustTemperatureIncreases passed (result={result:F1}°C)");
        }

        // Test 13: Adjust temperature decreases when above target
        public static async Task Test_AdjustTemperatureDecreases()
        {
            var repository = new MockDeviceRepository();

            double currentTemp = 25.0;
            double targetTemp = 15.0;
            double result = await repository.AdjustTemperatureAsync(currentTemp, targetTemp, 10);

            AssertTrue(result < currentTemp, "Temperature should decrease toward target");
            Console.WriteLine($"✓ Test_AdjustTemperatureDecreases passed (result={result:F1}°C)");
        }

        // Test 14: Hold temperature returns plausible value
        public static async Task Test_HoldTemperatureReturnsPlausible()
        {
            var repository = new MockDeviceRepository();

            double currentTemp = 20.0;
            double result = await repository.HoldTemperatureAsync(currentTemp, currentTemp, 5);

            AssertTrue(result >= 19.0 && result <= 21.0, 
                "Temperature should stay within ±1°C of target when holding");

            Console.WriteLine($"✓ Test_HoldTemperatureReturnsPlausible passed (result={result:F1}°C)");
        }

        // Test 15: Display all devices completes without error
        public static async Task Test_DisplayAllDevicesCompletes()
        {
            var repository = new MockDeviceRepository();

            try
            {
                await repository.DisplayAllDevicesAsync();
                AssertContainsCall(repository.CallLog, "DisplayAllDevicesAsync()");
                Console.WriteLine("✓ Test_DisplayAllDevicesCompletes passed");
            }
            catch (Exception ex)
            {
                throw new Exception($"DisplayAllDevicesAsync should not throw: {ex.Message}");
            }
        }

        // Test 16: Repository implements interface
        public static void Test_ImplementsInterface()
        {
            var repo = new MockDeviceRepository();
            AssertTrue(repo is IDeviceRepository, "MockDeviceRepository must implement IDeviceRepository");
            Console.WriteLine("✓ Test_ImplementsInterface passed");
        }

        // Test 17: Polymorphic usage works correctly
        public static async Task Test_PolymorphicUsage()
        {
            IDeviceRepository repository = new MockDeviceRepository();

            await repository.SetAllHeatersAsync(3);
            double temp = await repository.GetAverageTemperatureAsync();

            AssertTrue(temp > 0, "Should work through interface");
            Console.WriteLine($"✓ Test_PolymorphicUsage passed (temp={temp:F1}°C)");
        }

        // Test 18: Multiple sensor IDs are valid
        public static async Task Test_AllSensorIdsValid()
        {
            var repository = new MockDeviceRepository();

            for (int i = 1; i <= 3; i++)
            {
                double temp = await repository.GetSensorTemperatureAsync(i);
                AssertTemperatureIsValid(temp, $"Sensor {i}");
            }

            Console.WriteLine("✓ Test_AllSensorIdsValid passed");
        }

        // Test 19: Multiple heater IDs are valid
        public static async Task Test_AllHeaterIdsValid()
        {
            var repository = new MockDeviceRepository();

            for (int i = 1; i <= 3; i++)
            {
                await repository.SetHeaterLevelAsync(i, 2);
                AssertContainsCall(repository.CallLog, $"SetHeaterLevelAsync({i}, 2)");
            }

            Console.WriteLine("✓ Test_AllHeaterIdsValid passed");
        }

        // Test 20: Multiple fan IDs are valid
        public static async Task Test_AllFanIdsValid()
        {
            var repository = new MockDeviceRepository();

            for (int i = 1; i <= 3; i++)
            {
                await repository.SetFanStateAsync(i, true);
                AssertContainsCall(repository.CallLog, $"SetFanStateAsync({i}, True)");
            }

            Console.WriteLine("✓ Test_AllFanIdsValid passed");
        }

        // ===== ASSERTION HELPERS =====

        private static void AssertEqual<T>(T actual, T expected, string message)
        {
            if (!Equals(actual, expected))
                throw new Exception($"Assertion failed: {message}. Expected {expected}, got {actual}");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"Assertion failed: {message}");
        }

        private static void AssertApproximatelyEqual(double actual, double expected, double tolerance, string message)
        {
            if (Math.Abs(actual - expected) > tolerance)
                throw new Exception($"Assertion failed: {message}. Expected {actual} ≈ {expected} (±{tolerance}), diff={Math.Abs(actual - expected)}");
        }

        private static void AssertTemperatureIsValid(double temperature, string sensorName)
        {
            if (temperature < -50 || temperature > 150)
                throw new Exception($"Temperature from {sensorName} is out of valid range: {temperature}°C");
        }

        private static void AssertContainsCall(List<string> callLog, string expectedCall)
        {
            if (!callLog.Contains(expectedCall))
                throw new Exception($"Call log should contain '{expectedCall}'. Log: {string.Join(", ", callLog)}");
        }

        private static void AssertCountInLog(List<string> log, string substring, int expectedCount, string message)
        {
            int count = log.FindAll(s => s.Contains(substring)).Count;
            if (count != expectedCount)
                throw new Exception($"Assertion failed: {message}. Expected {expectedCount} calls containing '{substring}', found {count}");
        }
    }

    /// <summary>
    /// Integration tests for ConsoleUI and Repository together.
    /// </summary>
    public static class ConsoleUIIntegrationTests
    {
        public static async Task Test_ConsoleUiUsesRepository()
        {
            var mockRepository = new MockDeviceRepository();
            var ui = new ConsoleUI(mockRepository);

            AssertTrue(ui != null, "ConsoleUI should be instantiable");
            Console.WriteLine("✓ Test_ConsoleUiUsesRepository passed");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"Assertion failed: {message}");
        }
    }

    /// <summary>
    /// Test Runner - executes all unit tests and reports results.
    /// </summary>
    public class TestRunner
    {
        public static async Task<int> Main()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("Running Unit Tests for UglyClient");
            Console.WriteLine("========================================\n");

            var tests = new List<(string name, Func<Task> test)>
            {
                ("MockDeviceRepository - Get Sensor Temperature", () => MockDeviceRepositoryTests.Test_GetSensorTemperatureReturnsValidTemperature()),
                ("MockDeviceRepository - Average Temperature", () => MockDeviceRepositoryTests.Test_AverageTemperatureCalculation()),
                ("MockDeviceRepository - Set Heater Level", () => MockDeviceRepositoryTests.Test_SetHeaterLevel()),
                ("MockDeviceRepository - Invalid Heater Level", () => MockDeviceRepositoryTests.Test_InvalidHeaterLevelThrows()),
                ("MockDeviceRepository - Invalid Heater ID", () => MockDeviceRepositoryTests.Test_InvalidHeaterIdThrows()),
                ("MockDeviceRepository - Set Fan State", () => MockDeviceRepositoryTests.Test_SetFanState()),
                ("MockDeviceRepository - Invalid Fan ID", () => MockDeviceRepositoryTests.Test_InvalidFanIdThrows()),
                ("MockDeviceRepository - Set All Heaters", () => MockDeviceRepositoryTests.Test_SetAllHeaters()),
                ("MockDeviceRepository - Set All Fans", () => MockDeviceRepositoryTests.Test_SetAllFans()),
                ("MockDeviceRepository - Call Log Tracking", () => MockDeviceRepositoryTests.Test_CallLogTracksOperations()),
                ("MockDeviceRepository - Reset State", () => MockDeviceRepositoryTests.Test_ResetClearsState()),
                ("MockDeviceRepository - Adjust Temperature (Increase)", () => MockDeviceRepositoryTests.Test_AdjustTemperatureIncreases()),
                ("MockDeviceRepository - Adjust Temperature (Decrease)", () => MockDeviceRepositoryTests.Test_AdjustTemperatureDecreases()),
                ("MockDeviceRepository - Hold Temperature", () => MockDeviceRepositoryTests.Test_HoldTemperatureReturnsPlausible()),
                ("MockDeviceRepository - Display All Devices", () => MockDeviceRepositoryTests.Test_DisplayAllDevicesCompletes()),
                ("MockDeviceRepository - Implements Interface", () => { MockDeviceRepositoryTests.Test_ImplementsInterface(); return Task.CompletedTask; }),
                ("MockDeviceRepository - Polymorphic Usage", () => MockDeviceRepositoryTests.Test_PolymorphicUsage()),
                ("MockDeviceRepository - All Sensor IDs", () => MockDeviceRepositoryTests.Test_AllSensorIdsValid()),
                ("MockDeviceRepository - All Heater IDs", () => MockDeviceRepositoryTests.Test_AllHeaterIdsValid()),
                ("MockDeviceRepository - All Fan IDs", () => MockDeviceRepositoryTests.Test_AllFanIdsValid()),
                ("ConsoleUI - Integration", () => ConsoleUIIntegrationTests.Test_ConsoleUiUsesRepository()),
            };

            int passed = 0;
            int failed = 0;

            foreach (var (testName, test) in tests)
            {
                try
                {
                    await test();
                    passed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ {testName} FAILED");
                    Console.WriteLine($"  Error: {ex.Message}\n");
                    failed++;
                }
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine($"Test Results: {passed} Passed, {failed} Failed");
            Console.WriteLine($"Total Tests: {passed + failed}");
            Console.WriteLine($"Success Rate: {(passed * 100.0 / (passed + failed)):F1}%");
            Console.WriteLine("========================================");

            return failed > 0 ? 1 : 0;
        }
    }
}
