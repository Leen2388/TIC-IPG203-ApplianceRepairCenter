using ApplianceRepairCenter.Models;
using ApplianceRepairCenter.Services;

namespace ApplianceRepairCenter;

internal static class SelfTests
{
    public static void Run()
    {
        int passed = 0;
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception($"FAIL: {name}");
            Console.WriteLine($"PASS: {name}");
            passed++;
        }
        void Reject<T>(Action action, string name) where T : Exception
        {
            try { action(); }
            catch (T) { Check(true, name); return; }
            throw new Exception($"FAIL: {name} did not reject input");
        }
        DateTime today = DateTime.Today;
        Refrigerator Fridge(string serial = "RF-001", bool extra = true) =>
            new(serial, "Beko", "Ahmad", today, today.AddDays(2), extra);
        var center = new RepairCenter();
        var fridge = Fridge();
        center.AddDevice(fridge);
        Check(fridge.Status == RepairStatus.Received, "initial state");
        Check(fridge.CalculateRepairCost() == 0m, "no cost before diagnosis");
        Check(center.Find("rf-001") == fridge, "normalized lookup");
        Reject<ArgumentException>(() => center.AddDevice(Fridge()), "duplicate serial");
        Reject<ArgumentException>(() => Fridge("?"), "invalid serial");
        Reject<ArgumentException>(() => new Refrigerator("RF-002", "X", "Y", today.AddDays(1), today.AddDays(2), false), "future received date");
        Reject<ArgumentException>(() => new Refrigerator("RF-002", "X", "Y", today, today.AddDays(-1), false), "invalid date order");
        Reject<InvalidOperationException>(() => fridge.Deliver(), "delivery before repair");
        Reject<InvalidOperationException>(() => fridge.SetRepairDetails("work", 1m, 1m), "work before start");
        int events = 0;
        int completionEvents = 0;
        center.DeviceStatusChanged += (d, oldStatus, newStatus) =>
        {
            events++;
            if (newStatus == RepairStatus.Repaired) completionEvents++;
        };
        fridge.StartRepair();
        Check(events == 1, "center forwards status event");
        Reject<InvalidOperationException>(() => fridge.CompleteRepair(), "completion without work");
        fridge.SetRepairDetails("cooling work", 40m, 60m);
        Check(fridge.CalculateRepairCost() == 140m, "refrigerator formula");
        Reject<ArgumentOutOfRangeException>(() => fridge.SetRepairDetails("bad", 5m, -1m), "negative cost");
        Check(fridge.RepairDescription == "cooling work" && fridge.LaborCost == 40m, "failed update is atomic");
        Reject<ArgumentException>(() => fridge.OwnerName = " ", "empty owner");
        fridge.OwnerName = "New owner";
        Check(fridge.OwnerName == "New owner", "validated property update");
        int initialCompleted = Appliance.RepairedCount;
        fridge.CompleteRepair();
        Check(completionEvents == 1 && events == 2, "completion event once");
        Check(Appliance.RepairedCount == initialCompleted + 1, "static completed count");
        Reject<InvalidOperationException>(() => fridge.CompleteRepair(), "repeated completion");
        Reject<InvalidOperationException>(() => fridge.SetRepairDetails("changed", 0m, 0m), "cost frozen after completion");
        fridge.Deliver();
        Check(fridge.Status == RepairStatus.Delivered && events == 3, "delivery transition");
        Check(Appliance.RepairedCount == initialCompleted + 1, "delivery preserves cumulative count");
        Reject<InvalidOperationException>(() => fridge.StartRepair(), "cannot restart delivered device");
        Appliance washer = new WashingMachine("WM-001", "LG", "Sara", today, today, true);
        Appliance ac = new AirConditioner("AC-001", "Gree", "Omar", today, today, true);
        foreach (Appliance d in new[] { washer, ac }) { center.AddDevice(d); d.StartRepair(); }
        washer.SetRepairDetails("drum", 30m, 50m);
        ac.SetRepairDetails("cleaning", 25m, 40m);
        Check(washer.CalculateRepairCost() == 105m, "washer override");
        Check(ac.CalculateRepairCost() == 95m, "air conditioner override");
        Check(center.CalculateTotalCost() == 340m, "polymorphic total");
        Check(center.GetByStatus(RepairStatus.UnderRepair).Count() == 2, "status filtering");
        var basic = Fridge("RF-003", false);
        basic.StartRepair(); basic.SetRepairDetails("basic", 10m, 20m);
        Check(basic.CalculateRepairCost() == 30m, "no optional surcharge");
        Reject<NotSupportedException>(() => ((IList<Appliance>)center.Devices).Add(basic), "read-only device list");
        Console.WriteLine($"RESULT: {passed} checks passed.");
    }
}
