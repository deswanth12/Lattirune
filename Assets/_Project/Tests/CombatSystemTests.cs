using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Runes;
using Lattirune.Synergy;

namespace Lattirune.Tests
{
    [TestFixture]
    public class CombatSystemTests
    {
        private GameObject _holderObj;
        private PlayerCombatant _player;
        private EnemyCombatant _enemy;
        private CombatSystem _combatSystem;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("CombatTestHolder");

            _player = _holderObj.AddComponent<PlayerCombatant>();
            _player.SetupDefaultPlayer(initialHp: 100);

            _enemy = _holderObj.AddComponent<EnemyCombatant>();
            _enemy.SetupTrainingDummy(hp: 50, baseArmor: 2, attack: 4, interval: 1.5f);

            _combatSystem = _holderObj.AddComponent<CombatSystem>();
            _combatSystem.Initialize(_player, _enemy);
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
        public void Combatant_InitializesAndClampsHpCorrectly()
        {
            Assert.AreEqual(100, _player.MaxHp);
            Assert.AreEqual(100, _player.CurrentHp);
            Assert.IsTrue(_player.IsAlive);

            // Apply fatal damage
            DamageResult fatalHit = new DamageResult("Enemy", "Hero", 150, 0, 1f, 1f, 0, 150, false);
            _player.TakeDamage(fatalHit);

            Assert.AreEqual(0, _player.CurrentHp, "HP must clamp to 0 and not become negative.");
            Assert.IsFalse(_player.IsAlive);
        }

        [Test]
        public void DeadCombatant_CannotAttackOrTickCooldown()
        {
            DamageResult fatalHit = new DamageResult("Enemy", "Hero", 100, 0, 1f, 1f, 0, 100, false);
            _player.TakeDamage(fatalHit);

            bool canTick = _player.TickCooldown(5.0f);
            Assert.IsFalse(canTick);
        }

        [Test]
        public void Combat_PlayerAttack_ReducesEnemyHp()
        {
            _player.SetExplicitStats(baseDamage: 10, runeBonus: 0, armorValue: 0, interval: 1.0f);
            _enemy.SetupTrainingDummy(hp: 50, baseArmor: 2, attack: 4, interval: 1.0f);

            _combatSystem.StartCombat();
            Assert.AreEqual(CombatState.Fighting, _combatSystem.CurrentState);

            // Advance time by 1.0 second to trigger player attack (10 base - 2 armor = 8 DMG)
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(42, _enemy.CurrentHp);
        }

        [Test]
        public void Combat_EnemyAttack_ReducesPlayerHp()
        {
            _player.SetExplicitStats(baseDamage: 10, runeBonus: 0, armorValue: 1, interval: 2.0f);
            _enemy.SetupTrainingDummy(hp: 50, baseArmor: 0, attack: 6, interval: 1.0f);

            _combatSystem.StartCombat();

            // Advance time by 1.0 second: Enemy attacks (6 attack - 1 armor = 5 DMG)
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(95, _player.CurrentHp);
        }

        [Test]
        public void Combat_Victory_TriggersWhenEnemyReachesZeroHp()
        {
            _player.SetExplicitStats(baseDamage: 60, runeBonus: 0, armorValue: 0, interval: 1.0f);
            _enemy.SetupTrainingDummy(hp: 50, baseArmor: 0, attack: 4, interval: 1.0f);

            bool victoryFired = false;
            _combatSystem.OnVictory += () => victoryFired = true;

            _combatSystem.StartCombat();
            _combatSystem.UpdateCombat(1.0f); // Player deals 60 DMG to 50 HP enemy

            Assert.AreEqual(0, _enemy.CurrentHp);
            Assert.IsFalse(_enemy.IsAlive);
            Assert.AreEqual(CombatState.Victory, _combatSystem.CurrentState);
            Assert.IsTrue(victoryFired);

            // Further updates must not execute any actions
            _combatSystem.UpdateCombat(10.0f);
            Assert.AreEqual(CombatState.Victory, _combatSystem.CurrentState);
        }

        [Test]
        public void Combat_Defeat_TriggersWhenPlayerReachesZeroHp()
        {
            _player.SetExplicitStats(baseDamage: 2, runeBonus: 0, armorValue: 0, interval: 2.0f);
            _enemy.SetupTrainingDummy(hp: 50, baseArmor: 0, attack: 150, interval: 1.0f);

            bool defeatFired = false;
            _combatSystem.OnDefeat += () => defeatFired = true;

            _combatSystem.StartCombat();
            _combatSystem.UpdateCombat(1.0f); // Enemy deals 150 DMG to 100 HP player

            Assert.AreEqual(0, _player.CurrentHp);
            Assert.IsFalse(_player.IsAlive);
            Assert.AreEqual(CombatState.Defeat, _combatSystem.CurrentState);
            Assert.IsTrue(defeatFired);
        }

        [Test]
        public void Combat_FireSynergy_ContributesBonusDamageToCombat()
        {
            ItemDataSO swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            swordData.Initialize("sword", "Sword", "Sword", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.white);

            ItemInstance sword = ItemFactory.CreateInstance(swordData, Vector3.zero);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);
            sword.SetSynergyState("fire_sword"); // Flamebound Edge active (+5 DMG)

            _player.UpdateStatsFromBuild(new List<ItemInstance> { sword });

            Assert.AreEqual(10, _player.BaseAttackDamage);
            Assert.AreEqual(5, _player.ActiveRuneBonus);
            Assert.IsTrue(_player.HasActiveSynergy);

            // Enemy has 2 armor -> Damage = (10 + 5) - 2 = 13
            DamageResult result = DamageCalculator.CalculateDamage(
                _player.CombatantName, 
                _enemy.CombatantName, 
                _player.BaseAttackDamage, 
                _player.ActiveRuneBonus, 
                _enemy.Armor
            );

            Assert.AreEqual(13, result.FinalDamage);
        }

        [Test]
        public void Combat_BreakingSynergy_RemovesBonusFromCombat()
        {
            ItemDataSO swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            swordData.Initialize("sword", "Sword", "Sword", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.white);

            ItemInstance sword = ItemFactory.CreateInstance(swordData, Vector3.zero);
            sword.OnPlaced(new Vector2Int(2, 2), Vector3.zero);
            sword.SetSynergyState("fire_sword");
            _player.UpdateStatsFromBuild(new List<ItemInstance> { sword });
            Assert.AreEqual(5, _player.ActiveRuneBonus);

            // Break synergy
            sword.SetSynergyState(null);
            _player.UpdateStatsFromBuild(new List<ItemInstance> { sword });

            Assert.AreEqual(0, _player.ActiveRuneBonus);
            Assert.IsFalse(_player.HasActiveSynergy);

            DamageResult result = DamageCalculator.CalculateDamage(
                _player.CombatantName, 
                _enemy.CombatantName, 
                _player.BaseAttackDamage, 
                _player.ActiveRuneBonus, 
                _enemy.Armor
            );

            // 10 base - 2 armor = 8
            Assert.AreEqual(8, result.FinalDamage);
        }

        [Test]
        public void Combat_EnemyDamageReflect_DamagesPlayerOnHit()
        {
            var reflectTrait = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            reflectTrait.Initialize("trait_reflect", "Damage Reflect", EnemyTraitType.DamageReflect, 0.25f);

            _enemy.SetupCustom("Armored Skeleton", 100, 0, 0, 99.0f, new[] { reflectTrait });
            _player.SetExplicitStats(baseDamage: 20, runeBonus: 0, armorValue: 0, interval: 1.0f);

            _combatSystem.StartCombat();
            int playerInitialHp = _player.CurrentHp;

            // Player attacks for 20 DMG. 25% of 20 = 5 damage reflected back to player.
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(80, _enemy.CurrentHp);
            Assert.AreEqual(playerInitialHp - 5, _player.CurrentHp, "Player should take 5 reflected thorn damage.");
        }

        [Test]
        public void Combat_EnemyPoisonOnHit_AppliesPoisonDoT()
        {
            var effectSystem = _holderObj.AddComponent<Lattirune.Combat.Effects.CombatEffectSystem>();
            effectSystem.EnsureDefaultDatabase();

            var poisonTrait = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            poisonTrait.Initialize("trait_poison", "Poison On Hit", EnemyTraitType.ApplyPoisonOnHit, 2f);

            _enemy.SetupCustom("Venomous Spider", 100, 0, 5, 1.0f, new[] { poisonTrait });
            _player.SetExplicitStats(baseDamage: 0, runeBonus: 0, armorValue: 0, interval: 99.0f);

            _combatSystem.Initialize(_player, _enemy, effectSystem);
            _combatSystem.StartCombat();

            // Enemy strikes player at 1.0s (5 physical damage + Poison DoT applied)
            _combatSystem.UpdateCombat(1.0f);
            Assert.AreEqual(95, _player.CurrentHp);
            Assert.AreEqual(1, effectSystem.ActiveEffectCount, "Poison DoT effect must be active on player.");

            // Advance 1.0s: both the DoT tick (4 damage) and the enemy's 2nd attack (5 damage) fire.
            _combatSystem.UpdateCombat(1.0f);
            Assert.AreEqual(86, _player.CurrentHp, "Player should take DoT tick + 2nd enemy strike damage.");
        }

        [Test]
        public void Combat_EnemyStartTraits_TriggeredOnCombatStart()
        {
            bool bagSlotDisabled = false;
            var acidTrait = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            acidTrait.Initialize("trait_acid", "Acid Spit", EnemyTraitType.DisableBagSlot, 1f);

            _enemy.SetupCustom("Acid Slime", 100, 0, 5, 2.0f, new[] { acidTrait });
            _enemy.OnBagSlotDisabled += () => bagSlotDisabled = true;

            _combatSystem.Initialize(_player, _enemy);
            _combatSystem.StartCombat();

            Assert.IsTrue(bagSlotDisabled, "Acid Spit trait must trigger on encounter start.");
        }

        [Test]
        public void Combat_EliteAffix_Juggernaut_IncreasesHpAndArmor()
        {
            _enemy.SetupCustom("Acid Slime", 100, 2, 6, 2.0f);
            _enemy.ApplyEliteAffix(EliteAffixType.Juggernaut);

            Assert.IsTrue(_enemy.IsElite);
            Assert.AreEqual(EliteAffixType.Juggernaut, _enemy.EliteAffix);
            Assert.AreEqual(140, _enemy.MaxHp);
            Assert.AreEqual(140, _enemy.CurrentHp);
            Assert.AreEqual(10, _enemy.Armor);
            Assert.IsTrue(_enemy.CombatantName.Contains("Juggernaut"));
        }

        [Test]
        public void Combat_EliteAffix_Vampiric_LeechesHealthOnHit()
        {
            _enemy.SetupCustom("Goblin Thief", 100, 0, 20, 1.0f);
            _enemy.TakeDirectDamage(40); // 60 HP remaining
            _enemy.ApplyEliteAffix(EliteAffixType.Vampiric);

            _player.SetExplicitStats(baseDamage: 0, runeBonus: 0, armorValue: 0, interval: 99f);

            _combatSystem.Initialize(_player, _enemy);
            _combatSystem.StartCombat();

            // Enemy deals 20 damage to player -> leeches 25% of 20 = 5 HP
            _combatSystem.UpdateCombat(1.0f);

            Assert.AreEqual(80, _player.CurrentHp);
            Assert.AreEqual(65, _enemy.CurrentHp, "Vampiric enemy should heal 5 HP on hit.");
        }

        [Test]
        public void Combat_EliteAffix_Frenzied_IncreasesAttackSpeed()
        {
            _enemy.SetupCustom("Sewer Rat", 50, 0, 4, 1.0f);
            _enemy.ApplyEliteAffix(EliteAffixType.Frenzied);

            Assert.AreEqual(0.65f, _enemy.AttackInterval, 0.01f);
            Assert.IsTrue(_enemy.CombatantName.Contains("Frenzied"));
        }
    }
}
