using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Dungeon;
using Lattirune.Save;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RunManagerTests
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
            _holderObj = new GameObject("RunManagerTestHolder");
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
        public void RunManager_ThreeFloorProgression_CompletesRunOnFinalBoss()
        {
            bool runCompleted = false;
            _runManager.OnRunCompleted += () => runCompleted = true;

            _runManager.StartRun();

            // FLOOR 1:
            Assert.AreEqual(1, _runManager.CurrentFloorNumber);
            _runManager.StartEncounterCombat();
            // Win Floor 1
            _enemy.TakeDamage(new DamageResult("Hero", "Sewer Rat", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);
            Assert.AreEqual(RunState.RewardSelection, _runManager.CurrentState);
            _runManager.ContinueAfterReward();

            // FLOOR 2:
            Assert.AreEqual(2, _runManager.CurrentFloorNumber);
            Assert.AreEqual("Armored Skeleton", _enemy.CombatantName);
            _runManager.StartEncounterCombat();
            // Win Floor 2
            _enemy.TakeDamage(new DamageResult("Hero", "Skeleton", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);
            Assert.AreEqual(RunState.RewardSelection, _runManager.CurrentState);
            _runManager.ContinueAfterReward();

            // FLOOR 3 (Boss):
            Assert.AreEqual(3, _runManager.CurrentFloorNumber);
            Assert.AreEqual("The Lich Lord", _enemy.CombatantName);
            _runManager.StartEncounterCombat();
            // Win Boss
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);
            Assert.AreEqual(RunState.RewardSelection, _runManager.CurrentState);
            _runManager.ContinueAfterReward();

            // RUN COMPLETE:
            Assert.AreEqual(RunState.RunComplete, _runManager.CurrentState);
            Assert.IsTrue(runCompleted);
        }

        [Test]
        public void RunManager_EncounterSetup_InitializesEnemyStatsCorrectly()
        {
            _runManager.StartRun();
            // Floor 1 Sewer Rat
            Assert.AreEqual(35, _enemy.MaxHp);
            Assert.AreEqual(0, _enemy.Armor);
            Assert.AreEqual(3, _enemy.BaseAttackDamage);
        }

        [Test]
        public void RunManager_SaveData_PersistsAndRestoresRunState()
        {
            _runManager.StartRun();
            _runManager.StartEncounterCombat();
            // Win Floor 1
            _enemy.TakeDamage(new DamageResult("Hero", "Sewer Rat", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);
            _runManager.ContinueAfterReward();

            // Now on Floor 2
            Assert.AreEqual(2, _runManager.CurrentFloorNumber);

            SavedRunData saved = new SavedRunData(
                active: true,
                floorIdx: _runManager.CurrentFloorIndex,
                encIdx: _runManager.CurrentEncounterIndex,
                state: (int)_runManager.CurrentState
            );

            // Restore into a fresh RunManager instance
            GameObject freshObj = new GameObject("FreshRunManager");
            RunManager freshRun = freshObj.AddComponent<RunManager>();
            EnemyCombatant freshEnemy = freshObj.AddComponent<EnemyCombatant>();
            freshEnemy.SetupTrainingDummy(50, 1, 1, 1f);
            freshRun.Initialize(_dungeon, null, null, null, freshEnemy);

            freshRun.RestoreRunState(saved.currentFloorIndex, saved.currentEncounterIndex, (RunState)saved.runState);

            Assert.AreEqual(2, freshRun.CurrentFloorNumber);
            Assert.AreEqual("Armored Skeleton", freshEnemy.CombatantName);

            Object.DestroyImmediate(freshObj);
        }

        [Test]
        public void RunManager_EndlessMode_ScalesEnemyHpAndAttack()
        {
            _runManager.EnableEndlessMode();
            Assert.IsTrue(_runManager.IsEndlessMode);

            _runManager.StartRun();
            // Restore state to Endless Floor 12 (FloorIndex 11)
            _runManager.RestoreRunState(floorIdx: 11, encIdx: 0, state: RunState.FloorPreparing);

            // Base Sewer Rat is 35 HP, 3 ATK.
            // On Floor 12 (tier = 2):
            // HP: 35 * (1.18)^2 ≈ 49 HP
            // ATK: 3 * (1.12)^2 ≈ 4 ATK
            // Armor: 0 + 2*3 = 6 Armor
            Assert.Greater(_enemy.MaxHp, 35);
            Assert.GreaterOrEqual(_enemy.BaseAttackDamage, 3);
            Assert.Greater(_enemy.Armor, 0);
        }

        [Test]
        public void RunManager_EliteEncounter_AssignsEliteAffix()
        {
            _runManager.StartRun();
            // Restore state to Floor 5 (Elite floor)
            _runManager.RestoreRunState(floorIdx: 4, encIdx: 0, state: RunState.FloorPreparing);

            Assert.IsTrue(_enemy.IsElite, "Floor 5 enemy must have an active Elite Affix.");
            Assert.AreNotEqual(EliteAffixType.None, _enemy.EliteAffix);
        }
    }
}
