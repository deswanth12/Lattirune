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
    public class SynergyMatrixTests
    {
        private GameObject _holderObj;
        private SynergySystem _synergySystem;
        private SynergyDatabaseSO _database;
        private LatticeGrid _grid;

        private ItemDataSO _swordData;
        private ItemDataSO _shieldData;
        private ItemDataSO _relicData;

        private RuneData _fireRune;
        private RuneData _iceRune;
        private RuneData _lightningRune;
        private RuneData _poisonRune;
        private RuneData _lightRune;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("SynergyMatrixTestHolder");
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            _database = SynergyDatabaseSO.CreateDefaultDatabase();
            _synergySystem = _holderObj.AddComponent<SynergySystem>();
            _synergySystem.Initialize(_database);

            // Item Definitions
            _swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            _swordData.Initialize("item_sword", "Training Sword", "Sword", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);

            _shieldData = ScriptableObject.CreateInstance<ItemDataSO>();
            _shieldData.Initialize("item_shield", "Guard Plate", "Shield", ItemCategory.Shield, new Vector2Int(2, 2), true, Color.blue);

            _relicData = ScriptableObject.CreateInstance<ItemDataSO>();
            _relicData.Initialize("item_relic", "Arcane Relic", "Relic", ItemCategory.Relic, new Vector2Int(1, 1), false, Color.magenta);

            // Rune Definitions (5 Elements)
            _fireRune = ScriptableObject.CreateInstance<RuneData>();
            _fireRune.Initialize("rune_fire", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);

            _iceRune = ScriptableObject.CreateInstance<RuneData>();
            _iceRune.Initialize("rune_ice", "Ice Rune", ConduitDirection.East, ElementType.Ice, 3);

            _lightningRune = ScriptableObject.CreateInstance<RuneData>();
            _lightningRune.Initialize("rune_lightning", "Lightning Rune", ConduitDirection.North, ElementType.Lightning, 3);

            _poisonRune = ScriptableObject.CreateInstance<RuneData>();
            _poisonRune.Initialize("rune_poison", "Poison Rune", ConduitDirection.North, ElementType.Poison, 3);

            _lightRune = ScriptableObject.CreateInstance<RuneData>();
            _lightRune.Initialize("rune_light", "Light Rune", ConduitDirection.North, ElementType.Light, 3);
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
        public void SynergyMatrix_FireRune_Plus_Sword_ResolvesFlameboundEdge()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            SynergyResult result = _synergySystem.EvaluateConnection(_fireRune, new Vector2Int(2, 1), conduit, sword);

            Assert.IsTrue(result.IsSynergyActive);
            Assert.AreEqual("fire_sword", result.SynergyId);
            Assert.AreEqual("Flamebound Edge", result.SynergyName);
            Assert.AreEqual(5, result.RuneBonus);
        }

        [Test]
        public void SynergyMatrix_IceRune_Plus_Shield_ResolvesGlacialBastion()
        {
            ItemInstance shield = ItemFactory.CreateInstance(_shieldData, Vector3.zero, _holderObj.transform);
            shield.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(1, 2), ConduitDirection.East, 3);
            SynergyResult result = _synergySystem.EvaluateConnection(_iceRune, new Vector2Int(1, 2), conduit, shield);

            Assert.IsTrue(result.IsSynergyActive);
            Assert.AreEqual("ice_shield", result.SynergyId);
            Assert.AreEqual("Glacial Bastion", result.SynergyName);
            Assert.AreEqual(4, result.RuneBonus);
        }

        [Test]
        public void SynergyMatrix_LightningRune_Plus_Weapon_ResolvesStormSurge()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            SynergyResult result = _synergySystem.EvaluateConnection(_lightningRune, new Vector2Int(2, 1), conduit, sword);

            Assert.IsTrue(result.IsSynergyActive);
            Assert.AreEqual("lightning_weapon", result.SynergyId);
            Assert.AreEqual(8, result.RuneBonus);
        }

        [Test]
        public void SynergyMatrix_PoisonRune_Plus_Weapon_ResolvesVenomousStrike()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            SynergyResult result = _synergySystem.EvaluateConnection(_poisonRune, new Vector2Int(2, 1), conduit, sword);

            Assert.IsTrue(result.IsSynergyActive);
            Assert.AreEqual("poison_blade", result.SynergyId);
            Assert.AreEqual(3, result.RuneBonus);
        }

        [Test]
        public void SynergyMatrix_LightRune_Plus_Relic_ResolvesRadiantDawn()
        {
            ItemInstance relic = ItemFactory.CreateInstance(_relicData, Vector3.zero, _holderObj.transform);
            relic.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            SynergyResult result = _synergySystem.EvaluateConnection(_lightRune, new Vector2Int(2, 1), conduit, relic);

            Assert.IsTrue(result.IsSynergyActive);
            Assert.AreEqual("light_relic", result.SynergyId);
            Assert.AreEqual(4, result.RuneBonus);
        }

        [Test]
        public void SynergyMatrix_MismatchedItemCategory_RejectsSynergy()
        {
            // Fire Rune + Shield (Fire only targets Weapons)
            ItemInstance shield = ItemFactory.CreateInstance(_shieldData, Vector3.zero, _holderObj.transform);
            shield.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            SynergyResult result = _synergySystem.EvaluateConnection(_fireRune, new Vector2Int(2, 1), conduit, shield);

            Assert.IsFalse(result.IsSynergyActive);
            Assert.IsNull(result.SynergyId);
        }

        [Test]
        public void SynergyMatrix_MultipleItemInstances_RemainIndependent()
        {
            // Two swords on grid: Sword A at (2,2), Sword B at (0,2)
            ItemInstance swordA = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);
            swordA.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            ItemInstance swordB = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);
            swordB.OnPlaced(new Vector2Int(0, 2), Vector3.zero);

            // Fire Rune at (2,1) emitting North hits Sword A only
            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            var activeConduits = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (_fireRune, new Vector2Int(2, 1), conduit)
            };

            _synergySystem.UpdateSynergies(activeConduits, new List<ItemInstance> { swordA, swordB });

            Assert.IsTrue(swordA.HasActiveSynergy);
            Assert.AreEqual("fire_sword", swordA.ActiveSynergyId);

            Assert.IsFalse(swordB.HasActiveSynergy);
            Assert.IsNull(swordB.ActiveSynergyId);
        }

        [Test]
        public void SynergyMatrix_DatabaseValidation_DetectsDuplicatesAndErrors()
        {
            List<string> errors;
            Assert.IsTrue(_database.ValidateDatabase(out errors));
            Assert.AreEqual(0, errors.Count);

            // Try adding duplicate
            SynergyDefinitionSO duplicate = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            duplicate.Initialize("fire_sword", "Duplicate Flame", "Desc", ElementType.Fire, ItemCategory.Weapon, 5, Color.red);
            _database.Register(duplicate);

            Assert.AreEqual(5, _database.Count, "Duplicate ID must not increase count.");
        }

        [Test]
        public void SynergyMatrix_ItemDataSO_RemainsImmutable()
        {
            ItemInstance sword = ItemFactory.CreateInstance(_swordData, Vector3.zero, _holderObj.transform);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);

            RuneConduitResult conduit = RuneConduitEngine.CalculateConduit(_grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            _synergySystem.EvaluateConnection(_fireRune, new Vector2Int(2, 1), conduit, sword);

            Assert.AreEqual("item_sword", _swordData.ItemId);
            Assert.AreEqual(ItemCategory.Weapon, _swordData.Category);
            Assert.AreEqual(new Vector2Int(1, 2), _swordData.BaseDimensions);
        }
    }
}
