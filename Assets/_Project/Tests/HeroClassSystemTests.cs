using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Progression;
using Lattirune.Runes;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class HeroClassSystemTests
    {
        private GameObject _holder;
        private HeroClassManager _classManager;
        private MetaProgressionManager _meta;
        private ItemDatabaseSO _itemDb;
        private RuneDatabaseSO _runeDb;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""HeroClassTestHolder"");

            _itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            _runeDb = RuneDatabaseSO.CreateCanonicalDatabase();

            _meta = _holder.AddComponent<MetaProgressionManager>();
            _meta.Initialize(startingEmbers: 200);

            _classManager = _holder.AddComponent<HeroClassManager>();
            _classManager.Initialize(
                HeroClassDatabaseSO.CreateCanonicalDatabase(),
                _itemDb,
                _runeDb
            );
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
        public void CanonicalDatabase_ContainsAll4CanonicalClasses()
        {
            var db = _classManager.Database;
            Assert.IsNotNull(db);
            Assert.AreEqual(4, db.TotalClassCount);

            Assert.IsTrue(db.HasClass(""class_rune_knight""));
            Assert.IsTrue(db.HasClass(""class_elementalist""));
            Assert.IsTrue(db.HasClass(""class_shadow_rogue""));
            Assert.IsTrue(db.HasClass(""class_iron_juggernaut""));

            foreach (var c in db.AllClasses)
            {
                Assert.IsTrue(c.IsValid(out string err), $""Invalid class: {err}"");
                Assert.Greater(c.BaseHp, 0);
                Assert.Greater(c.StartingItemIds.Count, 0);
            }
        }

        [Test]
        public void HeroClassManager_DefaultUnlockedClass_IsRuneKnight()
        {
            Assert.AreEqual(""class_rune_knight"", _classManager.SelectedClassId);
            Assert.IsTrue(_classManager.IsClassUnlocked(""class_rune_knight""));
            Assert.IsFalse(_classManager.IsClassUnlocked(""class_elementalist""));
        }

        [Test]
        public void UnlockClass_SpendsEmbersAndEnablesSelection()
        {
            Assert.AreEqual(200, _meta.CurrentEmbers);

            // Elementalist costs 80 Embers
            bool unlocked = _classManager.UnlockClass(""class_elementalist"", _meta);
            Assert.IsTrue(unlocked);
            Assert.AreEqual(120, _meta.CurrentEmbers); // 200 - 80 = 120
            Assert.IsTrue(_classManager.IsClassUnlocked(""class_elementalist""));

            bool selected = _classManager.SelectClass(""class_elementalist"");
            Assert.IsTrue(selected);
            Assert.AreEqual(""class_elementalist"", _classManager.SelectedClassId);
        }

        [Test]
        public void UnlockClass_FailsWhenInsufficientEmbers()
        {
            _meta.SpendEmbers(190); // 10 embers remaining
            Assert.AreEqual(10, _meta.CurrentEmbers);

            // Shadow Rogue costs 120
            bool unlocked = _classManager.UnlockClass(""class_shadow_rogue"", _meta);
            Assert.IsFalse(unlocked);
            Assert.IsFalse(_classManager.IsClassUnlocked(""class_shadow_rogue""));
            Assert.AreEqual(10, _meta.CurrentEmbers);
        }

        [Test]
        public void ApplyStartingLoadout_ConfiguresPlayerAndStagingInventory()
        {
            var gridObj = new GameObject(""Grid"");
            gridObj.transform.SetParent(_holder.transform);
            var grid = gridObj.AddComponent<LatticeGrid>();
            grid.Initialize();

            var invObj = new GameObject(""Inventory"");
            invObj.transform.SetParent(_holder.transform);
            var inventory = invObj.AddComponent<InventorySystem>();
            inventory.Initialize(grid);

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();

            // Unlock and Select Iron Juggernaut (140 HP, 6 Armor, 14 Attack)
            _classManager.UnlockClass(""class_iron_juggernaut"", _meta);
            _classManager.SelectClass(""class_iron_juggernaut"");

            _classManager.ApplyStartingLoadout(player, inventory, grid);

            Assert.AreEqual(140, player.MaxHp);
            Assert.AreEqual(140, player.CurrentHp);
            Assert.AreEqual(6, player.Armor);
            Assert.AreEqual(14, player.BaseAttackDamage);

            // Staging inventory contains 3 starting items
            Assert.AreEqual(3, inventory.StagingItemCount);
        }

        [Test]
        public void SaveLoadPersistence_PreservesSelectedAndUnlockedClasses()
        {
            _classManager.UnlockClass(""class_shadow_rogue"", _meta);
            _classManager.SelectClass(""class_shadow_rogue"");

            SaveData save = SaveData.CreateDefault();
            save.meta = new SavedMetaData(
                emberCount: _meta.CurrentEmbers,
                blueprints: _meta.UnlockedBlueprints,
                bossClears: 1,
                runs: 5,
                selectedClass: _classManager.SelectedClassId,
                unlockedClasses: _classManager.ExportUnlockedClassIds()
            );

            string json = SaveSerializer.SerializeToJson(save);
            Assert.IsNotNull(json);

            SaveData restored = SaveSerializer.DeserializeFromJson(json);
            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.version);
            Assert.AreEqual(""class_shadow_rogue"", restored.meta.selectedHeroClass);
            Assert.Contains(""class_shadow_rogue"", restored.meta.unlockedHeroClasses);
        }
    }
}
