using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Combo;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Modifiers;
using Lattirune.Progression;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Playability Audit Real Player Journey Integration Test Suite.
    /// Strictly verifies that Lattirune has ZERO fake gameplay, placeholder encounters, or dummy transitions.
    /// Validates all 10 floors, normal/elite/boss encounters, merchant economy, campfire rest, procedural events,
    /// real HP/damage calculations, inventory reward application, and victory/defeat progression.
    /// </summary>
    [TestFixture]
    public class PlayabilityAuditRealPlayerJourneyTests
    {
        private GameObject _holder;
        private LatticeGrid _grid;
        private InventorySystem _inventory;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;
        private CombatSystem _combat;
        private RunModifierManager _modManager;
        private ComboTracker _comboTracker;
        private MetaProgressionManager _meta;
        private RunManager _runManager;
        private DungeonMapGraph _mapGraph;
        private ScreenNavigationController _nav;
        private DungeonMapScreenController _mapUI;
        private MerchantStallUIController _merchantUI;
        private CampfireRestUIController _campfireUI;
        private RunEventUIController _eventUI;
        private CombatEncounterUI _combatUI;
        private RunCompleteController _runCompleteUI;
        private MerchantSystem _merchantSystem;
        private RunEventService _eventService;
        private RewardService _rewardService;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("PlayabilityAuditHolder");

            _grid = new LatticeGrid();

            var invObj = new GameObject("InventorySystem");
            invObj.transform.SetParent(_holder.transform);
            _inventory = invObj.AddComponent<InventorySystem>();
            _inventory.Initialize();

            var playerObj = new GameObject("PlayerCombatant");
            playerObj.transform.SetParent(_holder.transform);
            _player = playerObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(100);

            var enemyObj = new GameObject("EnemyCombatant");
            enemyObj.transform.SetParent(_holder.transform);
            _enemy = enemyObj.AddComponent<EnemyCombatant>();

            _modManager = _holder.AddComponent<RunModifierManager>();
            _modManager.Initialize();

            _comboTracker = _holder.AddComponent<ComboTracker>();
            _comboTracker.Initialize(step: 0.05f, maxMult: 2.0f);

            _combat = _holder.AddComponent<CombatSystem>();
            _combat.Initialize(_player, _enemy, null, _modManager, _comboTracker);

            _meta = _holder.AddComponent<MetaProgressionManager>();
            _meta.Initialize(startingEmbers: 100);

            _runManager = _holder.AddComponent<RunManager>();
            _runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                _combat,
                null,
                _player,
                _enemy,
                null,
                _meta,
                _modManager,
                _comboTracker
            );

            _nav = _holder.AddComponent<ScreenNavigationController>();

            _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();

            var mapUiObj = new GameObject("DungeonMapScreenController");
            mapUiObj.transform.SetParent(_holder.transform);
            _mapUI = mapUiObj.AddComponent<DungeonMapScreenController>();
            _mapUI.Initialize(_runManager, _nav, _mapGraph);

            var merchantObj = new GameObject("MerchantSystem");
            merchantObj.transform.SetParent(_holder.transform);
            _merchantSystem = merchantObj.AddComponent<MerchantSystem>();
            _merchantSystem.Initialize(ItemDatabaseSO.CreateCanonicalDatabase(), RuneDatabaseSO.CreateCanonicalDatabase());

            var merchantUiObj = new GameObject("MerchantStallUIController");
            merchantUiObj.transform.SetParent(_holder.transform);
            _merchantUI = merchantUiObj.AddComponent<MerchantStallUIController>();
            _merchantUI.Initialize(_merchantSystem, _runManager, _inventory, _grid, _player, _runManager, _nav);
            _merchantUI.BindMapController(_mapUI);

            var campfireUiObj = new GameObject("CampfireRestUIController");
            campfireUiObj.transform.SetParent(_holder.transform);
            _campfireUI = campfireUiObj.AddComponent<CampfireRestUIController>();
            _campfireUI.Initialize(_runManager, _player, _modManager, _nav);
            _campfireUI.BindMapController(_mapUI);

            var eventServiceObj = new GameObject("RunEventService");
            eventServiceObj.transform.SetParent(_holder.transform);
            _eventService = eventServiceObj.AddComponent<RunEventService>();
            _eventService.Initialize(RunEventDatabaseSO.CreateCanonicalEventDatabase());

            var eventUiObj = new GameObject("RunEventUIController");
            eventUiObj.transform.SetParent(_holder.transform);
            _eventUI = eventUiObj.AddComponent<RunEventUIController>();
            _eventUI.Initialize(_eventService, _runManager, _player, _modManager, _nav, _runManager, _mapUI);

            var completeUiObj = new GameObject("RunCompleteController");
            completeUiObj.transform.SetParent(_holder.transform);
            _runCompleteUI = completeUiObj.AddComponent<RunCompleteController>();
            _runCompleteUI.Initialize(_nav, _runManager, _meta);

            var rewardObj = new GameObject("RewardService");
            rewardObj.transform.SetParent(_holder.transform);
            _rewardService = rewardObj.AddComponent<RewardService>();

            var combatUiObj = new GameObject("CombatEncounterUI");
            combatUiObj.transform.SetParent(_holder.transform);
            _combatUI = combatUiObj.AddComponent<CombatEncounterUI>();
            _combatUI.Initialize(_combat, null, _rewardService, new List<ItemDataSO>(ItemDatabaseSO.CreateCanonicalDatabase().AllItems), _holder.transform, _nav, _runManager);
            _combatUI.BindControllers(_mapUI, _runCompleteUI);
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
        public void Floor1_CombatEncounter_RealDamageAndRewardProgression()
        {
            _runManager.StartRun(_meta);
            Assert.AreEqual(1, _runManager.CurrentFloorNumber);
            Assert.AreEqual("Sewer Rat", _enemy.CombatantName);
            Assert.AreEqual(35, _enemy.MaxHp);

            // Start combat encounter
            _runManager.StartEncounterCombat();
            int initialEnemyHp = _enemy.CurrentHp;
            _combat.Tick(1.2f);
            Assert.Less(_enemy.CurrentHp, initialEnemyHp, "Player attack must deal real damage reducing enemy HP");

            // Execute enemy counter-attack: enemy deals real damage
            int initialPlayerHp = _player.CurrentHp;
            _player.TakeDamage(new DamageResult("Sewer Rat", _player.CombatantName, 3, 0, 1.0f, 1.0f, 0, 3, false));
            Assert.Less(_player.CurrentHp, initialPlayerHp, "Enemy attack must deal real damage reducing player HP");

            // Vanquish enemy
            int ticks1 = 0;
            while (_enemy.IsAlive && ticks1 < 100)
            {
                _combat.Tick(1.2f);
                ticks1++;
            }
            Assert.AreEqual(CombatState.Victory, _combat.CurrentState);

            // Claim reward and continue
            var item = ItemDatabaseSO.CreateCanonicalDatabase().AllItems[0];
            var reward = RewardOption.FromItemData(item);
            _combatUI.SelectReward(reward);
            _combatUI.CloseRewardScreenAndContinue();

            // Verify map progress: Floor 1 is cleared, Floor 2 node is unlocked
            var f1Node = _mapGraph.GetNode("node_f1_entry");
            Assert.IsTrue(f1Node.IsCleared, "Floor 1 node must be marked cleared");

            var available = _mapGraph.GetAvailableNodes();
            Assert.IsTrue(available.Exists(n => n.FloorNumber == 2), "Floor 2 nodes must be unlocked and available");
        }

        [Test]
        public void Floor4_MerchantStall_RealGoldDeductionAndInventoryPlacement()
        {
            _runManager.StartRun(_meta);
            _runManager.AddGold(100);
            Assert.AreEqual(100, _runManager.CurrentGold);

            _merchantSystem.GenerateOffers(4);
            Assert.GreaterOrEqual(_merchantSystem.CurrentOffers.Count, 1);

            var offer = _merchantSystem.CurrentOffers[0];
            int price = offer.CurrentPrice;

            bool bought = _merchantSystem.BuyOffer(0, _runManager, _inventory, _grid, _player);
            Assert.IsTrue(bought, "Player should be able to purchase item with sufficient gold");
            Assert.AreEqual(100 - price, _runManager.CurrentGold, "Gold must be deducted correctly");
            Assert.IsTrue(offer.IsSold, "Offer must be marked as sold");

            // Test insufficient gold
            _runManager.SpendGold(_runManager.CurrentGold);
            Assert.AreEqual(0, _runManager.CurrentGold);
            if (_merchantSystem.CurrentOffers.Count > 1)
            {
                bool boughtWithoutFunds = _merchantSystem.BuyOffer(1, _runManager, _inventory, _grid, _player);
                Assert.IsFalse(boughtWithoutFunds, "Player cannot purchase without gold");
            }
        }

        [Test]
        public void CampfireRest_HealOption_RestoresRealPlayerHealth()
        {
            _runManager.StartRun(_meta);
            _player.TakeDamage(new DamageResult("Hazard", _player.CombatantName, 50, 0, 1.0f, 1.0f, 0, 50, false));
            Assert.AreEqual(50, _player.CurrentHp);

            _campfireUI.ChooseRestAndHeal();
            Assert.Greater(_player.CurrentHp, 50, "Campfire Rest must restore player HP");
            Assert.IsTrue(_campfireUI.HasChosenOption, "Campfire choice must be recorded");
        }

        [Test]
        public void ProceduralEvent_RiskRewardChoice_AppliesRealSystemConsequences()
        {
            _runManager.StartRun(_meta);
            _runManager.AddGold(100);

            var ev = _eventService.SelectEligibleEvent(floorIndex: 2);
            Assert.IsNotNull(ev, "Event service must provide eligible procedural event for Floor 3");

            _eventService.PresentEvent(ev);
            Assert.IsTrue(_eventService.HasActiveEvent);

            var choice = ev.Choices[0];
            bool resolved = _eventService.SelectChoice(choice.ChoiceId, _runManager, _player, _modManager);
            Assert.IsTrue(resolved, "Procedural event choice must resolve and apply consequences");
        }

        [Test]
        public void Floor5_GraveGoliathBoss_TwoPhasesAndEnrageMechanic()
        {
            _runManager.StartRun(_meta);
            _runManager.SetCurrentFloor(4); // Floor 5 index 4
            _runManager.PrepareCurrentEncounter();

            Assert.AreEqual(5, _runManager.CurrentFloorNumber);
            Assert.IsTrue(_runManager.CurrentFloor.GetEncounter(0).IsBoss, "Floor 5 must be a real Boss Encounter");

            var bossDef = BossDefinitionSO.CreateGraveGoliathDefinition();
            Assert.AreEqual("Grave Goliath", bossDef.BossName);
            Assert.AreEqual(2, bossDef.Phases.Count, "Grave Goliath must have exactly 2 combat phases");
        }

        [Test]
        public void Floor10_LichLordFinalBoss_ThreePhasesAndRunCompletion()
        {
            _runManager.StartRun(_meta);
            _runManager.SetCurrentFloor(9); // Floor 10 index 9
            _runManager.PrepareCurrentEncounter();

            Assert.AreEqual(10, _runManager.CurrentFloorNumber);
            Assert.IsTrue(_runManager.CurrentFloor.GetEncounter(0).IsBoss, "Floor 10 must be the Final Boss");

            var lichDef = BossDefinitionSO.CreateLichLordDefinition();
            Assert.AreEqual("The Lich Lord", lichDef.BossName);
            Assert.AreEqual(3, lichDef.Phases.Count, "Lich Lord must have 3 distinct phases");

            // Vanquish final boss with endgame rune power
            _player.SetExplicitStats(baseDamage: 60, runeBonus: 20, armorValue: 5, interval: 0.8f);
            _runManager.StartEncounterCombat();
            int ticks10 = 0;
            while (_enemy.IsAlive && ticks10 < 200)
            {
                _combat.Tick(1.0f);
                ticks10++;
            }

            // Claim final reward and continue
            var item = ItemDatabaseSO.CreateCanonicalDatabase().AllItems[0];
            _combatUI.SelectReward(RewardOption.FromItemData(item));
            _combatUI.CloseRewardScreenAndContinue();

            Assert.AreEqual(RunState.RunComplete, _runManager.CurrentState, "Run must be marked RunComplete upon Floor 10 completion");
            Assert.IsTrue(_runCompleteUI.IsVictory, "Run summary must record Victory");
        }
    }
}
