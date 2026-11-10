using ApplianceRepairCenter.Models;
using ApplianceRepairCenter.Services;

namespace ApplianceRepairCenter;

public static class Demo
{
    public static void Run()
    {
        var center = new RepairCenter();
        center.DeviceStatusChanged += (device, oldStatus, newStatus) =>
        {
            Console.WriteLine($"EVENT {device.SerialNumber}: {oldStatus} -> {newStatus}");
            if (newStatus == RepairStatus.Repaired)
                Console.WriteLine($"NOTICE: {device.SerialNumber} is ready for delivery.");
        };
        DateTime today = DateTime.Today;
        Appliance fridge = new Refrigerator("RF-101", "Beko", "Ahmad", today, today.AddDays(2), true);
        Appliance washer = new WashingMachine("WM-102", "LG", "Sara", today, today.AddDays(3), true);
        Appliance ac = new AirConditioner("AC-103", "Gree", "Omar", today, today.AddDays(1), true);
        foreach (Appliance device in new[] { fridge, washer, ac }) center.AddDevice(device);
        fridge.StartRepair();
        fridge.SetRepairDetails("Cooling inspection and refrigerant service", 40m, 60m);
        fridge.CompleteRepair();
        washer.StartRepair();
        washer.SetRepairDetails("Drum and bearing service", 30m, 50m);
        ac.StartRepair();
        ac.SetRepairDetails("Filter service and deep cleaning", 25m, 40m);
        ac.CompleteRepair();
        ac.Deliver();
        Console.WriteLine("\nSERIAL | TYPE | STATUS | COST");
        foreach (Appliance device in center.Devices)
            Console.WriteLine($"{device.SerialNumber} | {device.GetType().Name} | {device.Status} | {device.CalculateRepairCost():F2}");
        Console.WriteLine($"Total recorded cost: {center.CalculateTotalCost():F2}");
        Console.WriteLine($"Under repair: {center.GetByStatus(RepairStatus.UnderRepair).Count()}");
        Console.WriteLine($"Ready for delivery: {center.GetByStatus(RepairStatus.Repaired).Count()}");
        Console.WriteLine($"Completed repairs this process: {Appliance.RepairedCount}");
    }
}
