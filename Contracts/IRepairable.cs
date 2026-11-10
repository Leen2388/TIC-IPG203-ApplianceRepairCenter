namespace ApplianceRepairCenter.Contracts;

public interface IRepairable
{
    void StartRepair();
    void SetRepairDetails(string description, decimal laborCost, decimal partsCost);
    decimal CalculateRepairCost();
    void CompleteRepair();
    void Deliver();
}
