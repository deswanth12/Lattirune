using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combo;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Modifiers;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.Tests
{
    /// <summary>
    /// Master Milestone Verification Test Suite for Lattirune 1.1 Fun Update (TASK-050 through TASK-055).
    /// Audits the unified end-to-end interaction of Run Modifiers, Combo Engine, Procedural Events,
    /// Live Combat Simulation, In-Run Economy, and Encrypted Save Persistence.
    /// </summary>
    [TestFixture]
    public class Phase3Milestone11VerificationTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("Milestone11VerificationHolder");
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
        public void Lattirune11_CompleteEndToEndRunFlow_ExecutesWithHighFidelity()
        {
            // 1. Initialize Subsystems
            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.SetExplicitStats(baseDamage: 10, runeBonus: 5, armorValue: 2);

            var enemyObj = new GameObject("Enemy");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize(step: 0.05f, maxMult: 2.5f);

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, null, modManager, tracker);

            var runManager = _holder.AddComponent<RunManager>();
            runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                combat,
                null,
                player,
                enemy,
                null,
                null,
                modManager,
                tracker
            );

            var econObj = new GameObject("Economy");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<SimpleEconomyService>();
            economy.Initialize(startingGold: 20);

            var eventService = _holder.AddComponent<RunEventService>();
            eventService.Initialize(random: new SystemRandomSource(777));

            var eventTrigger = _holder.AddComponent<RunEventTrigger>();
            eventTrigger.Configure(1.0f, cadence: true);

            var eventPanel = _holder.AddComponent<RunEventMobilePanel>();
            var presenter = _holder.AddComponent<RunEventPresenter>();
            presenter.Initialize(runManager, eventService, eventTrigger, eventPanel, combat, economy, player, modManager, new SystemRandomSource(777));

            // 2. Start Run
            runManager.StartRun();
            Assert.AreEqual(RunState.FloorPreparing, runManager.CurrentState);
            Assert.AreEqual(1, runManager.CurrentFloorNumber);

            // 3. Resolve Procedural Event (Ancient Shrine -> Gain Sharpened Runes)
            var shrine = eventService.Database.GetEvent("event_ancient_shrine");
            Assert.IsNotNull(shrine);
            eventService.PresentEvent(shrine);
            bool eventResolved = eventService.SelectChoice("choice_shrine_touch", economy, player, modManager);
            Assert.IsTrue(eventResolved);
            Assert.IsTrue(modManager.HasModifier("mod_sharpened_runes"));

            // 4. Start Combat Encounter
            runManager.StartEncounterCombat();
            Assert.AreEqual(RunState.EncounterActive, runManager.CurrentState);
            Assert.AreEqual(CombatState.Fighting, combat.CurrentState);

            // 5. Fight & Accumulate Combo
            // Player deals damage: (10 base + 5 rune) * 1.15 (modifier) * 1.0 (combo) = ~17 DMG
            combat.Tick(1.5f);
            Assert.AreEqual(1, tracker.CurrentCombo);
            Assert.Greater(tracker.ComboMultiplier, 1.0f);

            // 6. Finish Enemy
            enemy.TakeDirectDamage(200);
            combat.Tick(0.1f); // Victory
            Assert.AreEqual(RunState.RewardSelection, runManager.CurrentState);

            // 7. Validate Economy & Combo Rewards
            Assert.Greater(runManager.CurrentGold, 0);

            // 8. Test Save/Load Persistence Roundtrip
            SaveData save = SaveData.CreateDefault();
            save.run = new SavedRunData(
                active: true,
                floorIdx: runManager.CurrentFloorIndex,
                encIdx: runManager.CurrentEncounterIndex,
                state: (int)runManager.CurrentState,
                modifierIds: modManager.ExportActiveModifierIds(),
                combo: tracker.HighestCombo
            );

            string json = SaveSerializer.SerializeToJson(save);
            Assert.IsNotNull(json);

            SaveData restored = SaveSerializer.DeserializeFromJson(json);
            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.version);
            Assert.Contains("mod_sharpened_runes", restored.run.activeModifierIds);
            Assert.AreEqual(tracker.HighestCombo, restored.run.highestCombo);
        }
    }
}
