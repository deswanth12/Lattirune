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
    public class CrossfireIntersectionTests
    {
        private GameObject _holderObj;
        private LatticeGrid _grid;
        private ElementalReactionDatabaseSO _reactionDatabase;
        private ElementalReactionSystem _reactionSystem;
        private SynergyDatabaseSO _synergyDatabase;
        private SynergySystem _synergySystem;
        private RuneData _crossfireFire;
        private RuneData _iceRune;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("CrossfireIntersectionTestHolder");
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _reactionDatabase = ElementalReactionDatabaseSO.CreateDefaultDatabase();
            _reactionSystem = _holderObj.AddComponent<ElementalReactionSystem>();
            _reactionSystem.Initialize(_reactionDatabase);

            _synergyDatabase = SynergyDatabaseSO.CreateDefaultDatabase();
            _synergySystem = _holderObj.AddComponent<SynergySystem>();
            _synergySystem.Initialize(_synergyDatabase);

            _crossfireFire = ScriptableObject.CreateInstance<RuneData>();
            _crossfireFire.Initialize("crossfire_fire", "Crossfire Fire", ConduitDirection.Cross, ElementType.Fire, 4);

            _iceRune = ScriptableObject.CreateInstance<RuneData>();
            _iceRune.Initialize("ice_east", "Ice East", ConduitDirection.East, ElementType.Ice, 4);
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
        public void Crossfire_SelfBeams_DoNotIntersectEachOther()
        {
            // All 4 cardinal beams originate from the same Crossfire rune at (2,2)
            List<ConduitBeamPath> beams = MultiDirectionalEmitter.EmitBeams(_grid, _crossfireFire, new Vector2Int(2, 2), 4);

            _reactionSystem.UpdateReactions(beams);

            Assert.AreEqual(0, _reactionSystem.ActiveReactionCount);
        }

        [Test]
        public void CrossfireFire_CrossingIceBeam_TriggersSteamReaction()
        {
            // Crossfire at (2,1) emitting North through (2,2), (2,3), (2,4)
            List<ConduitBeamPath> fireBeams = MultiDirectionalEmitter.EmitBeams(_grid, _crossfireFire, new Vector2Int(2, 1), 3);

            // Ice Rune at (0,3) emitting East through (1,3), (2,3), (3,3), (4,3)
            List<ConduitBeamPath> iceBeams = MultiDirectionalEmitter.EmitBeams(_grid, _iceRune, new Vector2Int(0, 3), 4);

            List<ConduitBeamPath> combined = new List<ConduitBeamPath>();
            combined.AddRange(fireBeams);
            combined.AddRange(iceBeams);

            _reactionSystem.UpdateReactions(combined);

            Assert.AreEqual(1, _reactionSystem.ActiveReactionCount);
            foreach (var r in _reactionSystem.ActiveReactions)
            {
                Assert.AreEqual("reaction_steam", r.ReactionId);
                Assert.AreEqual(new Vector2Int(2, 3), r.GridCoordinate);
            }
        }

        [Test]
        public void CrossfireFire_BeamsIntersectWeapons_ActivatesMultipleSynergies()
        {
            // Crossfire at (2,2). Emits North to (2,3), South to (2,1), East to (3,2), West to (1,2).
            List<ConduitBeamPath> fireBeams = MultiDirectionalEmitter.EmitBeams(_grid, _crossfireFire, new Vector2Int(2, 2), 2);

            ItemDataSO swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            swordData.Initialize("item_sword", "Sword", "Weapon", ItemCategory.Weapon, new Vector2Int(1, 1), false, Color.yellow);

            // Place Sword A at (2,3) [North] and Sword B at (3,2) [East]
            ItemInstance swordA = ItemFactory.CreateInstance(swordData, Vector3.zero, _holderObj.transform);
            swordA.OnPlaced(new Vector2Int(2, 3), Vector3.zero);

            ItemInstance swordB = ItemFactory.CreateInstance(swordData, Vector3.zero, _holderObj.transform);
            swordB.OnPlaced(new Vector2Int(3, 2), Vector3.zero);

            _synergySystem.UpdateSynergies(fireBeams, new List<ItemInstance> { swordA, swordB });

            Assert.IsTrue(swordA.HasActiveSynergy);
            Assert.AreEqual("fire_sword", swordA.ActiveSynergyId);

            Assert.IsTrue(swordB.HasActiveSynergy);
            Assert.AreEqual("fire_sword", swordB.ActiveSynergyId);
        }
    }
}
