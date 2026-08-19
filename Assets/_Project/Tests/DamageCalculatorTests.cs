using NUnit.Framework;
using Lattirune.Combat;

namespace Lattirune.Tests
{
    [TestFixture]
    public class DamageCalculatorTests
    {
        [Test]
        public void DamageCalculator_MatchesPlanMd_ExamplePipeline()
        {
            // PLAN.md Section 9.2: Base=10, Rune=6, Crit=1.5, Mod=1.0, Armor=4 -> (10+6)*1.5 - 4 = 20
            DamageResult result = DamageCalculator.CalculateDamage(
                sourceName: "Hero",
                targetName: "Enemy",
                baseDamage: 10,
                runeBonus: 6,
                targetArmor: 4,
                isCritical: true,
                damageModifier: 1.0f,
                minimumDamage: 1
            );

            Assert.AreEqual(20, result.FinalDamage);
            Assert.AreEqual(10, result.BaseDamage);
            Assert.AreEqual(6, result.RuneBonus);
            Assert.AreEqual(1.5f, result.CritMultiplier);
            Assert.AreEqual(4, result.TargetArmor);
            Assert.IsTrue(result.IsCritical);
            Assert.IsTrue(result.HasSynergyBonus);
        }

        [Test]
        public void DamageCalculator_Armor_ReducesDamage()
        {
            // Base 10, Rune 0, Normal Crit (1.0), Armor 3 -> 10 - 3 = 7
            DamageResult result = DamageCalculator.CalculateDamage("Hero", "Enemy", 10, 0, 3, isCritical: false);
            Assert.AreEqual(7, result.FinalDamage);
        }

        [Test]
        public void DamageCalculator_MinimumDamageFloor_EnforcedWhenArmorExceedsDamage()
        {
            // Base 4, Rune 0, Armor 10 -> (4 - 10) = -6 -> clamped to minimum 1
            DamageResult result = DamageCalculator.CalculateDamage("Hero", "HeavyTank", 4, 0, 10, isCritical: false);
            Assert.AreEqual(1, result.FinalDamage, "Damage must never drop below the minimum damage floor of 1.");
        }

        [Test]
        public void DamageCalculator_NormalHit_UsesMultiplier1_0()
        {
            DamageResult result = DamageCalculator.CalculateDamage("Hero", "Enemy", 12, 0, 0, isCritical: false);
            Assert.AreEqual(1.0f, result.CritMultiplier);
            Assert.AreEqual(12, result.FinalDamage);
            Assert.IsFalse(result.IsCritical);
        }

        [Test]
        public void DamageCalculator_CriticalHit_UsesMultiplier1_5()
        {
            // Base 10 * 1.5 = 15
            DamageResult result = DamageCalculator.CalculateDamage("Hero", "Enemy", 10, 0, 0, isCritical: true);
            Assert.AreEqual(1.5f, result.CritMultiplier);
            Assert.AreEqual(15, result.FinalDamage);
            Assert.IsTrue(result.IsCritical);
        }

        [Test]
        public void DamageCalculator_SynergyRuneBonus_AddedDirectlyToBase()
        {
            // Base 10 + Rune 5 = 15, Armor 2 -> 13
            DamageResult withSynergy = DamageCalculator.CalculateDamage("Hero", "Enemy", 10, 5, 2, isCritical: false);
            Assert.AreEqual(13, withSynergy.FinalDamage);
            Assert.IsTrue(withSynergy.HasSynergyBonus);

            // Without Synergy: Base 10 + Rune 0 = 10, Armor 2 -> 8
            DamageResult withoutSynergy = DamageCalculator.CalculateDamage("Hero", "Enemy", 10, 0, 2, isCritical: false);
            Assert.AreEqual(8, withoutSynergy.FinalDamage);
            Assert.IsFalse(withoutSynergy.HasSynergyBonus);
        }
    }
}
