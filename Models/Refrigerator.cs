namespace ApplianceRepairCenter.Models;

public class Refrigerator : Appliance
{
    private readonly bool _needsRefrigerant;
    public bool NeedsRefrigerant => _needsRefrigerant;

    public Refrigerator(string serial, string brand, string owner,
        DateTime received, DateTime expected, bool extraService)
        : base(serial, brand, owner, received, expected)
    {
        _needsRefrigerant = extraService;
    }

    // Educational prices in a common, unspecified currency unit.
    public override decimal CalculateRepairCost()
    {
        if (!HasRepairDetails) return 0m;
        return LaborCost + PartsCost + (NeedsRefrigerant ? 40m : 0m);
    }
}
