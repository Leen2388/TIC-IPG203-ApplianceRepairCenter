using ApplianceRepairCenter.Models;

namespace ApplianceRepairCenter.Events;

public delegate void DeviceStatusChangedHandler(
    Appliance device, RepairStatus oldStatus, RepairStatus newStatus);
