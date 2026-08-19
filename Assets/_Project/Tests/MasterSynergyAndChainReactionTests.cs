using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Runes;
using Lattirune.Synergy;

namespace Lattirune.Tests
{
    /// <summary>
    /// Test suite for Master Item Combinations (PLAN.md Section 7.1) and Chain Reaction Loop Guards (PLAN.md Section 8.1).
    /// </summary>
    [TestFixture]
    public class MasterSynergyAndChainReactionTests
    {
        private SynergyDatabaseSO _synergyDb;
        private ItemDatabaseSO _itemDb;
        private RuneDatabaseSO _runeDb;
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _synergyDb = SynergyDatabaseSO.CreateDefaultDatabase();
            _itemDb = ItemDatabaseSO.CreateCanonicalDatabase();
            _runeDb = RuneDatabaseSO.CreateCanonicalDatabase();
            _holderObj = new GameObject("MasterSynergyTestHolder");
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
        // 1. MASTER ITEM COMBINATIONS (Section 7.1)
        // ==========================================

        [Test]
        public void FlamingBlade_EmberRune_And_Broadsword_Activates()
        {
            var ember = _runeDb.GetRune("rune_ember");
            var broadsword = _itemDb.GetItem("item_iron_broadsword");

            var match = _synergyDb.FindMatchingDefinition(ember, broadsword);
            Assert.IsNotNull(match);
            Assert.AreEqual("combo_flaming_blade", match.SynergyId);
            Assert.AreEqual("Flaming Blade", match.DisplayName);
            Assert.AreEqual(6, match.RuneBonus);
        }

        [Test]
        public void VenomShiv_VenomRune_And_RustyDagger_Activates()
        {
            var venom = _runeDb.GetRune("rune_venom");
            var dagger = _itemDb.GetItem("item_rusty_dagger");

            var match = _synergyDb.FindMatchingDefinition(venom, dagger);
            Assert.IsNotNull(match);
            Assert.AreEqual("combo_venom_shiv", match.SynergyId);
            Assert.AreEqual("Venom Shiv", match.DisplayName);
            Assert.AreEqual(3, match.RuneBonus);
        }

        [Test]
        public void ThunderBow_SparkRune_And_Shortbow_Activates()
        {
            var spark = _runeDb.GetRune("rune_spark");
            var shortbow = _itemDb.GetItem("item_shortbow");

            var match = _synergyDb.FindMatchingDefinition(spark, shortbow);
            Assert.IsNotNull(match);
            Assert.AreEqual("combo_thunder_bow", match.SynergyId);
            Assert.AreEqual("Thunder Bow", match.DisplayName);
            Assert.AreEqual(8, match.RuneBonus);
        }

        [Test]
        public void MoltenWall_EmberRune_And_TowerShield_Activates()
        {
            var ember = _runeDb.GetRune("rune_ember");
            var shield = _itemDb.GetItem("item_iron_tower_shield");

            var match = _synergyDb.FindMatchingDefinition(ember, shield);
            Assert.IsNotNull(match);
            Assert.AreEqual("combo_molten_wall", match.SynergyId);
            Assert.AreEqual("Molten Wall", match.DisplayName);
            Assert.AreEqual(8, match.RuneBonus);
        }

        [Test]
        public void Shatterstrike_FrostRune_And_Battleaxe_Activates()
        {
            var frost = _runeDb.GetRune("rune_frost");
            var axe = _itemDb.GetItem("item_battleaxe");

            var match = _synergyDb.FindMatchingDefinition(frost, axe);
            Assert.IsNotNull(match);
            Assert.AreEqual("combo_shatterstrike", match.SynergyId);
            Assert.AreEqual("Shatterstrike", match.DisplayName);
            Assert.AreEqual(6, match.RuneBonus);
        }

        // ==========================================
        // 2. MASTER SYNERGY PRIORITY
        // ==========================================

        [Test]
        public void MasterSynergy_TakesPriorityOverGenericCategorySynergy()
        {
            var ember = _runeDb.GetRune("rune_ember");
            var broadsword = _itemDb.GetItem("item_iron_broadsword");

            var match = _synergyDb.FindMatchingDefinition(ember, broadsword);
            // Specific Flaming Blade combo (priority 100) must beat generic fire_sword (priority 0)
            Assert.AreEqual("combo_flaming_blade", match.SynergyId);
            Assert.Greater(match.Priority, 0);
        }

        [Test]
        public void GenericSynergy_StillAppliesToUnrelatedWeapons()
        {
            var ember = _runeDb.GetRune("rune_ember");
            var wand = _itemDb.GetItem("item_apprentice_wand"); // Weapon without a specific master combo

            var match = _synergyDb.FindMatchingDefinition(ember, wand);
            Assert.IsNotNull(match);
            Assert.AreEqual("fire_sword", match.SynergyId);
            Assert.AreEqual(5, match.RuneBonus);
        }

        // ==========================================
        // 3. DATABASE VALIDATION
        // ==========================================

        [Test]
        public void SynergyDatabase_IsValid_NoDuplicateIds()
        {
            Assert.IsTrue(_synergyDb.ValidateDatabase(out var errors), string.Join("; ", errors));
        }

        // ==========================================
        // 4. CHAIN REACTION ENGINE & LOOP GUARDS (Section 8.1)
        // ==========================================

        [Test]
        public void ChainEngine_EventsExecuteSequentially()
        {
            ChainReactionEngine engine = _holderObj.AddComponent<ChainReactionEngine>();

            List<string> executionOrder = new List<string>();
            engine.OnChainEventProcessed += evt => executionOrder.Add(evt.EventId);

            engine.EnqueueEvent(new ChainEvent("evt_1", "src_1", "tgt_1", 0, 0f, "detonate", 10));
            engine.EnqueueEvent(new ChainEvent("evt_2", "src_2", "tgt_2", 1, 0f, "arc", 5));
            engine.EnqueueEvent(new ChainEvent("evt_3", "src_3", "tgt_3", 2, 0f, "pulse", 3));

            int processed = engine.ProcessQueue();
            Assert.AreEqual(3, processed);
            Assert.AreEqual(3, executionOrder.Count);
            Assert.AreEqual("evt_1", executionOrder[0]);
            Assert.AreEqual("evt_2", executionOrder[1]);
            Assert.AreEqual("evt_3", executionOrder[2]);
        }

        [Test]
        public void ChainEngine_Depth0To4_ExecuteSuccessfully()
        {
            ChainReactionEngine engine = _holderObj.AddComponent<ChainReactionEngine>();

            for (int d = 0; d <= 4; d++)
            {
                bool enqueued = engine.EnqueueEvent(new ChainEvent($"evt_depth_{d}", $"src_{d}", "tgt", d, 0f, "chain", 1));
                Assert.IsTrue(enqueued, $"Depth {d} should be allowed");
            }

            Assert.AreEqual(5, engine.ProcessQueue());
        }

        [Test]
        public void ChainEngine_Depth5_IsRejected()
        {
            ChainReactionEngine engine = _holderObj.AddComponent<ChainReactionEngine>();

            bool depth5Enqueued = engine.EnqueueEvent(new ChainEvent("evt_depth_5", "src", "tgt", 5, 0f, "overflow", 1));
            Assert.IsFalse(depth5Enqueued, "Depth 5 must be rejected by recursion depth limit");

            bool depth6Enqueued = engine.EnqueueEvent(new ChainEvent("evt_depth_6", "src", "tgt", 6, 0f, "overflow", 1));
            Assert.IsFalse(depth6Enqueued, "Depth 6 must be rejected by recursion depth limit");
        }

        [Test]
        public void ChainEngine_FrameTickPropagationCap_RejectsWithin0_02s()
        {
            ChainReactionEngine engine = _holderObj.AddComponent<ChainReactionEngine>();

            // First event at t = 0.00s
            bool first = engine.EnqueueEvent(new ChainEvent("evt_a1", "src_item", "tgt", 0, 0.00f, "trigger", 5), currentTime: 0.00f);
            Assert.IsTrue(first);
            engine.ProcessQueue(currentTime: 0.00f);

            // Second event from same source at t = 0.01s (less than 0.02s interval) -> rejected
            bool second = engine.EnqueueEvent(new ChainEvent("evt_a2", "src_item", "tgt", 1, 0.01f, "trigger", 5), currentTime: 0.01f);
            Assert.IsFalse(second, "Event within 0.02s tick cap from same source must be rejected");

            // Third event at t = 0.025s (>= 0.02s interval) -> accepted
            bool third = engine.EnqueueEvent(new ChainEvent("evt_a3", "src_item", "tgt", 1, 0.025f, "trigger", 5), currentTime: 0.025f);
            Assert.IsTrue(third, "Event after 0.02s interval must be accepted");
        }

        [Test]
        public void ChainEngine_CyclicDuplicateEvents_AreRejected()
        {
            ChainReactionEngine engine = _holderObj.AddComponent<ChainReactionEngine>();

            engine.EnqueueEvent(new ChainEvent("dup_id", "src", "tgt", 0, 0f, "pulse", 10));
            engine.ProcessQueue();

            // Attempting to re-enqueue processed event ID
            bool reEnqueued = engine.EnqueueEvent(new ChainEvent("dup_id", "src", "tgt", 1, 0.1f, "pulse", 10), currentTime: 0.1f);
            Assert.IsFalse(reEnqueued, "Duplicate event ID must be rejected by cycle protection");
        }

        [Test]
        public void ChainEngine_ResetEngine_ClearsTransientState()
        {
            ChainReactionEngine engine = _holderObj.AddComponent<ChainReactionEngine>();

            engine.EnqueueEvent(new ChainEvent("evt_reset", "src", "tgt", 0, 0f, "pulse", 10));
            engine.ProcessQueue();
            Assert.AreEqual(1, engine.ProcessedCount);

            engine.ResetEngine();
            Assert.AreEqual(0, engine.QueueCount);
            Assert.AreEqual(0, engine.ProcessedCount);
        }
    }
}
