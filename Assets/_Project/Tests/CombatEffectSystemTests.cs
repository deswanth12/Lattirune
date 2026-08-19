using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combat.Effects;

namespace Lattirune.Tests
{
    [TestFixture]
    public class CombatEffectSystemTests
    {
        private GameObject _holderObj;
        private CombatEffectSystem _effectSystem;
        private CombatEffectDatabaseSO _database;
        private EnemyCombatant _dummy;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("CombatEffectTestHolder");
            _database = CombatEffectDatabaseSO.CreateDefaultDatabase();

            _effectSystem = _holderObj.AddComponent<CombatEffectSystem>();
            _effectSystem.Initialize(_database);

            _dummy = _holderObj.AddComponent<EnemyCombatant>();
            _dummy.SetupTrainingDummy(hp: 100, baseArmor: 10, attack: 5, interval: 1.5f);
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        [Test]
        public void CombatEffect_AppliesToTarget_IncreasesActiveCount()
        {
            CombatEffectDefinitionSO steamDef = _database.GetByEffectId("effect_steam_blind");
            CombatEffectInstance instance = new CombatEffectInstance(steamDef, "rune_a", "rune_b", _dummy);

            _effectSystem.ApplyEffect(instance);

            Assert.AreEqual(1, _effectSystem.GetActiveEffectCount(_dummy));
            Assert.AreEqual(0.75f, _effectSystem.GetAttackMultiplier(_dummy), 0.001f);
        }

        [Test]
        public void CombatEffect_DurationDecreasesDeterministically_AndExpires()
        {
            CombatEffectDefinitionSO steamDef = _database.GetByEffectId("effect_steam_blind"); // 4.0s duration
            CombatEffectInstance instance = new CombatEffectInstance(steamDef, "rune_a", "rune_b", _dummy);
            _effectSystem.ApplyEffect(instance);

            // Advance 2.0s
            _effectSystem.UpdateEffects(2.0f);
            Assert.AreEqual(1, _effectSystem.GetActiveEffectCount(_dummy));
            Assert.AreEqual(2.0f, instance.RemainingDuration, 0.001f);

            // Advance remaining 2.0s + 0.1s -> Expired
            _effectSystem.UpdateEffects(2.1f);
            Assert.AreEqual(0, _effectSystem.GetActiveEffectCount(_dummy));
            Assert.AreEqual(1.0f, _effectSystem.GetAttackMultiplier(_dummy), 0.001f);
        }

        [Test]
        public void CombatEffect_DoT_TicksAtConfiguredInterval()
        {
            // Plasma: 3.0s duration, 0.5s interval, 9 dmg per tick
            CombatEffectDefinitionSO plasmaDef = _database.GetByEffectId("effect_plasma_ray");
            CombatEffectInstance instance = new CombatEffectInstance(plasmaDef, "rune_a", "rune_b", _dummy);
            _effectSystem.ApplyEffect(instance);

            int hpInitial = _dummy.CurrentHp;

            // Advance 0.4s (no tick yet)
            _effectSystem.UpdateEffects(0.4f);
            Assert.AreEqual(hpInitial, _dummy.CurrentHp);

            // Advance 0.1s (total 0.5s -> tick 1 triggers 9 dmg)
            _effectSystem.UpdateEffects(0.1f);
            Assert.AreEqual(hpInitial - 9, _dummy.CurrentHp);

            // Advance another 0.5s (total 1.0s -> tick 2 triggers another 9 dmg)
            _effectSystem.UpdateEffects(0.5f);
            Assert.AreEqual(hpInitial - 18, _dummy.CurrentHp);
        }

        [Test]
        public void CombatEffect_DirectDamage_AppliesImmediatelyWithoutDuration()
        {
            // Toxic Flame: instant 20 burst damage
            CombatEffectDefinitionSO toxicDef = _database.GetByEffectId("effect_toxic_detonation");
            CombatEffectInstance instance = new CombatEffectInstance(toxicDef, "rune_a", "rune_b", _dummy);

            int hpBefore = _dummy.CurrentHp;
            _effectSystem.ApplyEffect(instance);

            // Target has 10 armor -> 20 base - 10 armor = 10 damage dealt
            Assert.AreEqual(hpBefore - 10, _dummy.CurrentHp);
            // Instant effects do not linger in active list
            Assert.AreEqual(0, _effectSystem.GetActiveEffectCount(_dummy));
        }

        [Test]
        public void CombatEffect_ArmorModifier_ReducesArmorCorrectly()
        {
            // Superconductor: -40% resistance / armor
            CombatEffectDefinitionSO superDef = _database.GetByEffectId("effect_superconductor_shred");
            CombatEffectInstance instance = new CombatEffectInstance(superDef, "rune_a", "rune_b", _dummy);
            _effectSystem.ApplyEffect(instance);

            Assert.AreEqual(0.60f, _effectSystem.GetArmorMultiplier(_dummy), 0.001f);
        }

        [Test]
        public void CombatEffect_DuplicateApplication_RefreshesDuration()
        {
            CombatEffectDefinitionSO steamDef = _database.GetByEffectId("effect_steam_blind"); // 4.0s
            CombatEffectInstance first = new CombatEffectInstance(steamDef, "rune_a", "rune_b", _dummy);
            _effectSystem.ApplyEffect(first);

            _effectSystem.UpdateEffects(3.0f); // 1.0s remaining

            // Re-apply same effect
            CombatEffectInstance second = new CombatEffectInstance(steamDef, "rune_a", "rune_b", _dummy);
            _effectSystem.ApplyEffect(second);

            // Active count is still 1 (no duplicate stack), duration refreshed to 4.0s
            Assert.AreEqual(1, _effectSystem.GetActiveEffectCount(_dummy));
            Assert.AreEqual(4.0f, first.RemainingDuration, 0.001f);
        }

        [Test]
        public void CombatEffect_DeadTarget_CleansUpImmediately()
        {
            CombatEffectDefinitionSO plasmaDef = _database.GetByEffectId("effect_plasma_ray");
            CombatEffectInstance instance = new CombatEffectInstance(plasmaDef, "rune_a", "rune_b", _dummy);
            _effectSystem.ApplyEffect(instance);

            // Kill dummy
            DamageResult lethal = new DamageResult("Test", "Dummy", 500, 0, 1f, 1f, 0, 500, false);
            _dummy.TakeDamage(lethal);
            Assert.IsFalse(_dummy.IsAlive);

            _effectSystem.UpdateEffects(0.5f);
            Assert.AreEqual(0, _effectSystem.GetActiveEffectCount(_dummy));
        }
    }
}
