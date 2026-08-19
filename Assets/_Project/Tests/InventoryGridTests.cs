using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Inventory;

namespace Lattirune.Tests
{
    [TestFixture]
    public class InventoryGridTests
    {
        private InventoryGrid _grid;

        [SetUp]
        public void Setup()
        {
            // 4x4 grid with initial 6 unlocked cells: (0,0)..(2,1)
            List<Vector2Int> initial = new List<Vector2Int>
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)
            };
            _grid = new InventoryGrid(4, 4, initial);
        }

        [Test]
        public void InventoryGrid_InitialLayout_MatchesDimensionsAndUnlockedCells()
        {
            Assert.AreEqual(4, _grid.Width);
            Assert.AreEqual(4, _grid.Height);
            Assert.AreEqual(16, _grid.TotalCellCount);
            Assert.AreEqual(6, _grid.UnlockedCellCount);

            Assert.IsFalse(_grid.IsCellLocked(0, 0));
            Assert.IsFalse(_grid.IsCellLocked(2, 1));
            Assert.IsTrue(_grid.IsCellLocked(3, 0)); // Locked
            Assert.IsTrue(_grid.IsCellLocked(0, 2)); // Locked
        }

        [Test]
        public void InventoryGrid_LockedCells_RejectPlacement()
        {
            // (3,0) is locked
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(3, 0), new Vector2Int(1, 1)));
            Assert.IsFalse(_grid.PlaceItem("item_1", new Vector2Int(3, 0), new Vector2Int(1, 1)));
        }

        [Test]
        public void InventoryGrid_UnlockedCells_AcceptPlacement_1x1()
        {
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(0, 0), new Vector2Int(1, 1)));
            Assert.IsTrue(_grid.PlaceItem("item_1x1", new Vector2Int(0, 0), new Vector2Int(1, 1)));
            Assert.IsTrue(_grid.IsCellOccupied(0, 0));
        }

        [Test]
        public void InventoryGrid_MultiTilePlacement_1x2_2x1_2x2()
        {
            // 1x2 at (0,0) covers (0,0) and (0,1)
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(0, 0), new Vector2Int(1, 2)));
            Assert.IsTrue(_grid.PlaceItem("sword", new Vector2Int(0, 0), new Vector2Int(1, 2)));

            Assert.IsTrue(_grid.IsCellOccupied(0, 0));
            Assert.IsTrue(_grid.IsCellOccupied(0, 1));

            // 2x1 at (1,0) covers (1,0) and (2,0)
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(1, 0), new Vector2Int(2, 1)));
            Assert.IsTrue(_grid.PlaceItem("dagger", new Vector2Int(1, 0), new Vector2Int(2, 1)));

            Assert.IsTrue(_grid.IsCellOccupied(1, 0));
            Assert.IsTrue(_grid.IsCellOccupied(2, 0));
        }

        [Test]
        public void InventoryGrid_OverlapRejection()
        {
            _grid.PlaceItem("item_1", new Vector2Int(0, 0), new Vector2Int(1, 1));

            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(0, 0), new Vector2Int(1, 1)));
            Assert.IsFalse(_grid.PlaceItem("item_2", new Vector2Int(0, 0), new Vector2Int(1, 1)));
        }

        [Test]
        public void InventoryGrid_OutOfBoundsRejection()
        {
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(-1, 0), new Vector2Int(1, 1)));
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(3, 0), new Vector2Int(2, 1))); // (4,0) out of bounds
        }

        [Test]
        public void InventoryGrid_ItemRemoval()
        {
            _grid.PlaceItem("item_to_remove", new Vector2Int(1, 0), new Vector2Int(2, 1));
            Assert.IsTrue(_grid.IsCellOccupied(1, 0));
            Assert.IsTrue(_grid.IsCellOccupied(2, 0));

            Assert.IsTrue(_grid.RemoveItem("item_to_remove", new Vector2Int(1, 0), new Vector2Int(2, 1)));
            Assert.IsFalse(_grid.IsCellOccupied(1, 0));
            Assert.IsFalse(_grid.IsCellOccupied(2, 0));
        }
    }
}
