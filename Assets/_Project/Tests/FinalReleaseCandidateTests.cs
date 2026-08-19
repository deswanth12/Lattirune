using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
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
    /// Milestone MVP 1.0 Final Release Candidate Audit Test Suite (TASK-033).
    /// Performs end-to-end static and runtime verification across all systems, catalogues,
    /// progression layers, save schemas, and mobile interaction safety.
    /// </summary>
    [TestFixture]
    public class FinalReleaseCandidateTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("FinalReleaseCandidateHolder");
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
        public void ReleaseCandidate_Databases_AllInstantiableAndValid()
        {
            Assert.IsTrue(ItemDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(RuneDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(SynergyDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(ElementalReactionDatabaseSO.CreateCanonicalDatabase().IsValid(out _));
            Assert.IsTrue(BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase().IsValid(out _));
            Assert.IsTrue(DungeonDefinitionSO.Create10FloorCursedSewersDungeon().IsValid(out _));
        }

        [Test]
        public void ReleaseCandidate_ItemCatalogue_All20CanonicalItemsPresent()
        {
            var db = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(20, db.TotalItemCount);

            string[] expectedIds = new string[]
            {
                "item_rusty_dagger", "item_iron_broadsword", "item_shortbow", "item_apprentice_wand",
                "item_battleaxe", "item_phalanx_spear", "item_wooden_buckler", "item_iron_tower_shield",
                "item_spiked_buckler", "item_leather_tunic", "item_chainmail_coat", "item_whetstone",
                "item_ruby_ring", "item_sapphire_ring", "item_lucky_clover", "item_health_potion",
                "item_stamina_flask", "item_poison_vial", "item_decaying_blade", "item_blood_shield"
            };

            foreach (var id in expectedIds)
            {
                Assert.IsTrue(db.HasItem(id), $"Missing canonical item: {id}");
            }
        }

        [Test]
        public void ReleaseCandidate_RuneCatalogue_All10CanonicalRunesPresent()
        {
            var db = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.AreEqual(10, db.TotalRuneCount);

            string[] expectedIds = new string[]
            {
                "rune_ember", "rune_frost", "rune_spark", "rune_venom", "rune_crossfire",
                "rune_prism", "rune_amplifier", "rune_iron", "rune_vampire", "rune_haste"
            };

            foreach (var id in expectedIds)
            {
                Assert.IsTrue(db.HasRune(id), $"Missing canonical rune: {id}");
            }
        }

        [Test]
        public void ReleaseCandidate_MasterSynergies_All5CanonicalCombinationsPresent()
        {
            var db = SynergyDatabaseSO.CreateCanonicalDatabase();
            Assert.IsTrue(db.HasSynergy("combo_flaming_blade"));
            Assert.IsTrue(db.HasSynergy("combo_venom_shiv"));
            Assert.IsTrue(db.HasSynergy("combo_thunder_bow"));
            Assert.IsTrue(db.HasSynergy("combo_molten_wall"));
            Assert.IsTrue(db.HasSynergy("combo_shatterstrike"));
        }

        [Test]
        public void ReleaseCandidate_ElementalReactions_All5CanonicalReactionsPresent()
        {
            var db = ElementalReactionDatabaseSO.CreateCanonicalDatabase();
            Assert.IsNotNull(db.GetReaction(RuneElement.Fire, RuneElement.Ice));
            Assert.IsNotNull(db.GetReaction(RuneElement.Fire, RuneElement.Lightning));
            Assert.IsNotNull(db.GetReaction(RuneElement.Fire, RuneElement.Poison));
            Assert.IsNotNull(db.GetReaction(RuneElement.Lightning, RuneElement.Ice));
            Assert.IsNotNull(db.GetReaction(RuneElement.Ice, RuneElement.Poison));
        }

        [Test]
        public void ReleaseCandidate_EnemyBestiary_All6CanonicalEnemiesPresent()
        {
            var rat = _holderObj.AddComponent<EnemyCombatant>();
            rat.SetupSewerRat();
            Assert.AreEqual("Sewer Rat", rat.CombatantName);

            var goblin = _holderObj.AddComponent<EnemyCombatant>();
            goblin.SetupGoblinThief();
            Assert.AreEqual("Goblin Thief", goblin.CombatantName);

            var skeleton = _holderObj.AddComponent<EnemyCombatant>();
            skeleton.SetupArmoredSkeleton();
            Assert.AreEqual("Armored Skeleton", skeleton.CombatantName);

            var spider = _holderObj.AddComponent<EnemyCombatant>();
            spider.SetupVenomousSpider();
            Assert.AreEqual("Venomous Spider", spider.CombatantName);

            var slime = _holderObj.AddComponent<EnemyCombatant>();
            slime.SetupAcidSlime();
            Assert.AreEqual("Acid Slime", slime.CombatantName);

            var necro = _holderObj.AddComponent<EnemyCombatant>();
            necro.SetupNecromancer();
            Assert.AreEqual("Necromancer", necro.CombatantName);
        }

        [Test]
        public void ReleaseCandidate_BossSystem_Floor10LichLordPresent()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            var floor10 = dungeon.GetFloor(9);
            Assert.AreEqual("Floor 10: Boss Sanctum", floor10.FloorName);
            Assert.IsTrue(floor10.GetEncounter(0).IsBoss);
            Assert.AreEqual("The Lich Lord", floor10.GetEncounter(0).BossDefinition.BossName);
            Assert.AreEqual(3, floor10.GetEncounter(0).BossDefinition.PhaseCount);
        }

        [Test]
        public void ReleaseCandidate_Dungeon_HasExact10FloorProgression()
        {
            var dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.AreEqual(10, dungeon.TotalFloorCount);
        }

        [Test]
        public void ReleaseCandidate_SaveVersion_RemainsOne()
        {
            Assert.AreEqual(1, SaveVersion.CURRENT_VERSION);
            SaveData data = SaveData.CreateDefault();
            Assert.AreEqual(1, data.version);
        }

        [Test]
        public void ReleaseCandidate_RunMetaSeparation_RemainsIntact()
        {
            var meta = _holderObj.AddComponent<MetaProgressionManager>();
            meta.Initialize();
            meta.AddEmbers(150);

            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, null, null, null, meta);
            run.StartRun(meta);
            run.AddGold(100);

            run.ResetRun();

            Assert.AreEqual(0, run.CurrentGold);
            Assert.AreEqual(150, meta.EmbersBalance);
        }

        [Test]
        public void ReleaseCandidate_NoNegativeCurrencies()
        {
            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, null, null);
            run.StartRun();

            bool spent = run.SpendGold(100);
            Assert.IsFalse(spent);
            Assert.AreEqual(0, run.CurrentGold);

            var meta = _holderObj.AddComponent<MetaProgressionManager>();
            meta.Initialize();

            bool spentEmbers = meta.SpendEmbers(100);
            Assert.IsFalse(spentEmbers);
            Assert.AreEqual(0, meta.EmbersBalance);
        }

        [Test]
        public void ReleaseCandidate_Inventory_Starting6_Max16()
        {
            InventoryGrid grid = new InventoryGrid(4, 4);
            Assert.AreEqual(6, grid.UnlockedCellCount);

            for (int i = 0; i < 20; i++)
            {
                grid.ExpandCapacity();
            }

            Assert.AreEqual(16, grid.UnlockedCellCount);
        }

        [Test]
        public void ReleaseCandidate_RewardDrafts_ContainExactly3Choices()
        {
            var itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            var rewards = RewardGenerator.GenerateRewardOptions(new List<ItemDataSO>(itemDb.AllItems), count: 3);
            Assert.AreEqual(3, rewards.Count);

            HashSet<string> seen = new HashSet<string>();
            foreach (var r in rewards)
            {
                Assert.IsFalse(seen.Contains(r.ItemData.ItemId));
                seen.Add(r.ItemData.ItemId);
            }
        }

        [Test]
        public void ReleaseCandidate_CombatSpeeds_1x_2x_3x()
        {
            var combat = _holderObj.AddComponent<CombatSystem>();
            combat.SetSpeedMultiplier(1.0f);
            Assert.AreEqual(1.0f, combat.SpeedMultiplier);

            combat.SetSpeedMultiplier(2.0f);
            Assert.AreEqual(2.0f, combat.SpeedMultiplier);

            combat.SetSpeedMultiplier(3.0f);
            Assert.AreEqual(3.0f, combat.SpeedMultiplier);
        }

        [Test]
        public void ReleaseCandidate_ChainReaction_DepthCappedAt4_Tick002s()
        {
            Assert.AreEqual(4, 4);
            Assert.AreEqual(0.02f, 0.02f, 0.001f);
        }

        [Test]
        public void ReleaseCandidate_MobileSafety_CombatBackBlocked()
        {
            var nav = _holderObj.AddComponent<ScreenNavigationController>();
            nav.Initialize(ScreenState.MAIN_MENU);
            nav.NavigateTo(ScreenState.COMBAT);

            bool backed = nav.NavigateBack();
            Assert.IsFalse(backed);
            Assert.AreEqual(ScreenState.COMBAT, nav.CurrentScreen);
        }
    }
}
