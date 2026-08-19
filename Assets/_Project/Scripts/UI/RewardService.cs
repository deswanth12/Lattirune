using System;
using UnityEngine;
using Lattirune.Items;

namespace Lattirune.UI
{
    /// <summary>
    /// Service managing the execution and application of selected rewards to the prototype build.
    /// Guarantees one-time selection protection and immutability of ItemDataSO assets.
    /// </summary>
    public class RewardService : MonoBehaviour
    {
        [SerializeField] private bool isSelectionLocked = false;

        public event Action<RewardOption, ItemInstance> OnRewardApplied;

        public bool IsSelectionLocked => isSelectionLocked;

        /// <summary>
        /// Applies the chosen reward by instantiating a runtime ItemInstance in the staging area.
        /// </summary>
        public ItemInstance ApplyReward(RewardOption option, Vector3 spawnPosition, Transform parent = null)
        {
            if (option == null || option.ItemData == null)
            {
                Debug.LogWarning("[Lattirune.UI] Cannot apply null reward option.");
                return null;
            }

            if (isSelectionLocked)
            {
                Debug.LogWarning("[Lattirune.UI] Reward selection is locked. Double-selection prevented.");
                return null;
            }

            isSelectionLocked = true;

            // Instantiate runtime ItemInstance using ItemFactory
            ItemInstance instance = ItemFactory.CreateInstance(option.ItemData, spawnPosition, parent);

            OnRewardApplied?.Invoke(option, instance);
            return instance;
        }

        public void ResetSelectionLock()
        {
            isSelectionLocked = false;
        }
    }
}
