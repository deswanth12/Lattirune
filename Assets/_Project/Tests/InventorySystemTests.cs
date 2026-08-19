using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Inventory;
using Lattirune.Items;

namespace Lattirune.Tests
{
    [TestFixture]
    public class InventorySystemTests
    {
        private GameObject _holderObj;
        private InventorySystem _inventorySystem;
        private ItemDataSO _swordData;
        private ItemDataSO _relicData;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("InventorySystemTestHolder");
            _inventorySystem = _holderObj.AddComponent<InventorySystem>();
            _inventorySystem.Initialize();

            _swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            _swordData.Initialize("item_sword", "Sword", "Weapon", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);

            _relicData = ScriptableObject.CreateInstance<ItemDataSO>();
            _relicData.Initialize("item_relic", "Relic", "Relic", ItemCategory.Relic, new Vector2Int(1, 1), false, Color.magenta);
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
        public void InventorySystem_InitialState_MatchesDefaultDefinition()
        {
            Assert.AreEqual(6, _inventorySystem.Capacity);
            Assert.AreEqual(16, _inventorySystem.TotalCapacity);
            Assert.AreEqual(6, _inventorySystem.UnlockedCount);
            Assert.AreEqual(10, _inventorySystem.LockedCount);
            Assert.AreEqual(0, _inventorySystem.StoredItemCount);
            Assert.IsTrue(_inventorySystem.CanExpand);
        }

        [Test]
        public void InventorySystem_AddItem_AutoFindsFirstAvailableFootprint()
        {
            ItemInstance relic = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);

            bool added = _inventorySystem.AddItem(relic);

            Assert.IsTrue(added);
            Assert.AreEqual(1, _inventorySystem.StoredItemCount);
            Assert.AreEqual(new Vector2Int(0, 0), relic.GridPosition);
            Assert.IsTrue(_inventorySystem.Grid.IsCellOccupied(0, 0));
        }

        [Test]
        public void InventorySystem_AddItem_PreferredPosition()
        {
            ItemInstance relic = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);

            bool added = _inventorySystem.AddItem(relic, preferredPosition: new Vector2Int(2, 1));

            Assert.IsTrue(added);
            Assert.AreEqual(new Vector2Int(2, 1), relic.GridPosition);
            Assert.IsTrue(_inventorySystem.Grid.IsCellOccupied(2, 1));
        }

        [Test]
        public void InventorySystem_RemoveItem_FreesCapacity()
        {
            ItemInstance relic = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);
            _inventorySystem.AddItem(relic, new Vector2Int(1, 1));

            Assert.AreEqual(1, _inventorySystem.StoredItemCount);

            bool removed = _inventorySystem.RemoveItem(relic);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, _inventorySystem.StoredItemCount);
            Assert.IsFalse(_inventorySystem.Grid.IsCellOccupied(1, 1));
        }

        [Test]
        public void InventorySystem_MoveItem_SuccessfullyRepositioned()
        {
            ItemInstance relic = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);
            _inventorySystem.AddItem(relic, new Vector2Int(0, 0));

            bool moved = _inventorySystem.MoveItem(relic, new Vector2Int(2, 1));

            Assert.IsTrue(moved);
            Assert.AreEqual(new Vector2Int(2, 1), relic.GridPosition);
            Assert.IsFalse(_inventorySystem.Grid.IsCellOccupied(0, 0));
            Assert.IsTrue(_inventorySystem.Grid.IsCellOccupied(2, 1));
        }

        [Test]
        public void InventorySystem_InventoryFull_ReturnsFalse()
        {
            // Initial unlocked area is 6 cells. Fill with six 1x1 relics.
            for (int i = 0; i < 6; i++)
            {
                ItemInstance item = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);
                Assert.IsTrue(_inventorySystem.AddItem(item));
            }

            Assert.AreEqual(6, _inventorySystem.StoredItemCount);

            // 7th relic cannot fit into 6-cell inventory
            ItemInstance overflowItem = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);
            bool added = _inventorySystem.AddItem(overflowItem);

            Assert.IsFalse(added);
            Assert.AreEqual(6, _inventorySystem.StoredItemCount);
        }
    }
}
