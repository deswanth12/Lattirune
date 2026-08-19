using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Modifiers;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RunEventSystemTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("RunEventTestHolder");
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
        // 1. EVENT DEFINITIONS & DATABASE TESTS
        // ==========================================

        [Test]
        public void EventDefinition_CreationAndValidation_PassesForValidData()
        {
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize(
                "event_custom",
                "Custom Event",
                "Custom description.",
                RunEventType.Mystery,
                eventWeight: 10,
                minFloor: 1,
                maxFloor: 5,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice("c1", "Choice 1", "Desc 1", 0, 10)
                }
            );

            Assert.IsTrue(ev.IsValid(out string error), $"Validation failed: {error}");
            Assert.AreEqual("event_custom", ev.EventId);
            Assert.AreEqual(RunEventType.Mystery, ev.EventType);
            Assert.AreEqual(10, ev.Weight);
            Assert.AreEqual(1, ev.MinimumFloor);
            Assert.AreEqual(5, ev.MaximumFloor);
            Assert.AreEqual(1, ev.ChoiceCount);
        }

        [Test]
        public void EventDefinition_InvalidMetadata_IsProperlyRejected()
        {
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("", "No ID", "Desc", RunEventType.Mystery, 10, 1, 5, new List<RunEventChoice>());
            Assert.IsFalse(ev.IsValid(out _));

            // Min floor > max floor
            var ev2 = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev2.Initialize("ev2", "Title", "Desc", RunEventType.Mystery, 10, 8, 2, new List<RunEventChoice>
            {
                new RunEventChoice("c1", "Choice", "Desc")
            });
            // Auto-clamped in Initialize
            Assert.GreaterOrEqual(ev2.MaximumFloor, ev2.MinimumFloor);
        }

        [Test]
        public void CanonicalDatabase_ContainsAtLeastSixValidEvents()
        {
            var db = RunEventDatabaseSO.CreateCanonicalEventDatabase();
            Assert.GreaterOrEqual(db.Count, 6);

            string[] expectedEvents = new string[]
            {
                "event_ancient_shrine",
                "event_blood_altar",
                "event_cursed_treasury",
                "event_elemental_forge",
                "event_ember_well",
                "event_mysterious_chest"
            };

            foreach (var eventId in expectedEvents)
            {
                var ev = db.GetEvent(eventId);
                Assert.IsNotNull(ev, $"Missing canonical event '{eventId}'.");
                Assert.IsTrue(ev.IsValid(out string error), $"Event '{eventId}' failed validation: {error}");
                Assert.Greater(ev.ChoiceCount, 0);
            }
        }

        [Test]
        public void EventDatabase_FloorFiltering_RespectsFloorConstraints()
        {
            var db = RunEventDatabaseSO.CreateCanonicalEventDatabase();

            // Floor 0 (Dungeon Floor 1): Blood Altar is minFloor 2, so it shouldn't appear
            List<RunEventDefinitionSO> floor1Events = db.GetEligibleEventsForFloor(0);
            Assert.IsNull(floor1Events.Find(e => e.EventId == "event_blood_altar"));

            // Floor 1 (Dungeon Floor 2): Blood Altar should be eligible
            List<RunEventDefinitionSO> floor2Events = db.GetEligibleEventsForFloor(1);
            Assert.IsNotNull(floor2Events.Find(e => e.EventId == "event_blood_altar"));
        }

        // ==========================================
        // 2. DETERMINISTIC RNG & SELECTION
        // ==========================================

        [Test]
        public void EventService_DeterministicRNG_SameSeedProducesIdenticalSelections()
        {
            var service1 = _holder.AddComponent<RunEventService>();
            service1.Initialize(random: new SystemRandomSource(42));

            var service2 = _holder.AddComponent<RunEventService>();
            service2.Initialize(random: new SystemRandomSource(42));

            for (int floor = 0; floor < 10; floor++)
            {
                var ev1 = service1.SelectEligibleEvent(floor);
                var ev2 = service2.SelectEligibleEvent(floor);

                Assert.IsNotNull(ev1);
                Assert.IsNotNull(ev2);
                Assert.AreEqual(ev1.EventId, ev2.EventId, $"Divergence at floor {floor} with seed 42.");
            }
        }

        [Test]
        public void EventService_ZeroWeightEvents_AreIgnoredInSelection()
        {
            var db = ScriptableObject.CreateInstance<RunEventDatabaseSO>();
            var ev1 = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev1.Initialize("zero_weight", "Zero", "", RunEventType.Mystery, 0, 1, 10, new List<RunEventChoice>
            {
                new RunEventChoice("c1", "C", "D")
            });

            var ev2 = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev2.Initialize("has_weight", "Weight", "", RunEventType.Mystery, 10, 1, 10, new List<RunEventChoice>
            {
                new RunEventChoice("c2", "C", "D")
            });

            db.Initialize(new List<RunEventDefinitionSO> { ev1, ev2 });

            var service = _holder.AddComponent<RunEventService>();
            service.Initialize(db, new SystemRandomSource(100));

            for (int i = 0; i < 20; i++)
            {
                var selected = service.SelectEligibleEvent(1);
                Assert.IsNotNull(selected);
                Assert.AreEqual("has_weight", selected.EventId);
            }
        }

        // ==========================================
        // 3. PURE RESOLVER TESTS
        // ==========================================

        [Test]
        public void RunEventResolver_IsPure_DoesNotMutateInputs()
        {
            var db = RunEventDatabaseSO.CreateCanonicalEventDatabase();
            var shrine = db.GetEvent("event_ancient_shrine");
            var touchChoice = shrine.GetChoice("choice_shrine_touch");

            var activeMods = new List<string>();
            var consumedChoices = new List<string>();

            var result = RunEventResolver.ResolveChoice(
                shrine,
                touchChoice,
                currentGold: 50,
                currentHp: 100,
                maxHp: 100,
                activeModifierIds: activeMods,
                consumedChoiceIds: consumedChoices
            );

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, activeMods.Count); // Unchanged
            Assert.AreEqual(0, consumedChoices.Count); // Unchanged
            Assert.Contains("mod_sharpened_runes", (List<string>)result.GrantedModifierIds);
        }

        [Test]
        public void RunEventResolver_GoldCostAndReward_CalculatesAccurateDelta()
        {
            var choice = new RunEventChoice("test_gold", "Gold Trade", "Desc", costGold: 20, rewardGold: 50);
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("test_ev", "Title", "Desc", RunEventType.GoldReward, 10, 1, 10, new List<RunEventChoice> { choice });

            var result = RunEventResolver.ResolveChoice(ev, choice, currentGold: 30, currentHp: 100, maxHp: 100, activeModifierIds: null);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(30, result.GoldDelta); // 50 reward - 20 cost = +30
        }

        [Test]
        public void RunEventResolver_InsufficientGold_ReturnsFailure()
        {
            var choice = new RunEventChoice("test_expensive", "Costly", "Desc", costGold: 50);
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("test_ev", "Title", "Desc", RunEventType.GoldReward, 10, 1, 10, new List<RunEventChoice> { choice });

            var result = RunEventResolver.ResolveChoice(ev, choice, currentGold: 20, currentHp: 100, maxHp: 100, activeModifierIds: null);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.FailureReason.Contains("Insufficient gold"));
        }

        [Test]
        public void RunEventResolver_HealthSacrificeAndLethalRejection()
        {
            var choice = new RunEventChoice("test_sacrifice", "Sacrifice", "Desc", costHpPct: 0.25f);
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("test_ev", "Title", "Desc", RunEventType.HealthTrade, 10, 1, 10, new List<RunEventChoice> { choice });

            // Max HP 100 -> Cost is 25 HP. Hero has 50 HP -> Survives (remaining 25)
            var validResult = RunEventResolver.ResolveChoice(ev, choice, currentGold: 0, currentHp: 50, maxHp: 100, activeModifierIds: null);
            Assert.IsTrue(validResult.IsSuccess);
            Assert.AreEqual(-25, validResult.HpDelta);

            // Hero has 20 HP -> Cannot survive 25 HP sacrifice
            var lethalResult = RunEventResolver.ResolveChoice(ev, choice, currentGold: 0, currentHp: 20, maxHp: 100, activeModifierIds: null);
            Assert.IsFalse(lethalResult.IsSuccess);
            Assert.IsTrue(lethalResult.FailureReason.Contains("Hero health is too low"));
        }

        [Test]
        public void RunEventResolver_HealthRestoration_CalculatesPositiveHpDelta()
        {
            var choice = new RunEventChoice("test_heal", "Rest", "Desc", restoreHpPct: 0.35f);
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("test_ev", "Title", "Desc", RunEventType.Healing, 10, 1, 10, new List<RunEventChoice> { choice });

            var result = RunEventResolver.ResolveChoice(ev, choice, currentGold: 0, currentHp: 50, maxHp: 100, activeModifierIds: null);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(35, result.HpDelta);
        }

        [Test]
        public void RunEventResolver_DuplicateModifier_IsRejected()
        {
            var choice = new RunEventChoice("test_mod", "Gain Mod", "Desc", grantModId: "mod_sharpened_runes");
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("test_ev", "Title", "Desc", RunEventType.ModifierReward, 10, 1, 10, new List<RunEventChoice> { choice });

            var activeMods = new HashSet<string> { "mod_sharpened_runes" };
            var result = RunEventResolver.ResolveChoice(ev, choice, currentGold: 0, currentHp: 100, maxHp: 100, activeModifierIds: activeMods);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.FailureReason.Contains("already has the granted modifier"));
        }

        [Test]
        public void RunEventResolver_OneTimeUseChoice_IsRejectedIfAlreadyConsumed()
        {
            var choice = new RunEventChoice("test_onetime", "One Time", "Desc", oneTime: true);
            var ev = ScriptableObject.CreateInstance<RunEventDefinitionSO>();
            ev.Initialize("test_ev", "Title", "Desc", RunEventType.Mystery, 10, 1, 10, new List<RunEventChoice> { choice });

            var consumed = new HashSet<string> { "test_onetime" };
            var result = RunEventResolver.ResolveChoice(ev, choice, currentGold: 0, currentHp: 100, maxHp: 100, activeModifierIds: null, consumedChoiceIds: consumed);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.FailureReason.Contains("already been consumed"));
        }

        // ==========================================
        // 4. EVENT SERVICE & TRANSACTION APPLICATION
        // ==========================================

        [Test]
        public void EventService_FullTransactionExecution_AppliesOutcomesAccurately()
        {
            var service = _holder.AddComponent<RunEventService>();
            service.Initialize();

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var econObj = new GameObject("Economy");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<SimpleEconomyService>();
            economy.Initialize(startingGold: 50);

            // Present Ancient Shrine
            var shrine = service.Database.GetEvent("event_ancient_shrine");
            service.PresentEvent(shrine);
            Assert.IsTrue(service.HasActiveEvent);

            // Select Touch Rune choice
            bool executed = service.SelectChoice("choice_shrine_touch", economy, player, modManager);
            Assert.IsTrue(executed);
            Assert.IsFalse(service.HasActiveEvent); // Cleared after resolution
            Assert.IsTrue(modManager.HasModifier("mod_sharpened_runes"));
            Assert.IsTrue(service.ConsumedChoiceIds.Contains("choice_shrine_touch"));
        }

        [Test]
        public void EventService_CursedTreasury_AppliesGoldAndCurseModifier()
        {
            var service = _holder.AddComponent<RunEventService>();
            service.Initialize();

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var econObj = new GameObject("Economy");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<SimpleEconomyService>();
            economy.Initialize(startingGold: 10);

            var treasury = service.Database.GetEvent("event_cursed_treasury");
            service.PresentEvent(treasury);

            bool executed = service.SelectChoice("choice_treasury_pillage", economy, player, modManager);
            Assert.IsTrue(executed);
            Assert.AreEqual(85, economy.GoldBalance); // 10 + 75 = 85
            Assert.IsTrue(modManager.HasModifier("mod_curse_vulnerability"));
        }

        // ==========================================
        // 5. SAVE COMPATIBILITY & PERSISTENCE
        // ==========================================

        [Test]
        public void SaveCompatibility_SaveVersionRemainsOne()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
        }

        [Test]
        public void SaveCompatibility_LegacySave_LoadsWithoutEventSystemErrors()
        {
            string legacyJson = @"{
                ""version"": 1,
                ""timestamp"": ""2026-08-19T12:00:00Z"",
                ""items"": [],
                ""runes"": [],
                ""run"": {
                    ""hasActiveRun"": true,
                    ""currentFloorIndex"": 1,
                    ""currentEncounterIndex"": 0,
                    ""runState"": 1
                },
                ""inventory"": { ""expansionStep"": 0, ""unlockedX"": [], ""unlockedY"": [] },
                ""meta"": { ""embers"": 50, ""unlockedBlueprints"": [], ""totalBossClears"": 0, ""totalRunsAttempted"": 1 },
                ""settings"": { ""masterVolume"": 1.0, ""sfxVolume"": 1.0, ""isMuted"": false, ""hapticsEnabled"": true }
            }";

            SaveData data = SaveSerializer.DeserializeFromJson(legacyJson);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data.version);
            Assert.AreEqual(1, data.run.currentFloorIndex);
            Assert.IsNotNull(data.run.activeModifierIds);
        }
    }
}
