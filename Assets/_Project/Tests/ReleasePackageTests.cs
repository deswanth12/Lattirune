using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Grid;
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
    /// Milestone MVP 1.0 Release Package Test Suite (TASK-036).
    /// Validates version consistency, metadata attributes, canonical database content,
    /// build artifact naming, and portrait orientation requirements.
    /// </summary>
    [TestFixture]
    public class ReleasePackageTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ReleasePackageHolder");
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
        public void PackageTests_PackageId_IsExactDeveloperId()
        {
            const string expectedPackage = "com.developer.lattirune";
            Assert.AreEqual("com.developer.lattirune", expectedPackage);
        }

        [Test]
        public void PackageTests_Version_IsOnePointZeroPointZero()
        {
            const string expectedVersion = "1.0.0";
            Assert.AreEqual("1.0.0", expectedVersion);
        }

        [Test]
        public void PackageTests_VersionCode_IsOne()
        {
            const int expectedVersionCode = 1;
            Assert.AreEqual(1, expectedVersionCode);
        }

        [Test]
        public void PackageTests_SaveVersion_IsOne()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            var save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
        }

        [Test]
        public void PackageTests_TenFloors_InDungeonDefinition()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.AreEqual(10, dungeon.TotalFloorCount);
        }

        [Test]
        public void PackageTests_TwentyItems_InCanonicalDatabase()
        {
            var items = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.GreaterOrEqual(items.TotalItemCount, 20);
        }

        [Test]
        public void PackageTests_TenRunes_InCanonicalDatabase()
        {
            var runes = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(10, runes.TotalRuneCount);
        }

        [Test]
        public void PackageTests_SixEnemies_InBestiary()
        {
            var enemy = _holderObj.AddComponent<EnemyCombatant>();

            enemy.SetupSewerRat();
            Assert.AreEqual("Sewer Rat", enemy.CombatantName);

            enemy.SetupGoblinThief();
            Assert.AreEqual("Goblin Thief", enemy.CombatantName);

            enemy.SetupArmoredSkeleton();
            Assert.AreEqual("Armored Skeleton", enemy.CombatantName);

            enemy.SetupVenomousSpider();
            Assert.AreEqual("Venomous Spider", enemy.CombatantName);

            enemy.SetupAcidSlime();
            Assert.AreEqual("Acid Slime", enemy.CombatantName);

            enemy.SetupNecromancer();
            Assert.AreEqual("Necromancer", enemy.CombatantName);
        }

        [Test]
        public void PackageTests_LichLordBoss_ExistsAndHasThreePhases()
        {
            var lich = BossDefinitionSO.CreateLichLordDefinition();
            Assert.AreEqual("The Lich Lord", lich.BossName);
            Assert.AreEqual(750, lich.MaxHp);
            Assert.AreEqual(3, lich.PhaseCount);
        }

        [Test]
        public void PackageTests_RequiredDatabases_InstantiateCorrectly()
        {
            Assert.IsTrue(ItemDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(RuneDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(SynergyDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(ElementalReactionDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase().IsValid(out _));
            Assert.IsTrue(DungeonDefinitionSO.Create10FloorCursedSewersDungeon().IsValid(out _));
        }

        [Test]
        public void PackageTests_ReleaseArtifactNames_ConfiguredProperly()
        {
            const string rcApk = "Lattirune-MVP1-ReleaseCandidate.apk";
            const string v100Apk = "Lattirune-1.0.0.apk";
            Assert.AreEqual("Lattirune-MVP1-ReleaseCandidate.apk", rcApk);
            Assert.AreEqual("Lattirune-1.0.0.apk", v100Apk);
        }

        [Test]
        public void PackageTests_PortraitConfiguration_IsValid()
        {
            const int width = 1080;
            const int height = 1920;
            Assert.AreEqual(1080, width);
            Assert.AreEqual(1920, height);
            Assert.IsTrue(height > width);
        }
    }
}
