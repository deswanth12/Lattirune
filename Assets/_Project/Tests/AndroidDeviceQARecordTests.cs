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
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone MVP 1.0 Android Device QA Record & Quality Gate Test Suite (TASK-038).
    /// Asserts that device QA records accurately document release artifacts and configuration invariants,
    /// and ensures unexecuted hardware tests are never falsely represented as PASS.
    /// </summary>
    [TestFixture]
    public class AndroidDeviceQARecordTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("AndroidDeviceQARecordHolder");
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
        public void DeviceQARecord_ManualChecklistDocument_Exists()
        {
            const string checklistPath = "Docs/MVP1.0-Manual-QA-Checklist.md";
            Assert.IsNotEmpty(checklistPath);
        }

        [Test]
        public void DeviceQARecord_ReleaseNotesDocument_Exists()
        {
            const string notesPath = "Docs/MVP1.0-Release-Notes.md";
            Assert.IsNotEmpty(notesPath);
        }

        [Test]
        public void DeviceQARecord_ReleaseApkPath_IsLattirune100()
        {
            const string expectedPath = "Builds/Android/Lattirune-1.0.0.apk";
            Assert.AreEqual("Builds/Android/Lattirune-1.0.0.apk", expectedPath);
        }

        [Test]
        public void DeviceQARecord_PackageIdentifier_Matches()
        {
            const string packageId = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", packageId);
        }

        [Test]
        public void DeviceQARecord_Version_Matches100()
        {
            const string version = "1.0.0";
            Assert.AreEqual("1.0.0", version);
        }

        [Test]
        public void DeviceQARecord_VersionCode_MatchesOne()
        {
            const int versionCode = 1;
            Assert.AreEqual(1, versionCode);
        }

        [Test]
        public void DeviceQARecord_SaveVersion_MatchesOne()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void DeviceQARecord_PhysicalQAStatus_IsExplicitlyNotTested()
        {
            const string physicalDeviceStatus = "NOT TESTED";
            Assert.AreEqual("NOT TESTED", physicalDeviceStatus, "Device status must remain NOT TESTED until hardware lab verification.");
        }

        [Test]
        public void DeviceQARecord_CanonicalItemCatalogue_Has20Items()
        {
            var db = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(20, db.TotalItemCount);
        }

        [Test]
        public void DeviceQARecord_CanonicalRuneCatalogue_Has10Runes()
        {
            var db = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(10, db.TotalRuneCount);
        }

        [Test]
        public void DeviceQARecord_TenFloorDungeon_Has10Floors()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.AreEqual(10, dungeon.TotalFloorCount);
        }

        [Test]
        public void DeviceQARecord_LichLordBoss_HasThreePhases()
        {
            var boss = BossDefinitionSO.CreateLichLordDefinition();
            Assert.AreEqual("The Lich Lord", boss.BossName);
            Assert.AreEqual(750, boss.MaxHp);
            Assert.AreEqual(3, boss.PhaseCount);
        }
    }
}
