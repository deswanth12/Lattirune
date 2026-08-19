using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    [TestFixture]
    public class CrossfireConduitTests
    {
        private LatticeGrid _grid;
        private RuneData _crossfireRune;
        private RuneData _omniRune;
        private PrismRuneDataSO _prismData;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _crossfireRune = ScriptableObject.CreateInstance<RuneData>();
            _crossfireRune.Initialize("rune_crossfire", "Crossfire Rune", ConduitDirection.Cross, ElementType.Fire, 4);

            _omniRune = ScriptableObject.CreateInstance<RuneData>();
            _omniRune.Initialize("rune_omni", "Amplifier Node", ConduitDirection.Omni, ElementType.Force, 4);

            _prismData = ScriptableObject.CreateInstance<PrismRuneDataSO>();
            _prismData.Initialize("prism_test", "Test Prism", branchCount: 2, maxDepth: 3);
        }

        [Test]
        public void CrossfireData_ValidatesCorrectly()
        {
            Assert.IsTrue(_crossfireRune.IsValid(out string err));
            Assert.IsNull(err);
            Assert.AreEqual(ConduitDirection.Cross, _crossfireRune.Direction);
            Assert.AreEqual(ElementType.Fire, _crossfireRune.Element);
        }

        [Test]
        public void CrossfireRune_EmitsAllFourCardinalDirections()
        {
            // Position at center (2,2) with range 2
            List<ConduitBeamPath> beams = MultiDirectionalEmitter.EmitBeams(
                _grid, _crossfireRune, new Vector2Int(2, 2), range: 2);

            Assert.AreEqual(4, beams.Count);

            HashSet<ConduitDirection> dirs = new HashSet<ConduitDirection>();
            foreach (var b in beams)
            {
                dirs.Add(b.Direction);
                Assert.AreEqual(new Vector2Int(2, 2), b.Origin);
            }

            Assert.IsTrue(dirs.Contains(ConduitDirection.North));
            Assert.IsTrue(dirs.Contains(ConduitDirection.South));
            Assert.IsTrue(dirs.Contains(ConduitDirection.East));
            Assert.IsTrue(dirs.Contains(ConduitDirection.West));
        }

        [Test]
        public void OmniRune_EmitsAllFourCardinalDirections()
        {
            List<ConduitBeamPath> beams = MultiDirectionalEmitter.EmitBeams(
                _grid, _omniRune, new Vector2Int(2, 2), range: 2);

            Assert.AreEqual(4, beams.Count);
        }

        [Test]
        public void Crossfire_BeamIds_AreDeterministicAndUnique()
        {
            List<ConduitBeamPath> beams = MultiDirectionalEmitter.EmitBeams(
                _grid, _crossfireRune, new Vector2Int(2, 2), range: 2);

            HashSet<string> seenIds = new HashSet<string>();
            foreach (var b in beams)
            {
                Assert.IsFalse(string.IsNullOrEmpty(b.BeamId));
                Assert.IsTrue(seenIds.Add(b.BeamId), $"Duplicate beam ID found: {b.BeamId}");
            }
        }

        [Test]
        public void Crossfire_EachBeamRespectsRange_AndBoundaries()
        {
            // Origin at (0, 2).
            // West beam immediately hits boundary (x=-1 out of bounds).
            // East beam can travel up to range 4 -> (1,2), (2,2), (3,2), (4,2).
            List<ConduitBeamPath> beams = MultiDirectionalEmitter.EmitBeams(
                _grid, _crossfireRune, new Vector2Int(0, 2), range: 4);

            Assert.AreEqual(4, beams.Count);

            ConduitBeamPath westBeam = beams.Find(b => b.Direction == ConduitDirection.West);
            Assert.IsNotNull(westBeam);
            Assert.AreEqual(0, westBeam.TraversalLength);
            Assert.AreEqual(ConduitTerminationReason.GridBoundary, westBeam.TerminationReason);

            ConduitBeamPath eastBeam = beams.Find(b => b.Direction == ConduitDirection.East);
            Assert.IsNotNull(eastBeam);
            Assert.AreEqual(4, eastBeam.TraversalLength);
        }

        [Test]
        public void Crossfire_BeamsHitPrismAndRefract()
        {
            // Crossfire at (2, 2). East beam goes towards (4,2).
            // Place Prism at (3, 2).
            // East beam from Crossfire hits Prism and refracts North & South!
            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(3, 2), _prismData);

            List<ConduitBeamPath> beams = MultiDirectionalEmitter.EmitBeams(
                _grid, _crossfireRune, new Vector2Int(2, 2), range: 3, getPrismAtCell: GetPrism);

            // 4 root beams (North, South, East, West) + 2 child branches from East beam refract
            Assert.AreEqual(6, beams.Count);

            int splitBranchCount = beams.FindAll(b => b.IsSplitBranch).Count;
            Assert.AreEqual(2, splitBranchCount);
        }

        [Test]
        public void Crossfire_MultipleEmitters_OperateIndependently()
        {
            RuneData secondCrossfire = ScriptableObject.CreateInstance<RuneData>();
            secondCrossfire.Initialize("rune_crossfire_2", "Crossfire 2", ConduitDirection.Cross, ElementType.Fire, 2);

            var emitters = new List<(RuneData, Vector2Int, int)>
            {
                (_crossfireRune, new Vector2Int(1, 2), 2),
                (secondCrossfire, new Vector2Int(3, 2), 2)
            };

            List<ConduitBeamPath> allBeams = MultiDirectionalEmitter.EmitAllActiveBeams(_grid, emitters);

            Assert.AreEqual(8, allBeams.Count); // 4 beams each
        }
    }
}
