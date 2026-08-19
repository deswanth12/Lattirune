using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Synergy;

namespace Lattirune.Tests
{
    [TestFixture]
    public class PrismIntersectionTests
    {
        private GameObject _holderObj;
        private LatticeGrid _grid;
        private ElementalReactionDatabaseSO _reactionDatabase;
        private ElementalReactionSystem _reactionSystem;
        private SynergyDatabaseSO _synergyDatabase;
        private SynergySystem _synergySystem;
        private PrismRuneDataSO _prismData;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("PrismIntersectionTestHolder");
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _reactionDatabase = ElementalReactionDatabaseSO.CreateDefaultDatabase();
            _reactionSystem = _holderObj.AddComponent<ElementalReactionSystem>();
            _reactionSystem.Initialize(_reactionDatabase);

            _synergyDatabase = SynergyDatabaseSO.CreateDefaultDatabase();
            _synergySystem = _holderObj.AddComponent<SynergySystem>();
            _synergySystem.Initialize(_synergyDatabase);

            _prismData = ScriptableObject.CreateInstance<PrismRuneDataSO>();
            _prismData.Initialize("prism_test", "Test Prism", branchCount: 2, maxDepth: 3);
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
        public void PrismBranch_FirePlusIce_TriggersSteamReaction()
        {
            // Fire Rune at (0,2) East. Hits Prism at (2,2).
            // Prism refracts North branch through (2,3), (2,4).
            // Ice Rune at (0,3) East passes through (1,3), (2,3), (3,3).
            // Crossing between Fire North Branch and Ice East Beam occurs at (2,3)!
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("rune_fire", "Fire Rune", ConduitDirection.East, ElementType.Fire, 4);

            RuneData iceRune = ScriptableObject.CreateInstance<RuneData>();
            iceRune.Initialize("rune_ice", "Ice Rune", ConduitDirection.East, ElementType.Ice, 4);

            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), _prismData);

            List<ConduitBeamPath> firePaths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, fireRune, new Vector2Int(0, 2), ConduitDirection.East, 4, GetPrism);

            List<ConduitBeamPath> icePaths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, iceRune, new Vector2Int(0, 3), ConduitDirection.East, 4, GetPrism);

            List<ConduitBeamPath> allBeams = new List<ConduitBeamPath>();
            allBeams.AddRange(firePaths);
            allBeams.AddRange(icePaths);

            _reactionSystem.UpdateReactions(allBeams);

            Assert.AreEqual(1, _reactionSystem.ActiveReactionCount);
            foreach (var reaction in _reactionSystem.ActiveReactions)
            {
                Assert.AreEqual("reaction_steam", reaction.ReactionId);
                Assert.AreEqual(new Vector2Int(2, 3), reaction.GridCoordinate);
            }
        }

        [Test]
        public void PrismBranch_PowersWeapon_ActivatesSynergy()
        {
            // Fire Rune at (0,2) East splits at Prism (2,2) into North branch (2,3), (2,4).
            // Sword placed at (2,3).
            // Synergy system evaluates connection and activates Flamebound Edge!
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("rune_fire", "Fire Rune", ConduitDirection.East, ElementType.Fire, 4);

            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), _prismData);

            List<ConduitBeamPath> firePaths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, fireRune, new Vector2Int(0, 2), ConduitDirection.East, 4, GetPrism);

            ItemDataSO swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            swordData.Initialize("item_sword", "Sword", "Weapon", ItemCategory.Weapon, new Vector2Int(1, 1), false, Color.yellow);
            ItemInstance sword = ItemFactory.CreateInstance(swordData, Vector3.zero, _holderObj.transform);
            sword.OnPlaced(new Vector2Int(2, 3), Vector3.zero);

            _synergySystem.UpdateSynergies(firePaths, new List<ItemInstance> { sword });

            Assert.IsTrue(sword.HasActiveSynergy);
            Assert.AreEqual("fire_sword", sword.ActiveSynergyId);
        }

        [Test]
        public void PrismBranch_SelfBranches_DoNotIntersectEachOther()
        {
            // Fire Rune splitting into North and South branches from (2,2) cannot self-intersect
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("rune_fire", "Fire Rune", ConduitDirection.East, ElementType.Fire, 4);

            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), _prismData);

            List<ConduitBeamPath> firePaths = RuneConduitEngine.CalculateConduitWithRefraction(
                _grid, fireRune, new Vector2Int(0, 2), ConduitDirection.East, 4, GetPrism);

            _reactionSystem.UpdateReactions(firePaths);

            Assert.AreEqual(0, _reactionSystem.ActiveReactionCount);
        }
    }
}
