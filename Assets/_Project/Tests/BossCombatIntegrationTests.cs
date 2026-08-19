using NUnit.Framework;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Dungeon;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class BossCombatIntegrationTests
    {
        private GameObject _holderObj;
        private DungeonDefinitionSO _dungeon;
        private BossDefinitionSO _lichLord;
        private RunManager _runManager;
        private BossSystem _bossSystem;
        private CombatSystem _combatSystem;
        private RewardService _rewardService;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("BossCombatIntegrationTestHolder");
            _dungeon = DungeonDefinitionSO.CreateDefaultPhase2Dungeon();
            _lichLord = BossDefinitionSO.CreateLichLordDefinition();

            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(500);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();

            _combatSystem = _holderObj.AddComponent<CombatSystem>();
            _combatSystem.Initialize(_player, _enemy);

            _rewardService = _holderObj.AddComponent<RewardService>();

            _bossSystem = _holderObj.AddComponent<BossSystem>();
            _bossSystem.Initialize(_lichLord, _enemy);

            _runManager = _holderObj.AddComponent<RunManager>();
            _runManager.Initialize(_dungeon, _combatSystem, _rewardService, _player, _enemy, _bossSystem);
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
        public void BossCombat_Victory_TriggersRewardAndRunCompletion()
        {
            bool runComplete = false;
            _runManager.OnRunCompleted += () => runComplete = true;

            _runManager.StartRun();

            // Clear Floor 1
            _runManager.StartEncounterCombat();
            _enemy.TakeDamage(new DamageResult("Hero", "Enemy", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);
            _runManager.ContinueAfterReward();

            // Clear Floor 2
            _runManager.StartEncounterCombat();
            _enemy.TakeDamage(new DamageResult("Hero", "Enemy", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);
            _runManager.ContinueAfterReward();

            // Floor 3 (The Lich Lord Boss)
            Assert.AreEqual(3, _runManager.CurrentFloorNumber);
            Assert.IsTrue(_bossSystem.IsBossActive);
            Assert.AreEqual("The Lich Lord", _enemy.CombatantName);
            Assert.AreEqual(750, _enemy.MaxHp);

            _runManager.StartEncounterCombat();

            // Deal damage to trigger Phase 2
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 300, 0, 1f, 1f, 0, 300, false));
            Assert.AreEqual(1, _bossSystem.CurrentPhaseIndex);

            // Deal final blow to defeat Boss
            _enemy.TakeDamage(new DamageResult("Hero", "Lich", 999, 0, 1f, 1f, 0, 999, false));
            _combatSystem.UpdateCombat(0.1f);

            Assert.IsFalse(_bossSystem.IsBossActive);
            Assert.AreEqual(RunState.RewardSelection, _runManager.CurrentState);

            _runManager.ContinueAfterReward();

            Assert.AreEqual(RunState.RunComplete, _runManager.CurrentState);
            Assert.IsTrue(runComplete);
        }
    }
}
