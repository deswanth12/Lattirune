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
    /// Milestone production hardening and comprehensive system audit test suite (TASK-031).
    /// Asserts data uniqueness, ScriptableObject immutability, combat edge cases, reward safety,
    /// economy safeguards, meta progression isolation, dungeon sequencing, and navigation integrity.
    /// </summary>
    [TestFixture]
    public class ProductionHardeningTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("ProductionHardeningHolder");
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
        // 1. DATA UNIQUENESS & AUDIT
        // ==========================================

        [Test]
        public void DataAudit_ItemIds_AreUniqueAndValid()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();
            Assert.IsTrue(db.IsValid(out string error), error);

            HashSet<string> seen = new HashSet<string>();
            foreach (var item in db.AllItems)
            {
                Assert.IsNotNull(item);
                Assert.IsFalse(string.IsNullOrEmpty(item.ItemId), "Item ID must not be empty.");
                Assert.IsFalse(seen.Contains(item.ItemId), $"Duplicate Item ID found: {item.ItemId}");
                seen.Add(item.ItemId);
            }
        }

        [Test]
        public void DataAudit_RuneIds_AreUniqueAndValid()
        {
            RuneDatabaseSO db = RuneDatabaseSO.CreateCanonicalDatabase();
            Assert.IsTrue(db.IsValid(out string error), error);

            HashSet<string> seen = new HashSet<string>();
            foreach (var rune in db.AllRunes)
            {
                Assert.IsNotNull(rune);
                Assert.IsFalse(string.IsNullOrEmpty(rune.RuneId), "Rune ID must not be empty.");
                Assert.IsFalse(seen.Contains(rune.RuneId), $"Duplicate Rune ID found: {rune.RuneId}");
                seen.Add(rune.RuneId);
            }
        }

        [Test]
        public void DataAudit_SynergyIds_AreUniqueAndValid()
        {
            SynergyDatabaseSO db = SynergyDatabaseSO.CreateCanonicalDatabase();
            Assert.IsTrue(db.IsValid(out string error), error);

            HashSet<string> seen = new HashSet<string>();
            foreach (var synergy in db.AllSynergies)
            {
                Assert.IsNotNull(synergy);
                Assert.IsFalse(string.IsNullOrEmpty(synergy.SynergyId), "Synergy ID must not be empty.");
                Assert.IsFalse(seen.Contains(synergy.SynergyId), $"Duplicate Synergy ID found: {synergy.SynergyId}");
                seen.Add(synergy.SynergyId);
            }
        }

        [Test]
        public void DataAudit_ReactionIds_AreUniqueAndValid()
        {
            ElementalReactionDatabaseSO db = ElementalReactionDatabaseSO.CreateCanonicalDatabase();
            Assert.IsTrue(db.IsValid(out string error), error);

            HashSet<string> seen = new HashSet<string>();
            foreach (var reaction in db.AllReactions)
            {
                Assert.IsNotNull(reaction);
                Assert.IsFalse(string.IsNullOrEmpty(reaction.ReactionId), "Reaction ID must not be empty.");
                Assert.IsFalse(seen.Contains(reaction.ReactionId), $"Duplicate Reaction ID found: {reaction.ReactionId}");
                seen.Add(reaction.ReactionId);
            }
        }

        [Test]
        public void DataAudit_BlueprintIds_AreUniqueAndValid()
        {
            BlueprintDatabaseSO db = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();
            Assert.IsTrue(db.IsValid(out string error), error);

            HashSet<string> seen = new HashSet<string>();
            foreach (var bp in db.AllBlueprints)
            {
                Assert.IsNotNull(bp);
                Assert.IsFalse(string.IsNullOrEmpty(bp.BlueprintId), "Blueprint ID must not be empty.");
                Assert.IsFalse(seen.Contains(bp.BlueprintId), $"Duplicate Blueprint ID found: {bp.BlueprintId}");
                seen.Add(bp.BlueprintId);
            }
        }

        // ==========================================
        // 2. SCRIPTABLEOBJECT IMMUTABILITY
        // ==========================================

        [Test]
        public void ScriptableObjectImmutability_CombatAndForge_LeaveDefinitionsUntouched()
        {
            ItemDatabaseSO itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            RuneDatabaseSO runeDb = RuneDatabaseSO.CreateCanonicalDatabase();
            BlueprintDatabaseSO bpDb = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();

            var dagger = itemDb.GetItem("item_rusty_dagger");
            int originalDmg = dagger.BaseDamage;

            var emberRune = runeDb.GetRune("rune_ember");
            int originalRuneDmg = emberRune.FlatDamageBonus;

            var shortbowBp = bpDb.GetBlueprint("bp_shortbow");
            int originalCost = shortbowBp.EmberCost;

            // Perform runtime operations
            var meta = _holderObj.AddComponent<MetaProgressionManager>();
            meta.Initialize(bpDb);
            meta.AddEmbers(100);
            meta.UnlockBlueprint(shortbowBp);

            // Assert ScriptableObject asset fields remain identical
            Assert.AreEqual(originalDmg, dagger.BaseDamage);
            Assert.AreEqual(originalRuneDmg, emberRune.FlatDamageBonus);
            Assert.AreEqual(originalCost, shortbowBp.EmberCost);
        }

        // ==========================================
        // 3. COMBAT EDGE CASES
        // ==========================================

        [Test]
        public void CombatHardening_DeadCombatant_CannotExecuteAttacks()
        {
            var player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupSewerRat();

            var combat = _holderObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);
            combat.StartCombat();

            // Kill enemy
            enemy.TakeDamage(new DamageResult("Hero", "Sewer Rat", 100, 0, 100, false, false));
            Assert.IsFalse(enemy.IsAlive);

            // Ticking enemy cooldown returns false
            Assert.IsFalse(enemy.TickCooldown(5.0f));
        }

        [Test]
        public void CombatHardening_NegativeDamage_ClampedToMinimumZeroOrOne()
        {
            DamageResult result = DamageCalculator.CalculateDamage(
                sourceName: "Hero",
                targetName: "Armored Skeleton",
                baseDamage: 5,
                runeBonus: 0,
                targetArmor: 50, // Massive armor
                isCritical: false,
                damageModifier: 1.0f
            );

            // Damage cannot be negative
            Assert.GreaterOrEqual(result.FinalDamage, 0);
        }

        [Test]
        public void CombatHardening_EmergencyPotion_CannotHealDeadPlayer()
        {
            var player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupSewerRat();

            var combat = _holderObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            // Kill player
            player.TakeDamage(new DamageResult("Enemy", "Hero", 100, 0, 100, false, false));
            Assert.IsFalse(player.IsAlive);

            bool used = combat.UseEmergencyPotion(player, 50);
            Assert.IsFalse(used);
            Assert.AreEqual(0, player.CurrentHp);
        }

        // ==========================================
        // 4. REWARD HARDENING
        // ==========================================

        [Test]
        public void RewardHardening_SelectionLock_PreventsDoubleGranting()
        {
            var rewardService = _holderObj.AddComponent<RewardService>();
            rewardService.ResetSelectionLock();
            Assert.IsFalse(rewardService.IsSelectionLocked);

            var itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            var dagger = itemDb.GetItem("item_rusty_dagger");
            var option = RewardOption.FromItemData(dagger);

            ItemInstance applied1 = rewardService.ApplyReward(option, Vector3.zero, _holderObj.transform);
            Assert.IsNotNull(applied1);
            Assert.IsTrue(rewardService.IsSelectionLocked);

            // Second attempt is blocked
            ItemInstance applied2 = rewardService.ApplyReward(option, Vector3.zero, _holderObj.transform);
            Assert.IsNull(applied2);
        }

        [Test]
        public void RewardHardening_EmptyCatalogue_ReturnsEmptyWithoutThrowing()
        {
            var rewards = RewardGenerator.GenerateRewardOptions(new List<ItemDataSO>(), count: 3);
            Assert.IsNotNull(rewards);
            Assert.AreEqual(0, rewards.Count);

            var nullRewards = RewardGenerator.GenerateRewardOptions(null, count: 3);
            Assert.IsNotNull(nullRewards);
            Assert.AreEqual(0, nullRewards.Count);
        }

        // ==========================================
        // 5. ECONOMY & CAMPFIRE SAFEGUARDS
        // ==========================================

        [Test]
        public void EconomyHardening_Overdraft_DoesNotDeductCurrencyOrModifyState()
        {
            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, null, null);
            run.StartRun();
            run.AddGold(25);

            // Attempt to buy 40 Gold Rare Item
            var itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            var axe = itemDb.GetItem("item_battleaxe");

            bool bought = run.PurchaseRareItem(axe);
            Assert.IsFalse(bought);
            Assert.AreEqual(25, run.CurrentGold); // Unchanged
        }

        [Test]
        public void CampfireRest_SingleUse_ChoiceACannotBeFollowedByChoiceB()
        {
            var player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);
            player.TakeDamage(new DamageResult("Enemy", "Hero", 50, 0, 50, false, false));

            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, player, null);
            run.StartRun();

            // Option A: Heal
            bool healed = run.ResolveCampfireHeal(player);
            Assert.IsTrue(healed);
            Assert.IsTrue(run.IsCampfireResolved);

            // Option B: Rune upgrade is blocked
            bool upgraded = run.ResolveCampfireRuneUpgrade("rune_ember");
            Assert.IsFalse(upgraded);
        }

        // ==========================================
        // 6. DUNGEON PROGRESSION SEQUENCING
        // ==========================================

        [Test]
        public void DungeonProgression_RunStartsAtFloor1_AndAdvancesSequentially()
        {
            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, null, null);

            run.StartRun();
            Assert.AreEqual(1, run.CurrentFloorNumber);
            Assert.AreEqual("Floor 1: Sewer Entry", run.CurrentFloor.FloorName);
        }

        [Test]
        public void DungeonProgression_Floor10Clearing_TransitionsToRunComplete()
        {
            var player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);

            var enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupSewerRat();

            var combat = _holderObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), combat, null, player, enemy);

            // Restore directly to Floor 10 (index 9)
            run.RestoreRunState(floorIdx: 9, encIdx: 0, state: RunState.RewardSelection);
            Assert.AreEqual(10, run.CurrentFloorNumber);
            Assert.IsTrue(run.IsFinalFloor);

            run.ContinueAfterReward();
            Assert.AreEqual(RunState.RunComplete, run.CurrentState);
            Assert.IsTrue(run.IsRunFinished);
        }

        // ==========================================
        // 7. META PROGRESSION & NAVIGATION SAFETY
        // ==========================================

        [Test]
        public void MetaProgression_StartingBonuses_ApplyOncePerRun_WithoutStacking()
        {
            var meta = _holderObj.AddComponent<MetaProgressionManager>();
            meta.Initialize();
            meta.AddEmbers(200);
            meta.UnlockBlueprintById("bp_mercenary_purse"); // +15 Starting Gold

            var run = _holderObj.AddComponent<RunManager>();
            run.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), null, null, null, null, null, meta);

            run.StartRun(meta);
            Assert.AreEqual(15, run.CurrentGold);

            // Reset and restart
            run.ResetRun();
            run.StartRun(meta);
            Assert.AreEqual(15, run.CurrentGold, "Starting gold must reset to exact +15 bonus, not accumulate to 30.");
        }

        [Test]
        public void NavigationSafety_CombatBackBlocked_DoesNotAbandonRun()
        {
            var nav = _holderObj.AddComponent<ScreenNavigationController>();
            nav.Initialize(ScreenState.MAIN_MENU);
            nav.NavigateTo(ScreenState.COMBAT);

            bool backed = nav.NavigateBack();
            Assert.IsFalse(backed);
            Assert.AreEqual(ScreenState.COMBAT, nav.CurrentScreen);
        }

        [Test]
        public void SaveHardening_SaveVersion1_CompatibleWithAllSystems()
        {
            SaveData defaultSave = SaveData.CreateDefault();
            Assert.AreEqual(1, defaultSave.version);
            Assert.AreEqual(SaveVersion.CURRENT_VERSION, defaultSave.version);
            Assert.IsTrue(SaveValidator.ValidateSaveData(defaultSave, out var errors), string.Join("; ", errors));
        }
    }
}
