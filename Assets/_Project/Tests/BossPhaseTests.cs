using NUnit.Framework;
using UnityEngine;
using Lattirune.Boss;

namespace Lattirune.Tests
{
    [TestFixture]
    public class BossPhaseTests
    {
        private BossDefinitionSO _lichLord;

        [SetUp]
        public void Setup()
        {
            _lichLord = BossDefinitionSO.CreateLichLordDefinition();
        }

        [Test]
        public void BossPhase_ValidatesCorrectly()
        {
            BossPhaseDefinitionSO phase = ScriptableObject.CreateInstance<BossPhaseDefinitionSO>();
            phase.Initialize("p_test", "Test Phase", 0.5f, extraArmor: 2, extraAttack: 3, speedMult: 0.9f);

            Assert.IsTrue(phase.IsValid(out string err));
            Assert.IsNull(err);
            Assert.AreEqual("p_test", phase.PhaseId);
            Assert.AreEqual(0.5f, phase.HpThresholdPercentage);
        }

        [Test]
        public void BossDefinition_ValidatesCorrectly()
        {
            Assert.IsTrue(_lichLord.IsValid(out string err));
            Assert.IsNull(err);
            Assert.AreEqual("boss_lich_lord", _lichLord.BossId);
            Assert.AreEqual("The Lich Lord", _lichLord.DisplayName);
        }

        [Test]
        public void LichLord_BaseStats_MatchPlan()
        {
            Assert.AreEqual(750, _lichLord.MaxHp);
            Assert.AreEqual(10, _lichLord.BaseArmor);
            Assert.AreEqual(8, _lichLord.BaseAttack);
            Assert.AreEqual(2.5f, _lichLord.BaseAttackInterval);
        }

        [Test]
        public void LichLord_PhaseCount_IsThree()
        {
            Assert.AreEqual(3, _lichLord.PhaseCount);
            Assert.AreEqual("phase_1_frost_warden", _lichLord.GetPhase(0).PhaseId);
            Assert.AreEqual("phase_2_soul_harvest", _lichLord.GetPhase(1).PhaseId);
            Assert.AreEqual("phase_3_necrotic_inversion", _lichLord.GetPhase(2).PhaseId);
        }

        [Test]
        public void LichLord_PhaseThresholds_AreDescending()
        {
            Assert.AreEqual(1.0f, _lichLord.GetPhase(0).HpThresholdPercentage);
            Assert.AreEqual(0.66f, _lichLord.GetPhase(1).HpThresholdPercentage);
            Assert.AreEqual(0.33f, _lichLord.GetPhase(2).HpThresholdPercentage);
        }

        [Test]
        public void BossPhase_ScriptableObjects_RemainImmutable()
        {
            int originalBaseArmor = _lichLord.BaseArmor;
            BossPhaseDefinitionSO p2 = _lichLord.GetPhase(1);

            Assert.AreEqual(5, p2.BonusArmor);
            Assert.AreEqual(10, originalBaseArmor);
        }
    }
}
