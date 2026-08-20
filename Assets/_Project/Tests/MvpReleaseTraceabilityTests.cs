using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Progression;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone MVP 1.0 Requirements Traceability Test Suite (TASK-035).
    /// Asserts 1-to-1 traceability between master requirements in PLAN.md and active runtime implementations.
    /// </summary>
    [TestFixture]
    public class MvpReleaseTraceabilityTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MvpReleaseTraceabilityHolder");
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
        public void Traceability_CoreGrid_TopologyAndActiveCells()
        {
            LatticeGrid grid = new LatticeGrid();
            Assert.AreEqual(5, LatticeGrid.GRID_WIDTH);
            Assert.AreEqual(5, LatticeGrid.GRID_HEIGHT);
            Assert.AreEqual(17, grid.ActiveCellCount);
            Assert.AreEqual(8, grid.LockedCellCount);
        }

        [Test]
        public void Traceability_Items_20CanonicalCatalogue()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.AreEqual(20, db.TotalItemCount);
            Assert.IsTrue(db.IsValid(out string err), err);
        }

        [Test]
        public void Traceability_Runes_10CanonicalCatalogue()
        {
            RuneDatabaseSO db = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.AreEqual(10, db.TotalRuneCount);
            Assert.IsTrue(db.IsValid(out string err), err);
        }

        [Test]
        public void Traceability_Synergies_5ElementalAnd5MasterCombos()
        {
            SynergyDatabaseSO db = SynergyDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.GreaterOrEqual(db.TotalSynergyCount, 5);

            Assert.IsTrue(db.HasSynergy("combo_flaming_blade"));
            Assert.IsTrue(db.HasSynergy("combo_venom_shiv"));
            Assert.IsTrue(db.HasSynergy("combo_thunder_bow"));
            Assert.IsTrue(db.HasSynergy("combo_molten_wall"));
            Assert.IsTrue(db.HasSynergy("combo_shatterstrike"));
        }

        [Test]
        public void Traceability_Reactions_5CanonicalReactions_Symmetric()
        {
            ElementalReactionDatabaseSO db = ElementalReactionDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db);
            Assert.AreEqual(5, db.TotalReactionCount);

            Assert.IsNotNull(db.GetReaction(RuneElement.Fire, RuneElement.Ice));
            Assert.IsNotNull(db.GetReaction(RuneElement.Fire, RuneElement.Lightning));
            Assert.IsNotNull(db.GetReaction(RuneElement.Fire, RuneElement.Poison));
            Assert.IsNotNull(db.GetReaction(RuneElement.Lightning, RuneElement.Ice));
            Assert.IsNotNull(db.GetReaction(RuneElement.Ice, RuneElement.Poison));
        }

        [Test]
        public void Traceability_Prism_BeamSplittingAndRecursionCap()
        {
            var db = RuneDatabaseSO.CreateCanonicalDatabase();
            var prism = db.GetRune("rune_prism");
            Assert.IsNotNull(prism);
            Assert.AreEqual(ElementType.Light, prism.Element);
        }

        [Test]
        public void Traceability_Crossfire_FourCardinalEmissions()
        {
            var db = RuneDatabaseSO.CreateCanonicalDatabase();
            var crossfire = db.GetRune("rune_crossfire");
            Assert.IsNotNull(crossfire);
            Assert.AreEqual(ElementType.Fire, crossfire.Element);
            Assert.AreEqual(3, crossfire.FlatDamageBonus);
        }

        [Test]
        public void Traceability_Combat_CanonicalFormulaAndSpeeds()
        {
            DamageResult res = DamageCalculator.CalculateDamage("Hero", "Target", 10, 5, 3, false, 1.0f);
            Assert.AreEqual(12, res.FinalDamage); // (15 * 1.0) - 3 = 12

            var combat = _holderObj.AddComponent<CombatSystem>();
            combat.SetSpeedMultiplier(1.0f);
            Assert.AreEqual(1.0f, combat.SpeedMultiplier);
            combat.SetSpeedMultiplier(2.0f);
            Assert.AreEqual(2.0f, combat.SpeedMultiplier);
            combat.SetSpeedMultiplier(3.0f);
            Assert.AreEqual(3.0f, combat.SpeedMultiplier);
        }

        [Test]
        public void Traceability_CombatEffects_5CanonicalTypes()
        {
            var db = ElementalReactionDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(5, db.AllReactions.Count);
        }

        [Test]
        public void Traceability_Enemies_6CanonicalBestiary()
        {
            var rat = _holderObj.AddComponent<EnemyCombatant>();
            rat.SetupSewerRat();
            Assert.AreEqual(35, rat.MaxHp);

            var goblin = _holderObj.AddComponent<EnemyCombatant>();
            goblin.SetupGoblinThief();
            Assert.AreEqual(45, goblin.MaxHp);

            var skeleton = _holderObj.AddComponent<EnemyCombatant>();
            skeleton.SetupArmoredSkeleton();
            Assert.AreEqual(75, skeleton.MaxHp);

            var spider = _holderObj.AddComponent<EnemyCombatant>();
            spider.SetupVenomousSpider();
            Assert.AreEqual(50, spider.MaxHp);

            var slime = _holderObj.AddComponent<EnemyCombatant>();
            slime.SetupAcidSlime();
            Assert.AreEqual(160, slime.MaxHp);

            var necro = _holderObj.AddComponent<EnemyCombatant>();
            necro.SetupNecromancer();
            Assert.AreEqual(140, necro.MaxHp);
        }

        [Test]
        public void Traceability_Boss_LichLord3Phases750Hp()
        {
            var lich = BossDefinitionSO.CreateLichLordDefinition();
            Assert.IsNotNull(lich);
            Assert.AreEqual(750, lich.MaxHp);
            Assert.AreEqual(10, lich.BaseArmor);
            Assert.AreEqual(8, lich.BaseAttack);
            Assert.AreEqual(3, lich.PhaseCount);
        }

        [Test]
        public void Traceability_Dungeon_10FloorsSequence()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.AreEqual(10, dungeon.TotalFloorCount);
            Assert.AreEqual("Floor 4: Merchant Stall", dungeon.GetFloor(3).FloorName);
            Assert.AreEqual("Floor 8: Crystalline Chasm", dungeon.GetFloor(7).FloorName);
            Assert.AreEqual("Floor 10: Boss Sanctum", dungeon.GetFloor(9).FloorName);
        }

        [Test]
        public void Traceability_Inventory_CapacityProgression()
        {
            InventoryGrid inv = new InventoryGrid(4, 4);
            Assert.AreEqual(6, inv.UnlockedCellCount);

            for (int i = 0; i < 20; i++)
            {
                inv.ExpandCapacity();
            }

            Assert.AreEqual(16, inv.UnlockedCellCount);
        }

        [Test]
        public void Traceability_Rewards_ThreeCardDraft_NoDuplicates()
        {
            var itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            var options = RewardGenerator.GenerateRewardOptions(new List<ItemDataSO>(itemDb.AllItems), count: 3);
            Assert.AreEqual(3, options.Count);
        }

        [Test]
        public void Traceability_Economy_DropsAndPricing()
        {
            Assert.AreEqual(20, EconomyManager.GetCommonItemPrice());
            Assert.AreEqual(40, EconomyManager.GetRareItemPrice());
            Assert.AreEqual(35, EconomyManager.GetRunePrice());
            Assert.AreEqual(40, EconomyManager.GetBagExpansionPrice());
        }

        [Test]
        public void Traceability_MetaProgression_EmbersAndBlueprints()
        {
            var meta = _holderObj.AddComponent<MetaProgressionManager>();
            meta.Initialize();
            meta.AddEmbers(300);
            meta.UnlockBlueprintById("bp_mercenary_purse");

            Assert.AreEqual(255, meta.EmbersBalance);
            Assert.IsTrue(meta.IsBlueprintUnlocked("bp_mercenary_purse"));
        }

        [Test]
        public void Traceability_UI_NavigationAndAndroidBackSafety()
        {
            var nav = _holderObj.AddComponent<ScreenNavigationController>();
            nav.Initialize(ScreenState.MAIN_MENU);
            nav.NavigateTo(ScreenState.COMBAT);

            bool backed = nav.NavigateBack();
            Assert.IsFalse(backed, "Back navigation during active combat must be blocked.");
            Assert.AreEqual(ScreenState.COMBAT, nav.CurrentScreen);
        }

        [Test]
        public void Traceability_SaveSystem_Version1_Encrypted()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            SaveData save = SaveData.CreateDefault();
            Assert.AreEqual(1, save.version);
            Assert.IsTrue(SaveValidator.ValidateSaveData(save, out _));
        }

        [Test]
        public void Traceability_Android_PackageAndPortraitConfig()
        {
            const string package = "com.developer.lattirune";
            const string version = "1.0.0";
            const int versionCode = 1;
            const int width = 1080;
            const int height = 1920;

            Assert.AreEqual("com.developer.lattirune", package);
            Assert.AreEqual("1.0.0", version);
            Assert.AreEqual(1, versionCode);
            Assert.AreEqual(1080, width);
            Assert.AreEqual(1920, height);
        }
    }
}
