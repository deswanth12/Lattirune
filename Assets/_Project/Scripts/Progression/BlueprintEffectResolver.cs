using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Progression
{
    /// <summary>
    /// Static resolver and aggregator for data-driven Blueprint effects.
    /// Calculates aggregate starting bonuses and reward pool eligibility from unlocked blueprints.
    /// </summary>
    public static class BlueprintEffectResolver
    {
        public static int ComputeStartingGoldBonus(IEnumerable<BlueprintDefinitionSO> blueprints)
        {
            int total = 0;
            if (blueprints != null)
            {
                foreach (var bp in blueprints)
                {
                    if (bp != null && bp.EffectType == BlueprintEffectType.PermanentStartingGoldBonus)
                    {
                        total += bp.EffectValue;
                    }
                }
            }
            return total;
        }

        public static int ComputeStartingHpBonus(IEnumerable<BlueprintDefinitionSO> blueprints)
        {
            int total = 0;
            if (blueprints != null)
            {
                foreach (var bp in blueprints)
                {
                    if (bp != null && bp.EffectType == BlueprintEffectType.PermanentStartingHpBonus)
                    {
                        total += bp.EffectValue;
                    }
                }
            }
            return total;
        }

        public static HashSet<string> ComputeUnlockedItemIds(IEnumerable<BlueprintDefinitionSO> blueprints)
        {
            HashSet<string> itemIds = new HashSet<string>();
            if (blueprints != null)
            {
                foreach (var bp in blueprints)
                {
                    if (bp != null && bp.EffectType == BlueprintEffectType.UnlockItemInRewardPool && !string.IsNullOrEmpty(bp.TargetUnlockId))
                    {
                        itemIds.Add(bp.TargetUnlockId);
                    }
                }
            }
            return itemIds;
        }

        public static HashSet<string> ComputeUnlockedRuneIds(IEnumerable<BlueprintDefinitionSO> blueprints)
        {
            HashSet<string> runeIds = new HashSet<string>();
            if (blueprints != null)
            {
                foreach (var bp in blueprints)
                {
                    if (bp != null && bp.EffectType == BlueprintEffectType.UnlockRuneInRewardPool && !string.IsNullOrEmpty(bp.TargetUnlockId))
                    {
                        runeIds.Add(bp.TargetUnlockId);
                    }
                }
            }
            return runeIds;
        }
    }
}
