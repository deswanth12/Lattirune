using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Choices;
using Lattirune.Combat;
using Lattirune.Combo;
using Lattirune.Core;
using Lattirune.Economy;
using Lattirune.Modifiers;
using Lattirune.Progression;
using Lattirune.Reactions;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class FunSystemsFoundationTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""FunSystemsTestHolder"");
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
        // 1. RUN MODIFIER TESTS
        // ==========================================

        [Test]
        public void RunModifier_CreationAndProperties_MatchDefinition()
        {
            var mod = ScriptableObject.CreateInstance<RunModifierDefinitionSO>();
            mod.Initialize(
                ""mod_test"",
                ""Test Modifier"",
                ""Increases damage by 20%"",
                RunModifierRarity.Rare,
                RunModifierPolarity.Positive,
                RunModifierType.DamageMultiplier,
                0.20f,
                Color.red
            );

            Assert.AreEqual(""mod_test"", mod.ModifierId);
            Assert.AreEqual(""Test Modifier"", mod.DisplayName);
            Assert.AreEqual(""Increases damage by 20%"", mod.Description);
            Assert.AreEqual(RunModifierRarity.Rare, mod.Rarity);
            Assert.AreEqual(RunModifierPolarity.Positive, mod.Polarity);
            Assert.AreEqual(RunModifierType.DamageMultiplier, mod.ModifierType);
            Assert.AreEqual(0.20f, mod.EffectValue);
            Assert.AreEqual(Color.red, mod.IconColor);
        }

        [Test]
        public void RunModifier_PolarityAndRarity_AreClassifiedCorrectly()
        {
            var db = RunModifierDatabaseSO.CreateCanonicalDatabase();
            Assert.GreaterOrEqual(db.Count, 5);

            var sharpened = db.GetModifier(""mod_sharpened_runes"");
            Assert.IsNotNull(sharpened);
            Assert.AreEqual(RunModifierPolarity.Positive, sharpened.Polarity);
            Assert.AreEqual(RunModifierRarity.Common, sharpened.Rarity);

            var glassCannon = db.GetModifier(""mod_glass_cannon"");
            Assert.IsNotNull(glassCannon);
            Assert.AreEqual(RunModifierPolarity.Hybrid, glassCannon.Polarity);
            Assert.AreEqual(RunModifierRarity.Epic, glassCannon.Rarity);

            var curse = db.GetModifier(""mod_curse_vulnerability"");
            Assert.IsNotNull(curse);
            Assert.AreEqual(RunModifierPolarity.Negative, curse.Polarity);
            Assert.AreEqual(RunModifierRarity.Curse, curse.Rarity);
        }

        [Test]
        public void RunModifierManager_AddRemoveAndDuplicatePrevention()
        {
            var manager = _holder.AddComponent<RunModifierManager>();
            manager.Initialize();

            var db = RunModifierDatabaseSO.CreateCanonicalDatabase();
            var mod1 = db.GetModifier(""mod_sharpened_runes"");
            var mod2 = db.GetModifier(""mod_midas_touch"");

            Assert.IsTrue(manager.AddModifier(mod1));
            Assert.AreEqual(1, manager.ActiveCount);
            Assert.IsTrue(manager.HasModifier(""mod_sharpened_runes""));

            // Duplicate prevention
            Assert.IsFalse(manager.AddModifier(mod1));
            Assert.AreEqual(1, manager.ActiveCount);

            // Add second
            Assert.IsTrue(manager.AddModifier(mod2));
            Assert.AreEqual(2, manager.ActiveCount);

            // Remove
            Assert.IsTrue(manager.RemoveModifier(""mod_sharpened_runes""));
            Assert.AreEqual(1, manager.ActiveCount);
            Assert.IsFalse(manager.HasModifier(""mod_sharpened_runes""));

            // Remove non-existent
            Assert.IsFalse(manager.RemoveModifier(""mod_non_existent""));
        }

        [Test]
        public void RunModifierManager_AggregateMultiplier_CalculatesCorrectly()
        {
            var manager = _holder.AddComponent<RunModifierManager>();
            manager.Initialize();

            var mod1 = ScriptableObject.CreateInstance<RunModifierDefinitionSO>();
            mod1.Initialize(""mod1"", ""Buff 1"", """", RunModifierRarity.Common, RunModifierPolarity.Positive, RunModifierType.DamageMultiplier, 0.15f);

            var mod2 = ScriptableObject.CreateInstance<RunModifierDefinitionSO>();
            mod2.Initialize(""mod2"", ""Buff 2"", """", RunModifierRarity.Rare, RunModifierPolarity.Positive, RunModifierType.DamageMultiplier, 0.25f);

            manager.AddModifier(mod1);
            manager.AddModifier(mod2);

            // Base 1.0 + 0.15 + 0.25 = 1.40
            float aggregate = manager.GetAggregateMultiplier(RunModifierType.DamageMultiplier, 1.0f);
            Assert.AreEqual(1.40f, aggregate, 0.001f);

            // Unrelated modifier type returns base
            float goldMult = manager.GetAggregateMultiplier(RunModifierType.GoldMultiplier, 1.0f);
            Assert.AreEqual(1.0f, goldMult, 0.001f);
        }

        // ==========================================
        // 2. COMBO TRACKER TESTS
        // ==========================================

        [Test]
        public void ComboTracker_IncrementAndHighestCombo_TracksCorrectly()
        {
            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize(step: 0.10f, maxMult: 3.0f, timeout: 5.0f);

            Assert.AreEqual(0, tracker.CurrentCombo);
            Assert.AreEqual(0, tracker.HighestCombo);
            Assert.AreEqual(1.0f, tracker.ComboMultiplier, 0.001f);

            tracker.RecordHit();
            Assert.AreEqual(1, tracker.CurrentCombo);
            Assert.AreEqual(1, tracker.HighestCombo);
            Assert.AreEqual(1.10f, tracker.ComboMultiplier, 0.001f);

            tracker.RecordHit();
            tracker.RecordHit();
            Assert.AreEqual(3, tracker.CurrentCombo);
            Assert.AreEqual(3, tracker.HighestCombo);
            Assert.AreEqual(1.30f, tracker.ComboMultiplier, 0.001f);

            tracker.ResetCombo();
            Assert.AreEqual(0, tracker.CurrentCombo);
            Assert.AreEqual(3, tracker.HighestCombo); // Highest preserved
            Assert.AreEqual(1.0f, tracker.ComboMultiplier, 0.001f);
        }

        [Test]
        public void ComboTracker_ReactionChain_TracksConsecutiveReactions()
        {
            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize();

            tracker.RecordReaction();
            tracker.RecordReaction();

            Assert.AreEqual(2, tracker.ConsecutiveReactions);
            Assert.AreEqual(2, tracker.CurrentCombo);

            tracker.ResetCombo();
            Assert.AreEqual(0, tracker.ConsecutiveReactions);
            Assert.AreEqual(0, tracker.CurrentCombo);
        }

        [Test]
        public void ComboTracker_Timeout_ResetsComboDeterministically()
        {
            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize(timeout: 2.0f);

            tracker.RecordHit();
            tracker.RecordHit();
            Assert.AreEqual(2, tracker.CurrentCombo);

            tracker.UpdateTimer(1.0f);
            Assert.AreEqual(2, tracker.CurrentCombo); // Not yet timed out

            tracker.UpdateTimer(1.5f); // Total 2.5s >= 2.0s
            Assert.AreEqual(0, tracker.CurrentCombo); // Timed out
            Assert.AreEqual(2, tracker.HighestCombo);
        }

        // ==========================================
        // 3. CHAIN REACTION REWARDS
        // ==========================================

        [Test]
        public void ChainReactionRewardCalculator_CalculatesTiersPurely()
        {
            // Low score
            var r0 = ChainReactionRewardCalculator.CalculateReward(comboDepth: 1, reactionChainDepth: 0);
            Assert.AreEqual(""Standard"", r0.TierName);
            Assert.AreEqual(0, r0.BonusGold);
            Assert.AreEqual(0, r0.BonusEmbers);

            // Minor Synergy Surge (score >= 4)
            var r1 = ChainReactionRewardCalculator.CalculateReward(comboDepth: 4, reactionChainDepth: 0);
            Assert.AreEqual(""Synergy Surge"", r1.TierName);
            Assert.Greater(r1.BonusGold, 0);

            // Greater Chain (score >= 10, e.g. 4 combo + 2 reactions = 4 + 6 = 10)
            var r2 = ChainReactionRewardCalculator.CalculateReward(comboDepth: 4, reactionChainDepth: 2);
            Assert.AreEqual(""Greater Chain"", r2.TierName);
            Assert.AreEqual(2, r2.BonusEmbers);

            // Legendary Cascade (score >= 20, e.g. 5 combo + 5 reactions = 5 + 15 = 20)
            var r3 = ChainReactionRewardCalculator.CalculateReward(comboDepth: 5, reactionChainDepth: 5);
            Assert.AreEqual(""Legendary Cascade"", r3.TierName);
            Assert.AreEqual(5, r3.BonusEmbers);
            Assert.AreEqual(0.50f, r3.QualityUpgradeChance, 0.001f);
        }

        // ==========================================
        // 4. RISK / REWARD CHOICES
        // ==========================================

        [Test]
        public void RunChoiceService_ValidationAndExecution()
        {
            var choiceService = _holder.AddComponent<RunChoiceService>();
            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var econObj = new GameObject(""Economy"");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<EconomyManager>();
            economy.Initialize(startingGold: 50);

            var db = RunChoiceDatabaseSO.CreateCanonicalChoiceDatabase();
            var bloodPact = db.GetChoice(""choice_blood_pact"");
            Assert.IsNotNull(bloodPact);

            // Apply Blood Pact (costs 20% HP = 20 HP, grants Sharpened Runes)
            bool applied = choiceService.ApplyChoice(bloodPact, economy, player, modManager);
            Assert.IsTrue(applied);
            Assert.AreEqual(80, player.CurrentHp);
            Assert.IsTrue(modManager.HasModifier(""mod_sharpened_runes""));

            // Second attempt is rejected because one-time use
            bool second = choiceService.ApplyChoice(bloodPact, economy, player, modManager);
            Assert.IsFalse(second);
        }

        [Test]
        public void RunChoiceService_InsufficientGold_RejectsChoice()
        {
            var choiceService = _holder.AddComponent<RunChoiceService>();
            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var econObj = new GameObject(""Economy"");
            econObj.transform.SetParent(_holder.transform);
            var economy = econObj.AddComponent<EconomyManager>();
            economy.Initialize(startingGold: 10); // Has 10, choice needs 30

            var db = RunChoiceDatabaseSO.CreateCanonicalChoiceDatabase();
            var transmutation = db.GetChoice(""choice_alchemical_transmutation"");

            bool applied = choiceService.ApplyChoice(transmutation, economy, player, modManager);
            Assert.IsFalse(applied);
            Assert.AreEqual(10, economy.GoldBalance); // Untouched
            Assert.IsFalse(modManager.HasModifier(""mod_elemental_surge""));
        }

        // ==========================================
        // 5. SAVE COMPATIBILITY & PERSISTENCE
        // ==========================================

        [Test]
        public void SaveCompatibility_SavedRunData_RoundtripWithModifiersAndCombo()
        {
            SaveData original = SaveData.CreateDefault();
            original.run = new SavedRunData(
                active: true,
                floorIdx: 3,
                encIdx: 2,
                state: 1,
                modifierIds: new List<string> { ""mod_sharpened_runes"", ""mod_midas_touch"" },
                combo: 14
            );

            string json = SaveSerializer.SerializeToJson(original);
            Assert.IsNotNull(json);

            SaveData restored = SaveSerializer.DeserializeFromJson(json);
            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.version);
            Assert.AreEqual(3, restored.run.currentFloorIndex);
            Assert.AreEqual(14, restored.run.highestCombo);
            Assert.AreEqual(2, restored.run.activeModifierIds.Count);
            Assert.Contains(""mod_sharpened_runes"", restored.run.activeModifierIds);
            Assert.Contains(""mod_midas_touch"", restored.run.activeModifierIds);
        }

        [Test]
        public void SaveCompatibility_LegacySave_LoadsWithSafeDefaults()
        {
            // Simulate legacy JSON produced in MVP 1.0 without activeModifierIds and highestCombo
            string legacyJson = @""{
                \""version\"": 1,
                \""timestamp\"": \""2026-08-19T12:00:00Z\"",
                \""items\"": [],
                \""runes\"": [],
                \""run\"": {
                    \""hasActiveRun\"": true,
                    \""currentFloorIndex\"": 2,
                    \""currentEncounterIndex\"": 1,
                    \""runState\"": 1
                },
                \""inventory\"": { \""expansionStep\"": 0, \""unlockedX\"": [], \""unlockedY\"": [] },
                \""meta\"": { \""embers\"": 100, \""unlockedBlueprints\"": [], \""totalBossClears\"": 1, \""totalRunsAttempted\"": 3 },
                \""settings\"": { \""masterVolume\"": 1.0, \""sfxVolume\"": 1.0, \""isMuted\"": false, \""hapticsEnabled\"": true }
            }"";

            SaveData legacySave = SaveSerializer.DeserializeFromJson(legacyJson);
            Assert.IsNotNull(legacySave);
            Assert.AreEqual(1, legacySave.version);
            Assert.AreEqual(2, legacySave.run.currentFloorIndex);
            Assert.IsNotNull(legacySave.run.activeModifierIds);
            Assert.AreEqual(0, legacySave.run.activeModifierIds.Count);
            Assert.AreEqual(0, legacySave.run.highestCombo);
        }

        [Test]
        public void MetaProgression_ImmutabilityFromTemporaryRunModifiers()
        {
            var metaManager = _holder.AddComponent<MetaProgressionManager>();
            metaManager.Initialize();
            metaManager.AddEmbers(50);
            int originalEmbers = metaManager.EmbersBalance;

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();
            modManager.AddModifierById(""mod_sharpened_runes"");
            modManager.AddModifierById(""mod_midas_touch"");

            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize();
            tracker.RecordHit();
            tracker.RecordReaction();

            // Permanent progression balance remains unchanged
            Assert.AreEqual(originalEmbers, metaManager.EmbersBalance);
            Assert.AreEqual(0, metaManager.TotalRunsAttempted);
        }
    }
}
