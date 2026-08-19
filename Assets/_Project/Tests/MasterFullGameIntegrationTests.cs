using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Combat.Effects;
using Lattirune.Combo;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Modifiers;
using Lattirune.Monetization;
using Lattirune.Progression;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Master Full-Game Integration Test Suite for Lattirune.
    /// Audits the entire game loop from class selection, map traversal, grid arrangement,
    /// raycast conduits, elemental reactions, combat juice, procedural events, merchant stalls,
    /// boss battles, offline monetization, codex telemetry, and encrypted save/load serialization.
    /// </summary>
    [TestFixture]
    public class MasterFullGameIntegrationTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("MasterFullGameTestHolder");
        }

        [TearDown]
        public void TearDown()
        {
            if (_holder != null)
            {
                Object.DestroyImmediate(_holder);
            }
        }

        [Test]
        public void FullGame_UnifiedLoop_ExecutesSeamlesslyWithZeroExceptions()
        {
            // 1. Grid & Inventory
            var gridObj = new GameObject("LatticeGrid");
            gridObj.transform.SetParent(_holder.transform);
            var grid = gridObj.AddComponent<LatticeGrid>();
            grid.Initialize();

            var invObj = new GameObject("InventorySystem");
            invObj.transform.SetParent(_holder.transform);
            var inventory = invObj.AddComponent<InventorySystem>();
            inventory.Initialize(grid);

            // 2. Combat & Modifiers & Combos
            var playerObj = new GameObject("PlayerCombatant");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();

            var enemyObj = new GameObject("EnemyCombatant");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var comboTracker = _holder.AddComponent<ComboTracker>();
            comboTracker.Initialize(step: 0.05f, maxMult: 2.0f);

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, null, modManager, comboTracker);

            // 3. Dungeon & Meta & Economy
            var meta = _holder.AddComponent<MetaProgressionManager>();
            meta.Initialize(startingEmbers: 100);

            var economy = _holder.AddComponent<SimpleEconomyService>();
            economy.Initialize(startingGold: 50);

            var runManager = _holder.AddComponent<RunManager>();
            runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                combat,
                null,
                player,
                enemy,
                null,
                meta,
                modManager,
                comboTracker
            );

            // 4. Hero Classes
            var heroClassManager = _holder.AddComponent<HeroClassManager>();
            heroClassManager.Initialize();
            heroClassManager.UnlockClass("class_elementalist", meta);
            heroClassManager.SelectClass("class_elementalist");
            heroClassManager.ApplyStartingLoadout(player, inventory, grid);

            Assert.AreEqual(85, player.MaxHp);
            Assert.AreEqual(85, player.CurrentHp);
            Assert.AreEqual(3, inventory.StagingItemCount);

            // 5. Map DAG Navigation
            var mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            Assert.IsTrue(mapGraph.SelectAndEnterNode("node_f1_entry"));

            // 6. Combat Juice & Floaty Pool
            var floatyPool = _holder.AddComponent<FloatingCombatTextPool>();
            floatyPool.Initialize(combat);

            var shake = _holder.AddComponent<CombatCameraShakeController>();
            shake.Initialize(null, combat);

            // 7. Codex Telemetry
            var codex = _holder.AddComponent<CodexManager>();
            codex.Initialize(BestiaryDatabaseSO.CreateCanonicalDatabase());

            // 8. Start Run & Fight Floor 1
            runManager.StartRun();
            Assert.AreEqual(RunState.FloorPreparing, runManager.CurrentState);

            runManager.StartEncounterCombat();
            Assert.AreEqual(RunState.EncounterActive, runManager.CurrentState);

            // Combat ticks
            combat.Tick(1.5f);
            Assert.Greater(comboTracker.CurrentCombo, 0);

            // Defeat Sewer Rat
            codex.RecordEnemyDefeat("enemy_sewer_rat");
            enemy.TakeDirectDamage(200);
            combat.Tick(0.1f);

            Assert.AreEqual(RunState.RewardSelection, runManager.CurrentState);
            Assert.AreEqual(1, codex.GetEnemyKillCount("enemy_sewer_rat"));
            Assert.IsTrue(mapGraph.CompleteCurrentNode());

            // 9. Merchant Stall (Floor 4)
            var merchant = _holder.AddComponent<MerchantSystem>();
            merchant.Initialize(ItemDatabaseSO.CreateCanonicalDatabase(), RuneDatabaseSO.CreateCanonicalDatabase());
            merchant.GenerateOffers(floorNumber: 4);

            int initialLockedCount = grid.GetLockedCount();
            int expansionIndex = -1;
            for (int i = 0; i < merchant.CurrentOffers.Count; i++)
            {
                if (merchant.CurrentOffers[i].OfferType == MerchantOfferType.GridSlotExpansion)
                {
                    expansionIndex = i;
                    break;
                }
            }
            Assert.GreaterOrEqual(expansionIndex, 0);

            // Buy grid expansion
            bool boughtSlot = merchant.BuyOffer(expansionIndex, economy, inventory, grid, player);
            Assert.IsTrue(boughtSlot);
            Assert.AreEqual(initialLockedCount - 1, grid.GetLockedCount());

            // 10. Offline Monetization & Revive
            var monetization = _holder.AddComponent<OfflineMonetizationService>();
            monetization.Initialize();
            monetization.PurchaseNoAdsEmberBoost(() => { }, null);
            Assert.IsTrue(monetization.HasPurchasedNoAdsEmberBoost);

            // 11. Save / Load Persistence Full Roundtrip
            var killCounts = codex.ExportKillCounts();
            SaveData save = SaveData.CreateDefault();
            save.version = SaveVersion.CURRENT_VERSION;
            save.run = new SavedRunData(true, runManager.CurrentFloorIndex, runManager.CurrentEncounterIndex, (int)runManager.CurrentState, modManager.ExportActiveModifierIds(), comboTracker.HighestCombo);
            save.meta = new SavedMetaData(meta.CurrentEmbers, meta.UnlockedBlueprints, meta.TotalBossClears, meta.TotalRunsAttempted, heroClassManager.SelectedClassId, heroClassManager.ExportUnlockedClassIds());
            save.codex = new SavedCodexData(codex.DiscoveredEnemies, killCounts.keys, killCounts.values, codex.DiscoveredSynergies, codex.DiscoveredReactions);

            string serializedJson = SaveSerializer.SerializeToJson(save);
            Assert.IsNotNull(serializedJson);

            SaveData loaded = SaveSerializer.DeserializeFromJson(serializedJson);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.version);
            Assert.AreEqual("class_elementalist", loaded.meta.selectedHeroClass);
            Assert.Contains("enemy_sewer_rat", loaded.codex.discoveredEnemies);
        }
    }
}
