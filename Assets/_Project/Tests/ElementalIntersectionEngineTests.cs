using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Reactions;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    [TestFixture]
    public class ElementalIntersectionEngineTests
    {
        private LatticeGrid _grid;
        private RuneData _fireRuneNorth;
        private RuneData _iceRuneEast;
        private RuneData _lightningRuneSouth;
        private RuneData _poisonRuneWest;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _fireRuneNorth = ScriptableObject.CreateInstance<RuneData>();
            _fireRuneNorth.Initialize("rune_fire_n", "Fire North", ConduitDirection.North, ElementType.Fire, 4);

            _iceRuneEast = ScriptableObject.CreateInstance<RuneData>();
            _iceRuneEast.Initialize("rune_ice_e", "Ice East", ConduitDirection.East, ElementType.Ice, 4);

            _lightningRuneSouth = ScriptableObject.CreateInstance<RuneData>();
            _lightningRuneSouth.Initialize("rune_lightning_s", "Lightning South", ConduitDirection.South, ElementType.Lightning, 4);

            _poisonRuneWest = ScriptableObject.CreateInstance<RuneData>();
            _poisonRuneWest.Initialize("rune_poison_w", "Poison West", ConduitDirection.West, ElementType.Poison, 4);
        }

        [Test]
        public void IntersectionEngine_EastAndNorth_DetectsCrossing()
        {
            // Fire at (2,0) North -> passes (2,1), (2,2), (2,3), (2,4)
            RuneConduitResult conduitNorth = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 0), ConduitDirection.North, 4);
            // Ice at (0,2) East -> passes (1,2), (2,2), (3,2), (4,2)
            RuneConduitResult conduitEast = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(0, 2), ConduitDirection.East, 4);

            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneNorth, new Vector2Int(2, 0), conduitNorth),
                (_iceRuneEast, new Vector2Int(0, 2), conduitEast)
            };

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(active);

            Assert.AreEqual(1, intersections.Count);
            Assert.AreEqual(new Vector2Int(2, 2), intersections[0].GridCoordinate);
        }

        [Test]
        public void IntersectionEngine_EastAndSouth_DetectsCrossing()
        {
            // Lightning at (2,4) South -> passes (2,3), (2,2), (2,1), (2,0)
            RuneConduitResult conduitSouth = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 4), ConduitDirection.South, 4);
            // Ice at (0,2) East -> passes (1,2), (2,2), (3,2), (4,2)
            RuneConduitResult conduitEast = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(0, 2), ConduitDirection.East, 4);

            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_lightningRuneSouth, new Vector2Int(2, 4), conduitSouth),
                (_iceRuneEast, new Vector2Int(0, 2), conduitEast)
            };

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(active);

            Assert.AreEqual(1, intersections.Count);
            Assert.AreEqual(new Vector2Int(2, 2), intersections[0].GridCoordinate);
        }

        [Test]
        public void IntersectionEngine_WestAndNorth_DetectsCrossing()
        {
            // Fire at (2,0) North -> passes (2,1), (2,2), (2,3), (2,4)
            RuneConduitResult conduitNorth = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 0), ConduitDirection.North, 4);
            // Poison at (4,2) West -> passes (3,2), (2,2), (1,2), (0,2)
            RuneConduitResult conduitWest = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(4, 2), ConduitDirection.West, 4);

            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneNorth, new Vector2Int(2, 0), conduitNorth),
                (_poisonRuneWest, new Vector2Int(4, 2), conduitWest)
            };

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(active);

            Assert.AreEqual(1, intersections.Count);
            Assert.AreEqual(new Vector2Int(2, 2), intersections[0].GridCoordinate);
        }

        [Test]
        public void IntersectionEngine_ParallelBeams_RejectedAsCrossing()
        {
            // Two East beams on different rows or same row
            RuneConduitResult east1 = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(0, 2), ConduitDirection.East, 4);
            RuneConduitResult east2 = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(0, 3), ConduitDirection.East, 4);

            RuneData secondEastRune = ScriptableObject.CreateInstance<RuneData>();
            secondEastRune.Initialize("rune_ice_e2", "Ice East 2", ConduitDirection.East, ElementType.Ice, 4);

            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_iceRuneEast, new Vector2Int(0, 2), east1),
                (secondEastRune, new Vector2Int(0, 3), east2)
            };

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(active);
            Assert.AreEqual(0, intersections.Count);
        }

        [Test]
        public void IntersectionEngine_SameRuneId_RejectsSelfIntersection()
        {
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 0), ConduitDirection.North, 4);
            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneNorth, new Vector2Int(2, 0), conduit),
                (_fireRuneNorth, new Vector2Int(2, 0), conduit)
            };

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(active);
            Assert.AreEqual(0, intersections.Count);
        }

        [Test]
        public void IntersectionEngine_NonIntersectingBeams_ReturnsEmpty()
        {
            // Fire at (1,0) North -> x=1
            RuneConduitResult conduitNorth = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(1, 0), ConduitDirection.North, 2); // reaches y=1, 2
            // Ice at (2,4) East -> y=4, x=3,4
            RuneConduitResult conduitEast = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 4), ConduitDirection.East, 2);

            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneNorth, new Vector2Int(1, 0), conduitNorth),
                (_iceRuneEast, new Vector2Int(2, 4), conduitEast)
            };

            List<BeamIntersection> intersections = ElementalIntersectionEngine.FindIntersections(active);
            Assert.AreEqual(0, intersections.Count);
        }
    }
}
