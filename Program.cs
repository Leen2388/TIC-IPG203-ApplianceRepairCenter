using System.Globalization;
using System.Text;
using ApplianceRepairCenter.Models;
using ApplianceRepairCenter.Services;

namespace ApplianceRepairCenter;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (args.Contains("--self-test")) { SelfTests.Run(); return; }
        if (args.Contains("--demo")) { Demo.Run(); return; }
        var center = new RepairCenter();
        center.DeviceStatusChanged += (device, oldStatus, newStatus) =>
        {
            Console.WriteLine($"[Status] {device.SerialNumber}: {oldStatus} -> {newStatus}");
            if (newStatus == RepairStatus.Repaired)
                Console.WriteLine($"[Notification] Repair complete for {device.OwnerName}. Ready for delivery.");
        };
        Console.WriteLine("Home Appliance Repair Center - data is kept in memory only.");
        while (true)
        {
            Console.WriteLine("\n1 Add device  2 Start repair  3 Record work/cost  4 Complete repair");
            Console.WriteLine("5 Deliver  6 List all  7 Filter by status  8 Totals  0 Exit");
            Console.Write("Choice: ");
            string? choice = Console.ReadLine();
            if (choice is null or "0") return;
            try
            {
                switch (choice)
                {
                    case "1": AddDevice(center); break;
                    case "2": SelectDevice(center).StartRepair(); break;
                    case "3":
                        Appliance device = SelectDevice(center);
                        string description = Read("Work description");
                        decimal labor = ReadCost("Labor cost");
                        decimal parts = ReadCost("Parts cost");
                        device.SetRepairDetails(description, labor, parts);
                        break;
                    case "4": SelectDevice(center).CompleteRepair(); break;
                    case "5": SelectDevice(center).Deliver(); break;
                    case "6": PrintDevices(center.Devices); break;
                    case "7":
                        Console.WriteLine("0 Received, 1 UnderRepair, 2 Repaired, 3 Delivered");
                        string input = Read("Status number");
                        if (!int.TryParse(input, out int status) || status < 0 || status > 3)
                            throw new ArgumentException("Choose a status from 0 to 3.");
                        PrintDevices(center.GetByStatus((RepairStatus)status));
                        break;
                    case "8":
                        Console.WriteLine($"Registered devices: {center.Devices.Count}");
                        Console.WriteLine($"Total recorded cost: {center.CalculateTotalCost():F2}");
                        Console.WriteLine($"Completed repairs this process: {Appliance.RepairedCount}");
                        break;
                    default: Console.WriteLine("Choose a number from the menu."); break;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FormatException)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void AddDevice(RepairCenter center)
    {
        Console.WriteLine("1 Refrigerator, 2 Washing machine, 3 Air conditioner");
        string type = Read("Type");
        if (type is not ("1" or "2" or "3")) throw new ArgumentException("Unknown device type.");
        string serial = Read("Serial number");
        string brand = Read("Brand");
        string owner = Read("Owner name");
        DateTime received = ReadDate("Received date (yyyy-MM-dd)");
        DateTime expected = ReadDate("Expected delivery date (yyyy-MM-dd)");
        string extraPrompt = type switch
        {
            "1" => "Refrigerant service (+40)? y/n",
            "2" => "Drum service (+25)? y/n",
            _ => "Deep cleaning (+30)? y/n"
        };
        string answer = Read(extraPrompt).ToLowerInvariant();
        if (answer is not ("y" or "n")) throw new ArgumentException("Enter y or n.");
        bool extra = answer == "y";
        Appliance device = type switch
        {
            "1" => new Refrigerator(serial, brand, owner, received, expected, extra),
            "2" => new WashingMachine(serial, brand, owner, received, expected, extra),
            _ => new AirConditioner(serial, brand, owner, received, expected, extra)
        };
        center.AddDevice(device);
        Console.WriteLine("Device added.");
    }

    private static Appliance SelectDevice(RepairCenter center) => center.Find(Read("Serial number"));
    private static string Read(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine() ?? throw new InvalidOperationException("Input ended.");
    }
    private static decimal ReadCost(string label)
    {
        if (!decimal.TryParse(Read(label), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal cost))
            throw new FormatException("Enter a positive number using a dot, such as 25.50.");
        return cost;
    }
    private static DateTime ReadDate(string label)
    {
        if (!DateTime.TryParseExact(Read(label), "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime date))
            throw new FormatException("Use date format yyyy-MM-dd.");
        return date;
    }
    private static void PrintDevices(IEnumerable<Appliance> devices)
    {
        bool found = false;
        foreach (Appliance device in devices)
        {
            found = true;
            Console.WriteLine($"{device.SerialNumber} | {device.GetType().Name} | {device.OwnerName} | {device.Brand}");
            Console.WriteLine($"  {device.Status} | Due: {device.ExpectedDeliveryDate:yyyy-MM-dd} | Cost: {device.CalculateRepairCost():F2}");
            Console.WriteLine($"  Work: {device.RepairDescription}");
        }
        if (!found) Console.WriteLine("No devices found.");
    }
}
