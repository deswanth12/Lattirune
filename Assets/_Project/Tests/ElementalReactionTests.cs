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
    public class ElementalReactionTests
    {
        private GameObject _holderObj;
        private ElementalReactionDatabaseSO _database;
        private ElementalReactionSystem _reactionSystem;
        private LatticeGrid _grid;

        private RuneData _fireRune;
        private RuneData _iceRune;
        private RuneData _lightningRune;
        private RuneData _poisonRune;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ElementalReactionTestHolder");
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _database = ElementalReactionDatabaseSO.CreateDefaultDatabase();
            _reactionSystem = _holderObj.AddComponent<ElementalReactionSystem>();
            _reactionSystem.Initialize(_database);

            _fireRune = ScriptableObject.CreateInstance<RuneData>();
            _fireRune.Initialize("rune_fire", "Fire Rune", ConduitDirection.North, ElementType.Fire, 4);

            _iceRune = ScriptableObject.CreateInstance<RuneData>();
            _iceRune.Initialize("rune_ice", "Ice Rune", ConduitDirection.East, ElementType.Ice, 4);

            _lightningRune = ScriptableObject.CreateInstance<RuneData>();
            _lightningRune.Initialize("rune_lightning", "Lightning Rune", ConduitDirection.East, ElementType.Lightning, 4);

            _poisonRune = ScriptableObject.CreateInstance<RuneData>();
            _poisonRune.Initialize("rune_poison", "Poison Rune", ConduitDirection.East, ElementType.Poison, 4);
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
        public void ElementalReaction_FirePlusIce_ResolvesSteam()
        {
            ElementalReactionDefinitionSO def = _database.FindReaction(ElementType.Fire, ElementType.Ice);
            Assert.IsNotNull(def);
            Assert.AreEqual("reaction_steam", def.ReactionId);
            Assert.AreEqual("Steam", def.DisplayName);
        }

        [Test]
        public void ElementalReaction_FirePlusLightning_ResolvesPlasma()
        {
            ElementalReactionDefinitionSO def = _database.FindReaction(ElementType.Fire, ElementType.Lightning);
            Assert.IsNotNull(def);
            Assert.AreEqual("reaction_plasma", def.ReactionId);
            Assert.AreEqual("Plasma", def.DisplayName);
        }

        [Test]
        public void ElementalReaction_FirePlusPoison_ResolvesToxicFlame()
        {
            ElementalReactionDefinitionSO def = _database.FindReaction(ElementType.Fire, ElementType.Poison);
            Assert.IsNotNull(def);
            Assert.AreEqual("reaction_toxic_flame", def.ReactionId);
            Assert.AreEqual("Toxic Flame", def.DisplayName);
        }

        [Test]
        public void ElementalReaction_LightningPlusIce_ResolvesSuperconductor()
        {
            ElementalReactionDefinitionSO def = _database.FindReaction(ElementType.Lightning, ElementType.Ice);
            Assert.IsNotNull(def);
            Assert.AreEqual("reaction_superconductor", def.ReactionId);
            Assert.AreEqual("Superconductor", def.DisplayName);
        }

        [Test]
        public void ElementalReaction_IcePlusPoison_ResolvesFrostbite()
        {
            ElementalReactionDefinitionSO def = _database.FindReaction(ElementType.Ice, ElementType.Poison);
            Assert.IsNotNull(def);
            Assert.AreEqual("reaction_frostbite", def.ReactionId);
            Assert.AreEqual("Frostbite", def.DisplayName);
        }

        [Test]
        public void ElementalReaction_SymmetricLookup_ResolvesIdentically()
        {
            ElementalReactionDefinitionSO defAB = _database.FindReaction(ElementType.Fire, ElementType.Ice);
            ElementalReactionDefinitionSO defBA = _database.FindReaction(ElementType.Ice, ElementType.Fire);

            Assert.AreEqual(defAB.ReactionId, defBA.ReactionId);
        }

        [Test]
        public void ElementalReaction_UnknownPair_ReturnsNullSafely()
        {
            ElementalReactionDefinitionSO def = _database.FindReaction(ElementType.Light, ElementType.Physical);
            Assert.IsNull(def);
        }

        [Test]
        public void ElementalReactionSystem_CrossingBeams_ActivatesAndDeactivatesReaction()
        {
            RuneConduitResult conduitNorth = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 0), ConduitDirection.North, 4);
            RuneConduitResult conduitEast = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(0, 2), ConduitDirection.East, 4);

            var active = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRune, new Vector2Int(2, 0), conduitNorth),
                (_iceRune, new Vector2Int(0, 2), conduitEast)
            };

            bool activated = false;
            _reactionSystem.OnReactionActivated += res => activated = true;

            _reactionSystem.UpdateReactions(active);

            Assert.AreEqual(1, _reactionSystem.ActiveReactionCount);
            Assert.IsTrue(activated);

            // Break conduit by removing the Ice rune
            bool deactivated = false;
            _reactionSystem.OnReactionDeactivated += res => deactivated = true;

            var activeRemaining = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRune, new Vector2Int(2, 0), conduitNorth)
            };

            _reactionSystem.UpdateReactions(activeRemaining);

            Assert.AreEqual(0, _reactionSystem.ActiveReactionCount);
            Assert.IsTrue(deactivated);
        }

        [Test]
        public void ElementalReaction_DatabaseValidation_DetectsDuplicates()
        {
            List<string> errors;
            Assert.IsTrue(_database.ValidateDatabase(out errors));
            Assert.AreEqual(0, errors.Count);

            // Try registering duplicate reaction ID
            ElementalReactionDefinitionSO duplicate = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            duplicate.Initialize("reaction_steam", "Duplicate Steam", "Desc", ElementType.Fire, ElementType.Ice, Color.white);
            _database.Register(duplicate);

            Assert.AreEqual(5, _database.Count);
        }
    }
}
