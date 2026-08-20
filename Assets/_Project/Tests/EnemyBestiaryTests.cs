using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Dungeon;
using Lattirune.Boss;

namespace Lattirune.Tests
{
    /// <summary>
    /// Unit test suite for the complete 6-Enemy Bestiary + Lich Lord Boss architecture.
    /// Strictly verifies PLAN.md Section 10 enemy balance sheets and trait callbacks.
    /// </summary>
    [TestFixture]
    public class EnemyBestiaryTests
    {
        private GameObject _holderObj;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("EnemyBestiaryTestHolder");
        }

        [TearDown]
        public void Teardown()
        {
            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        [Test]
        public void SewerRat_Stats_35Hp_1_2sInterval()
        {
            EncounterDefinitionSO rat = EncounterDefinitionSO.CreateSewerRat();
            Assert.AreEqual("Sewer Rat", rat.EnemyName);
            Assert.AreEqual(35, rat.EnemyHp);
            Assert.AreEqual(0, rat.EnemyArmor);
            Assert.AreEqual(1.2f, rat.AttackInterval);
            Assert.IsFalse(rat.IsBoss);
        }

        [Test]
        public void GoblinThief_GoldStealTrait_ExecutesOnAttack()
        {
            EncounterDefinitionSO goblin = EncounterDefinitionSO.CreateGoblinThief();
            Assert.AreEqual("Goblin Thief", goblin.EnemyName);
            Assert.AreEqual(45, goblin.EnemyHp);
            Assert.AreEqual(1.0f, goblin.AttackInterval);
            Assert.AreEqual(1, goblin.EnemyTraits.Count);
            Assert.AreEqual(EnemyTraitType.GoldSteal, goblin.EnemyTraits[0].TraitType);

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupCustom(goblin.EnemyName, goblin.EnemyHp, goblin.EnemyArmor, goblin.EnemyAttack, goblin.AttackInterval, goblin.EnemyTraits);

            int goldStolen = 0;
            enemy.OnGoldStolen += (amount) => goldStolen += amount;

            DamageResult dummyDamage = new DamageResult("Goblin", "Hero", 4, 0, 1f, 1f, 0, 4, false);
            enemy.TriggerAttackTraits(enemy, dummyDamage);

            Assert.AreEqual(3, goldStolen); // Steals 3 gold per hit
        }

        [Test]
        public void ArmoredSkeleton_ReflectTrait_ExecutesOnDamageTaken()
        {
            EncounterDefinitionSO skeleton = EncounterDefinitionSO.CreateArmoredSkeleton();
            Assert.AreEqual("Armored Skeleton", skeleton.EnemyName);
            Assert.AreEqual(75, skeleton.EnemyHp);
            Assert.AreEqual(15, skeleton.EnemyArmor);
            Assert.AreEqual(2.0f, skeleton.AttackInterval);

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupCustom(skeleton.EnemyName, skeleton.EnemyHp, skeleton.EnemyArmor, skeleton.EnemyAttack, skeleton.AttackInterval, skeleton.EnemyTraits);

            int reflectedDmg = 0;
            enemy.OnDamageReflected += (amount) => reflectedDmg += amount;

            // Hero deals 100 incoming damage -> 20% reflected = 20 damage
            DamageResult heroHit = new DamageResult("Hero", "Armored Skeleton", 100, 0, 1f, 1f, 15, 100, false, isReflected: false);
            int calculatedReflect = enemy.CalculateDamageReflect(heroHit);

            Assert.AreEqual(20, calculatedReflect);
            Assert.AreEqual(20, reflectedDmg);
        }

        [Test]
        public void ArmoredSkeleton_Reflect_PreventsInfiniteRecursion()
        {
            EncounterDefinitionSO skeleton = EncounterDefinitionSO.CreateArmoredSkeleton();
            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupCustom(skeleton.EnemyName, skeleton.EnemyHp, skeleton.EnemyArmor, skeleton.EnemyAttack, skeleton.AttackInterval, skeleton.EnemyTraits);

            // Reflected damage hitting the enemy should NOT reflect back again
            DamageResult alreadyReflected = new DamageResult("Hero", "Armored Skeleton", 50, 0, 1f, 1f, 15, 50, false, isReflected: true);
            int secondaryReflect = enemy.CalculateDamageReflect(alreadyReflected);

            Assert.AreEqual(0, secondaryReflect);
        }

        [Test]
        public void VenomousSpider_PoisonTrait_ExecutesOnAttack()
        {
            EncounterDefinitionSO spider = EncounterDefinitionSO.CreateVenomousSpider();
            Assert.AreEqual("Venomous Spider", spider.EnemyName);
            Assert.AreEqual(50, spider.EnemyHp);
            Assert.AreEqual(1.4f, spider.AttackInterval);

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupCustom(spider.EnemyName, spider.EnemyHp, spider.EnemyArmor, spider.EnemyAttack, spider.AttackInterval, spider.EnemyTraits);

            int poisonStacksApplied = 0;
            enemy.OnPoisonInflicted += (stacks) => poisonStacksApplied += stacks;

            DamageResult hit = new DamageResult("Spider", "Hero", 4, 0, 1f, 1f, 0, 4, false);
            enemy.TriggerAttackTraits(enemy, hit);

            Assert.AreEqual(2, poisonStacksApplied); // Inflicts 2 poison stacks
        }

        [Test]
        public void AcidSlime_BagDisableTrait_ExecutesOnEncounterStart()
        {
            EncounterDefinitionSO slime = EncounterDefinitionSO.CreateAcidSlime();
            Assert.AreEqual("Acid Slime", slime.EnemyName);
            Assert.AreEqual(160, slime.EnemyHp);
            Assert.AreEqual(2.0f, slime.AttackInterval);

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupCustom(slime.EnemyName, slime.EnemyHp, slime.EnemyArmor, slime.EnemyAttack, slime.AttackInterval, slime.EnemyTraits);

            bool bagDisabled = false;
            enemy.OnBagSlotDisabled += () => bagDisabled = true;

            enemy.TriggerEncounterStartTraits();

            Assert.IsTrue(bagDisabled);
        }

        [Test]
        public void Necromancer_SummonTrait_MatchesConfig()
        {
            EncounterDefinitionSO necro = EncounterDefinitionSO.CreateNecromancer();
            Assert.AreEqual("Necromancer", necro.EnemyName);
            Assert.AreEqual(140, necro.EnemyHp);
            Assert.AreEqual(3.0f, necro.AttackInterval);
            Assert.AreEqual(1, necro.EnemyTraits.Count);
            Assert.AreEqual(EnemyTraitType.SummonMinions, necro.EnemyTraits[0].TraitType);
            Assert.AreEqual(4.0f, necro.EnemyTraits[0].TriggerInterval);
            Assert.AreEqual(2.0f, necro.EnemyTraits[0].TraitValue); // Summons 2 Skeletons
        }

        [Test]
        public void ShadowStalker_ArchetypeStats_MatchDefinition()
        {
            EncounterDefinitionSO stalker = EncounterDefinitionSO.CreateShadowStalker();
            Assert.AreEqual("Shadow Stalker", stalker.EnemyName);
            Assert.AreEqual(85, stalker.EnemyHp);
            Assert.AreEqual(4, stalker.EnemyArmor);
            Assert.AreEqual(7, stalker.EnemyAttack);
            Assert.AreEqual(1.1f, stalker.AttackInterval);
            Assert.AreEqual(1, stalker.EnemyTraits.Count);
            Assert.AreEqual(EnemyTraitType.GoldSteal, stalker.EnemyTraits[0].TraitType);
        }

        [Test]
        public void CrystalGolem_ArchetypeStats_MatchDefinition()
        {
            EncounterDefinitionSO golem = EncounterDefinitionSO.CreateCrystalGolem();
            Assert.AreEqual("Crystal Golem", golem.EnemyName);
            Assert.AreEqual(175, golem.EnemyHp);
            Assert.AreEqual(16, golem.EnemyArmor);
            Assert.AreEqual(8, golem.EnemyAttack);
            Assert.AreEqual(2.2f, golem.AttackInterval);
            Assert.AreEqual(1, golem.EnemyTraits.Count);
            Assert.AreEqual(EnemyTraitType.DamageReflect, golem.EnemyTraits[0].TraitType);
        }

        [Test]
        public void LichLord_BossStats_AndThreePhases_MatchPlan()
        {
            BossDefinitionSO boss = BossDefinitionSO.CreateLichLordDefinition();
            Assert.AreEqual("The Lich Lord", boss.BossName);
            Assert.AreEqual(750, boss.MaxHp);
            Assert.AreEqual(10, boss.BaseArmor);
            Assert.AreEqual(8, boss.BaseAttack);
            Assert.AreEqual(2.5f, boss.BaseAttackInterval);
            Assert.AreEqual(3, boss.PhaseCount);
        }
    }
}
