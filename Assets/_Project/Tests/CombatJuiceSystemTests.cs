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
    }
}
