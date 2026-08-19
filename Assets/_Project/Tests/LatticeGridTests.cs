using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Tests
{
    [TestFixture]
    public class LatticeGridTests
    {
        private LatticeGrid _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);
        }

        [Test]
        public void Grid_Initializes_WithExactDimensionsAndCounts()
        {
            Assert.AreEqual(5, LatticeGrid.WIDTH);
            Assert.AreEqual(5, LatticeGrid.HEIGHT);
            Assert.AreEqual(25, LatticeGrid.TOTAL_CELLS);
            Assert.AreEqual(17, _grid.GetActiveCount(), "Default diamond-square layout must have exactly 17 active tiles.");
            Assert.AreEqual(8, _grid.GetLockedCount(), "Default layout must have exactly 8 locked perimeter tiles.");
            Assert.AreEqual(0, _grid.GetOccupiedCount());
        }

        [Test]
        public void Coordinates_BoundsValidation_WorksCorrectly()
        {
            // Valid coordinates
            Assert.IsTrue(_grid.IsValidCoordinate(0, 0));
            Assert.IsTrue(_grid.IsValidCoordinate(4, 4));
            Assert.IsTrue(_grid.IsValidCoordinate(2, 2));

            // Invalid coordinates
            Assert.IsFalse(_grid.IsValidCoordinate(-1, 0));
            Assert.IsFalse(_grid.IsValidCoordinate(0, -1));
            Assert.IsFalse(_grid.IsValidCoordinate(5, 0));
            Assert.IsFalse(_grid.IsValidCoordinate(0, 5));
            Assert.IsFalse(_grid.IsValidCoordinate(5, 5));
        }

        [Test]
        public void CanPlaceItem_1x1_ValidSingleTile_ReturnsTrue()
        {
            // Center tile (2,2) is always Active
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(2, 2), new Vector2Int(1, 1)));
        }

        [Test]
        public void CanPlaceItem_1x1_OverLockedTile_ReturnsFalse()
        {
            // (0,0) is locked by default
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(0, 0), new Vector2Int(1, 1)));
        }

        [Test]
        public void CanPlaceItem_1x2_VerticalFootprint_ValidAndInvalid()
        {
            // Origin at (2,1) spanning to (2,2): both are Active
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(2, 1), new Vector2Int(1, 2)));

            // Origin at (0,0) spanning to (0,1): (0,0) is Locked
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(0, 0), new Vector2Int(1, 2)));

            // Origin at (2,4) spanning to (2,5): out of bounds vertically
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(2, 4), new Vector2Int(1, 2)));
        }

        [Test]
        public void CanPlaceItem_2x1_HorizontalFootprint_ValidAndInvalid()
        {
            // Origin at (1,2) spanning to (2,2): both are Active
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(1, 2), new Vector2Int(2, 1)));

            // Origin at (4,2) spanning to (5,2): out of bounds horizontally
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(4, 2), new Vector2Int(2, 1)));
        }

        [Test]
        public void CanPlaceItem_2x2_QuadFootprint_ValidAndInvalid()
        {
            // Origin at (1,1) covering (1,1), (2,1), (1,2), (2,2): all are Active
            Assert.IsTrue(_grid.CanPlaceItem(new Vector2Int(1, 1), new Vector2Int(2, 2)));

            // Origin at (0,1) covering (0,1), (1,1), (0,2), (1,2): (0,1) is Locked
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(0, 1), new Vector2Int(2, 2)));

            // Out of bounds at (4,4)
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(4, 4), new Vector2Int(2, 2)));
        }

        [Test]
        public void PlaceItem_ValidFootprint_OccupyCellsSuccessfully()
        {
            Vector2Int origin = new Vector2Int(1, 1);
            Vector2Int size = new Vector2Int(2, 2);

            bool placed = _grid.PlaceItem("sword_01", origin, size);
            Assert.IsTrue(placed);
            Assert.AreEqual(4, _grid.GetOccupiedCount());
            Assert.AreEqual(13, _grid.GetActiveCount());

            // Check that attempting to place another item on top fails
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(1, 1), new Vector2Int(1, 1)));
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(2, 2), new Vector2Int(1, 1)));
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(1, 1), new Vector2Int(2, 2)));
        }

        [Test]
        public void RemoveItem_FreesOccupiedCells()
        {
            Vector2Int origin = new Vector2Int(1, 2);
            Vector2Int size = new Vector2Int(1, 2); // 1x2 item at (1,2) and (1,3)

            _grid.PlaceItem("dagger_01", origin, size);
            Assert.AreEqual(2, _grid.GetOccupiedCount());

            bool removed = _grid.RemoveItem("dagger_01", origin, size);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, _grid.GetOccupiedCount());
            Assert.AreEqual(17, _grid.GetActiveCount());
            Assert.IsTrue(_grid.CanPlaceItem(origin, size));
        }

        [Test]
        public void UnlockTile_TransitionsLockedTileToActive()
        {
            // (0,0) starts locked
            GridCell cornerCell = _grid.GetCell(0, 0);
            Assert.IsTrue(cornerCell.IsLocked());

            bool unlocked = _grid.UnlockTile(0, 0);
            Assert.IsTrue(unlocked);
            Assert.IsTrue(cornerCell.IsAvailable());
            Assert.AreEqual(18, _grid.GetActiveCount());
            Assert.AreEqual(7, _grid.GetLockedCount());
        }

        [Test]
        public void GridCoordinateUtility_Conversions_AreDeterministic()
        {
            Vector2 origin = new Vector2(-3f, -3f);
            float cellSize = 1.2f;
            float spacing = 0.1f;

            // (0,0) center
            Vector3 worldPos00 = GridCoordinateUtility.GridToWorldPosition(0, 0, origin, cellSize, spacing);
            Assert.AreEqual(origin.x + (cellSize * 0.5f), worldPos00.x, 0.001f);
            Assert.AreEqual(origin.y + (cellSize * 0.5f), worldPos00.y, 0.001f);

            // Convert world pos back to grid coord
            bool inBounds = GridCoordinateUtility.WorldToGridCoordinate(worldPos00, origin, out Vector2Int coord, cellSize, spacing);
            Assert.IsTrue(inBounds);
            Assert.AreEqual(new Vector2Int(0, 0), coord);

            // Footprint centering for 2x2 at (1,1)
            Vector3 center2x2 = GridCoordinateUtility.GetFootprintWorldCenter(new Vector2Int(1, 1), new Vector2Int(2, 2), origin, cellSize, spacing);
            Assert.IsTrue(center2x2.x > worldPos00.x);
            Assert.IsTrue(center2x2.y > worldPos00.y);
        }
    }
}
