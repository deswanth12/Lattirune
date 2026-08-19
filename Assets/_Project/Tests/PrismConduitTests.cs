using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    [TestFixture]
    public class PrismConduitTests
    {
        private LatticeGrid _grid;
        private RuneData _fireRune;
        private PrismRuneDataSO _prismData;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _fireRune = ScriptableObject.CreateInstance<RuneData>();
            _fireRune.Initialize("rune_fire", "Fire Rune", ConduitDirection.East, ElementType.Fire, 4);

            _prismData = ScriptableObject.CreateInstance<PrismRuneDataSO>();
            _prismData.Initialize("prism_test", "Test Prism", branchCount: 2, maxDepth: 3);
        }

        [Test]
        public void PrismData_ValidatesCorrectly()
        {
            Assert.IsTrue(_prismData.IsValid(out string err));
            Assert.IsNull(err);
            Assert.AreEqual("prism_test", _prismData.PrismId);
            Assert.AreEqual(ElementType.Light, _prismData.Element);
        }

        [Test]
        public void NonPrismRune_ProducesSingleBeamPath()
        {
            List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, _fireRune, new Vector2Int(0, 2), ConduitDirection.East, 4);

            Assert.AreEqual(1, paths.Count);
            Assert.IsFalse(paths[0].IsSplitBranch);
            Assert.AreEqual(4, paths[0].TraversalLength);
        }

        [Test]
        public void Prism_EastBeamSplitsIntoNorthAndSouth()
        {
            // Fire at (0, 2) East (range 4). Prism at (2, 2).
            // Root beam goes from (0,2) to (2,2) (steps = 2).
            // Remaining range = 4 - 2 = 2.
            // Split branches from (2,2):
            // North branch: (2,3), (2,4)
            // South branch: (2,1), (2,0)
            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), _prismData);

            List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, _fireRune, new Vector2Int(0, 2), ConduitDirection.East, 4, GetPrism);

            Assert.AreEqual(3, paths.Count); // Root + North branch + South branch

            ConduitBeamPath root = paths[0];
            Assert.AreEqual(2, root.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 2), root.TraversedCells[1]);

            ConduitBeamPath branchA = paths[1];
            Assert.IsTrue(branchA.IsSplitBranch);
            Assert.AreEqual(ConduitDirection.North, branchA.Direction);
            Assert.AreEqual(2, branchA.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 3), branchA.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(2, 4), branchA.TraversedCells[1]);

            ConduitBeamPath branchB = paths[2];
            Assert.IsTrue(branchB.IsSplitBranch);
            Assert.AreEqual(ConduitDirection.South, branchB.Direction);
            Assert.AreEqual(2, branchB.TraversalLength);
            Assert.AreEqual(new Vector2Int(2, 1), branchB.TraversedCells[0]);
            Assert.AreEqual(new Vector2Int(2, 0), branchB.TraversedCells[1]);
        }

        [Test]
        public void Prism_NorthBeamSplitsIntoEastAndWest()
        {
            RuneData northRune = ScriptableObject.CreateInstance<RuneData>();
            northRune.Initialize("rune_north", "North Rune", ConduitDirection.North, ElementType.Fire, 4);

            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), _prismData);

            List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, northRune, new Vector2Int(2, 0), ConduitDirection.North, 4, GetPrism);

            Assert.AreEqual(3, paths.Count);
            Assert.AreEqual(ConduitDirection.East, paths[1].Direction);
            Assert.AreEqual(ConduitDirection.West, paths[2].Direction);
        }

        [Test]
        public void Prism_SplitBranches_StopAtGridBoundaries()
        {
            // Prism at (2, 4) (top row). East beam hits (2,4).
            // North branch from (2,4) with range 2 immediately hits grid boundary (y=5 out of bounds).
            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 4), _prismData);

            List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, _fireRune, new Vector2Int(0, 4), ConduitDirection.East, 4, GetPrism);

            Assert.AreEqual(3, paths.Count);
            ConduitBeamPath northBranch = paths[1];
            Assert.AreEqual(0, northBranch.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, northBranch.TerminationReason);
        }

        [Test]
        public void Prism_ParentChildRelationship_AndUniqueIds()
        {
            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), _prismData);

            List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, _fireRune, new Vector2Int(0, 2), ConduitDirection.East, 4, GetPrism);

            ConduitBeamPath root = paths[0];
            ConduitBeamPath branchA = paths[1];
            ConduitBeamPath branchB = paths[2];

            Assert.AreEqual(root.BeamId, branchA.ParentBeamId);
            Assert.AreEqual(root.BeamId, branchB.ParentBeamId);
            Assert.AreNotEqual(branchA.BeamId, branchB.BeamId);
            Assert.AreEqual(1, branchA.Depth);
            Assert.AreEqual(1, branchB.Depth);
        }

        [Test]
        public void Prism_CircularLoopProtection_TerminatesSafely()
        {
            // Setup two facing prisms: Prism 1 at (2,2), Prism 2 at (2,3)
            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => 
                (coord == new Vector2Int(2, 2) || coord == new Vector2Int(2, 3), _prismData);

            List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, _fireRune, new Vector2Int(0, 2), ConduitDirection.East, 5, GetPrism);

            // Must terminate without stack overflow or infinite loops
            Assert.IsTrue(paths.Count > 0 && paths.Count <= 10);
        }
    }
}
