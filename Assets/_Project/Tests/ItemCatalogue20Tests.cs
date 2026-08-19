using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Comprehensive test suite for the MVP 1.0 20-Item Catalogue and ItemDatabase architecture.
    /// Strictly verifies PLAN.md Section 6.1 item definitions, footprints, categories, stats, and aggregation.
    /// </summary>
    [TestFixture]
    public class ItemCatalogue20Tests
    {
        private ItemDatabaseSO _db;
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _db = ItemDatabaseSO.CreateCanonicalDatabase();
            _holderObj = new GameObject("ItemCatalogue20TestHolder");
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
        public void ItemDatabase_Contains20CanonicalItems_PlusAliases()
        {
            Assert.IsNotNull(_db);
            Assert.IsTrue(_db.IsValid(out string error), error);

            // 20 canonical + 5 prototype aliases = 25 total
            Assert.GreaterOrEqual(_db.TotalItemCount, 20);

            // Verify all 20 canonical IDs exist in database
            string[] canonicalIds = new string[]
            {
                "item_rusty_dagger",
                "item_iron_broadsword",
                "item_shortbow",
                "item_apprentice_wand",
                "item_battleaxe",
                "item_phalanx_spear",
                "item_wooden_buckler",
                "item_iron_tower_shield",
                "item_spiked_buckler",
                "item_leather_tunic",
                "item_chainmail_coat",
                "item_whetstone",
                "item_ruby_ring",
                "item_sapphire_ring",
                "item_lucky_clover",
                "item_health_potion",
                "item_stamina_flask",
                "item_poison_vial",
                "item_decaying_blade",
                "item_blood_shield"
            };

            foreach (var id in canonicalIds)
            {
                Assert.IsTrue(_db.HasItem(id), $"Database missing canonical item ID: {id}");
            }
        }

        [Test]
        public void Weapons_CategoryAndStats_MatchPlan()
        {
            // 1. Rusty Dagger (1x1, 4 Dmg, 0.8s)
            var dagger = _db.GetItem("item_rusty_dagger");
            Assert.AreEqual(ItemCategory.Weapon, dagger.Category);
            Assert.AreEqual(new Vector2Int(1, 1), dagger.BaseDimensions);
            Assert.AreEqual(4, dagger.BaseDamage);
            Assert.AreEqual(0.8f, dagger.Cooldown);

            // 2. Iron Broadsword (1x2, 10 Dmg, 2.0s)
            var sword = _db.GetItem("item_iron_broadsword");
            Assert.AreEqual(ItemCategory.Weapon, sword.Category);
            Assert.AreEqual(new Vector2Int(1, 2), sword.BaseDimensions);
            Assert.AreEqual(10, sword.BaseDamage);
            Assert.AreEqual(2.0f, sword.Cooldown);

            // 3. Shortbow (2x1, 6 Dmg, 1.4s, 5 Pierce)
            var bow = _db.GetItem("item_shortbow");
            Assert.AreEqual(ItemCategory.Weapon, bow.Category);
            Assert.AreEqual(new Vector2Int(2, 1), bow.BaseDimensions);
            Assert.AreEqual(6, bow.BaseDamage);
            Assert.AreEqual(1.4f, bow.Cooldown);
            Assert.AreEqual(5, bow.ArmorPierce);

            // 4. Apprentice Wand (1x2, 7 Dmg, 1.8s, +50% Rune Mod)
            var wand = _db.GetItem("item_apprentice_wand");
            Assert.AreEqual(ItemCategory.Weapon, wand.Category);
            Assert.AreEqual(new Vector2Int(1, 2), wand.BaseDimensions);
            Assert.AreEqual(7, wand.BaseDamage);
            Assert.AreEqual(1.8f, wand.Cooldown);
            Assert.AreEqual(0.5f, wand.ElementalRuneDamageModifier);

            // 5. Battleaxe (L-Shape / 2x2, 18 Dmg, 3.0s)
            var axe = _db.GetItem("item_battleaxe");
            Assert.AreEqual(ItemCategory.Weapon, axe.Category);
            Assert.AreEqual(new Vector2Int(2, 2), axe.BaseDimensions);
            Assert.IsTrue(axe.IsLShape);
            Assert.AreEqual(18, axe.BaseDamage);
            Assert.AreEqual(3.0f, axe.Cooldown);

            // 6. Phalanx Spear (1x3, 12 Dmg, 1.8s)
            var spear = _db.GetItem("item_phalanx_spear");
            Assert.AreEqual(ItemCategory.Weapon, spear.Category);
            Assert.AreEqual(new Vector2Int(1, 3), spear.BaseDimensions);
            Assert.AreEqual(12, spear.BaseDamage);
            Assert.AreEqual(1.8f, spear.Cooldown);
        }

        [Test]
        public void Shields_CategoryAndStats_MatchPlan()
        {
            // 7. Wooden Buckler (1x1, 8 Shield)
            var buckler = _db.GetItem("item_wooden_buckler");
            Assert.AreEqual(ItemCategory.Shield, buckler.Category);
            Assert.AreEqual(new Vector2Int(1, 1), buckler.BaseDimensions);
            Assert.AreEqual(8, buckler.ShieldValue);

            // 8. Iron Tower Shield (2x2, 25 Shield)
            var tower = _db.GetItem("item_iron_tower_shield");
            Assert.AreEqual(ItemCategory.Shield, tower.Category);
            Assert.AreEqual(new Vector2Int(2, 2), tower.BaseDimensions);
            Assert.AreEqual(25, tower.ShieldValue);

            // 9. Spiked Buckler (1x2, 12 Shield, 4 Thorns)
            var spiked = _db.GetItem("item_spiked_buckler");
            Assert.AreEqual(ItemCategory.Shield, spiked.Category);
            Assert.AreEqual(new Vector2Int(1, 2), spiked.BaseDimensions);
            Assert.AreEqual(12, spiked.ShieldValue);
            Assert.AreEqual(4, spiked.ThornsDamage);
        }

        [Test]
        public void Armor_CategoryAndStats_MatchPlan()
        {
            // 10. Leather Tunic (2x2, +25 Max HP)
            var tunic = _db.GetItem("item_leather_tunic");
            Assert.AreEqual(ItemCategory.Armor, tunic.Category);
            Assert.AreEqual(new Vector2Int(2, 2), tunic.BaseDimensions);
            Assert.AreEqual(25, tunic.MaxHpBonus);

            // 11. Chainmail Coat (2x2, +15 Max HP, 2 Dmg Reduction)
            var coat = _db.GetItem("item_chainmail_coat");
            Assert.AreEqual(ItemCategory.Armor, coat.Category);
            Assert.AreEqual(new Vector2Int(2, 2), coat.BaseDimensions);
            Assert.AreEqual(15, coat.MaxHpBonus);
            Assert.AreEqual(2, coat.DamageTakenReduction);
        }

        [Test]
        public void Relics_CategoryAndStats_MatchPlan()
        {
            // 12. Whetstone (1x1, +3 flat dmg)
            var whetstone = _db.GetItem("item_whetstone");
            Assert.AreEqual(ItemCategory.Relic, whetstone.Category);
            Assert.AreEqual(new Vector2Int(1, 1), whetstone.BaseDimensions);
            Assert.AreEqual(3, whetstone.FlatDamageBonus);

            // 13. Ruby Ring (1x1)
            var ruby = _db.GetItem("item_ruby_ring");
            Assert.AreEqual(ItemCategory.Relic, ruby.Category);
            Assert.AreEqual(new Vector2Int(1, 1), ruby.BaseDimensions);

            // 14. Sapphire Ring (1x1)
            var sapphire = _db.GetItem("item_sapphire_ring");
            Assert.AreEqual(ItemCategory.Relic, sapphire.Category);
            Assert.AreEqual(new Vector2Int(1, 1), sapphire.BaseDimensions);

            // 15. Lucky Clover (1x1, +10% Crit)
            var clover = _db.GetItem("item_lucky_clover");
            Assert.AreEqual(ItemCategory.Relic, clover.Category);
            Assert.AreEqual(new Vector2Int(1, 1), clover.BaseDimensions);
            Assert.AreEqual(0.10f, clover.CritBonus);
        }

        [Test]
        public void Consumables_CategoryAndStats_MatchPlan()
        {
            // 16. Health Potion (1x1)
            var potion = _db.GetItem("item_health_potion");
            Assert.AreEqual(ItemCategory.Consumable, potion.Category);
            Assert.AreEqual(new Vector2Int(1, 1), potion.BaseDimensions);

            // 17. Stamina Flask (1x1)
            var flask = _db.GetItem("item_stamina_flask");
            Assert.AreEqual(ItemCategory.Consumable, flask.Category);
            Assert.AreEqual(new Vector2Int(1, 1), flask.BaseDimensions);

            // 18. Poison Vial (1x1)
            var vial = _db.GetItem("item_poison_vial");
            Assert.AreEqual(ItemCategory.Consumable, vial.Category);
            Assert.AreEqual(new Vector2Int(1, 1), vial.BaseDimensions);
        }

        [Test]
        public void CursedItems_CategoryAndStats_MatchPlan()
        {
            // 19. Decaying Blade (Cursed Weapon, 1x2, 22 Dmg, 1.2s)
            var decaying = _db.GetItem("item_decaying_blade");
            Assert.AreEqual(ItemCategory.Weapon, decaying.Category);
            Assert.IsTrue(decaying.IsCursed);
            Assert.AreEqual(new Vector2Int(1, 2), decaying.BaseDimensions);
            Assert.AreEqual(22, decaying.BaseDamage);
            Assert.AreEqual(1.2f, decaying.Cooldown);

            // 20. Blood Shield (Cursed Shield, 2x2, 45 Shield)
            var blood = _db.GetItem("item_blood_shield");
            Assert.AreEqual(ItemCategory.Shield, blood.Category);
            Assert.IsTrue(blood.IsCursed);
            Assert.AreEqual(new Vector2Int(2, 2), blood.BaseDimensions);
            Assert.AreEqual(45, blood.ShieldValue);
        }

        [Test]
        public void PlayerCombatant_BuildStatsAggregation_DerivesWeaponDamageAndShield()
        {
            PlayerCombatant player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            // Broadsword (10 dmg) + Tower Shield (25 shield)
            var broadsword = _db.GetItem("item_iron_broadsword");
            var towerShield = _db.GetItem("item_iron_tower_shield");

            ItemInstance swordInst = ItemFactory.CreateInstance(broadsword, Vector3.zero, _holderObj.transform);
            swordInst.OnPlaced(new Vector2Int(0, 0), Vector3.zero);

            ItemInstance shieldInst = ItemFactory.CreateInstance(towerShield, Vector3.zero, _holderObj.transform);
            shieldInst.OnPlaced(new Vector2Int(2, 0), Vector3.zero);

            player.UpdateStatsFromBuild(new List<ItemInstance> { swordInst, shieldInst });

            Assert.AreEqual(10, player.BaseAttackDamage);
            Assert.AreEqual(25, player.Armor);
        }

        [Test]
        public void RewardGenerator_With20ItemCatalogue_Generates3UniqueChoices()
        {
            List<RewardOption> rewards = RewardGenerator.GenerateRewardOptions(_db.AllItems, count: 3);

            Assert.AreEqual(3, rewards.Count);
            HashSet<string> chosenIds = new HashSet<string>();
            foreach (var r in rewards)
            {
                Assert.IsNotNull(r.ItemData);
                Assert.IsFalse(chosenIds.Contains(r.ItemData.ItemId));
                chosenIds.Add(r.ItemData.ItemId);
            }
        }
    }
}
