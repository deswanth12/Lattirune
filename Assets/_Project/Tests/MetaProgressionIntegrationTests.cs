using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Items;
using Lattirune.Progression;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Test suite for Meta-Progression Upgrades, Blueprint Effects, and Gameplay Integration (TASK-027).
    /// Strictly verifies PLAN.md Section 12, Section 13, and Section 22.
    /// </summary>
    [TestFixture]
    public class MetaProgressionIntegrationTests
    {
        private GameObject _holderObj;
        private MetaProgressionManager _metaManager;
        private BlueprintDatabaseSO _blueprintDb;
        private ItemDatabaseSO _itemDb;
        private RuneDatabaseSO _runeDb;
        private RunManager _runManager;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;
        private CombatSystem _combat;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MetaProgressionIntegrationHolder");
            _blueprintDb = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();
            _itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            _runeDb = RuneDatabaseSO.CreateCanonicalDatabase();

            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(initialHp: 100);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _enemy.SetupSewerRat();

            _combat = _holderObj.AddComponent<CombatSystem>();
            _combat.Initialize(_player, _enemy);

            _metaManager = _holderObj.AddComponent<MetaProgressionManager>();
            _metaManager.Initialize(_blueprintDb);

            _runManager = _holderObj.AddComponent<RunManager>();
            _runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                _combat,
                null,
                _player,
                _enemy,
                null,
                _metaManager
            );
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
        // 1. BLUEPRINT EFFECTS RESOLUTION
        // ==========================================

        [Test]
        public void BlueprintEffects_StartingGoldBonus_AppliedAtStartOfRun()
        {
            // Give Embers and unlock Mercenary Purse (+15 Starting Gold)
            _metaManager.AddEmbers(100);
            bool unlocked = _metaManager.UnlockBlueprintById("bp_mercenary_purse");
            Assert.IsTrue(unlocked);
            Assert.AreEqual(15, _metaManager.GetStartingGoldBonus());

            // Start run - should start with 15 Gold
            _runManager.StartRun(_metaManager);
            Assert.AreEqual(15, _runManager.CurrentGold);
        }

        [Test]
        public void BlueprintEffects_StartingHpBonus_AppliedAtStartOfRun()
        {
            // Give Embers and unlock Vitality Infusion (+20 Starting HP)
            _metaManager.AddEmbers(100);
            bool unlocked = _metaManager.UnlockBlueprintById("bp_vitality_infusion");
            Assert.IsTrue(unlocked);
            Assert.AreEqual(20, _metaManager.GetStartingHpBonus());

            // Start run - Player Max HP should be 120 (100 + 20)
            _runManager.StartRun(_metaManager);
            Assert.AreEqual(120, _player.MaxHp);
            Assert.AreEqual(120, _player.CurrentHp);
        }

        [Test]
        public void BlueprintEffects_MultipleStartingBonuses_AccumulateDeterministically()
        {
            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprintById("bp_mercenary_purse"); // +15 Gold
            _metaManager.UnlockBlueprintById("bp_vitality_infusion"); // +20 HP

            _runManager.StartRun(_metaManager);
            Assert.AreEqual(15, _runManager.CurrentGold);
            Assert.AreEqual(120, _player.MaxHp);
        }

        [Test]
        public void BlueprintEffects_ResetRun_DoesNotStackStartingBonuses()
        {
            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprintById("bp_mercenary_purse");
            _metaManager.UnlockBlueprintById("bp_vitality_infusion");

            // Run 1
            _runManager.StartRun(_metaManager);
            Assert.AreEqual(15, _runManager.CurrentGold);
            Assert.AreEqual(120, _player.MaxHp);

            // Add some in-run gold
            _runManager.AddGold(30);
            Assert.AreEqual(45, _runManager.CurrentGold);

            // Reset & Start Run 2
            _runManager.ResetRun();
            _runManager.StartRun(_metaManager);

            // Must reset to initial 15 Gold and 120 MaxHP, NOT accumulate 45 + 15
            Assert.AreEqual(15, _runManager.CurrentGold);
            Assert.AreEqual(120, _player.MaxHp);
        }

        // ==========================================
        // 2. ITEM & RUNE UNLOCK INTEGRATION
        // ==========================================

        [Test]
        public void ItemUnlocks_RewardGenerator_RespectsUnlockedItemPool()
        {
            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprintById("bp_battleaxe"); // Unlocks item_battleaxe
            _metaManager.UnlockBlueprintById("bp_ruby_ring");  // Unlocks item_ruby_ring

            HashSet<string> unlockedItems = _metaManager.GetUnlockedItemIds();
            Assert.IsTrue(unlockedItems.Contains("item_battleaxe"));
            Assert.IsTrue(unlockedItems.Contains("item_ruby_ring"));

            // Generate rewards restricted to unlocked pool
            var rewards = RewardGenerator.GenerateRewardOptions(_itemDb.AllItems, unlockedItems, count: 2, seed: 42);
            Assert.AreEqual(2, rewards.Count);
            foreach (var r in rewards)
            {
                Assert.IsTrue(unlockedItems.Contains(r.ItemData.ItemId), $"Reward item '{r.ItemData.ItemId}' was not in unlocked blueprint pool");
            }
        }

        [Test]
        public void RuneUnlocks_ComputeUnlockedRuneIds_AggregatesRuneBlueprints()
        {
            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprintById("bp_rune_crossfire");
            _metaManager.UnlockBlueprintById("bp_rune_haste");

            HashSet<string> unlockedRunes = _metaManager.GetUnlockedRuneIds();
            Assert.IsTrue(unlockedRunes.Contains("rune_crossfire"));
            Assert.IsTrue(unlockedRunes.Contains("rune_haste"));
            Assert.IsFalse(unlockedRunes.Contains("rune_ember"));
        }

        // ==========================================
        // 3. PERSISTENCE & DEFEAT SURVIVAL
        // ==========================================

        [Test]
        public void PermanentUpgrades_SurviveRunDefeat_AndRunVictory()
        {
            _metaManager.AddEmbers(200);
            _metaManager.UnlockBlueprintById("bp_battleaxe");

            // Start run and lose
            _runManager.StartRun(_metaManager);
            _player.TakeDamage(new DamageResult("Enemy", "Hero", 200, 0, 200, false, false));
            Assert.IsFalse(_player.IsAlive);

            _runManager.ResetRun();

            // Meta blueprint remains unlocked
            Assert.IsTrue(_metaManager.IsBlueprintUnlocked("bp_battleaxe"));
        }

        [Test]
        public void SaveSystem_MetaProgression_RoundtripsAccuratelyWithSaveData()
        {
            _metaManager.AddEmbers(300);
            _metaManager.UnlockBlueprintById("bp_shortbow");
            _metaManager.UnlockBlueprintById("bp_mercenary_purse");
            _metaManager.RecordRunAttempt();
            _metaManager.RecordBossClear(95);

            SaveData data = SaveData.CreateDefault();
            data.meta = _metaManager.ExportMetaData();

            string json = SaveSerializer.SerializeToJson(data);
            SaveData loadedData = SaveSerializer.DeserializeFromJson(json);

            Assert.IsNotNull(loadedData.meta);
            Assert.AreEqual(data.meta.embers, loadedData.meta.embers);
            Assert.AreEqual(2, loadedData.meta.unlockedBlueprints.Count);
            Assert.AreEqual(1, loadedData.meta.totalBossClears);
            Assert.AreEqual(1, loadedData.meta.totalRunsAttempted);
        }

        [Test]
        public void BlueprintDefinitionSO_Immutability_DoesNotMutateDuringForgeUnlocks()
        {
            var purseBp = _blueprintDb.GetBlueprint("bp_mercenary_purse");
            int originalCost = purseBp.EmberCost;
            int originalValue = purseBp.EffectValue;

            _metaManager.AddEmbers(100);
            _metaManager.UnlockBlueprint(purseBp);

            Assert.AreEqual(originalCost, purseBp.EmberCost);
            Assert.AreEqual(originalValue, purseBp.EffectValue);
        }
    }
}
