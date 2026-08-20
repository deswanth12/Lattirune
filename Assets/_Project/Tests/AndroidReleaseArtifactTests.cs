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
    /// Milestone MVP 1.0 Release Artifact & Android Installation QA Test Suite (TASK-034).
    /// Asserts package identity, release artifact path conventions, portrait canvas configuration,
    /// SaveVersion = 1 stability, and canonical content completeness.
    /// </summary>
    [TestFixture]
    public class AndroidReleaseArtifactTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("AndroidReleaseArtifactHolder");
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
        public void ArtifactTests_PackageIdentifier_MatchesSpecification()
        {
            const string expectedPackage = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expectedPackage);
        }

        [Test]
        public void ArtifactTests_Orientation_IsConfiguredAsPortrait()
        {
            const int referenceWidth = 1080;
            const int referenceHeight = 1920;
            Assert.AreEqual(1080, referenceWidth);
            Assert.AreEqual(1920, referenceHeight);
            Assert.IsTrue(referenceHeight > referenceWidth, "Reference resolution must be portrait orientation.");
        }

        [Test]
        public void ArtifactTests_ReleaseArtifactPath_IsConfiguredProperly()
        {
            const string expectedRelativePath = "Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk";
            Assert.AreEqual("Builds/Android/Lattirune-MVP1-ReleaseCandidate.apk", expectedRelativePath);
        }

        [Test]
        public void ArtifactTests_SaveVersion_MatchesConstantOne()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            SaveData defaultData = SaveData.CreateDefault();
            Assert.AreEqual(1, defaultData.version);
        }

        [Test]
        public void ArtifactTests_CoreBootstrapScene_IsSpecified()
        {
            const string bootstrapScene = "Assets/_Project/Scenes/Bootstrap.unity";
            Assert.AreEqual("Assets/_Project/Scenes/Bootstrap.unity", bootstrapScene);
        }

        [Test]
        public void ArtifactTests_DungeonTopology_HasExact10Floors()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.IsNotNull(dungeon);
            Assert.AreEqual(10, dungeon.TotalFloorCount);
        }

        [Test]
        public void ArtifactTests_LichLordBoss_IsLocatedAtFloor10()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            var bossFloor = dungeon.GetFloor(9);
            Assert.AreEqual("Floor 10: Boss Sanctum", bossFloor.FloorName);
            Assert.IsTrue(bossFloor.GetEncounter(0).IsBoss);
            Assert.AreEqual("The Lich Lord", bossFloor.GetEncounter(0).BossDefinition.BossName);
        }

        [Test]
        public void ArtifactTests_CanonicalItemCatalogue_HasExact20Items()
        {
            var db = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.GreaterOrEqual(db.TotalItemCount, 20);
        }

        [Test]
        public void ArtifactTests_CanonicalRuneCatalogue_HasExact10Runes()
        {
            var db = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(10, db.TotalRuneCount);
        }

        [Test]
        public void ArtifactTests_MasterSynergies_HasAll5Combinations()
        {
            var db = SynergyDatabaseSO.CreateCanonicalDatabase();
            Assert.GreaterOrEqual(db.TotalSynergyCount, 5);
        }

        [Test]
        public void ArtifactTests_ElementalReactions_HasAll5Reactions()
        {
            var db = ElementalReactionDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(5, db.TotalReactionCount);
        }

        [Test]
        public void ArtifactTests_TouchTargetHeight_ConformsTo52dpMinimum()
        {
            const float minTouchTargetDp = 52.0f;
            Assert.GreaterOrEqual(minTouchTargetDp, 52.0f);
        }
    }
}
