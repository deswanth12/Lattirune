using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Dungeon;

namespace Lattirune.Tests
{
    [TestFixture]
    public class EndlessDungeonModeTests
    {
        private GameObject _holder;
        private RunManager _runManager;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;
        private CombatSystem _combat;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("EndlessTestHolder");
            _player = _holder.AddComponent<PlayerCombatant>();
            _player.SetStats(100, 5, 1.5f);

            _enemy = _holder.AddComponent<EnemyCombatant>();
            _enemy.SetupCustom("TestDummy", 50, 2, 8, 1.8f);

            _combat = _holder.AddComponent<CombatSystem>();
            _combat.Initialize(_player, _enemy);

            _runManager = _holder.AddComponent<RunManager>();
            _runManager.Initialize(
                dungeon: DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                combat: _combat,
                rewards: null,
                player: _player,
                enemy: _enemy
            );
        }

        [TearDown]
        public void Teardown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
        }

        [Test]
        public void StandardMode_TerminatesOnFloor10()
        {
            _runManager.StartRun();
            _runManager.RestoreRunState(9, 0, RunState.RewardSelection);

            Assert.IsFalse(_runManager.IsEndlessMode);
            Assert.IsTrue(_runManager.IsFinalFloor);

            _runManager.ContinueAfterReward();
            Assert.AreEqual(RunState.RunComplete, _runManager.CurrentState);
        }

        [Test]
        public void EndlessMode_AllowsContinuousDescentPastFloor10()
        {
            _runManager.StartRun();
            _runManager.EnableEndlessMode();
            Assert.IsTrue(_runManager.IsEndlessMode);

            _runManager.RestoreRunState(9, 0, RunState.RewardSelection);
            Assert.IsFalse(_runManager.IsFinalFloor, "In Endless Mode, Floor 10 should not be marked as final floor.");

            _runManager.ContinueAfterReward();
            Assert.AreEqual(11, _runManager.CurrentFloorNumber, "Should advance to Floor 11.");
            Assert.AreEqual(RunState.FloorPreparing, _runManager.CurrentState);

            var floor11 = _runManager.CurrentFloor;
            Assert.IsNotNull(floor11);
            Assert.AreEqual(11, floor11.FloorNumber);
            Assert.AreEqual(1, floor11.EncounterCount);

            var enc = floor11.GetEncounter(0);
            Assert.IsNotNull(enc);
            Assert.IsTrue(enc.EnemyHp > 120, "Endless enemy HP should scale above base HP.");
            Assert.IsTrue(enc.EnemyAttack > 15, "Endless enemy Attack should scale above base Attack.");
        }

        [Test]
        public void EndlessMode_ScalesMonotonicallyWithFloorDepth()
        {
            _runManager.StartRun();
            _runManager.EnableEndlessMode();

            // Floor 11 (Endless Tier 1)
            _runManager.RestoreRunState(10, 0, RunState.FloorPreparing);
            int hpFloor11 = _runManager.CurrentFloor.GetEncounter(0).EnemyHp;

            // Floor 15 (Endless Tier 5 - Boss)
            _runManager.RestoreRunState(14, 0, RunState.FloorPreparing);
            int hpFloor15 = _runManager.CurrentFloor.GetEncounter(0).EnemyHp;
            bool isBossFloor15 = _runManager.CurrentFloor.GetEncounter(0).IsBoss;

            Assert.IsTrue(hpFloor15 > hpFloor11, "Floor 15 enemy HP should exceed Floor 11 HP.");
            Assert.IsTrue(isBossFloor15, "Every 5th endless floor should be marked as boss encounter.");
        }
    }
}
