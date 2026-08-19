using System;
using UnityEngine;

namespace Lattirune.Combo
{
    /// <summary>
    /// Calculated reward multipliers and bonuses resulting from high combo depths and reaction chains.
    /// </summary>
    public struct ChainReactionRewardOutcome
    {
        public int BonusGold;
        public int BonusEmbers;
        public float QualityUpgradeChance;
        public string TierName;

        public ChainReactionRewardOutcome(int gold, int embers, float quality, string tier)
        {
            BonusGold = gold;
            BonusEmbers = embers;
            QualityUpgradeChance = quality;
            TierName = tier;
        }
    }

    /// <summary>
    /// Pure mathematical evaluator mapping combo depth and elemental reaction chains to temporary in-run rewards.
    /// Does not directly mutate persistent meta state.
    /// </summary>
    public static class ChainReactionRewardCalculator
    {
        public static ChainReactionRewardOutcome CalculateReward(int comboDepth, int reactionChainDepth)
        {
            if (comboDepth < 0) comboDepth = 0;
            if (reactionChainDepth < 0) reactionChainDepth = 0;

            int effectiveScore = comboDepth + (reactionChainDepth * 3);

            if (effectiveScore >= 20)
            {
                // Grand Elemental Cascade
                return new ChainReactionRewardOutcome(
                    gold: 25 + (effectiveScore * 2),
                    embers: 5,
                    quality: 0.50f,
                    tier: "Legendary Cascade"
                );
            }
            else if (effectiveScore >= 10)
            {
                // Greater Elemental Chain
                return new ChainReactionRewardOutcome(
                    gold: 15 + (effectiveScore * 1),
                    embers: 2,
                    quality: 0.25f,
                    tier: "Greater Chain"
                );
            }
            else if (effectiveScore >= 4)
            {
                // Minor Synergy Surge
                return new ChainReactionRewardOutcome(
                    gold: 5 + effectiveScore,
                    embers: 0,
                    quality: 0.10f,
                    tier: "Synergy Surge"
                );
            }
            else
            {
                // Standard
                return new ChainReactionRewardOutcome(
                    gold: 0,
                    embers: 0,
                    quality: 0f,
                    tier: "Standard"
                );
            }
        }
    }
}
