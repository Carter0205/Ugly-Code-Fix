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

            // Try to locate the built Version2 assembly in a few likely locations.
            string? candidate = null;

            // 1) If environment variable VERSION2_DLL is set, prefer it
            var env = Environment.GetEnvironmentVariable("VERSION2_DLL");
            if (!string.IsNullOrEmpty(env) && File.Exists(env)) candidate = env;

            // 2) Look for common relative output paths (bin/Debug/net8.0)
            if (candidate == null)
            {
                var checks = new[] {
                    Path.Combine("..", "Version2Sim-master", "Version2Sim-master", "bin", "Debug", "net8.0", "Version2.dll"),
                    Path.Combine("..", "..", "Version2Sim-master", "Version2Sim-master", "bin", "Debug", "net8.0", "Version2.dll"),
                    Path.Combine("..", "..", "..", "Version2Sim-master", "Version2Sim-master", "bin", "Debug", "net8.0", "Version2.dll"),
                    Path.Combine("..", "..", "..", "..", "Version2Sim-master", "Version2Sim-master", "bin", "Debug", "net8.0", "Version2.dll")
                };
                foreach (var c in checks)
                {
                    var p = Path.GetFullPath(c);
                    if (File.Exists(p))
                    {
                        candidate = p;
                        break;
                    }
                }
            }

            // 3) As a last resort, search within the repository tree for Version2.dll (limited depth)
            if (candidate == null)
            {
                try
                {
                    var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
                    var files = Directory.GetFiles(root, "Version2.dll", SearchOption.AllDirectories);
                    if (files.Length > 0) candidate = files[0];
                }
                catch { }
            }

            if (candidate == null || !File.Exists(candidate))
            {
                Console.WriteLine("Could not find Version2.dll. Please build the Version2 project first and ensure Version2.dll exists in its bin folder.");
                Console.WriteLine("You may also set the VERSION2_DLL environment variable to the full path of Version2.dll.");
                return 2;
            }

            Console.WriteLine($"Loading Version2 assembly from: {candidate}");
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
