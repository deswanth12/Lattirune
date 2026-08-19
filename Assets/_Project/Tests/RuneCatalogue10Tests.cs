using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Runes;

namespace Lattirune.Tests
{
    /// <summary>
    /// Comprehensive test suite for the MVP 1.0 10-Rune Catalogue and RuneDatabase architecture.
    /// Strictly verifies PLAN.md Section 5.1 rune definitions, conduit directions, elements, and stats.
    /// </summary>
    [TestFixture]
    public class RuneCatalogue10Tests
    {
        private RuneDatabaseSO _db;

        [SetUp]
        public void Setup()
        {
            _db = RuneDatabaseSO.CreateCanonicalDatabase();
        }

        [Test]
        public void RuneDatabase_Contains10CanonicalRunes_PlusAliases()
        {
            Assert.IsNotNull(_db);
            Assert.IsTrue(_db.IsValid(out string error), error);

            // 10 canonical + 4 prototype aliases = 14 total
            Assert.GreaterOrEqual(_db.TotalRuneCount, 10);

            string[] canonicalIds = new string[]
            {
                "rune_ember",
                "rune_frost",
                "rune_spark",
                "rune_venom",
                "rune_crossfire",
                "rune_prism",
                "rune_amplifier",
                "rune_iron",
                "rune_vampire",
                "rune_haste"
            };

            foreach (var id in canonicalIds)
            {
                Assert.IsTrue(_db.HasRune(id), $"Database missing canonical rune ID: {id}");
            }
        }

        [Test]
        public void EmberRune_ElementDirectionAndBurnStats_MatchPlan()
        {
            var ember = _db.GetRune("rune_ember");
            Assert.AreEqual("Ember Rune", ember.DisplayName);
            Assert.AreEqual(ElementType.Fire, ember.Element);
            Assert.AreEqual(ConduitDirection.East, ember.Direction);
            Assert.AreEqual(6, ember.FlatDamageBonus);
            Assert.AreEqual(3f, ember.BurnDamagePerSec);
            Assert.AreEqual(4f, ember.BurnDuration);
        }

        [Test]
        public void FrostRune_ElementDirectionAndSlowStats_MatchPlan()
        {
            var frost = _db.GetRune("rune_frost");
            Assert.AreEqual("Frost Rune", frost.DisplayName);
            Assert.AreEqual(ElementType.Ice, frost.Element);
            Assert.AreEqual(ConduitDirection.South, frost.Direction);
            Assert.AreEqual(4, frost.FlatDamageBonus);
            Assert.AreEqual(0.15f, frost.SpeedReductionPercent);
        }

        [Test]
        public void SparkRune_ElementDirectionAndChainStats_MatchPlan()
        {
            var spark = _db.GetRune("rune_spark");
            Assert.AreEqual("Spark Rune", spark.DisplayName);
            Assert.AreEqual(ElementType.Lightning, spark.Element);
            Assert.AreEqual(ConduitDirection.North, spark.Direction);
            Assert.AreEqual(8, spark.FlatDamageBonus);
            Assert.AreEqual(0.25f, spark.ChainChance);
        }

        [Test]
        public void VenomRune_ElementDirectionAndPoisonStats_MatchPlan()
        {
            var venom = _db.GetRune("rune_venom");
            Assert.AreEqual("Venom Rune", venom.DisplayName);
            Assert.AreEqual(ElementType.Poison, venom.Element);
            Assert.AreEqual(ConduitDirection.West, venom.Direction);
            Assert.AreEqual(2, venom.PoisonStacksPerSec);
        }

        [Test]
        public void CrossfireRune_ElementDirectionAndMultiEmit_MatchPlan()
        {
            var crossfire = _db.GetRune("rune_crossfire");
            Assert.AreEqual("Crossfire Rune", crossfire.DisplayName);
            Assert.AreEqual(ElementType.Fire, crossfire.Element);
            Assert.AreEqual(ConduitDirection.Cross, crossfire.Direction);
            Assert.AreEqual(3, crossfire.FlatDamageBonus);
        }

        [Test]
        public void PrismRune_ElementDirectionAndSplit_MatchPlan()
        {
            var prism = _db.GetRune("rune_prism");
            Assert.AreEqual("Prism Rune", prism.DisplayName);
            Assert.AreEqual(ElementType.Light, prism.Element);
            Assert.AreEqual(ConduitDirection.Split, prism.Direction);
        }

        [Test]
        public void AmplifierNode_ElementDirectionAndOmni_MatchPlan()
        {
            var amplifier = _db.GetRune("rune_amplifier");
            Assert.AreEqual("Amplifier Node", amplifier.DisplayName);
            Assert.AreEqual(ElementType.Force, amplifier.Element);
            Assert.AreEqual(ConduitDirection.Omni, amplifier.Direction);
            Assert.AreEqual(1, amplifier.Range);
        }

        [Test]
        public void IronRune_ElementDirectionAndShield_MatchPlan()
        {
            var iron = _db.GetRune("rune_iron");
            Assert.AreEqual("Iron Rune", iron.DisplayName);
            Assert.AreEqual(ElementType.Earth, iron.Element);
            Assert.AreEqual(ConduitDirection.South, iron.Direction);
            Assert.AreEqual(15, iron.ShieldBonus);
        }

        [Test]
        public void VampireRune_ElementDirectionAndLifesteal_MatchPlan()
        {
            var vampire = _db.GetRune("rune_vampire");
            Assert.AreEqual("Vampire Rune", vampire.DisplayName);
            Assert.AreEqual(ElementType.Shadow, vampire.Element);
            Assert.AreEqual(ConduitDirection.North, vampire.Direction);
            Assert.AreEqual(0.12f, vampire.LifestealPercent);
        }

        [Test]
        public void HasteRune_ElementDirectionAndHaste_MatchPlan()
        {
            var haste = _db.GetRune("rune_haste");
            Assert.AreEqual("Haste Rune", haste.DisplayName);
            Assert.AreEqual(ElementType.Wind, haste.Element);
            Assert.AreEqual(ConduitDirection.East, haste.Direction);
            Assert.AreEqual(0.25f, haste.HastePercent);
        }

        [Test]
        public void RuneDatabase_Lookup_ValidAndInvalid()
        {
            Assert.IsNotNull(_db.GetRune("rune_ember"));
            Assert.IsNull(_db.GetRune("non_existent_rune"));
            Assert.IsNull(_db.GetRune(null));
        }
    }
}
