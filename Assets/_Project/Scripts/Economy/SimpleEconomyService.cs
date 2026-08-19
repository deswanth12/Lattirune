using UnityEngine;

namespace Lattirune.Economy
{
    /// <summary>
    /// Lightweight, standalone implementation of IEconomyService for tests and standalone systems.
    /// Strictly adheres to PLAN.md Section 13.1.
    /// </summary>
    public class SimpleEconomyService : MonoBehaviour, IEconomyService
    {
        [SerializeField] private int currentGold = 0;

        public int CurrentGold => currentGold;
        public int GoldBalance => currentGold;

        public void Initialize(int startingGold = 0)
        {
            currentGold = Mathf.Max(0, startingGold);
        }

        public void AddGold(int amount)
        {
            if (amount > 0) currentGold += amount;
        }

        public bool SpendGold(int amount)
        {
            if (amount > 0 && currentGold >= amount)
            {
                currentGold -= amount;
                return true;
            }
            return false;
        }

        public bool CanAfford(int amount)
        {
            return amount >= 0 && currentGold >= amount;
        }
    }
}
