using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combat.Effects;
using Lattirune.Dungeon;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RunStateMachineTests
    {
        private GameObject _holderObj;
        private DungeonDefinitionSO _dungeon;
        private RunManager _runManager;
        private CombatSystem _combatSystem;
        private RewardService _rewardService;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("RunStateMachineTestHolder");
            _dungeon = DungeonDefinitionSO.CreateDefaultPhase2Dungeon();

            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(100);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _enemy.SetupTrainingDummy(40, 1, 3, 1.5f);

            _combatSystem = _holderObj.AddComponent<CombatSystem>();
            _combatSystem.Initialize(_player, _enemy);

            _rewardService = _holderObj.AddComponent<RewardService>();

            _runManager = _holderObj.AddComponent<RunManager>();
            _runManager.Initialize(_dungeon, _combatSystem, _rewardService, _player, _enemy);
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
        public void DungeonData_ValidatesCorrectly()
        {
            Assert.IsTrue(_dungeon.IsValid(out string err));
            Assert.IsNull(err);
            Assert.AreEqual(3, _dungeon.TotalFloorCount);
        }

        [Test]
        public void FloorData_ValidatesCorrectly()
        {
            DungeonFloorDefinitionSO floor1 = _dungeon.GetFloor(0);
            Assert.IsNotNull(floor1);
            Assert.IsTrue(floor1.IsValid(out string err));
            Assert.IsNull(err);
            Assert.AreEqual(1, floor1.FloorNumber);
            Assert.AreEqual(1, floor1.EncounterCount);
        }

        [Test]
        public void EncounterData_ValidatesCorrectly()
        {
            EncounterDefinitionSO bossEnc = _dungeon.GetFloor(2).GetEncounter(0);
            Assert.IsNotNull(bossEnc);
            Assert.IsTrue(bossEnc.IsValid(out string err));
            Assert.IsNull(err);
            Assert.IsTrue(bossEnc.IsBoss);
            Assert.AreEqual("Lich Lord", bossEnc.EnemyName);
        }

        [Test]
        public void RunState_InitialStateIsNotStarted()
        {
            Assert.AreEqual(RunState.NotStarted, _runManager.CurrentState);
            Assert.AreEqual(0, _runManager.CurrentFloorIndex);
            Assert.AreEqual(1, _runManager.CurrentFloorNumber);
        }

        [Test]
        public void RunManager_StartRun_TransitionsToFloorPreparing()
        {
            bool floorStarted = false;
            _runManager.OnFloorStarted += (num, floor) => floorStarted = true;

            _runManager.StartRun();

            Assert.AreEqual(RunState.FloorPreparing, _runManager.CurrentState);
            Assert.AreEqual(1, _runManager.CurrentFloorNumber);
            Assert.IsTrue(floorStarted);
        }

        [Test]
        public void RunManager_StartEncounterCombat_TransitionsToEncounterActive()
        {
            _runManager.StartRun();
            _runManager.StartEncounterCombat();

            Assert.AreEqual(RunState.EncounterActive, _runManager.CurrentState);
            Assert.AreEqual(CombatState.Fighting, _combatSystem.CurrentState);
        }

        [Test]
        public void RunManager_CombatDefeat_TransitionsToDefeated()
        {
            _runManager.StartRun();
            _runManager.StartEncounterCombat();

            bool runDefeated = false;
            _runManager.OnRunDefeated += () => runDefeated = true;

            // Kill player
            _player.TakeDamage(new DamageResult("Enemy", "Hero", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);

            Assert.AreEqual(RunState.Defeated, _runManager.CurrentState);
            Assert.IsTrue(runDefeated);
        }

        [Test]
        public void RunManager_ResetRun_ReturnsToNotStarted()
        {
            _runManager.StartRun();
            _runManager.ResetRun();

            Assert.AreEqual(RunState.NotStarted, _runManager.CurrentState);
            Assert.AreEqual(0, _runManager.CurrentFloorIndex);
        }
    }
}
