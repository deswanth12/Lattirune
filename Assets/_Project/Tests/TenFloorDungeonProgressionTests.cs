using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Dungeon;
using Lattirune.Items;
using Lattirune.Save;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Integration test suite for the full 10-Floor Biome 1 ("The Cursed Sewers") Dungeon Progression.
    /// Strictly verifies PLAN.md Section 11 room topology and RunManager transitions.
    /// </summary>
    [TestFixture]
    public class TenFloorDungeonProgressionTests
    {
        private GameObject _holderObj;
        private DungeonDefinitionSO _dungeonDef;
        private RunManager _runManager;
        private CombatSystem _combatSystem;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;
        private BossSystem _bossSystem;
        private RewardService _rewardService;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("TenFloorProgressionTestHolder");

            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(initialHp: 500);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _enemy.SetupTrainingDummy(hp: 50, baseArmor: 2, attack: 4, interval: 1.5f);

            _combatSystem = _holderObj.AddComponent<CombatSystem>();
            _combatSystem.Initialize(_player, _enemy);

            _bossSystem = _holderObj.AddComponent<BossSystem>();
            _bossSystem.Initialize(BossDefinitionSO.CreateLichLordDefinition(), _enemy);

            _rewardService = _holderObj.AddComponent<RewardService>();

            _dungeonDef = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();

            _runManager = _holderObj.AddComponent<RunManager>();
            _runManager.Initialize(_dungeonDef, _combatSystem, _rewardService, _player, _enemy, _bossSystem);
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
        public void TenFloorDungeon_Definition_Contains10FloorsAndCorrectEncounters()
        {
            Assert.AreEqual(10, _dungeonDef.TotalFloorCount);
            Assert.IsTrue(_dungeonDef.IsValid(out _));

            // Floor 1: Sewer Rat
            Assert.AreEqual("Floor 1: Sewer Entry", _dungeonDef.GetFloor(0).DisplayName);
            Assert.AreEqual("Sewer Rat", _dungeonDef.GetFloor(0).GetEncounter(0).EnemyName);

            // Floor 2: Goblin Thief
            Assert.AreEqual("Floor 2: Drain Basin", _dungeonDef.GetFloor(1).DisplayName);
            Assert.AreEqual("Goblin Thief", _dungeonDef.GetFloor(1).GetEncounter(0).EnemyName);

            // Floor 3: Acid Slime
            Assert.AreEqual("Floor 3: Slime Cavern", _dungeonDef.GetFloor(2).DisplayName);
            Assert.AreEqual("Acid Slime", _dungeonDef.GetFloor(2).GetEncounter(0).EnemyName);

            // Floor 7: Necromancer
            Assert.AreEqual("Floor 7: Bone Crypt", _dungeonDef.GetFloor(6).DisplayName);
            Assert.AreEqual("Necromancer", _dungeonDef.GetFloor(6).GetEncounter(0).EnemyName);

            // Floor 10: The Lich Lord
            Assert.AreEqual("Floor 10: Boss Sanctum", _dungeonDef.GetFloor(9).DisplayName);
            Assert.AreEqual("The Lich Lord", _dungeonDef.GetFloor(9).GetEncounter(0).EnemyName);
            Assert.IsTrue(_dungeonDef.GetFloor(9).GetEncounter(0).IsBoss);
        }

        [Test]
        public void TenFloorDungeon_Full10FloorRun_Floor1ToFloor10LichLord_ReachesRunComplete()
        {
            _runManager.StartRun();
            Assert.AreEqual(1, _runManager.CurrentFloorNumber);

            // Floors 1 to 9
            for (int f = 1; f <= 9; f++)
            {
                Assert.AreEqual(f, _runManager.CurrentFloorNumber);
                Assert.AreEqual(RunState.FloorPreparing, _runManager.CurrentState);

                _runManager.StartEncounterCombat();
                Assert.AreEqual(RunState.EncounterActive, _runManager.CurrentState);

                // Simulate victory
                _enemy.TakeDamage(new DamageResult("Hero", _enemy.CombatantName, 1000, 0, 1f, 1f, 0, 1000, false));
                _combatSystem.UpdateCombat(0.1f);

                Assert.AreEqual(RunState.RewardSelection, _runManager.CurrentState);
                _runManager.ContinueAfterReward();
            }

            // Floor 10: Boss Sanctum (The Lich Lord)
            Assert.AreEqual(10, _runManager.CurrentFloorNumber);
            Assert.IsTrue(_runManager.IsFinalFloor);
            Assert.IsTrue(_bossSystem.IsBossActive);
            Assert.AreEqual("The Lich Lord", _enemy.CombatantName);

            _runManager.StartEncounterCombat();
            Assert.AreEqual(RunState.EncounterActive, _runManager.CurrentState);

            // Defeat Boss
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 1000, 0, 1f, 1f, 0, 1000, false));
            _combatSystem.UpdateCombat(0.1f);

            Assert.AreEqual(RunState.RewardSelection, _runManager.CurrentState);
            _runManager.ContinueAfterReward();

            // Run Complete!
            Assert.AreEqual(RunState.RunComplete, _runManager.CurrentState);
            Assert.IsTrue(_runManager.IsRunFinished);
        }

        [Test]
        public void TenFloorDungeon_MidRunSaveAndRestore_RestoresFloorIndexCorrectly()
        {
            _runManager.StartRun();

            // Advance to Floor 5 (Mid-Boss Armored Skeleton)
            for (int f = 1; f < 5; f++)
            {
                _runManager.StartEncounterCombat();
                _enemy.TakeDamage(new DamageResult("Hero", _enemy.CombatantName, 1000, 0, 1f, 1f, 0, 1000, false));
                _combatSystem.UpdateCombat(0.1f);
                _runManager.ContinueAfterReward();
            }

            Assert.AreEqual(5, _runManager.CurrentFloorNumber);

            SavedRunData saved = new SavedRunData(true, _runManager.CurrentFloorIndex, _runManager.CurrentEncounterIndex, (int)_runManager.CurrentState);

            // Restore in new RunManager
            GameObject freshObj = new GameObject("FreshRunManagerHolder");
            RunManager freshManager = freshObj.AddComponent<RunManager>();
            freshManager.Initialize(_dungeonDef, _combatSystem, _rewardService, _player, _enemy, _bossSystem);

            freshManager.RestoreRunState(saved.currentFloorIndex, saved.currentEncounterIndex, (RunState)saved.runState);

            Assert.AreEqual(5, freshManager.CurrentFloorNumber);
            Assert.AreEqual(4, freshManager.CurrentFloorIndex);
            Assert.AreEqual("Armored Skeleton", _enemy.CombatantName);

            Object.DestroyImmediate(freshObj);
        }

        [Test]
        public void TenFloorDungeon_ConsecutiveRuns_ResetAndTrackIndependently()
        {
            _runManager.StartRun();
            _runManager.StartEncounterCombat();
            _enemy.TakeDamage(new DamageResult("Hero", "Enemy", 1000, 0, 1f, 1f, 0, 1000, false));
            _combatSystem.UpdateCombat(0.1f);
            _runManager.ContinueAfterReward();

            Assert.AreEqual(2, _runManager.CurrentFloorNumber);

            _runManager.ResetRun();
            Assert.AreEqual(RunState.NotStarted, _runManager.CurrentState);
            Assert.AreEqual(0, _runManager.CurrentFloorIndex);

            // Start new run
            _runManager.StartRun();
            Assert.AreEqual(1, _runManager.CurrentFloorNumber);
            Assert.AreEqual(RunState.FloorPreparing, _runManager.CurrentState);
        }
    }
}
