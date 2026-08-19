using NUnit.Framework;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;

namespace Lattirune.Tests
{
    [TestFixture]
    public class BossSystemTests
    {
        private GameObject _holderObj;
        private BossDefinitionSO _lichLord;
        private EnemyCombatant _enemy;
        private BossSystem _bossSystem;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("BossSystemTestHolder");
            _lichLord = BossDefinitionSO.CreateLichLordDefinition();

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _bossSystem = _holderObj.AddComponent<BossSystem>();
            _bossSystem.Initialize(_lichLord, _enemy);
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
        public void BossSystem_StartsInPhase1()
        {
            _bossSystem.StartBossFight();

            Assert.AreEqual(0, _bossSystem.CurrentPhaseIndex);
            Assert.AreEqual(1, _bossSystem.CurrentPhaseNumber);
            Assert.AreEqual("Phase 1: Frost Warden", _bossSystem.CurrentPhase.DisplayName);
            Assert.AreEqual(750, _enemy.MaxHp);
            Assert.AreEqual(10, _enemy.Armor);
            Assert.AreEqual(8, _enemy.BaseAttackDamage);
        }

        [Test]
        public void BossSystem_ThresholdTransition_MovesToPhase2()
        {
            _bossSystem.StartBossFight();

            bool phaseChanged = false;
            _bossSystem.OnPhaseChanged += (idx, phase) => phaseChanged = true;

            // Reduce HP to 450 / 750 (60% HP <= 66% threshold)
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 300, 0, 1f, 1f, 0, 300, false));

            Assert.AreEqual(1, _bossSystem.CurrentPhaseIndex);
            Assert.AreEqual("Phase 2: Soul Harvest", _bossSystem.CurrentPhase.DisplayName);
            Assert.IsTrue(phaseChanged);
            Assert.AreEqual(15, _enemy.Armor); // 10 + 5
            Assert.AreEqual(12, _enemy.BaseAttackDamage); // 8 + 4
        }

        [Test]
        public void BossSystem_ThresholdTransition_MovesToPhase3()
        {
            _bossSystem.StartBossFight();

            // Reduce HP to 200 / 750 (26.6% HP <= 33% threshold)
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 550, 0, 1f, 1f, 0, 550, false));

            Assert.AreEqual(2, _bossSystem.CurrentPhaseIndex);
            Assert.AreEqual("Phase 3: Necrotic Inversion", _bossSystem.CurrentPhase.DisplayName);
            Assert.AreEqual(20, _enemy.Armor); // 10 + 10
            Assert.AreEqual(16, _enemy.BaseAttackDamage); // 8 + 8
        }

        [Test]
        public void BossSystem_DeadBoss_CannotTransitionPhases()
        {
            _bossSystem.StartBossFight();

            // Kill boss in single strike
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 9999, 0, 1f, 1f, 0, 9999, false));

            Assert.IsFalse(_bossSystem.IsBossActive);
            Assert.IsFalse(_enemy.IsAlive);
        }

        [Test]
        public void BossSystem_ResetBoss_RestoresInitialState()
        {
            _bossSystem.StartBossFight();
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 300, 0, 1f, 1f, 0, 300, false));
            Assert.AreEqual(1, _bossSystem.CurrentPhaseIndex);

            _bossSystem.ResetBoss();

            Assert.AreEqual(0, _bossSystem.CurrentPhaseIndex);
            Assert.AreEqual(0, _bossSystem.PhaseTransitionCount);
            Assert.IsFalse(_bossSystem.IsBossActive);
        }

        [Test]
        public void BossSystem_Telemetry_MatchesRuntimeState()
        {
            _bossSystem.StartBossFight();

            BossTelemetry telem = _bossSystem.GetTelemetry();
            Assert.AreEqual("boss_lich_lord", telem.BossId);
            Assert.AreEqual("The Lich Lord", telem.BossName);
            Assert.AreEqual(0, telem.CurrentPhaseIndex);
            Assert.AreEqual(750, telem.CurrentHp);
            Assert.AreEqual(750, telem.MaxHp);
            Assert.AreEqual(1.0f, telem.HpPercentage);
            Assert.AreEqual(8, telem.EffectiveAttack);
            Assert.AreEqual(10, telem.EffectiveArmor);
        }
    }
}
