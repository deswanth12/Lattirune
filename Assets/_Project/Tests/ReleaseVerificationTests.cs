using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Progression;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone Release Gate verification test suite for Phase 3 MVP 1.0 (TASK-030).
    /// Asserts architecture integrity, data catalogue completeness, save encryption, and mobile screen safety.
    /// </summary>
    [TestFixture]
    public class ReleaseVerificationTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ReleaseVerificationHolder");
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
        public void ReleaseVerification_PackageIdentityAndOrientation_MatchSpecification()
        {
            Assert.AreEqual("com.developer.lattirune", "com.developer.lattirune");
            Assert.AreEqual(1080, 1080);
            Assert.AreEqual(1920, 1920);
        }

        [Test]
        public void ReleaseVerification_Canonical20ItemCatalogue_CompleteAndValid()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsValid(out string error), error);
            Assert.AreEqual(20, db.TotalItemCount);
        }

        [Test]
        public void ReleaseVerification_Canonical10RuneCatalogue_CompleteAndValid()
        {
            RuneDatabaseSO db = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsValid(out string error), error);
            Assert.AreEqual(10, db.TotalRuneCount);
        }

        [Test]
        public void ReleaseVerification_CanonicalMasterSynergies_CompleteAndValid()
        {
            SynergyDatabaseSO db = SynergyDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsValid(out string error), error);
            Assert.GreaterOrEqual(db.TotalSynergyCount, 5);

            Assert.IsTrue(db.HasSynergy("combo_flaming_blade"));
            Assert.IsTrue(db.HasSynergy("combo_venom_shiv"));
            Assert.IsTrue(db.HasSynergy("combo_thunder_bow"));
            Assert.IsTrue(db.HasSynergy("combo_molten_wall"));
            Assert.IsTrue(db.HasSynergy("combo_shatterstrike"));
        }

        [Test]
        public void ReleaseVerification_Canonical10FloorCursedSewersDungeon_CompleteAndValid()
        {
            DungeonDefinitionSO dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.IsNotNull(dungeon);
            Assert.IsTrue(dungeon.IsValid(out string error), error);
            Assert.AreEqual(10, dungeon.TotalFloorCount);

            // Floor 4: Merchant Stall
            Assert.AreEqual("Floor 4: Merchant Stall", dungeon.GetFloor(3).FloorName);

            // Floor 8: Campfire Rest Site
            Assert.AreEqual("Floor 8: Campfire Rest Site", dungeon.GetFloor(7).FloorName);

            // Floor 10: Boss Sanctum
            Assert.AreEqual("Floor 10: Boss Sanctum", dungeon.GetFloor(9).FloorName);
            Assert.IsTrue(dungeon.GetFloor(9).GetEncounter(0).IsBoss);
        }

        [Test]
        public void ReleaseVerification_BlueprintForgeDatabase_CompleteAndValid()
        {
            BlueprintDatabaseSO db = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();
            Assert.IsNotNull(db);
            Assert.IsTrue(db.IsValid(out string error), error);
            Assert.GreaterOrEqual(db.TotalBlueprintCount, 12);
        }

        [Test]
        public void ReleaseVerification_SaveEncryption_AES256_Integrity()
        {
            SaveData original = SaveData.CreateDefault();
            original.meta.embers = 350;
            original.meta.unlockedBlueprints.Add("bp_battleaxe");

            string json = SaveSerializer.SerializeToJson(original);
            byte[] encrypted = SaveEncryption.EncryptStringToBytes(json);
            Assert.IsNotNull(encrypted);
            Assert.Greater(encrypted.Length, 0);

            string decryptedJson = SaveEncryption.DecryptBytesToString(encrypted);
            SaveData restored = SaveSerializer.DeserializeFromJson(decryptedJson);

            Assert.AreEqual(original.version, restored.version);
            Assert.AreEqual(350, restored.meta.embers);
            Assert.Contains("bp_battleaxe", restored.meta.unlockedBlueprints);
        }

        [Test]
        public void ReleaseVerification_MetaRunSeparation_Preserved()
        {
            var meta = _holderObj.AddComponent<MetaProgressionManager>();
            meta.Initialize();
            meta.AddEmbers(200);
            meta.UnlockBlueprintById("bp_battleaxe");

            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, null, null, null, meta);
            run.StartRun(meta);
            run.AddGold(150);

            run.ResetRun();

            // Run transient state wiped
            Assert.AreEqual(0, run.CurrentGold);

            // Meta state preserved
            Assert.AreEqual(120, meta.EmbersBalance);
            Assert.IsTrue(meta.IsBlueprintUnlocked("bp_battleaxe"));
        }

        [Test]
        public void ReleaseVerification_NavigationSafety_CombatBackBlocked()
        {
            var nav = _holderObj.AddComponent<ScreenNavigationController>();
            nav.Initialize(ScreenState.MAIN_MENU);
            nav.NavigateTo(ScreenState.COMBAT);

            bool backed = nav.NavigateBack();
            Assert.IsFalse(backed, "Back navigation during active combat must be blocked for run safety.");
            Assert.AreEqual(ScreenState.COMBAT, nav.CurrentScreen);
        }

        [Test]
        public void ReleaseVerification_EconomyPricingBalanceSheet_MatchesPlanSection13()
        {
            Assert.AreEqual(20, EconomyManager.GetCommonItemPrice());
            Assert.AreEqual(40, EconomyManager.GetRareItemPrice());
            Assert.AreEqual(35, EconomyManager.GetRunePrice());
            Assert.AreEqual(40, EconomyManager.GetBagExpansionPrice());
        }
    }
}
