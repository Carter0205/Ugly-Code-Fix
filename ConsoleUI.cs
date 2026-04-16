using System;
using System.Threading.Tasks;

/// <summary>
/// Console UI separated from Program logic for easier testing and clearer structure.
/// </summary>
public class ConsoleUI
{
    private readonly ApiClient _api;

    public ConsoleUI(ApiClient api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine("Simulation Control:");
            Console.WriteLine("1. Control Fan");
            Console.WriteLine("2. Control Heater");
            Console.WriteLine("3. Read Temperature");
            Console.WriteLine("4. Display State of All Devices");
            Console.WriteLine("5. Control Simulation");
            Console.WriteLine("6. Reset Simulation");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    await HandleControlFan();
                    break;
                case "2":
                    await HandleControlHeater();
                    break;
                case "3":
                    await HandleReadTemperature();
                    break;
                case "4":
                    await _api.DisplayAllDevices();
                    break;
                case "5":
                    await HandleControlSimulation();
                    break;
                case "6":
                    try
                    {
                        await _api.Reset();
                        Console.WriteLine("Client state has been successfully reset.");
                        await _api.DisplayAllDevices();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while resetting client state: {ex.Message}");
                    }
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

            Console.WriteLine();
        }
    }

    private async Task HandleControlFan()
    {
        Console.Write("Enter Fan Number: ");
        if (int.TryParse(Console.ReadLine(), out int fanId))
        {
            Console.Write("Turn Fan On or Off? (on/off): ");
            var stateInput = Console.ReadLine();
            bool isOn = stateInput?.ToLower() == "on";

            try
            {
                await _api.SetFanState(fanId, isOn);
                Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Invalid Fan Number.");
        }
    }

    private async Task HandleControlHeater()
    {
        Console.Write("Enter Heater Number: ");
        if (int.TryParse(Console.ReadLine(), out int heaterId))
        {
            Console.Write("Set Heater Level (0-5): ");
            if (int.TryParse(Console.ReadLine(), out int level) && level >= 0 && level <= 5)
            {
                try
                {
                    await _api.SetHeaterLevel(heaterId, level);
                    Console.WriteLine($"Heater {heaterId} level set to {level}.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid Heater Level. Please enter a value between 0 and 5.");
            }
        }
        else
        {
            Console.WriteLine("Invalid Heater Number.");
        }
    }

    private async Task HandleReadTemperature()
    {
        Console.Write("Enter Sensor Number: ");
        if (int.TryParse(Console.ReadLine(), out int sensorId))
        {
            try
            {
                double temperature = await _api.GetSensorTemperature(sensorId);
                Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Invalid Sensor Number.");
        }
    }

    private async Task HandleControlSimulation()
    {
        Console.WriteLine("Starting temperature control algorithm...");
        Console.Write("Provide a final Temp Value: ");
        double currentTemperature = await _api.GetAverageTemperature();
        while (true)
        {
            currentTemperature = await _api.AdjustTemperature(currentTemperature, 20.0, 30);
            currentTemperature = await _api.AdjustTemperature(currentTemperature, 16.0, 10);
            currentTemperature = await _api.HoldTemperature(currentTemperature, 16.0, 10);
            currentTemperature = await _api.AdjustTemperature(currentTemperature, 18.0, 20);
            currentTemperature = await _api.HoldTemperature(currentTemperature, 18.0, int.MaxValue);
        }
    }
}
