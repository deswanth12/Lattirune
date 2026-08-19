namespace Lattirune.Economy
{
    /// <summary>
    /// Contract for in-run Gold operations (balance, debit, credit, affordability).
    /// Strictly adheres to PLAN.md Section 13.1.
    /// </summary>
    public interface IEconomyService
    {
        int CurrentGold { get; }
        int GoldBalance => CurrentGold;
        void AddGold(int amount);
        bool SpendGold(int amount);
        bool CanAfford(int amount);
    }
}
