using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Items;

namespace Lattirune.UI
{
    /// <summary>
    /// Generates distinct, non-duplicate reward options from the available prototype ItemDataSO pool.
    /// Supports deterministic seed generation for testing and reproducibility.
    /// </summary>
    public static class RewardGenerator
    {
        public const int DEFAULT_REWARD_COUNT = 3;

        /// <summary>
        /// Generates exactly 'count' non-duplicate reward options from availableItems.
        /// </summary>
        public static List<RewardOption> GenerateRewardOptions(
            IReadOnlyList<ItemDataSO> availableItems, 
            int count = DEFAULT_REWARD_COUNT, 
            int? seed = null)
        {
            List<RewardOption> results = new List<RewardOption>();
            if (availableItems == null || availableItems.Count == 0)
            {
                return results;
            }

            // Create a valid candidate pool
            List<ItemDataSO> validCandidates = new List<ItemDataSO>();
            for (int i = 0; i < availableItems.Count; i++)
            {
                if (availableItems[i] != null && !string.IsNullOrEmpty(availableItems[i].ItemId))
                {
                    validCandidates.Add(availableItems[i]);
                }
            }

            if (validCandidates.Count == 0)
            {
                return results;
            }

            System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

            // Shuffle valid candidates using Fisher-Yates
            List<ItemDataSO> shuffled = new List<ItemDataSO>(validCandidates);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                ItemDataSO temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }

            int targetCount = Mathf.Min(count, shuffled.Count);
            HashSet<string> selectedIds = new HashSet<string>();

            for (int i = 0; i < shuffled.Count && results.Count < targetCount; i++)
            {
                ItemDataSO item = shuffled[i];
                if (!selectedIds.Contains(item.ItemId))
                {
                    selectedIds.Add(item.ItemId);
                    results.Add(RewardOption.FromItemData(item));
                }
            }

            return results;
        }
    }
}
