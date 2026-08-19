using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;

namespace Lattirune.Tests
{
    [TestFixture]
    public class CombatJuiceSystemTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("JuiceTestHolder");
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
        }

        [Test]
        public void FloatingCombatTextPool_SpawnsAndRecyclesSlots()
        {
            var pool = _holder.AddComponent<FloatingCombatTextPool>();
            pool.Initialize();

            Assert.AreEqual(FloatingCombatTextPool.POOL_SIZE, pool.ActivePool.Count);

            // Spawn critical floaty
            var f1 = pool.SpawnText("CRIT! -45", new Vector2(500, 500), FloatingTextType.CriticalDamage, duration: 1.0f);
            Assert.IsNotNull(f1);
            Assert.IsTrue(f1.IsActive);
            Assert.AreEqual(Color.red.r, f1.TextColor.r);
            Assert.Greater(f1.Scale, 1.0f); // Crit scale-up

            // Tick 0.5s -> Still active, moved upwards (Y decreased)
            float initialY = f1.ScreenPosition.y;
            pool.Tick(0.5f);
            Assert.IsTrue(f1.IsActive);
            Assert.Less(f1.ScreenPosition.y, initialY);

            // Tick another 0.6s (total 1.1s) -> Expired and inactive
            pool.Tick(0.6f);
            Assert.IsFalse(f1.IsActive);
        }

        [Test]
        public void CombatCameraShake_AddsTraumaAndDecaysSmoothly()
        {
            var shake = _holder.AddComponent<CombatCameraShakeController>();
            shake.Initialize();

            Assert.AreEqual(0f, shake.CurrentTrauma);

            shake.AddTrauma(0.5f);
            Assert.AreEqual(0.5f, shake.CurrentTrauma);

            // Tick 0.1s -> Decaying
            shake.Tick(0.1f);
            Assert.Less(shake.CurrentTrauma, 0.5f);
            Assert.Greater(shake.CurrentTrauma, 0f);

            // Tick 1.0s -> Fully returned to rest (0)
            shake.Tick(1.0f);
            Assert.AreEqual(0f, shake.CurrentTrauma);
            Assert.AreEqual(Vector3.zero, shake.CurrentOffset);
        }

        [Test]
        public void FloatingCombatTextPool_EmergencyPotion_SpawnsHealFloaty()
        {
            var player = _holder.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.TakeDamage(new DamageResult("Enemy", "Hero", 40, 0, 40));

            var enemy = _holder.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy();

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            var pool = _holder.AddComponent<FloatingCombatTextPool>();
            pool.Initialize(combat);

            combat.UseEmergencyPotion(player, 25);

            bool foundHeal = false;
            for (int i = 0; i < pool.ActivePool.Count; i++)
            {
                if (pool.ActivePool[i].IsActive && pool.ActivePool[i].Text == "+25 HP")
                {
                    foundHeal = true;
                    Assert.AreEqual(0.2f, pool.ActivePool[i].TextColor.r, 0.05f);
                    Assert.AreEqual(1.0f, pool.ActivePool[i].TextColor.g, 0.05f);
                    break;
                }
            }

            Assert.IsTrue(foundHeal, "Emergency potion must spawn +25 HP green floaty.");
        }

        [Test]
        public void FloatingCombatTextPool_ReflectedDamage_SpawnsThornsFloaty()
        {
            var player = _holder.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var enemy = _holder.AddComponent<EnemyCombatant>();
            var reflectTrait = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            reflectTrait.Initialize("trait_reflect", "Damage Reflect", EnemyTraitType.DamageReflect, 0.50f);
            enemy.SetupCustom("Armored Skeleton", 100, 0, 0, 99f, new[] { reflectTrait });

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            var pool = _holder.AddComponent<FloatingCombatTextPool>();
            pool.Initialize(combat);

            player.SetExplicitStats(baseDamage: 20, runeBonus: 0, armorValue: 0, interval: 1.0f);
            combat.StartCombat();
            combat.UpdateCombat(1.0f); // Player attacks for 20 DMG -> 10 DMG reflected

            bool foundThorns = false;
            for (int i = 0; i < pool.ActivePool.Count; i++)
            {
                if (pool.ActivePool[i].IsActive && pool.ActivePool[i].Text.Contains("THORNS! -10"))
                {
                    foundThorns = true;
                    break;
                }
            }

            Assert.IsTrue(foundThorns, "Damage reflection must spawn THORNS! floaty.");
        }
    }
}
