using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class InventoryExpansionTests
    {
        private GameObject _holderObj;
        private InventorySystem _inventorySystem;
        private ItemDataSO _swordData;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("InventoryExpansionTestHolder");
            _inventorySystem = _holderObj.AddComponent<InventorySystem>();
            _inventorySystem.Initialize();

            _swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            _swordData.Initialize("item_sword", "Sword", "Weapon", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);
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
        public void InventoryExpansion_UnlocksCellsInDeterministicOrder()
        {
            Assert.AreEqual(6, _inventorySystem.Capacity);
            Assert.IsTrue(_inventorySystem.Grid.IsCellLocked(3, 0));

            bool expanded = _inventorySystem.ExpandBag();

            Assert.IsTrue(expanded);
            Assert.AreEqual(1, _inventorySystem.ExpansionStep);
            Assert.AreEqual(7, _inventorySystem.Capacity);
            Assert.IsFalse(_inventorySystem.Grid.IsCellLocked(3, 0)); // Unlocked (3,0)
        }

        [Test]
        public void InventoryExpansion_EnablesPlacementInNewlyUnlockedCells()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);

            // Attempt to place 1x2 at (3,0) before expanding (3,0) and (3,1)
            Assert.IsFalse(_inventorySystem.AddItem(sword, new Vector2Int(3, 0)));

            // Expand step 1: unlocks (3,0)
            _inventorySystem.ExpandBag();
            // Still cannot place 1x2 because (3,1) is locked
            Assert.IsFalse(_inventorySystem.AddItem(sword, new Vector2Int(3, 0)));

            // Expand step 2: unlocks (3,1)
            _inventorySystem.ExpandBag();

            // Now both (3,0) and (3,1) are unlocked -> placement succeeds!
            Assert.IsTrue(_inventorySystem.AddItem(sword, new Vector2Int(3, 0)));
            Assert.AreEqual(new Vector2Int(3, 0), sword.GridPosition);
        }

        [Test]
        public void InventoryExpansion_MaxExpansionEnforced()
        {
            // Default expansion sequence has 10 steps (taking 6 cells to full 16)
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(_inventorySystem.ExpandBag());
            }

            Assert.AreEqual(16, _inventorySystem.Capacity);
            Assert.AreEqual(0, _inventorySystem.LockedCount);
            Assert.IsFalse(_inventorySystem.CanExpand);

            // Cannot expand further
            Assert.IsFalse(_inventorySystem.ExpandBag());
        }

        [Test]
        public void InventoryExpansion_SaveAndRestore_MaintainsExpansionState()
        {
            _inventorySystem.ExpandBag();
            _inventorySystem.ExpandBag();
            _inventorySystem.ExpandBag();

            Assert.AreEqual(9, _inventorySystem.Capacity);
            Assert.AreEqual(3, _inventorySystem.ExpansionStep);

            SavedInventoryData saved = new SavedInventoryData(
                _inventorySystem.ExpansionStep,
                _inventorySystem.Grid.GetUnlockedCoordinates()
            );

            // Restore in fresh instance
            GameObject freshObj = new GameObject("FreshInventorySystem");
            InventorySystem freshInv = freshObj.AddComponent<InventorySystem>();
            freshInv.Initialize();

            freshInv.RestoreState(saved.GetCoordinates(), saved.expansionStep);

            Assert.AreEqual(9, freshInv.Capacity);
            Assert.AreEqual(3, freshInv.ExpansionStep);

            Object.DestroyImmediate(freshObj);
        }
    }
}
