namespace ApplianceRepairCenter.Models;

public class WashingMachine : Appliance
{
    private readonly bool _needsDrumService;
    public bool NeedsDrumService => _needsDrumService;

    public WashingMachine(string serial, string brand, string owner,
        DateTime received, DateTime expected, bool extraService)
        : base(serial, brand, owner, received, expected)
    {
        _needsDrumService = extraService;
    }

    public override decimal CalculateRepairCost()
    {
        if (!HasRepairDetails) return 0m;
        return LaborCost + PartsCost + (NeedsDrumService ? 25m : 0m);
    }
}
