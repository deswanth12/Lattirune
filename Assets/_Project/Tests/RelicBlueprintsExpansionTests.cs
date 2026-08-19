using NUnit.Framework;
using UnityEngine;
using Lattirune.Progression;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RelicBlueprintsExpansionTests
    {
        private BlueprintDatabaseSO _database;
        private MetaProgressionManager _metaManager;

        [SetUp]
        public void Setup()
        {
            _database = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();

            GameObject metaObj = new GameObject("TestMetaManager");
            _metaManager = metaObj.AddComponent<MetaProgressionManager>();
            _metaManager.Initialize(_database);
        }

        [TearDown]
        public void Teardown()
        {
            if (_metaManager != null)
            {
                Object.DestroyImmediate(_metaManager.gameObject);
            }
            if (_database != null)
            {
                Object.DestroyImmediate(_database);
            }
        }

        [Test]
        public void BlueprintDatabase_ContainsAll19CanonicalBlueprints()
        {
            Assert.AreEqual(19, _database.TotalBlueprintCount, "Expected exactly 19 canonical blueprints in database.");
            Assert.IsTrue(_database.IsValid(out string error), $"Database validation failed: {error}");
        }

        [Test]
        public void NewLegendaryBlueprints_ExistWithValidConfigurations()
        {
            string[] newBpIds = new string[]
            {
                "bp_celestial_compass",
                "bp_alchemists_flask",
                "bp_vampiric_edge",
                "bp_prism_core",
                "bp_eternal_embers"
            };

            foreach (var id in newBpIds)
            {
                var bp = _database.GetBlueprint(id);
                Assert.IsNotNull(bp, $"Blueprint '{id}' must exist in database.");
                Assert.IsTrue(bp.EmberCost > 0, $"Blueprint '{id}' must have positive Ember cost.");
                Assert.IsFalse(string.IsNullOrEmpty(bp.DisplayName), $"Blueprint '{id}' must have a valid DisplayName.");
                Assert.IsFalse(string.IsNullOrEmpty(bp.Description), $"Blueprint '{id}' must have a valid Description.");
            }
        }

        [Test]
        public void MetaProgression_CanPurchaseAndUnlockNewBlueprints()
        {
            _metaManager.AddEmbers(500);
            Assert.AreEqual(500, _metaManager.CurrentEmbers);

            var flask = _database.GetBlueprint("bp_alchemists_flask");
            Assert.IsNotNull(flask);

            bool success = _metaManager.UnlockBlueprint(flask);
            Assert.IsTrue(success, "Should successfully unlock Alchemist's Flask.");
            Assert.IsTrue(_metaManager.IsBlueprintUnlocked("bp_alchemists_flask"));
            Assert.AreEqual(500 - flask.EmberCost, _metaManager.CurrentEmbers);
        }

        [Test]
        public void MetaProgression_CannotPurchaseWithInsufficientEmbers()
        {
            _metaManager.AddEmbers(10);
            var vampire = _database.GetBlueprint("bp_vampiric_edge");
            Assert.IsNotNull(vampire);

            bool success = _metaManager.UnlockBlueprint(vampire);
            Assert.IsFalse(success, "Should reject purchase when player lacks sufficient embers.");
            Assert.IsFalse(_metaManager.IsBlueprintUnlocked("bp_vampiric_edge"));
        }
    }
}
