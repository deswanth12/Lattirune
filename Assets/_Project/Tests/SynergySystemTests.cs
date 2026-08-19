using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Runes;
using Lattirune.Synergy;

namespace Lattirune.Tests
{
    [TestFixture]
    public class SynergySystemTests
    {
        private LatticeGrid _grid;
        private SynergySystem _synergySystem;
        private RuneData _fireRuneData;
        private RuneData _iceRuneData;
        private ItemDataSO _trainingSwordData;
        private ItemDataSO _guardPlateData;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            GameObject sysObj = new GameObject("TestSynergySystem");
            _synergySystem = sysObj.AddComponent<SynergySystem>();
            _synergySystem.EnsureDefaultDefinitions();

            _fireRuneData = ScriptableObject.CreateInstance<RuneData>();
            _fireRuneData.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);

            _iceRuneData = ScriptableObject.CreateInstance<RuneData>();
            _iceRuneData.Initialize("ice_rune_01", "Ice Rune", ConduitDirection.North, ElementType.Ice, 3);

            _trainingSwordData = ScriptableObject.CreateInstance<ItemDataSO>();
            _trainingSwordData.Initialize("item_training_sword", "Training Sword", "Basic Sword", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);

            _guardPlateData = ScriptableObject.CreateInstance<ItemDataSO>();
            _guardPlateData.Initialize("item_guard_plate", "Guard Plate", "Shield", ItemCategory.Shield, new Vector2Int(2, 2), true, Color.blue);
        }

        [TearDown]
        public void Teardown()
        {
            if (_synergySystem != null)
            {
                Object.DestroyImmediate(_synergySystem.gameObject);
            }
        }

        [Test]
        public void Synergy_FireRuneAndSword_ActivatesFlameboundEdge()
        {
            // Place Sword at (2,2) with footprint 1x2 (covers (2,2) and (2,3))
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            // Fire Rune at (2,1) emitting North
            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            SynergyResult result = _synergySystem.EvaluateConnection(_fireRuneData, runePos, conduit, sword);

            Assert.IsTrue(result.IsSynergyActive);
            Assert.AreEqual("fire_sword", result.SynergyId);
            Assert.AreEqual("Flamebound Edge", result.SynergyName);
            Assert.AreEqual("fire_rune_01", result.RuneId);
            Assert.AreEqual("item_training_sword", result.TargetItemId);
            Assert.AreEqual(sword.InstanceId, result.TargetInstanceId);
        }

        [Test]
        public void Synergy_FireRuneAndNonSword_DoesNotActivate()
        {
            // Place Guard Plate at (2,2) with footprint 2x2
            ItemInstance plate = ItemFactory.CreateInstance(_guardPlateData, Vector3.zero);
            _grid.PlaceItem(plate.InstanceId, new Vector2Int(2, 2), plate.CurrentDimensions);
            plate.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            SynergyResult result = _synergySystem.EvaluateConnection(_fireRuneData, runePos, conduit, plate);

            Assert.IsFalse(result.IsSynergyActive);
            Assert.IsNull(result.SynergyId);
        }

        [Test]
        public void Synergy_FireRuneOutsideRange_DoesNotActivate()
        {
            // Place Sword at (2,4)
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(2, 4), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(2, 4), Vector3.zero);

            // Fire Rune at (2,1) emitting North with range 1 (only reaches (2,2))
            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 1);

            SynergyResult result = _synergySystem.EvaluateConnection(_fireRuneData, runePos, conduit, sword);

            Assert.IsFalse(result.IsSynergyActive);
        }

        [Test]
        public void Synergy_ItemDataSO_RemainsImmutableDuringEvaluation()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            _synergySystem.EvaluateConnection(_fireRuneData, runePos, conduit, sword);

            // Verify ItemDataSO properties did not mutate
            Assert.AreEqual("item_training_sword", _trainingSwordData.ItemId);
            Assert.AreEqual(ItemCategory.Weapon, _trainingSwordData.Category);
            Assert.AreEqual(new Vector2Int(1, 2), _trainingSwordData.BaseDimensions);
        }

        [Test]
        public void Synergy_MultipleSwords_OnlyActivatesConnectedSword()
        {
            // Sword A at (2,2) - connected to North conduit from (2,1)
            ItemInstance swordA = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(swordA.InstanceId, new Vector2Int(2, 2), swordA.CurrentDimensions);
            swordA.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            // Sword B at (4,2) - outside conduit path
            ItemInstance swordB = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(swordB.InstanceId, new Vector2Int(4, 2), swordB.CurrentDimensions);
            swordB.OnPlaced(new Vector2Int(4, 2), Vector3.zero);

            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            var activeConduits = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneData, runePos, conduit)
            };
            var activeItems = new List<ItemInstance> { swordA, swordB };

            _synergySystem.UpdateSynergies(activeConduits, activeItems);

            Assert.IsTrue(swordA.HasActiveSynergy);
            Assert.AreEqual("fire_sword", swordA.ActiveSynergyId);

            Assert.IsFalse(swordB.HasActiveSynergy);
            Assert.IsNull(swordB.ActiveSynergyId);
        }

        [Test]
        public void Synergy_MovingSwordAway_DeactivatesSynergyCleanly()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            var activeConduits = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneData, runePos, conduit)
            };
            var activeItems = new List<ItemInstance> { sword };

            // 1. Initial connection -> activates
            _synergySystem.UpdateSynergies(activeConduits, activeItems);
            Assert.IsTrue(sword.HasActiveSynergy);

            // 2. Move sword away to (4,2)
            _grid.RemoveItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(4, 2), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(4, 2), Vector3.zero);

            // 3. Update synergies -> deactivates cleanly
            _synergySystem.UpdateSynergies(activeConduits, activeItems);
            Assert.IsFalse(sword.HasActiveSynergy);
            Assert.IsNull(sword.ActiveSynergyId);
        }

        [Test]
        public void Synergy_Events_FireActivationAndDeactivationExactlyOnce()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            var activeConduits = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRuneData, runePos, conduit)
            };
            var activeItems = new List<ItemInstance> { sword };

            int activationCount = 0;
            int deactivationCount = 0;

            _synergySystem.OnSynergyActivated += (res) => activationCount++;
            _synergySystem.OnSynergyDeactivated += (res) => deactivationCount++;

            // Pass 1: Activates
            _synergySystem.UpdateSynergies(activeConduits, activeItems);
            Assert.AreEqual(1, activationCount);
            Assert.AreEqual(0, deactivationCount);

            // Pass 2: Re-evaluation without changes -> no duplicate events
            _synergySystem.UpdateSynergies(activeConduits, activeItems);
            Assert.AreEqual(1, activationCount);
            Assert.AreEqual(0, deactivationCount);

            // Pass 3: Disconnect (remove item from grid)
            _grid.RemoveItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            sword.OnRemoved(Vector3.zero);

            _synergySystem.UpdateSynergies(activeConduits, activeItems);
            Assert.AreEqual(1, activationCount);
            Assert.AreEqual(1, deactivationCount);
        }

        [Test]
        public void Synergy_NonFireRune_DoesNotTriggerFireSword()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            _grid.PlaceItem(sword.InstanceId, new Vector2Int(2, 2), sword.CurrentDimensions);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            Vector2Int runePos = new Vector2Int(2, 1);
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, runePos, ConduitDirection.North, 3);

            SynergyResult result = _synergySystem.EvaluateConnection(_iceRuneData, runePos, conduit, sword);

            Assert.IsFalse(result.IsSynergyActive);
        }
    }
}
