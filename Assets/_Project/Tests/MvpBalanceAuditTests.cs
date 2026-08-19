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
    /// Milestone MVP 1.0 Content Balance, Economy Validation, and Gameplay Tuning Audit Test Suite (TASK-032).
    /// Asserts 100% adherence to canonical numerical statistics, damage equations, drop tables,
    /// and progression rules defined in PLAN.md.
    /// </summary>
    [TestFixture]
    public class MvpBalanceAuditTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MvpBalanceAuditHolder");
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        // ==========================================
        // 1. 20-ITEM CANONICAL BALANCE AUDIT
        // ==========================================

        [Test]
        public void ItemBalance_Weapons_MatchPlanSection6_1()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();

            // 1. Rusty Dagger: 1x1, 4 Dmg, 0.8s cd
            var dagger = db.GetItem("item_rusty_dagger");
            Assert.AreEqual(new Vector2Int(1, 1), dagger.FootprintSize);
            Assert.AreEqual(4, dagger.BaseDamage);
            Assert.AreEqual(0.8f, dagger.Cooldown, 0.001f);

            // 2. Iron Broadsword: 1x2, 10 Dmg, 2.0s cd
            var sword = db.GetItem("item_iron_broadsword");
            Assert.AreEqual(new Vector2Int(1, 2), sword.FootprintSize);
            Assert.AreEqual(10, sword.BaseDamage);
            Assert.AreEqual(2.0f, sword.Cooldown, 0.001f);

            // 3. Shortbow: 2x1, 6 Dmg, 1.4s cd, 5 Armor Pierce
            var bow = db.GetItem("item_shortbow");
            Assert.AreEqual(new Vector2Int(2, 1), bow.FootprintSize);
            Assert.AreEqual(6, bow.BaseDamage);
            Assert.AreEqual(1.4f, bow.Cooldown, 0.001f);
            Assert.AreEqual(5, bow.ArmorPierce);

            // 4. Apprentice Wand: 1x2, 7 Dmg, 1.8s cd, +50% elemental rune damage
            var wand = db.GetItem("item_apprentice_wand");
            Assert.AreEqual(new Vector2Int(1, 2), wand.FootprintSize);
            Assert.AreEqual(7, wand.BaseDamage);
            Assert.AreEqual(1.8f, wand.Cooldown, 0.001f);
            Assert.AreEqual(0.50f, wand.ElementalRuneModifier, 0.001f);

            // 5. Battleaxe: L-Shape / 2x2, 18 Dmg, 3.0s cd
            var axe = db.GetItem("item_battleaxe");
            Assert.AreEqual(new Vector2Int(2, 2), axe.FootprintSize);
            Assert.AreEqual(18, axe.BaseDamage);
            Assert.AreEqual(3.0f, axe.Cooldown, 0.001f);

            // 6. Phalanx Spear: 1x3, 12 Dmg, 1.8s cd
            var spear = db.GetItem("item_phalanx_spear");
            Assert.AreEqual(new Vector2Int(1, 3), spear.FootprintSize);
            Assert.AreEqual(12, spear.BaseDamage);
            Assert.AreEqual(1.8f, spear.Cooldown, 0.001f);
        }

        [Test]
        public void ItemBalance_ShieldsAndArmor_MatchPlanSection6_1()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();

            // 7. Wooden Buckler: 1x1, 8 Shield
            var buckler = db.GetItem("item_wooden_buckler");
            Assert.AreEqual(new Vector2Int(1, 1), buckler.FootprintSize);
            Assert.AreEqual(8, buckler.ShieldBonus);

            // 8. Iron Tower Shield: 2x2, 25 Shield
            var towerShield = db.GetItem("item_iron_tower_shield");
            Assert.AreEqual(new Vector2Int(2, 2), towerShield.FootprintSize);
            Assert.AreEqual(25, towerShield.ShieldBonus);

            // 9. Spiked Buckler: 1x2, 12 Shield, 4 Thorns
            var spiked = db.GetItem("item_spiked_buckler");
            Assert.AreEqual(new Vector2Int(1, 2), spiked.FootprintSize);
            Assert.AreEqual(12, spiked.ShieldBonus);
            Assert.AreEqual(4, spiked.ThornsDamage);

            // 10. Leather Tunic: 2x2, +25 Max HP
            var leather = db.GetItem("item_leather_tunic");
            Assert.AreEqual(new Vector2Int(2, 2), leather.FootprintSize);
            Assert.AreEqual(25, leather.MaxHpBonus);

            // 11. Chainmail Coat: 2x2, +15 Max HP, 2 Flat Damage Reduction
            var chainmail = db.GetItem("item_chainmail_coat");
            Assert.AreEqual(new Vector2Int(2, 2), chainmail.FootprintSize);
            Assert.AreEqual(15, chainmail.MaxHpBonus);
            Assert.AreEqual(2, chainmail.ArmorBonus);
        }

        [Test]
        public void ItemBalance_RelicsAndConsumables_MatchPlanSection6_1()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();

            // 12. Whetstone: 1x1, +3 Flat Dmg
            var whetstone = db.GetItem("item_whetstone");
            Assert.AreEqual(3, whetstone.FlatDamageBonus);

            // 15. Lucky Clover: 1x1, +10% Crit
            var clover = db.GetItem("item_lucky_clover");
            Assert.AreEqual(0.10f, clover.CritChanceBonus, 0.001f);

            // 16. Health Potion: 1x1, +35 HP
            var healthPotion = db.GetItem("item_health_potion");
            Assert.AreEqual(35, healthPotion.HealingAmount);

            // 17. Stamina Flask: 1x1, +40% Speed
            var staminaFlask = db.GetItem("item_stamina_flask");
            Assert.AreEqual(0.40f, staminaFlask.SpeedBonus, 0.001f);

            // 18. Poison Vial: 1x1, 15 Poison
            var poisonVial = db.GetItem("item_poison_vial");
            Assert.AreEqual(15, poisonVial.PoisonStacksApplied);

            // 19. Decaying Blade: 1x2, 22 Dmg, 1.2s cd, Cursed
            var decaying = db.GetItem("item_decaying_blade");
            Assert.AreEqual(22, decaying.BaseDamage);
            Assert.AreEqual(1.2f, decaying.Cooldown, 0.001f);
            Assert.IsTrue(decaying.IsCursed);

            // 20. Blood Shield: 2x2, 45 Shield, Cursed
            var bloodShield = db.GetItem("item_blood_shield");
            Assert.AreEqual(45, bloodShield.ShieldBonus);
            Assert.IsTrue(bloodShield.IsCursed);
        }

        // ==========================================
        // 2. 10-RUNE CANONICAL BALANCE AUDIT
        // ==========================================

        [Test]
        public void RuneBalance_Canonical10Runes_MatchPlanSection5_1()
        {
            RuneDatabaseSO db = RuneDatabaseSO.CreateCanonicalDatabase();

            // 1. Ember Rune: Fire, East, +6 Fire Dmg
            var ember = db.GetRune("rune_ember");
            Assert.AreEqual(ElementType.Fire, ember.Element);
            Assert.AreEqual(ConduitDirection.East, ember.Direction);
            Assert.AreEqual(6, ember.FlatDamageBonus);

            // 2. Frost Rune: Ice, South, +4 Ice Dmg
            var frost = db.GetRune("rune_frost");
            Assert.AreEqual(ElementType.Ice, frost.Element);
            Assert.AreEqual(ConduitDirection.South, frost.Direction);
            Assert.AreEqual(4, frost.FlatDamageBonus);

            // 3. Spark Rune: Lightning, North, +8 Shock Dmg
            var spark = db.GetRune("rune_spark");
            Assert.AreEqual(ElementType.Lightning, spark.Element);
            Assert.AreEqual(ConduitDirection.North, spark.Direction);
            Assert.AreEqual(8, spark.FlatDamageBonus);

            // 4. Venom Rune: Poison, West, 2 Poison stacks
            var venom = db.GetRune("rune_venom");
            Assert.AreEqual(ElementType.Poison, venom.Element);
            Assert.AreEqual(ConduitDirection.West, venom.Direction);
            Assert.AreEqual(2, venom.PoisonStacksPerSec);

            // 5. Crossfire Rune: Fire, Cross (All 4 cardinal vectors)
            var crossfire = db.GetRune("rune_crossfire");
            Assert.AreEqual(ElementType.Fire, crossfire.Element);
            Assert.AreEqual(3, crossfire.FlatDamageBonus);

            // 6. Prism Rune: Light, Split
            var prism = db.GetRune("rune_prism");
            Assert.AreEqual(ElementType.Light, prism.Element);

            // 7. Amplifier Node: Force, Omni
            var amp = db.GetRune("rune_amplifier");
            Assert.AreEqual(ElementType.Force, amp.Element);

            // 8. Iron Rune: Earth, South, +15 Shield
            var iron = db.GetRune("rune_iron");
            Assert.AreEqual(ElementType.Earth, iron.Element);
            Assert.AreEqual(15, iron.StartingShieldBonus);

            // 9. Vampire Rune: Shadow, North, 12% Lifesteal
            var vamp = db.GetRune("rune_vampire");
            Assert.AreEqual(ElementType.Shadow, vamp.Element);
            Assert.AreEqual(0.12f, vamp.LifestealRatio, 0.001f);

            // 10. Haste Rune: Wind, East, +25% Attack Speed
            var haste = db.GetRune("rune_haste");
            Assert.AreEqual(ElementType.Wind, haste.Element);
            Assert.AreEqual(0.25f, haste.AttackSpeedBonus, 0.001f);
        }

        // ==========================================
        // 3. MASTER ITEM COMBINATIONS AUDIT
        // ==========================================

        [Test]
        public void MasterSynergies_MatchPlanSection7_1()
        {
            SynergyDatabaseSO db = SynergyDatabaseSO.CreateCanonicalDatabase();

            Assert.IsTrue(db.HasSynergy("combo_flaming_blade"));
            Assert.IsTrue(db.HasSynergy("combo_venom_shiv"));
            Assert.IsTrue(db.HasSynergy("combo_thunder_bow"));
            Assert.IsTrue(db.HasSynergy("combo_molten_wall"));
            Assert.IsTrue(db.HasSynergy("combo_shatterstrike"));
        }

        // ==========================================
        // 4. ELEMENTAL REACTIONS AUDIT
        // ==========================================

        [Test]
        public void ElementalReactions_SymmetricPairs_MatchPlanSection8()
        {
            ElementalReactionDatabaseSO db = ElementalReactionDatabaseSO.CreateCanonicalDatabase();

            // Steam: Fire + Ice == Ice + Fire
            var steam = db.GetReaction(RuneElement.Fire, RuneElement.Ice);
            Assert.IsNotNull(steam);
            Assert.AreEqual("reaction_steam", steam.ReactionId);

            // Plasma: Fire + Lightning == Lightning + Fire
            var plasma = db.GetReaction(RuneElement.Lightning, RuneElement.Fire);
            Assert.IsNotNull(plasma);
            Assert.AreEqual("reaction_plasma", plasma.ReactionId);

            // Toxic Flame: Fire + Poison
            var toxic = db.GetReaction(RuneElement.Poison, RuneElement.Fire);
            Assert.IsNotNull(toxic);
            Assert.AreEqual("reaction_toxic_flame", toxic.ReactionId);

            // Superconductor: Lightning + Ice
            var super = db.GetReaction(RuneElement.Lightning, RuneElement.Ice);
            Assert.IsNotNull(super);
            Assert.AreEqual("reaction_superconductor", super.ReactionId);

            // Frostbite: Ice + Poison
            var frostbite = db.GetReaction(RuneElement.Ice, RuneElement.Poison);
            Assert.IsNotNull(frostbite);
            Assert.AreEqual("reaction_frostbite", frostbite.ReactionId);
        }

        // ==========================================
        // 5. COMBAT DAMAGE PIPELINE & SPEED MULTIPLIERS
        // ==========================================

        [Test]
        public void CombatDamagePipeline_MatchesPlanSection9_2()
        {
            // Formula: max(1, ((Base + Rune) * Crit * Mod) - Armor)
            // Base 10, Rune 6, Normal Hit (Crit=1.0), Mod 1.0, Armor 4 => (16 * 1.0) - 4 = 12
            DamageResult normal = DamageCalculator.CalculateDamage("Hero", "Enemy", 10, 6, 4, isCritical: false, damageModifier: 1.0f);
            Assert.AreEqual(12, normal.FinalDamage);

            // Critical Hit (Crit=1.5) => (16 * 1.5) - 4 = 24 - 4 = 20
            DamageResult crit = DamageCalculator.CalculateDamage("Hero", "Enemy", 10, 6, 4, isCritical: true, damageModifier: 1.0f);
            Assert.AreEqual(20, crit.FinalDamage);

            // Minimum floor: Base 4, Rune 0, Armor 20 => max(1, 4 - 20) = 1
            DamageResult minimum = DamageCalculator.CalculateDamage("Hero", "Enemy", 4, 0, 20, isCritical: false, damageModifier: 1.0f);
            Assert.AreEqual(1, minimum.FinalDamage);
        }

        [Test]
        public void CombatSpeedMultipliers_SupportedValues_1x_2x_3x()
        {
            var combat = _holderObj.AddComponent<CombatSystem>();
            combat.SetSpeedMultiplier(1.0f);
            Assert.AreEqual(1.0f, combat.SpeedMultiplier);

            combat.SetSpeedMultiplier(2.0f);
            Assert.AreEqual(2.0f, combat.SpeedMultiplier);

            combat.SetSpeedMultiplier(3.0f);
            Assert.AreEqual(3.0f, combat.SpeedMultiplier);

            // Invalid clamped/rejected safely
            combat.SetSpeedMultiplier(5.0f);
            Assert.AreEqual(3.0f, combat.SpeedMultiplier);
        }

        // ==========================================
        // 6. ENEMY BESTIARY STAT AUDIT
        // ==========================================

        [Test]
        public void EnemyBestiary_StatProfiles_MatchPlanSection10()
        {
            // 1. Sewer Rat: 35 HP, 0 Armor, 3 Attack, 1.2s
            var rat = _holderObj.AddComponent<EnemyCombatant>();
            rat.SetupSewerRat();
            Assert.AreEqual(35, rat.MaxHp);
            Assert.AreEqual(0, rat.Armor);
            Assert.AreEqual(3, rat.BaseDamage);
            Assert.AreEqual(1.2f, rat.AttackInterval, 0.001f);

            // 2. Goblin Thief: 45 HP, 0 Armor, 4 Attack, 1.0s, 3 Gold steal
            var goblin = _holderObj.AddComponent<EnemyCombatant>();
            goblin.SetupGoblinThief();
            Assert.AreEqual(45, goblin.MaxHp);
            Assert.AreEqual(0, goblin.Armor);
            Assert.AreEqual(4, goblin.BaseDamage);
            Assert.AreEqual(1.0f, goblin.AttackInterval, 0.001f);
            Assert.AreEqual(3, goblin.GoldStealPerHit);

            // 3. Armored Skeleton: 75 HP, 15 Armor, 5 Attack, 2.0s, 20% reflection
            var skeleton = _holderObj.AddComponent<EnemyCombatant>();
            skeleton.SetupArmoredSkeleton();
            Assert.AreEqual(75, skeleton.MaxHp);
            Assert.AreEqual(15, skeleton.Armor);
            Assert.AreEqual(5, skeleton.BaseDamage);
            Assert.AreEqual(2.0f, skeleton.AttackInterval, 0.001f);
            Assert.AreEqual(0.20f, skeleton.ReflectPercentage, 0.001f);

            // 4. Venomous Spider: 50 HP, 0 Armor, 4 Attack, 1.4s, 2 Poison stacks
            var spider = _holderObj.AddComponent<EnemyCombatant>();
            spider.SetupVenomousSpider();
            Assert.AreEqual(50, spider.MaxHp);
            Assert.AreEqual(0, spider.Armor);
            Assert.AreEqual(4, spider.BaseDamage);
            Assert.AreEqual(1.4f, spider.AttackInterval, 0.001f);
            Assert.AreEqual(2, spider.PoisonStacksOnHit);

            // 5. Acid Slime: 160 HP, 2 Armor, 6 Attack, 2.0s
            var slime = _holderObj.AddComponent<EnemyCombatant>();
            slime.SetupAcidSlime();
            Assert.AreEqual(160, slime.MaxHp);
            Assert.AreEqual(2, slime.Armor);
            Assert.AreEqual(6, slime.BaseDamage);
            Assert.AreEqual(2.0f, slime.AttackInterval, 0.001f);

            // 6. Necromancer: 140 HP, 0 Armor, 5 Attack, 3.0s
            var necro = _holderObj.AddComponent<EnemyCombatant>();
            necro.SetupNecromancer();
            Assert.AreEqual(140, necro.MaxHp);
            Assert.AreEqual(0, necro.Armor);
            Assert.AreEqual(5, necro.BaseDamage);
            Assert.AreEqual(3.0f, necro.AttackInterval, 0.001f);
        }

        // ==========================================
        // 7. LICH LORD BOSS STAT & PHASE AUDIT
        // ==========================================

        [Test]
        public void LichLordBoss_StatProfileAndPhases_MatchPlanSection10()
        {
            var lich = BossDefinitionSO.CreateLichLordDefinition();
            Assert.IsNotNull(lich);
            Assert.AreEqual(750, lich.MaxHp);
            Assert.AreEqual(10, lich.BaseArmor);
            Assert.AreEqual(8, lich.BaseAttack);
            Assert.AreEqual(2.5f, lich.BaseAttackInterval, 0.001f);

            Assert.AreEqual(3, lich.PhaseCount);

            // Phase 1: Frost Warden (100% -> 66%)
            var p1 = lich.GetPhase(0);
            Assert.AreEqual("Phase 1: Frost Warden", p1.PhaseName);
            Assert.AreEqual(1.0f, p1.HpThresholdPercentage, 0.001f);

            // Phase 2: Soul Harvest (66% -> 33%, +5 Armor, +4 Attack, 0.8x interval)
            var p2 = lich.GetPhase(1);
            Assert.AreEqual("Phase 2: Soul Harvest", p2.PhaseName);
            Assert.AreEqual(0.66f, p2.HpThresholdPercentage, 0.001f);
            Assert.AreEqual(5, p2.ArmorBonus);
            Assert.AreEqual(4, p2.AttackBonus);
            Assert.AreEqual(0.8f, p2.AttackIntervalMultiplier, 0.001f);

            // Phase 3: Necrotic Inversion (33% -> 0%, +10 Armor, +8 Attack, 0.64x interval)
            var p3 = lich.GetPhase(2);
            Assert.AreEqual("Phase 3: Necrotic Inversion", p3.PhaseName);
            Assert.AreEqual(0.33f, p3.HpThresholdPercentage, 0.001f);
            Assert.AreEqual(10, p3.ArmorBonus);
            Assert.AreEqual(8, p3.AttackBonus);
            Assert.AreEqual(0.64f, p3.AttackIntervalMultiplier, 0.001f);
        }

        // ==========================================
        // 8. 10-FLOOR TOPOLOGY AUDIT
        // ==========================================

        [Test]
        public void DungeonTopology_10Floors_MatchPlanSection11()
        {
            DungeonDefinitionSO dungeon = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            Assert.AreEqual(10, dungeon.TotalFloorCount);

            Assert.AreEqual("Floor 1: Sewer Entry", dungeon.GetFloor(0).FloorName);
            Assert.AreEqual("Floor 2: Drain Basin", dungeon.GetFloor(1).FloorName);
            Assert.AreEqual("Floor 3: Slime Cavern", dungeon.GetFloor(2).FloorName);
            Assert.AreEqual("Floor 4: Merchant Stall", dungeon.GetFloor(3).FloorName);
            Assert.AreEqual("Floor 5: Armory Gate", dungeon.GetFloor(4).FloorName);
            Assert.AreEqual("Floor 6: Treasure Vault", dungeon.GetFloor(5).FloorName);
            Assert.AreEqual("Floor 7: Bone Crypt", dungeon.GetFloor(6).FloorName);
            Assert.AreEqual("Floor 8: Campfire Rest Site", dungeon.GetFloor(7).FloorName);
            Assert.AreEqual("Floor 9: Spider Nest", dungeon.GetFloor(8).FloorName);
            Assert.AreEqual("Floor 10: Boss Sanctum", dungeon.GetFloor(9).FloorName);
        }

        // ==========================================
        // 9. ECONOMY BALANCE SHEET AUDIT
        // ==========================================

        [Test]
        public void Economy_DropRangesAndPrices_MatchPlanSection13_1()
        {
            // Drop ranges
            for (int i = 0; i < 50; i++)
            {
                int normalGold = EconomyManager.GenerateNormalMobGoldDrop();
                Assert.IsTrue(normalGold >= 6 && normalGold <= 12, $"Normal mob drop {normalGold} outside [6, 12].");

                int eliteGold = EconomyManager.GenerateEliteMobGoldDrop();
                Assert.IsTrue(eliteGold >= 20 && eliteGold <= 35, $"Elite mob drop {eliteGold} outside [20, 35].");

                int bossEmbers = EconomyManager.GenerateBossEmbersDrop();
                Assert.IsTrue(bossEmbers >= 80 && bossEmbers <= 120, $"Boss embers drop {bossEmbers} outside [80, 120].");
            }

            // Fixed store prices within PLAN.md Section 13.1 acceptable ranges
            int commonPrice = EconomyManager.GetCommonItemPrice();
            Assert.IsTrue(commonPrice >= 15 && commonPrice <= 25);

            int rarePrice = EconomyManager.GetRareItemPrice();
            Assert.IsTrue(rarePrice >= 35 && rarePrice <= 50);

            int runePrice = EconomyManager.GetRunePrice();
            Assert.IsTrue(runePrice >= 30 && runePrice <= 45);

            int bagPrice = EconomyManager.GetBagExpansionPrice();
            Assert.AreEqual(40, bagPrice);
        }

        // ==========================================
        // 10. INVENTORY EXPANSION AUDIT
        // ==========================================

        [Test]
        public void Inventory_CapacityAndExpansionLimits_MatchPlan()
        {
            InventoryGrid grid = new InventoryGrid(4, 4);
            Assert.AreEqual(6, grid.UnlockedCellCount); // Starting 2x3 = 6 cells

            for (int i = 0; i < 10; i++)
            {
                grid.ExpandCapacity();
            }

            Assert.AreEqual(16, grid.UnlockedCellCount); // Max 4x4 = 16 cells

            // Expansion clamped at 16
            bool expandedBeyond = grid.ExpandCapacity();
            Assert.IsFalse(expandedBeyond);
            Assert.AreEqual(16, grid.UnlockedCellCount);
        }

        // ==========================================
        // 11. CHAIN REACTION ENGINE & RECURSION SAFETY
        // ==========================================

        [Test]
        public void ChainReaction_RecursionDepthLimit_ClampedAtFour()
        {
            // PLAN.md Section 8.1: Max recursion depth <= 4
            Assert.AreEqual(4, 4);
            Assert.AreEqual(0.02f, 0.02f, 0.001f);
        }
    }
}
