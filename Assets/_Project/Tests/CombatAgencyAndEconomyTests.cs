using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    /// <summary>
    /// Test suite for Combat Simulation Agency (PLAN.md Section 9.1), In-Run Economy (Section 13.1),
    /// Merchant Stall, and Campfire Rest Site non-combat room events (Section 11).
    /// </summary>
    [TestFixture]
    public class CombatAgencyAndEconomyTests
    {
        private GameObject _holderObj;
        private CombatSystem _combat;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;
        private RunManager _runManager;
        private ItemDatabaseSO _itemDb;
        private RuneDatabaseSO _runeDb;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("CombatAgencyTestHolder");
            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(initialHp: 100);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _enemy.SetupSewerRat();

            _combat = _holderObj.AddComponent<CombatSystem>();
            _combat.Initialize(_player, _enemy);

            _runManager = _holderObj.AddComponent<RunManager>();
            _runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                _combat,
                null,
                _player,
                _enemy
            );

            _itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            _runeDb = RuneDatabaseSO.CreateCanonicalDatabase();
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
        // 1. COMBAT SPEED (PLAN.md Section 9.1)
        // ==========================================

        [Test]
        public void CombatSpeed_1x_2x_3x_AreAccepted()
        {
            Assert.IsTrue(_combat.SetSpeedMultiplier(1.0f));
            Assert.AreEqual(1.0f, _combat.SpeedMultiplier);

            Assert.IsTrue(_combat.SetSpeedMultiplier(2.0f));
            Assert.AreEqual(2.0f, _combat.SpeedMultiplier);

            Assert.IsTrue(_combat.SetSpeedMultiplier(3.0f));
            Assert.AreEqual(3.0f, _combat.SpeedMultiplier);
        }

        [Test]
        public void CombatSpeed_Invalid_IsRejected()
        {
            Assert.IsFalse(_combat.SetSpeedMultiplier(0.5f));
            Assert.IsFalse(_combat.SetSpeedMultiplier(4.0f));
            Assert.IsFalse(_combat.SetSpeedMultiplier(-1.0f));
            Assert.AreEqual(1.0f, _combat.SpeedMultiplier);
        }

        [Test]
        public void CombatSpeed_AffectsCooldownProgressionDeterministically()
        {
            _combat.SetSpeedMultiplier(2.0f);
            _player.ResetCooldown(); // Cooldown = 1.2s

            // 0.3s real time at 2x speed simulates 0.6s delta
            _player.TickCooldown(0.3f * _combat.SpeedMultiplier);
            Assert.AreEqual(0.4f, _player.CooldownTimer, 0.001f);
        }

        [Test]
        public void CombatSpeed_ResetCombat_Restores1x()
        {
            _combat.SetSpeedMultiplier(3.0f);
            Assert.AreEqual(3.0f, _combat.SpeedMultiplier);

            _combat.ResetCombat();
            Assert.AreEqual(1.0f, _combat.SpeedMultiplier);
        }

        // ==========================================
        // 2. EMERGENCY POTION (PLAN.md Section 9.1)
        // ==========================================

        [Test]
        public void EmergencyPotion_HealsPlayer_ClampsAtMaxHp()
        {
            // Damage player to 50 HP
            _player.TakeDamage(new DamageResult("Enemy", "Hero", 50, 0, 50, false, false));
            Assert.AreEqual(50, _player.CurrentHp);

            // Drink potion +35 HP -> 85 HP
            bool used = _combat.UseEmergencyPotion(_player, 35);
            Assert.IsTrue(used);
            Assert.AreEqual(85, _player.CurrentHp);

            // Drink another potion +35 HP -> Clamps at 100 MaxHP
            bool used2 = _combat.UseEmergencyPotion(_player, 35);
            Assert.IsTrue(used2);
            Assert.AreEqual(100, _player.CurrentHp);
        }

        [Test]
        public void EmergencyPotion_DeadPlayer_IsRejected()
        {
            // Kill player
            _player.TakeDamage(new DamageResult("Enemy", "Hero", 100, 0, 100, false, false));
            Assert.IsFalse(_player.IsAlive);

            bool used = _combat.UseEmergencyPotion(_player, 35);
            Assert.IsFalse(used);
            Assert.AreEqual(0, _player.CurrentHp);
        }

        [Test]
        public void EmergencyPotion_InvalidHeal_IsRejected()
        {
            Assert.IsFalse(_combat.UseEmergencyPotion(_player, 0));
            Assert.IsFalse(_combat.UseEmergencyPotion(_player, -20));
            Assert.IsFalse(_combat.UseEmergencyPotion(null, 35));
        }

        // ==========================================
        // 3. ECONOMY MANAGER (PLAN.md Section 13.1)
        // ==========================================

        [Test]
        public void EconomyManager_NormalMobGold_Within6To12()
        {
            for (int i = 0; i < 50; i++)
            {
                int gold = EconomyManager.GetGoldDrop(isElite: false);
                Assert.GreaterOrEqual(gold, 6);
                Assert.LessOrEqual(gold, 12);
            }
        }

        [Test]
        public void EconomyManager_EliteMobGold_Within20To35()
        {
            for (int i = 0; i < 50; i++)
            {
                int gold = EconomyManager.GetGoldDrop(isElite: true);
                Assert.GreaterOrEqual(gold, 20);
                Assert.LessOrEqual(gold, 35);
            }
        }

        [Test]
        public void EconomyManager_BossEmbers_Within80To120()
        {
            for (int i = 0; i < 50; i++)
            {
                int embers = EconomyManager.GetBossEmbersDrop();
                Assert.GreaterOrEqual(embers, 80);
                Assert.LessOrEqual(embers, 120);
            }
        }

        [Test]
        public void EconomyManager_Prices_MatchSection13()
        {
            Assert.AreEqual(20, EconomyManager.GetCommonItemPrice());
            Assert.AreEqual(40, EconomyManager.GetRareItemPrice());
            Assert.AreEqual(35, EconomyManager.GetRunePrice());
            Assert.AreEqual(40, EconomyManager.GetBagExpansionPrice());
        }

        // ==========================================
        // 4. RUN MANAGER ECONOMY
        // ==========================================

        [Test]
        public void RunManager_AddGold_SpendGold_Work()
        {
            _runManager.StartRun();
            Assert.AreEqual(0, _runManager.CurrentGold);

            _runManager.AddGold(50);
            Assert.AreEqual(50, _runManager.CurrentGold);
            Assert.IsTrue(_runManager.CanAfford(40));

            bool spent = _runManager.SpendGold(30);
            Assert.IsTrue(spent);
            Assert.AreEqual(20, _runManager.CurrentGold);
        }

        [Test]
        public void RunManager_InsufficientFunds_IsRejected_NoNegativeGold()
        {
            _runManager.StartRun();
            _runManager.AddGold(15);

            bool spent = _runManager.SpendGold(20);
            Assert.IsFalse(spent);
            Assert.AreEqual(15, _runManager.CurrentGold);
        }

        [Test]
        public void RunManager_ResetRun_ClearsGoldAndEmbers()
        {
            _runManager.StartRun();
            _runManager.AddGold(100);
            _runManager.AddEmbers(50);

            _runManager.ResetRun();
            Assert.AreEqual(0, _runManager.CurrentGold);
            Assert.AreEqual(0, _runManager.CurrentEmbers);
        }

        // ==========================================
        // 5. MERCHANT STALL (Floors 4 & 9)
        // ==========================================

        [Test]
        public void MerchantStall_PurchaseCommonItem_Deducts20Gold()
        {
            _runManager.StartRun();
            _runManager.AddGold(50);

            var dagger = _itemDb.GetItem("item_rusty_dagger");
            bool bought = _runManager.PurchaseCommonItem(dagger);
            Assert.IsTrue(bought);
            Assert.AreEqual(30, _runManager.CurrentGold);
        }

        [Test]
        public void MerchantStall_PurchaseRareItem_Deducts40Gold()
        {
            _runManager.StartRun();
            _runManager.AddGold(50);

            var axe = _itemDb.GetItem("item_battleaxe");
            bool bought = _runManager.PurchaseRareItem(axe);
            Assert.IsTrue(bought);
            Assert.AreEqual(10, _runManager.CurrentGold);
        }

        [Test]
        public void MerchantStall_PurchaseRune_Deducts35Gold()
        {
            _runManager.StartRun();
            _runManager.AddGold(50);

            var ember = _runeDb.GetRune("rune_ember");
            bool bought = _runManager.PurchaseRune(ember);
            Assert.IsTrue(bought);
            Assert.AreEqual(15, _runManager.CurrentGold);
        }

        [Test]
        public void MerchantStall_PurchaseBagExpansion_Deducts40Gold()
        {
            _runManager.StartRun();
            _runManager.AddGold(50);

            bool bought = _runManager.PurchaseBagExpansion();
            Assert.IsTrue(bought);
            Assert.AreEqual(10, _runManager.CurrentGold);
        }

        [Test]
        public void MerchantStall_InsufficientFunds_RejectsPurchase()
        {
            _runManager.StartRun();
            _runManager.AddGold(10);

            var axe = _itemDb.GetItem("item_battleaxe");
            bool bought = _runManager.PurchaseRareItem(axe);
            Assert.IsFalse(bought);
            Assert.AreEqual(10, _runManager.CurrentGold);
        }

        // ==========================================
        // 6. CAMPFIRE REST SITE (Floor 8)
        // ==========================================

        [Test]
        public void Campfire_Heal_Restores40PercentMaxHp()
        {
            _runManager.StartRun();
            _player.TakeDamage(new DamageResult("Enemy", "Hero", 80, 0, 80, false, false));
            Assert.AreEqual(20, _player.CurrentHp);

            // 40% of 100 MaxHp = +40 HP -> 60 HP
            bool healed = _runManager.ResolveCampfireHeal(_player);
            Assert.IsTrue(healed);
            Assert.AreEqual(60, _player.CurrentHp);
            Assert.IsTrue(_runManager.IsCampfireResolved);

            // Second choice attempt rejected
            bool secondChoice = _runManager.ResolveCampfireRuneUpgrade("rune_ember");
            Assert.IsFalse(secondChoice, "Campfire allows only one choice per rest site");
        }

        [Test]
        public void Campfire_RuneUpgrade_StoresRuntimeBonus_LeavesAssetImmutable()
        {
            _runManager.StartRun();
            var ember = _runeDb.GetRune("rune_ember");
            int baseDmgBonus = ember.FlatDamageBonus;

            bool upgraded = _runManager.ResolveCampfireRuneUpgrade("rune_ember");
            Assert.IsTrue(upgraded);
            Assert.IsTrue(_runManager.IsCampfireResolved);
            Assert.AreEqual(2, _runManager.GetRuntimeRuneUpgrade("rune_ember"));

            // ScriptableObject asset must remain untouched
            Assert.AreEqual(baseDmgBonus, ember.FlatDamageBonus);
        }

        // ==========================================
        // 7. PROGRESSION ROOM TOPOLOGY
        // ==========================================

        [Test]
        public void Dungeon_RoomTopology_MatchesSection11()
        {
            var dungeon = _runManager.Dungeon;
            Assert.AreEqual(10, dungeon.TotalFloorCount);

            // Floor 4: Merchant Stall
            Assert.AreEqual("Floor 4: Merchant Stall", dungeon.GetFloor(3).FloorName);

            // Floor 8: Campfire Rest Site
            Assert.AreEqual("Floor 8: Campfire Rest Site", dungeon.GetFloor(7).FloorName);

            // Floor 9: Spider Nest (Pre-Boss)
            Assert.AreEqual("Floor 9: Spider Nest", dungeon.GetFloor(8).FloorName);

            // Floor 10: Boss Sanctum
            Assert.AreEqual("Floor 10: Boss Sanctum", dungeon.GetFloor(9).FloorName);
            Assert.IsTrue(dungeon.GetFloor(9).GetEncounter(0).IsBoss);
        }
    }
}
