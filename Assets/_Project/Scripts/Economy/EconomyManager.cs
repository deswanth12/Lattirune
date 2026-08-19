using System;
using UnityEngine;

namespace Lattirune.Economy
{
    /// <summary>
    /// Static and data-driven economy manager implementing the exact economy balance sheet from PLAN.md Section 13.1.
    /// Handles in-run Gold drops, Boss Embers, item store pricing, and bag slot expansion costs.
    /// </summary>
    public static class EconomyManager
    {
        // Pricing Constants (PLAN.md Section 13.1)
        public const int COMMON_ITEM_PRICE = 20;
        public const int RARE_ITEM_PRICE = 40;
        public const int RUNE_PRICE = 35;
        public const int BAG_EXPANSION_PRICE = 40;

        // Gold Drop Boundaries (PLAN.md Section 13.1)
        public const int NORMAL_MOB_GOLD_MIN = 6;
        public const int NORMAL_MOB_GOLD_MAX = 12;

        public const int ELITE_MOB_GOLD_MIN = 20;
        public const int ELITE_MOB_GOLD_MAX = 35;

        public const int BOSS_EMBERS_MIN = 80;
        public const int BOSS_EMBERS_MAX = 120;

        /// <summary>
        /// Generates a randomized in-run Gold drop for defeating an enemy mob.
        /// </summary>
        public static int GetGoldDrop(bool isElite)
        {
            if (isElite)
            {
                return UnityEngine.Random.Range(ELITE_MOB_GOLD_MIN, ELITE_MOB_GOLD_MAX + 1);
            }
            return UnityEngine.Random.Range(NORMAL_MOB_GOLD_MIN, NORMAL_MOB_GOLD_MAX + 1);
        }

        public static int GenerateNormalMobGoldDrop(int? seed = null)
        {
            if (seed.HasValue)
            {
                var rand = new System.Random(seed.Value);
                return rand.Next(NORMAL_MOB_GOLD_MIN, NORMAL_MOB_GOLD_MAX + 1);
            }
            return GetGoldDrop(false);
        }

        public static int GenerateEliteMobGoldDrop(int? seed = null)
        {
            if (seed.HasValue)
            {
                var rand = new System.Random(seed.Value);
                return rand.Next(ELITE_MOB_GOLD_MIN, ELITE_MOB_GOLD_MAX + 1);
            }
            return GetGoldDrop(true);
        }

        /// <summary>
        /// Generates a randomized persistent Embers drop for clearing a boss encounter.
        /// </summary>
        public static int GetBossEmbersDrop()
        {
            return UnityEngine.Random.Range(BOSS_EMBERS_MIN, BOSS_EMBERS_MAX + 1);
        }

        public static int GenerateBossEmbersDrop(int? seed = null)
        {
            if (seed.HasValue)
            {
                var rand = new System.Random(seed.Value);
                return rand.Next(BOSS_EMBERS_MIN, BOSS_EMBERS_MAX + 1);
            }
            return GetBossEmbersDrop();
        }

        public static int GetCommonItemPrice() => COMMON_ITEM_PRICE;
        public static int GetRareItemPrice() => RARE_ITEM_PRICE;
        public static int GetRunePrice() => RUNE_PRICE;
        public static int GetBagExpansionPrice() => BAG_EXPANSION_PRICE;
    }
}
