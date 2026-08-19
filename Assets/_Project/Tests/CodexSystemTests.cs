using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.Tests
{
    [TestFixture]
    public class CodexSystemTests
    {
        private GameObject _holder;
        private CodexManager _codex;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""CodexTestHolder"");
            _codex = _holder.AddComponent<CodexManager>();
            _codex.Initialize(BestiaryDatabaseSO.CreateCanonicalDatabase());
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
        public void CanonicalBestiaryDatabase_ContainsAll7CanonicalEnemies()
        {
            var db = _codex.Bestiary;
            Assert.IsNotNull(db);
            Assert.AreEqual(7, db.TotalCount);

            Assert.IsTrue(db.GetEntry(""enemy_sewer_rat"") != null);
            Assert.IsTrue(db.GetEntry(""enemy_goblin_thief"") != null);
            Assert.IsTrue(db.GetEntry(""enemy_armored_skeleton"") != null);
            Assert.IsTrue(db.GetEntry(""enemy_venomous_spider"") != null);
            Assert.IsTrue(db.GetEntry(""enemy_acid_slime"") != null);
            Assert.IsTrue(db.GetEntry(""enemy_necromancer"") != null);
            Assert.IsTrue(db.GetEntry(""enemy_lich_lord"") != null);

            foreach (var enemy in db.AllEntries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(enemy.EnemyName));
                Assert.Greater(enemy.BaseHp, 0);
                Assert.IsFalse(string.IsNullOrEmpty(enemy.UniqueMechanic));
                Assert.IsFalse(string.IsNullOrEmpty(enemy.CounterStrategy));
            }
        }

        [Test]
        public void RecordEnemyEncounterAndDefeat_IncrementsTelemetryAccurately()
        {
            Assert.IsFalse(_codex.IsEnemyDiscovered(""enemy_sewer_rat""));
            Assert.AreEqual(0, _codex.GetEnemyKillCount(""enemy_sewer_rat""));

            _codex.RecordEnemyEncounter(""enemy_sewer_rat"");
            Assert.IsTrue(_codex.IsEnemyDiscovered(""enemy_sewer_rat""));
            Assert.AreEqual(0, _codex.GetEnemyKillCount(""enemy_sewer_rat""));

            _codex.RecordEnemyDefeat(""enemy_sewer_rat"");
            _codex.RecordEnemyDefeat(""enemy_sewer_rat"");
            Assert.AreEqual(2, _codex.GetEnemyKillCount(""enemy_sewer_rat""));
        }

        [Test]
        public void RecordSynergiesAndReactions_TracksUniqueDiscoveries()
        {
            _codex.RecordSynergyDiscovered(""syn_flaming_blade"");
            _codex.RecordSynergyDiscovered(""syn_venom_shiv"");
            _codex.RecordReactionTriggered("react_plasma");
            _codex.RecordReactionTriggered("react_steam");

            Assert.AreEqual(2, _codex.DiscoveredSynergies.Count);
            Assert.AreEqual(2, _codex.DiscoveredReactions.Count);
            Assert.Contains("syn_flaming_blade", new List<string>(_codex.DiscoveredSynergies));
            Assert.Contains("react_plasma", new List<string>(_codex.DiscoveredReactions));
        }

        [Test]
        public void SaveLoadPersistence_PreservesCompleteCodexState()
        {
            _codex.RecordEnemyDefeat(""enemy_lich_lord"");
            _codex.RecordEnemyDefeat(""enemy_lich_lord"");
            _codex.RecordSynergyDiscovered(""syn_molten_wall"");
            _codex.RecordReactionTriggered("react_toxic_flame");

            var killCounts = _codex.ExportKillCounts();

            SaveData save = SaveData.CreateDefault();
            save.codex = new SavedCodexData(
                enemies: _codex.DiscoveredEnemies,
                killKeys: killCounts.keys,
                killVals: killCounts.values,
                synergies: _codex.DiscoveredSynergies,
                reactions: _codex.DiscoveredReactions
            );

            string json = SaveSerializer.SerializeToJson(save);
            Assert.IsNotNull(json);

            SaveData restored = SaveSerializer.DeserializeFromJson(json);
            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.version);
            Assert.Contains(""enemy_lich_lord"", restored.codex.discoveredEnemies);
            Assert.Contains(""syn_molten_wall"", restored.codex.discoveredSynergies);
            Assert.Contains(""react_toxic_flame"", restored.codex.discoveredReactions);
        }
    }
}
