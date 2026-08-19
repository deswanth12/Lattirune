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
    }
}
