using System;
using System.IO;
using System.Reflection;

class TestRunner
{
    static int Main()
    {
        try
        {
            Console.WriteLine("Running minimal test harness for Seed (dynamic load)...");

            // Attempt to locate built Version2 assembly under expected bin folder.
            var candidate = Path.GetFullPath(Path.Combine("..", "..", "..", "Desktop", "Software Engineering Component 2 Hand in", "Version2Sim-master", "Version2Sim-master", "bin", "Debug", "net8.0", "Version2.dll"));
            if (!File.Exists(candidate))
            {
                Console.WriteLine($"Could not find Version2.dll at {candidate}");
                return 2;
            }

            var asm = Assembly.LoadFrom(candidate);
            var seederType = asm.GetType("Version2.Seeding.EnvironmentStateSeeder");
            if (seederType == null)
            {
                Console.WriteLine("Could not find Version2.Seeding.EnvironmentStateSeeder in assembly.");
                return 3;
            }

            var method = seederType.GetMethod("SeedEnvironmentState", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                Console.WriteLine("SeedEnvironmentState method not found.");
                return 4;
            }

            var state = method.Invoke(null, null);
            if (state == null) throw new Exception("Seed returned null");

            // Use reflection to check collections counts
            var sensorsProp = state.GetType().GetProperty("Sensors");
            var heatersProp = state.GetType().GetProperty("Heaters");
            var fansProp = state.GetType().GetProperty("Fans");
            if (sensorsProp == null || heatersProp == null || fansProp == null) throw new Exception("Expected properties not found on state object.");

            var sensors = sensorsProp.GetValue(state) as System.Collections.ICollection;
            var heaters = heatersProp.GetValue(state) as System.Collections.ICollection;
            var fans = fansProp.GetValue(state) as System.Collections.ICollection;

            if (sensors == null || heaters == null || fans == null) throw new Exception("One of the device collections is null.");
            if (sensors.Count != 3) throw new Exception($"Expected 3 sensors, found {sensors.Count}");
            if (heaters.Count != 3) throw new Exception($"Expected 3 heaters, found {heaters.Count}");
            if (fans.Count != 3) throw new Exception($"Expected 3 fans, found {fans.Count}");

            Console.WriteLine("Seed checks passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failure: {ex.Message}");
            return 1;
        }
    }
}
