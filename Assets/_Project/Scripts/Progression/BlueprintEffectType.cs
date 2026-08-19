namespace Lattirune.Progression
{
    /// <summary>
    /// Classification of gameplay and reward effects triggered by unlocked Forge blueprints.
    /// Derived strictly from PLAN.md Section 12, Section 13, and Section 22.
    /// </summary>
    public enum BlueprintEffectType
    {
        UnlockItemInRewardPool,
        UnlockRuneInRewardPool,
        PermanentStartingGoldBonus,
        PermanentStartingHpBonus,
        PermanentDamageMultiplier,
        PotionHealBonus,
        VampirismBonus,
        BonusEmberReward,
        MapVision
    }
}
