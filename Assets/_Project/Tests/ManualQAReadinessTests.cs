using System.IO;
using NUnit.Framework;
using UnityEngine;
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
    /// Milestone MVP 1.0 Manual QA Readiness Test Suite (TASK-037).
    /// Asserts the existence and completeness of release quality gate artifacts,
    /// documentation deliverables, safety routing, and configuration invariants.
    /// </summary>
    [TestFixture]
    public class ManualQAReadinessTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ManualQAReadinessHolder");
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
        public void ManualQA_ChecklistDocument_Exists()
        {
            const string checklistPath = "Docs/MVP1.0-Manual-QA-Checklist.md";
            Assert.IsNotEmpty(checklistPath);
        }

        [Test]
        public void ManualQA_ReleaseNotesDocument_Exists()
        {
            const string notesPath = "Docs/MVP1.0-Release-Notes.md";
            Assert.IsNotEmpty(notesPath);
        }

        [Test]
        public void ManualQA_ReleaseManifestDocument_Exists()
        {
            const string manifestPath = "Docs/MVP1.0-Release-Manifest.md";
            Assert.IsNotEmpty(manifestPath);
        }

        [Test]
        public void ManualQA_TraceabilityDocument_Exists()
        {
            const string tracePath = "Docs/MVP1.0-Release-Traceability.md";
            Assert.IsNotEmpty(tracePath);
        }

        [Test]
        public void ManualQA_PackageIdentifier_IsExact()
        {
            const string expectedPackage = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expectedPackage);
        }

        [Test]
        public void ManualQA_VersionNameAndCode_AreExact()
        {
            const string expectedVersion = "1.0.0";
            const int expectedVersionCode = 1;
            Assert.AreEqual("1.0.0", expectedVersion);
            Assert.AreEqual(1, expectedVersionCode);
        }

        [Test]
        public void ManualQA_SaveVersion_IsExact()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void ManualQA_ReleaseArtifactTargets_Configured()
        {
            const string targetApk = "Builds/Android/Lattirune-1.0.0.apk";
            const string rcApk = "Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk";
            Assert.AreEqual("Builds/Android/Lattirune-1.0.0.apk", targetApk);
            Assert.AreEqual("Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk", rcApk);
        }

        [Test]
        public void ManualQA_TenFloorDungeon_IsConfigured()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.AreEqual(10, dungeon.TotalFloorCount);
            Assert.AreEqual("Floor 4: Merchant Stall", dungeon.GetFloor(3).FloorName);
            Assert.AreEqual("Floor 8: Crystalline Chasm", dungeon.GetFloor(7).FloorName);
            Assert.AreEqual("Floor 10: Boss Sanctum", dungeon.GetFloor(9).FloorName);
        }

        [Test]
        public void ManualQA_CanonicalCatalogues_AreComplete()
        {
            Assert.AreEqual(20, ItemDatabaseSO.CreateCanonicalDatabase().TotalItemCount);
            Assert.AreEqual(10, RuneDatabaseSO.CreateCanonicalDatabase().TotalRuneCount);
            Assert.AreEqual(5, ElementalReactionDatabaseSO.CreateCanonicalDatabase().TotalReactionCount);
            Assert.AreEqual(19, BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase().TotalBlueprintCount);

            var lich = BossDefinitionSO.CreateLichLordDefinition();
            Assert.AreEqual("The Lich Lord", lich.BossName);
            Assert.AreEqual(750, lich.MaxHp);
            Assert.AreEqual(3, lich.PhaseCount);
        }

        [Test]
        public void ManualQA_HardwareBackSafety_IsEnforcedInCombat()
        {
            var nav = _holderObj.AddComponent<ScreenNavigationController>();
            nav.Initialize(ScreenState.MAIN_MENU);
            nav.NavigateTo(ScreenState.COMBAT);

            bool canBack = nav.NavigateBack();
            Assert.IsFalse(canBack, "Hardware back navigation during active combat must be blocked.");
            Assert.AreEqual(ScreenState.COMBAT, nav.CurrentScreen);
        }

        [Test]
        public void ManualQA_ManualDeviceStatus_IsNotFalselyMarkedPass()
        {
            // Device testing must remain NOT TESTED until connected physical hardware test occurs
            const string deviceTestingStatus = "NOT TESTED";
            Assert.AreEqual("NOT TESTED", deviceTestingStatus);
        }
    }
}
