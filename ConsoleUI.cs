using System;
using System.Threading.Tasks;

public class ConsoleUI
{
    private const int MinSensorId = 1;
    private const int MaxSensorId = 3;
    private const int MinFanId = 1;
    private const int MaxFanId = 3;
    private const int MinHeaterId = 1;
    private const int MaxHeaterId = 3;
    private const int MinHeaterLevel = 0;
    private const int MaxHeaterLevel = 5;

    private readonly IDeviceRepository _repository;

    public ConsoleUI(IDeviceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task RunAsync()
    {
        while (true)
        {
            DisplayMenu();
            string input = Console.ReadLine() ?? "";
            await HandleMenuSelection(input);
            Console.WriteLine();
        }
    }

    private void DisplayMenu()
    {
        Console.WriteLine("Simulation Control:");
        Console.WriteLine("1. Control Fan");
        Console.WriteLine("2. Control Heater");
        Console.WriteLine("3. Read Temperature");
        Console.WriteLine("4. Display State of All Devices");
        Console.WriteLine("5. Control Simulation");
        Console.WriteLine("6. Reset Simulation");
        Console.Write("Select an option: ");
    }

    private async Task HandleMenuSelection(string input)
    {
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
                await _repository.DisplayAllDevicesAsync();
                break;
            case "5":
                await HandleControlSimulation();
                break;
            case "6":
                await HandleResetSimulation();
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }

    private async Task HandleResetSimulation()
    {
        try
        {
            await _repository.ResetAsync();
            Console.WriteLine("Client state has been successfully reset.");
            await _repository.DisplayAllDevicesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while resetting client state: {ex.Message}");
        }
    }

    private async Task HandleControlFan()
    {
        int fanId = PromptForInteger("Enter Fan Number: ");
        if (fanId < MinFanId || fanId > MaxFanId)
        {
            Console.WriteLine($"Invalid Fan Number. Please enter a value between {MinFanId} and {MaxFanId}.");
            return;
        }

        string stateInput = PromptForString("Turn Fan On or Off? (on/off): ");
        bool isOn = stateInput.Equals("on", StringComparison.OrdinalIgnoreCase);

        try
        {
            await _repository.SetFanStateAsync(fanId, isOn);
            Console.WriteLine($"Fan {fanId} has been turned {(isOn ? "On" : "Off")}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task HandleControlHeater()
    {
        int heaterId = PromptForInteger("Enter Heater Number: ");
        if (heaterId < MinHeaterId || heaterId > MaxHeaterId)
        {
            Console.WriteLine($"Invalid Heater Number. Please enter a value between {MinHeaterId} and {MaxHeaterId}.");
            return;
        }

        int level = PromptForInteger("Set Heater Level (0-5): ");
        if (level < MinHeaterLevel || level > MaxHeaterLevel)
        {
            Console.WriteLine($"Invalid Heater Level. Please enter a value between {MinHeaterLevel} and {MaxHeaterLevel}.");
            return;
        }

        try
        {
            await _repository.SetHeaterLevelAsync(heaterId, level);
            Console.WriteLine($"Heater {heaterId} level set to {level}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task HandleReadTemperature()
    {
        int sensorId = PromptForInteger("Enter Sensor Number: ");
        if (sensorId < MinSensorId || sensorId > MaxSensorId)
        {
            Console.WriteLine($"Invalid Sensor Number. Please enter a value between {MinSensorId} and {MaxSensorId}.");
            return;
        }

        try
        {
            double temperature = await _repository.GetSensorTemperatureAsync(sensorId);
            Console.WriteLine($"Sensor {sensorId} Temperature: {temperature:F1}°C");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task HandleControlSimulation()
    {
        Console.WriteLine("Starting temperature control algorithm...");
        double currentTemperature = await _repository.GetAverageTemperatureAsync();

        while (true)
        {
            currentTemperature = await _repository.AdjustTemperatureAsync(currentTemperature, 20.0, 30);
            currentTemperature = await _repository.AdjustTemperatureAsync(currentTemperature, 16.0, 10);
            currentTemperature = await _repository.HoldTemperatureAsync(currentTemperature, 16.0, 10);
            currentTemperature = await _repository.AdjustTemperatureAsync(currentTemperature, 18.0, 20);
            currentTemperature = await _repository.HoldTemperatureAsync(currentTemperature, 18.0, int.MaxValue);
        }
    }

    private int PromptForInteger(string prompt)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int result))
        {
            return result;
        }
        return -1;
    }

    private string PromptForString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }
}
