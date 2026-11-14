using ApplianceRepairCenter.Events;
using ApplianceRepairCenter.Models;
using ApplianceRepairCenter.Utilities;

namespace ApplianceRepairCenter.Services;

public class RepairCenter
{
    private readonly List<Appliance> _devices = new();
    public IReadOnlyList<Appliance> Devices => _devices.AsReadOnly();
    public event DeviceStatusChangedHandler? DeviceStatusChanged;

    public void AddDevice(Appliance device)
    {
        ArgumentNullException.ThrowIfNull(device);
        if (_devices.Any(d => d.SerialNumber == device.SerialNumber))
            throw new ArgumentException("A device with this serial number already exists.");
        _devices.Add(device);
        device.StatusChanged += ForwardStatusChange;
    }

    public Appliance Find(string serial)
    {
        string normalized = InputValidator.SerialNumber(serial);
        return _devices.FirstOrDefault(d => d.SerialNumber == normalized)
            ?? throw new ArgumentException("Device not found.");
    }

    public IEnumerable<Appliance> GetByStatus(RepairStatus status) =>
        _devices.Where(d => d.Status == status);

    public decimal CalculateTotalCost()
    {
        decimal total = 0m;
        // كل جهاز يحسب تكلفته حسب نوعه.
        foreach (Appliance device in _devices)
            total += device.CalculateRepairCost();
        return total;
    }

    private void ForwardStatusChange(Appliance device, RepairStatus oldStatus, RepairStatus newStatus)
        => DeviceStatusChanged?.Invoke(device, oldStatus, newStatus);
}
