namespace ApplianceRepairCenter.Models;

public class AirConditioner : Appliance
{
    private readonly bool _needsDeepCleaning;
    public bool NeedsDeepCleaning => _needsDeepCleaning;

    public AirConditioner(string serial, string brand, string owner,
        DateTime received, DateTime expected, bool extraService)
        : base(serial, brand, owner, received, expected)
    {
        _needsDeepCleaning = extraService;
    }

    public override decimal CalculateRepairCost()
    {
        if (!HasRepairDetails) return 0m;
        return LaborCost + PartsCost + (NeedsDeepCleaning ? 30m : 0m);
    }
}
