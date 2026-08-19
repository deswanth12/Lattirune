using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.Tests
{
    /// <summary>
    /// Test suite for Meta-Progression, Campfire Hub, Blueprint Forge, and Embers Economy.
    /// Strictly verifies PLAN.md Section 12, Section 13, and Section 22.
    /// </summary>
    [TestFixture]
    public class MetaProgressionTests
    {
        private GameObject _holderObj;
        private MetaProgressionManager _metaManager;
        private BlueprintDatabaseSO _blueprintDb;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MetaProgressionTestHolder");
            _blueprintDb = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();
            _metaManager = _holderObj.AddComponent<MetaProgressionManager>();
            _metaManager.Initialize(_blueprintDb);
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        // ==========================================
        // 1. INITIAL STATE & EMBER WALLET
        // ==========================================

        [Test]
        public void MetaProgression_InitialState_ZeroEmbersAndStats()
        {
            Assert.AreEqual(0, _metaManager.EmbersBalance);
            Assert.AreEqual(0, _metaManager.TotalRunsAttempted);
            Assert.AreEqual(0, _metaManager.TotalBossClears);
            Assert.AreEqual(0, _metaManager.UnlockedBlueprintCount);
        }

        [Test]
        public void EmberWallet_AddAndSpend_UpdatesBalanceCorrectly()
        {
            _metaManager.AddEmbers(120);
            Assert.AreEqual(120, _metaManager.EmbersBalance);
            Assert.IsTrue(_metaManager.CanAfford(100));

            bool spent = _metaManager.SpendEmbers(50);
            Assert.IsTrue(spent);
            Assert.AreEqual(70, _metaManager.EmbersBalance);
        }

        [Test]
        public void EmberWallet_InsufficientEmbers_Rejected_NoNegativeBalance()
        {
            _metaManager.AddEmbers(30);

            bool spent = _metaManager.SpendEmbers(50);
            Assert.IsFalse(spent);
            Assert.AreEqual(30, _metaManager.EmbersBalance);
            Assert.IsFalse(_metaManager.CanAfford(50));
        }

        // ==========================================
        // 2. BLUEPRINT DATABASE VALIDATION
        // ==========================================

        [Test]
        public void BlueprintDatabase_ContainsCanonicalBlueprints()
        {
            Assert.IsNotNull(_blueprintDb);
            Assert.IsTrue(_blueprintDb.IsValid(out string error), error);
            Assert.GreaterOrEqual(_blueprintDb.TotalBlueprintCount, 10);

            string[] expectedIds = new string[]
            {
                "bp_shortbow",
                "bp_apprentice_wand",
                "bp_battleaxe",
                "bp_phalanx_spear",
                "bp_iron_tower_shield",
                "bp_spiked_buckler",
                "bp_chainmail_coat",
                "bp_ruby_ring",
                "bp_sapphire_ring",
                "bp_lucky_clover",
                "bp_rune_crossfire",
                "bp_rune_haste"
            };

            foreach (var id in expectedIds)
            {
                Assert.IsTrue(_blueprintDb.HasBlueprint(id), $"Database missing canonical blueprint ID: {id}");
            }
        }

        // ==========================================
        // 3. BLUEPRINT FORGE UNLOCKS
        // ==========================================

        [Test]
        public void BlueprintForge_SuccessfulUnlock_DeductsEmbers_AddsToUnlocked()
        {
            _metaManager.AddEmbers(100);
            var shortbowBp = _blueprintDb.GetBlueprint("bp_shortbow"); // Cost = 50

            bool unlocked = _metaManager.UnlockBlueprint(shortbowBp);
            Assert.IsTrue(unlocked);
            Assert.AreEqual(50, _metaManager.EmbersBalance);
            Assert.IsTrue(_metaManager.IsBlueprintUnlocked("bp_shortbow"));
            Assert.AreEqual(1, _metaManager.UnlockedBlueprintCount);
        }

        [Test]
        public void BlueprintForge_DuplicateUnlock_IsRejected()
        {
            _metaManager.AddEmbers(150);
            var shortbowBp = _blueprintDb.GetBlueprint("bp_shortbow"); // Cost = 50

            Assert.IsTrue(_metaManager.UnlockBlueprint(shortbowBp));
            Assert.AreEqual(100, _metaManager.EmbersBalance);

            // Attempt duplicate unlock
            bool duplicate = _metaManager.UnlockBlueprint(shortbowBp);
            Assert.IsFalse(duplicate);
            Assert.AreEqual(100, _metaManager.EmbersBalance); // No extra embers deducted
        }

        [Test]
        public void BlueprintForge_InsufficientEmbers_RejectsUnlock()
        {
            _metaManager.AddEmbers(30);
            var wandBp = _blueprintDb.GetBlueprint("bp_apprentice_wand"); // Cost = 60

            bool unlocked = _metaManager.UnlockBlueprint(wandBp);
            Assert.IsFalse(unlocked);
            Assert.AreEqual(30, _metaManager.EmbersBalance);
            Assert.IsFalse(_metaManager.IsBlueprintUnlocked("bp_apprentice_wand"));
        }

        // ==========================================
        // 4. CAMPFIRE META-HUB & LIFETIME STATS
        // ==========================================

        [Test]
        public void CampfireHub_LifetimeStats_RecordRunAndBossClear()
        {
            _metaManager.RecordRunAttempt();
            _metaManager.RecordRunAttempt();
            Assert.AreEqual(2, _metaManager.TotalRunsAttempted);

            _metaManager.RecordBossClear(100);
            Assert.AreEqual(1, _metaManager.TotalBossClears);
            Assert.AreEqual(100, _metaManager.EmbersBalance);
        }

        // ==========================================
        // 5. RUN VS META SEPARATION
        // ==========================================

        [Test]
        public void RunVsMetaSeparation_RunReset_PreservesPersistentEmbersAndBlueprints()
        {
            var runManager = _holderObj.AddComponent<RunManager>();
            runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                null,
                null,
                null,
                null
            );

            // In-run state
            runManager.StartRun();
            runManager.AddGold(100);

            // Meta state
            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprintById("bp_battleaxe");

            // Reset in-run state
            runManager.ResetRun();

            // Run state is wiped
            Assert.AreEqual(0, runManager.CurrentGold);

            // Meta state remains intact
            Assert.AreEqual(120, _metaManager.EmbersBalance); // 200 - 80 for battleaxe
            Assert.IsTrue(_metaManager.IsBlueprintUnlocked("bp_battleaxe"));
        }

        // ==========================================
        // 6. PERSISTENCE ROUNDTRIP (SaveVersion 1)
        // ==========================================

        [Test]
        public void Persistence_ExportAndImportMetaData_RoundtripsAccurately()
        {
            _metaManager.AddEmbers(350);
            _metaManager.UnlockBlueprintById("bp_shortbow");
            _metaManager.UnlockBlueprintById("bp_ruby_ring");
            _metaManager.RecordRunAttempt();
            _metaManager.RecordBossClear(100);

            SavedMetaData savedMeta = _metaManager.ExportMetaData();
            Assert.IsNotNull(savedMeta);
            Assert.AreEqual(_metaManager.EmbersBalance, savedMeta.embers);
            Assert.AreEqual(2, savedMeta.unlockedBlueprints.Count);
            Assert.AreEqual(1, savedMeta.totalBossClears);
            Assert.AreEqual(1, savedMeta.totalRunsAttempted);

            // Import into fresh manager
            GameObject freshObj = new GameObject("FreshMetaHolder");
            MetaProgressionManager freshManager = freshObj.AddComponent<MetaProgressionManager>();
            freshManager.Initialize(_blueprintDb);

            freshManager.ImportMetaData(savedMeta);

            Assert.AreEqual(savedMeta.embers, freshManager.EmbersBalance);
            Assert.IsTrue(freshManager.IsBlueprintUnlocked("bp_shortbow"));
            Assert.IsTrue(freshManager.IsBlueprintUnlocked("bp_ruby_ring"));
            Assert.AreEqual(1, freshManager.TotalBossClears);
            Assert.AreEqual(1, freshManager.TotalRunsAttempted);

            Object.DestroyImmediate(freshObj);
        }

        [Test]
        public void BlueprintDefinitionSO_Immutability_DoesNotMutateDuringForgeUnlocks()
        {
            var axeBp = _blueprintDb.GetBlueprint("bp_battleaxe");
            int originalCost = axeBp.EmberCost;
            string originalTarget = axeBp.TargetUnlockId;

            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprint(axeBp);

            Assert.AreEqual(originalCost, axeBp.EmberCost);
            Assert.AreEqual(originalTarget, axeBp.TargetUnlockId);
        }
    }
}
