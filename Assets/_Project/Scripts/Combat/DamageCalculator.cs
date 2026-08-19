using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Deterministic damage calculation engine implementing PLAN.md Section 9.2:
    /// FinalDamage = max(MinimumDamage, ((BaseDamage + RuneBonus) * CritMultiplier * DamageModifiers) - EnemyArmor)
    /// </summary>
    public static class DamageCalculator
    {
        public const float NORMAL_CRIT_MULTIPLIER = 1.0f;
        public const float CRITICAL_HIT_MULTIPLIER = 1.5f;
        public const int DEFAULT_MINIMUM_DAMAGE = 1;
        public const float DEFAULT_DAMAGE_MODIFIER = 1.0f;

        /// <summary>
        /// Calculates final attack damage with full step-by-step formula compliance.
        /// </summary>
        public static DamageResult CalculateDamage(
            string sourceName,
            string targetName,
            int baseDamage,
            int runeBonus,
            int targetArmor,
            bool isCritical = false,
            float damageModifier = DEFAULT_DAMAGE_MODIFIER,
            int minimumDamage = DEFAULT_MINIMUM_DAMAGE)
        {
            float critMult = isCritical ? CRITICAL_HIT_MULTIPLIER : NORMAL_CRIT_MULTIPLIER;
            float rawDamage = (baseDamage + runeBonus) * critMult * damageModifier;
            int preArmorDamage = Mathf.RoundToInt(rawDamage);
            int postArmorDamage = preArmorDamage - targetArmor;
            int finalDamage = Mathf.Max(minimumDamage, postArmorDamage);

            return new DamageResult(
                source: sourceName,
                target: targetName,
                baseDamage: baseDamage,
                runeBonus: runeBonus,
                critMultiplier: critMult,
                damageModifiers: damageModifier,
                targetArmor: targetArmor,
                finalDamage: finalDamage,
                isCritical: isCritical
            );
        }
    }
}
