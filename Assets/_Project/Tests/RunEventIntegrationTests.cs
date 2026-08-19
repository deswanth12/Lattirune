using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Modifiers;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RunEventIntegrationTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""RunEventIntegrationTestHolder"");
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
        }

        // ==========================================
        // 1. COMBAT ISOLATION & TRIGGER TESTS
        // ==========================================

        [Test]
        public void EventTrigger_NeverTriggersDuringActiveCombat()
        {
            var triggerObj = new GameObject(""Trigger"");
            triggerObj.transform.SetParent(_holder.transform);
            var trigger = triggerObj.AddComponent<RunEventTrigger>();
            trigger.Configure(1.0f, cadence: true); // 100% chance

            var combatObj = new GameObject(""Combat"");
            combatObj.transform.SetParent(_holder.transform);
            var combat = combatObj.AddComponent<CombatSystem>();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var enemyObj = new GameObject(""Enemy"");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(50, 0, 5, 1.5f);

            combat.Initialize(player, enemy);
            combat.StartCombat();
            Assert.AreEqual(CombatState.Fighting, combat.CurrentState);

            // While Fighting, trigger MUST return false
            bool shouldTrigger = trigger.ShouldTriggerEvent(floorIndex: 1, encounterIndex: 0, combat, new SystemRandomSource(42));
            Assert.IsFalse(shouldTrigger, ""Event trigger fired while CombatSystem was in Fighting state!"");

            combat.ResetCombat();
            Assert.AreEqual(CombatState.Preparing, combat.CurrentState);

            // When Preparing, floor 2 is guaranteed cadence -> returns true
            bool triggerAfterCombat = trigger.ShouldTriggerEvent(floorIndex: 1, encounterIndex: 0, combat, new SystemRandomSource(42));
            Assert.IsTrue(triggerAfterCombat);
        }

        [Test]
        public void EventTrigger_ExcludesMerchantCampfireAndBossFloors()
        {
            var triggerObj = new GameObject(""Trigger"");
            triggerObj.transform.SetParent(_holder.transform);
            var trigger = triggerObj.AddComponent<RunEventTrigger>();
            trigger.Configure(1.0f, cadence: true);

            var rng = new SystemRandomSource(100);

            // Floor 3 (Index 3 -> Dungeon Floor 4 = Merchant) -> False
            Assert.IsFalse(trigger.ShouldTriggerEvent(3, 0, null, rng));

            // Floor 7 (Index 7 -> Dungeon Floor 8 = Campfire) -> False
            Assert.IsFalse(trigger.ShouldTriggerEvent(7, 0, null, rng));

            // Floor 8 (Index 8 -> Dungeon Floor 9 = Merchant) -> False
            Assert.IsFalse(trigger.ShouldTriggerEvent(8, 0, null, rng));

            // Floor 9 (Index 9 -> Dungeon Floor 10 = Final Boss) -> False
            Assert.IsFalse(trigger.ShouldTriggerEvent(9, 0, null, rng));
        }

        // ==========================================
        // 2. PRESENTER & UI INTEGRATION TESTS
        // ==========================================

        [Test]
        public void RunEventPresenter_PausesRunAndPresentsEligibleEvent()
        {
            var managerObj = new GameObject(""RunManager"");
            managerObj.transform.SetParent(_holder.transform);
            var runManager = managerObj.AddComponent<RunManager>();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var enemyObj = new GameObject(""Enemy"");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(50, 0, 5, 1.5f);

            var combatObj = new GameObject(""Combat"");
            combatObj.transform.SetParent(_holder.transform);
            var combat = combatObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            runManager.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), combat, null, player, enemy);
            runManager.StartRun();

            var service = _holder.AddComponent<RunEventService>();
            service.Initialize(random: new SystemRandomSource(42));

            var trigger = _holder.AddComponent<RunEventTrigger>();
            trigger.Configure(1.0f, true);

            var panel = _holder.AddComponent<RunEventMobilePanel>();
            var econObj = new GameObject(""Economy"");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<EconomyManager>();
            economy.Initialize(50);

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var presenter = _holder.AddComponent<RunEventPresenter>();
            presenter.Initialize(runManager, service, trigger, panel, combat, economy, player, modManager, new SystemRandomSource(42));

            // Trigger event on floor 1 (Dungeon Floor 2)
            bool triggered = presenter.TryTriggerBetweenEncounterEvent(floorIndex: 1, encounterIndex: 0);
            Assert.IsTrue(triggered);
            Assert.IsTrue(presenter.IsHandlingEvent);
            Assert.IsTrue(panel.IsVisible);
            Assert.AreEqual(RunState.EventActive, runManager.CurrentState);
            Assert.IsNotNull(panel.ActiveEvent);

            // Dismiss panel -> resumes run to FloorPreparing
            panel.Hide();
            Assert.IsFalse(presenter.IsHandlingEvent);
            Assert.IsFalse(panel.IsVisible);
            Assert.AreEqual(RunState.FloorPreparing, runManager.CurrentState);
        }

        [Test]
        public void RunEventMobilePanel_ChoiceExecution_UpdatesResourcesAndModifiersImmediately()
        {
            var service = _holder.AddComponent<RunEventService>();
            service.Initialize();

            var panel = _holder.AddComponent<RunEventMobilePanel>();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var econObj = new GameObject(""Economy"");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<EconomyManager>();
            economy.Initialize(30);

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            panel.Initialize(service, economy, player, modManager);

            // Show Elemental Forge (costs 30 Gold, grants mod_elemental_surge)
            var forge = service.Database.GetEvent(""event_elemental_forge"");
            Assert.IsNotNull(forge);
            service.PresentEvent(forge);
            panel.Show(forge);

            // Simulate selecting "Infuse Runes"
            bool success = service.SelectChoice(""choice_forge_infuse"", economy, player, modManager);
            Assert.IsTrue(success);
            Assert.AreEqual(0, economy.GoldBalance); // 30 - 30 = 0
            Assert.IsTrue(modManager.HasModifier(""mod_elemental_surge""));

            panel.SetOutcomeFeedback(""Outcome: Infuse Runes applied."", resolved: true);
            Assert.IsTrue(panel.IsResolved);

            // Continue exploration
            panel.Hide();
            Assert.IsFalse(panel.IsVisible);
        }

        // ==========================================
        // 3. SEED REPRODUCIBILITY & SAVE COMPATIBILITY
        // ==========================================

        [Test]
        public void EventPresenter_ReproducibleEventSequenceWithSameSeed()
        {
            var db = RunEventDatabaseSO.CreateCanonicalEventDatabase();

            var serviceA = _holder.AddComponent<RunEventService>();
            serviceA.Initialize(db, new SystemRandomSource(9999));

            var serviceB = _holder.AddComponent<RunEventService>();
            serviceB.Initialize(db, new SystemRandomSource(9999));

            for (int f = 0; f < 5; f++)
            {
                var evA = serviceA.SelectEligibleEvent(f);
                var evB = serviceB.SelectEligibleEvent(f);
                Assert.IsNotNull(evA);
                Assert.IsNotNull(evB);
                Assert.AreEqual(evA.EventId, evB.EventId);
            }
        }

        [Test]
        public void SaveCompatibility_SaveVersionRemainsOneAndLegacyLoadsSafely()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);

            string legacyJson = @""{
                \""version\"": 1,
                \""timestamp\"": \""2026-08-19T14:00:00Z\"",
                \""items\"": [],
                \""runes\"": [],
                \""run\"": {
                    \""hasActiveRun\"": true,
                    \""currentFloorIndex\"": 3,
                    \""currentEncounterIndex\"": 0,
                    \""runState\"": 1
                },
                \""inventory\"": { \""expansionStep\"": 0, \""unlockedX\"": [], \""unlockedY\"": [] },
                \""meta\"": { \""embers\"": 20, \""unlockedBlueprints\"": [], \""totalBossClears\"": 0, \""totalRunsAttempted\"": 2 },
                \""settings\"": { \""masterVolume\"": 1.0, \""sfxVolume\"": 1.0, \""isMuted\"": false, \""hapticsEnabled\"": true }
            }"";

            SaveData loaded = SaveSerializer.DeserializeFromJson(legacyJson);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.version);
            Assert.AreEqual(3, loaded.run.currentFloorIndex);
            Assert.IsNotNull(loaded.run.activeModifierIds);
        }
    }
}
