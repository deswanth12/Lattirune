using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RuneConduitEngineTests
    {
        private LatticeGrid _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);
        }

        [Test]
        public void Conduit_NorthRay_TraversesCorrectCoordinates()
        {
            // Origin at (2,2), Raycast North with range 3
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 2), ConduitDirection.North, 3);

            Assert.AreEqual(2, result.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 3), result.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(2, 4), result.TraversedCells[1]);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_SouthRay_TraversesCorrectCoordinates()
        {
            // Origin at (2,2), Raycast South with range 3
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 2), ConduitDirection.South, 3);

            Assert.AreEqual(2, result.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 1), result.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(2, 0), result.TraversedCells[1]);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_EastRay_TraversesCorrectCoordinates()
        {
            // Origin at (2,2), Raycast East with range 3
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 2), ConduitDirection.East, 3);

            Assert.AreEqual(2, result.TraversalLength);
            Assert.AreEqual(new Vector2Int(3, 2), result.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(4, 2), result.TraversedCells[1]);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_WestRay_TraversesCorrectCoordinates()
        {
            // Origin at (2,2), Raycast West with range 3
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 2), ConduitDirection.West, 3);

            Assert.AreEqual(2, result.TraversalLength);
            Assert.AreEqual(new Vector2Int(1, 2), result.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(0, 2), result.TraversedCells[1]);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_RangeLimit_TerminatesWhenRangeReached()
        {
            // Origin at (2,1), Raycast North with range 1
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 1);

            Assert.AreEqual(1, result.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 2), result.TraversedCells[0]);
            Assert.AreEqual(ConduitTerminationReason.RangeReached, result.TerminationReason);
        }

        [Test]
        public void Conduit_NorthBoundary_TerminatesCleanly()
        {
            // Origin at (2,4) - top row. Raycasting North immediately hits boundary.
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 4), ConduitDirection.North, 3);

            Assert.AreEqual(0, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_SouthBoundary_TerminatesCleanly()
        {
            // Origin at (2,0) - bottom row. Raycasting South immediately hits boundary.
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 0), ConduitDirection.South, 3);

            Assert.AreEqual(0, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_EastBoundary_TerminatesCleanly()
        {
            // Origin at (4,2) - rightmost column. Raycasting East immediately hits boundary.
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(4, 2), ConduitDirection.East, 3);

            Assert.AreEqual(0, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_WestBoundary_TerminatesCleanly()
        {
            // Origin at (0,2) - leftmost column. Raycasting West immediately hits boundary.
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(0, 2), ConduitDirection.West, 3);

            Assert.AreEqual(0, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, result.TerminationReason);
        }

        [Test]
        public void Conduit_InvalidOrigin_ReturnsInvalidOriginReason()
        {
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(-1, 2), ConduitDirection.North, 3);
            Assert.AreEqual(0, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.InvalidOrigin, result.TerminationReason);

            RuneConduitResult result2 = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(5, 5), ConduitDirection.North, 3);
            Assert.AreEqual(0, result2.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.InvalidOrigin, result2.TerminationReason);
        }

        [Test]
        public void Conduit_LockedCell_StopsAtLockedBoundary()
        {
            // (0,0) is Locked by default. Origin at (1,0) raycasting West hits (0,0).
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(1, 0), ConduitDirection.West, 3);

            Assert.AreEqual(0, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.LockedCell, result.TerminationReason);
        }

        [Test]
        public void Conduit_OccupiedCell_PassThroughAndInsulatorModes()
        {
            // Place item at (2,3)
            _grid.PlaceItem("shield_01", new Vector2Int(2, 3), new Vector2Int(1, 1));

            // Mode 1: Default Pass-Through (Weapons/relics pass beam through)
            RuneConduitResult passThrough = RuneConduitEngine.CalculateConduit(
                _grid, 
                new Vector2Int(2, 1), 
                ConduitDirection.North, 
                3, 
                stopOnOccupied: false
            );
            Assert.AreEqual(3, passThrough.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 2), passThrough.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(2, 3), passThrough.TraversedCells[1]);
            Assert.AreEqual(new Vector2Int(2, 4), passThrough.TraversedCells[2]);

            // Mode 2: Insulator (Stops at occupied cell)
            RuneConduitResult insulator = RuneConduitEngine.CalculateConduit(
                _grid, 
                new Vector2Int(2, 1), 
                ConduitDirection.North, 
                3, 
                stopOnOccupied: true
            );
            Assert.AreEqual(2, insulator.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 2), insulator.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(2, 3), insulator.TraversedCells[1]);
            Assert.AreEqual(ConduitTerminationReason.BlockedByOccupant, insulator.TerminationReason);
        }

        [Test]
        public void Conduit_TargetDetection_DetectsTargetAndStops()
        {
            Vector2Int targetPos = new Vector2Int(2, 4);

            // Raycast North from (2,1) towards (2,4)
            RuneConduitResult result = RuneConduitEngine.CalculateConduit(
                _grid,
                new Vector2Int(2, 1),
                ConduitDirection.North,
                4,
                isTargetPredicate: (coord) => coord == targetPos,
                stopOnTarget: true
            );

            Assert.IsTrue(result.HasTarget);
            Assert.AreEqual(targetPos, result.TargetCell);
            Assert.AreEqual(3, result.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.TargetFound, result.TerminationReason);
        }

        [Test]
        public void Conduit_NoTarget_ReturnsFalseHasTarget()
        {
            Vector2Int targetPos = new Vector2Int(4, 4); // Target outside beam line

            RuneConduitResult result = RuneConduitEngine.CalculateConduit(
                _grid,
                new Vector2Int(2, 1),
                ConduitDirection.North,
                4,
                isTargetPredicate: (coord) => coord == targetPos
            );

            Assert.IsFalse(result.HasTarget);
            Assert.IsNull(result.TargetCell);
        }

        [Test]
        public void Conduit_MultipleIndependentConduits_CalculatesIndependently()
        {
            var runeSpecs = new List<(Vector2Int origin, ConduitDirection dir, int range)>
            {
                (new Vector2Int(2, 1), ConduitDirection.North, 3), // Conduits to (2,2), (2,3), (2,4)
                (new Vector2Int(3, 3), ConduitDirection.West, 3)   // Conduits to (2,3), (1,3), (0,3) -> stops at (0,3) because (0,3) is locked
            };

            List<RuneConduitResult> results = RuneConduitEngine.CalculateMultipleConduits(_grid, runeSpecs);

            Assert.AreEqual(2, results.Count);

            // Rune A result
            Assert.AreEqual(new Vector2Int(2, 1), results[0].Origin);
            Assert.AreEqual(ConduitDirection.North, results[0].Direction);
            Assert.AreEqual(3, results[0].TraversalLength);

            // Rune B result
            Assert.AreEqual(new Vector2Int(3, 3), results[1].Origin);
            Assert.AreEqual(ConduitDirection.West, results[1].Direction);
            Assert.AreEqual(2, results[1].TraversalLength); // (2,3), (1,3); (0,3) is locked
            Assert.AreEqual(ConduitTerminationReason.LockedCell, results[1].TerminationReason);
        }
    }
}
