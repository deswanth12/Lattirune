using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;

namespace Lattirune.Tests
{
    [TestFixture]
    public class ItemDataTests
    {
        private LatticeGrid _grid;
        private ItemDataSO _trainingSwordData;
        private ItemDataSO _emberBladeData;
        private ItemDataSO _guardPlateData;
        private ItemDataSO _arcaneRelicData;
        private ItemDataSO _vitalFlaskData;

        [SetUp]
        public void Setup()
        {
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _trainingSwordData = ScriptableObject.CreateInstance<ItemDataSO>();
            _trainingSwordData.Initialize("item_training_sword", "Training Sword", "Basic weapon", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);

            _emberBladeData = ScriptableObject.CreateInstance<ItemDataSO>();
            _emberBladeData.Initialize("item_ember_blade", "Ember Blade", "Fire blade", ItemCategory.Weapon, new Vector2Int(2, 1), true, Color.red);

            _guardPlateData = ScriptableObject.CreateInstance<ItemDataSO>();
            _guardPlateData.Initialize("item_guard_plate", "Guard Plate", "Shield plate", ItemCategory.Shield, new Vector2Int(2, 2), true, Color.blue);

            _arcaneRelicData = ScriptableObject.CreateInstance<ItemDataSO>();
            _arcaneRelicData.Initialize("item_arcane_relic", "Arcane Relic", "Ancient relic", ItemCategory.Relic, new Vector2Int(1, 1), false, Color.magenta);

            _vitalFlaskData = ScriptableObject.CreateInstance<ItemDataSO>();
            _vitalFlaskData.Initialize("item_vital_flask", "Vital Flask", "Potion", ItemCategory.Consumable, new Vector2Int(1, 1), false, Color.green);
        }

        [Test]
        public void ItemDataSO_InitializesAndValidatesCorrectly()
        {
            Assert.AreEqual("item_training_sword", _trainingSwordData.ItemId);
            Assert.AreEqual(new Vector2Int(1, 2), _trainingSwordData.BaseDimensions);
            Assert.IsTrue(_trainingSwordData.RotationAllowed);
            Assert.IsTrue(_trainingSwordData.IsValid(out string error));
            Assert.IsNull(error);
        }

        [Test]
        public void ItemDataSO_Validation_RejectsInvalidConfigurations()
        {
            ItemDataSO emptyIdItem = ScriptableObject.CreateInstance<ItemDataSO>();
            emptyIdItem.Initialize("", "Empty", "No ID", ItemCategory.Weapon, new Vector2Int(1, 1), false, Color.white);
            Assert.IsFalse(emptyIdItem.IsValid(out string error1));
            Assert.IsNotNull(error1);

            ItemDataSO oversizedItem = ScriptableObject.CreateInstance<ItemDataSO>();
            oversizedItem.Initialize("oversized", "Big", "Too big", ItemCategory.Weapon, new Vector2Int(6, 6), false, Color.white);
            Assert.IsFalse(oversizedItem.IsValid(out string error2));
            Assert.IsNotNull(error2);
        }

        [Test]
        public void ItemDatabaseSO_DetectsDuplicateItemIds()
        {
            ItemDatabaseSO db = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            
            // Create duplicate item with same ID as training sword
            ItemDataSO duplicateSword = ScriptableObject.CreateInstance<ItemDataSO>();
            duplicateSword.Initialize("item_training_sword", "Duplicate Sword", "Dupe", ItemCategory.Weapon, new Vector2Int(1, 1), false, Color.yellow);

            db.Initialize(new List<ItemDataSO> { _trainingSwordData, duplicateSword });
            bool isValid = db.ValidateDatabase(out List<string> errors);

            Assert.IsFalse(isValid);
            Assert.IsTrue(errors.Count > 0);
        }

        [Test]
        public void RotationUtility_CalculatesRotatedFootprintsAccurately()
        {
            // 1x1 remains 1x1 across all angles
            Vector2Int dim1x1 = new Vector2Int(1, 1);
            Assert.AreEqual(new Vector2Int(1, 1), ItemRotationUtility.GetRotatedDimensions(dim1x1, 0));
            Assert.AreEqual(new Vector2Int(1, 1), ItemRotationUtility.GetRotatedDimensions(dim1x1, 90));
            Assert.AreEqual(new Vector2Int(1, 1), ItemRotationUtility.GetRotatedDimensions(dim1x1, 180));
            Assert.AreEqual(new Vector2Int(1, 1), ItemRotationUtility.GetRotatedDimensions(dim1x1, 270));

            // 1x2 rotates to 2x1 at 90° and 270°
            Vector2Int dim1x2 = new Vector2Int(1, 2);
            Assert.AreEqual(new Vector2Int(1, 2), ItemRotationUtility.GetRotatedDimensions(dim1x2, 0));
            Assert.AreEqual(new Vector2Int(2, 1), ItemRotationUtility.GetRotatedDimensions(dim1x2, 90));
            Assert.AreEqual(new Vector2Int(1, 2), ItemRotationUtility.GetRotatedDimensions(dim1x2, 180));
            Assert.AreEqual(new Vector2Int(2, 1), ItemRotationUtility.GetRotatedDimensions(dim1x2, 270));

            // 2x2 remains 2x2
            Vector2Int dim2x2 = new Vector2Int(2, 2);
            Assert.AreEqual(new Vector2Int(2, 2), ItemRotationUtility.GetRotatedDimensions(dim2x2, 90));
        }

        [Test]
        public void ItemInstance_Rotation_UpdatesRuntimeDimensions()
        {
            ItemInstance swordInstance = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            Assert.AreEqual(new Vector2Int(1, 2), swordInstance.CurrentDimensions);
            Assert.AreEqual(0, swordInstance.CurrentRotationDegrees);

            // Rotate 90° -> 2x1
            bool rotated = swordInstance.Rotate90();
            Assert.IsTrue(rotated);
            Assert.AreEqual(90, swordInstance.CurrentRotationDegrees);
            Assert.AreEqual(new Vector2Int(2, 1), swordInstance.CurrentDimensions);

            // Non-rotatable relic
            ItemInstance relicInstance = ItemFactory.CreateInstance(_arcaneRelicData, Vector3.zero);
            bool relicRotated = relicInstance.Rotate90();
            Assert.IsFalse(relicRotated);
            Assert.AreEqual(0, relicInstance.CurrentRotationDegrees);
            Assert.AreEqual(new Vector2Int(1, 1), relicInstance.CurrentDimensions);
        }

        [Test]
        public void MultipleItemInstances_MaintainIndependentRuntimeState()
        {
            ItemInstance swordA = ItemFactory.CreateInstance(_trainingSwordData, new Vector3(0, 0, 0));
            ItemInstance swordB = ItemFactory.CreateInstance(_trainingSwordData, new Vector3(2, 0, 0));

            Assert.AreNotEqual(swordA.InstanceId, swordB.InstanceId);
            Assert.AreSame(swordA.Data, swordB.Data);

            // Rotate Sword A only
            swordA.Rotate90();
            Assert.AreEqual(90, swordA.CurrentRotationDegrees);
            Assert.AreEqual(new Vector2Int(2, 1), swordA.CurrentDimensions);

            // Sword B remains unrotated
            Assert.AreEqual(0, swordB.CurrentRotationDegrees);
            Assert.AreEqual(new Vector2Int(1, 2), swordB.CurrentDimensions);
        }

        [Test]
        public void GridPlacement_WithRotatedFootprint_ValidatesAndOccupiesCells()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_trainingSwordData, Vector3.zero);
            sword.Rotate90(); // Rotated to 2x1

            Vector2Int origin = new Vector2Int(1, 2);
            Assert.IsTrue(_grid.CanPlaceItem(origin, sword.CurrentDimensions));

            bool placed = _grid.PlaceItem(sword.InstanceId, origin, sword.CurrentDimensions);
            Assert.IsTrue(placed);
            Assert.AreEqual(2, _grid.GetOccupiedCount());

            // Check that attempting to place another item on occupied cells fails
            ItemInstance flask = ItemFactory.CreateInstance(_vitalFlaskData, Vector3.zero);
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(1, 2), flask.CurrentDimensions));
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(2, 2), flask.CurrentDimensions));
        }

        [Test]
        public void GridPlacement_LockedAndBoundaryRejection_WorksForAllFootprints()
        {
            // 2x2 Guard plate at locked corner (0,0) fails
            ItemInstance plate = ItemFactory.CreateInstance(_guardPlateData, Vector3.zero);
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(0, 0), plate.CurrentDimensions));

            // 2x1 Ember blade at (4,2) extends outside grid -> fails
            ItemInstance ember = ItemFactory.CreateInstance(_emberBladeData, Vector3.zero);
            Assert.IsFalse(_grid.CanPlaceItem(new Vector2Int(4, 2), ember.CurrentDimensions));
        }

        [Test]
        public void ItemRemoval_FreesGridOccupancyCorrectly()
        {
            ItemInstance plate = ItemFactory.CreateInstance(_guardPlateData, Vector3.zero);
            Vector2Int origin = new Vector2Int(1, 1);

            _grid.PlaceItem(plate.InstanceId, origin, plate.CurrentDimensions);
            Assert.AreEqual(4, _grid.GetOccupiedCount());

            bool removed = _grid.RemoveItem(plate.InstanceId, origin, plate.CurrentDimensions);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, _grid.GetOccupiedCount());
            Assert.AreEqual(17, _grid.GetActiveCount());
        }
    }
}
