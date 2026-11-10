using ApplianceRepairCenter.Contracts;
using ApplianceRepairCenter.Events;
using ApplianceRepairCenter.Utilities;

namespace ApplianceRepairCenter.Models;

public abstract class Appliance : IRepairable
{
    private readonly string _serialNumber;
    private readonly string _brand;
    private readonly DateTime _receivedDate;
    private readonly DateTime _expectedDeliveryDate;
    private string _ownerName;
    private string _repairDescription = "Not diagnosed";
    private decimal _laborCost;
    private decimal _partsCost;
    private bool _hasRepairDetails;
    private RepairStatus _status = RepairStatus.Received;
    private static int _repairedCount;

    public string SerialNumber => _serialNumber;
    public string Brand => _brand;
    public DateTime ReceivedDate => _receivedDate;
    public DateTime ExpectedDeliveryDate => _expectedDeliveryDate;
    public RepairStatus Status => _status;
    public string RepairDescription => _repairDescription;
    public decimal LaborCost => _laborCost;
    public decimal PartsCost => _partsCost;
    public bool HasRepairDetails => _hasRepairDetails;
    // Cumulative completed repairs during this process, including delivered devices.
    public static int RepairedCount => _repairedCount;
    public string OwnerName
    {
        get => _ownerName;
        set => _ownerName = InputValidator.RequiredText(value, "Owner name");
    }
    public event DeviceStatusChangedHandler? StatusChanged;

    protected Appliance(string serial, string brand, string owner,
        DateTime received, DateTime expected)
    {
        InputValidator.Dates(received, expected);
        _serialNumber = InputValidator.SerialNumber(serial);
        _brand = InputValidator.RequiredText(brand, "Brand");
        _ownerName = InputValidator.RequiredText(owner, "Owner name");
        _receivedDate = received.Date;
        _expectedDeliveryDate = expected.Date;
    }

    public void StartRepair()
    {
        RequireStatus(RepairStatus.Received);
        ChangeStatus(RepairStatus.UnderRepair);
    }

    public void SetRepairDetails(string description, decimal laborCost, decimal partsCost)
    {
        RequireStatus(RepairStatus.UnderRepair);
        // Validate everything before changing any stored value.
        string validDescription = InputValidator.RequiredText(description, "Repair description");
        decimal validLabor = InputValidator.NonNegative(laborCost, "Labor cost");
        decimal validParts = InputValidator.NonNegative(partsCost, "Parts cost");
        _repairDescription = validDescription;
        _laborCost = validLabor;
        _partsCost = validParts;
        _hasRepairDetails = true;
    }

    public abstract decimal CalculateRepairCost();

    public void CompleteRepair()
    {
        RequireStatus(RepairStatus.UnderRepair);
        if (!_hasRepairDetails)
            throw new InvalidOperationException("Enter repair details before completing repair.");
        _repairedCount++;
        ChangeStatus(RepairStatus.Repaired);
    }

    public void Deliver()
    {
        RequireStatus(RepairStatus.Repaired);
        ChangeStatus(RepairStatus.Delivered);
    }

    private void RequireStatus(RepairStatus required)
    {
        if (_status != required)
            throw new InvalidOperationException($"Operation requires {required}; current status is {_status}.");
    }

    private void ChangeStatus(RepairStatus next)
    {
        RepairStatus old = _status;
        _status = next;
        StatusChanged?.Invoke(this, old, next);
    }
}
