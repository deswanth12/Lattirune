using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combat.Effects;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Modifiers;

namespace Lattirune.Tests
{
    [TestFixture]
    public class RunModifierGameplayIntegrationTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("ModifierIntegrationTestHolder");
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
        public void DamageMultiplier_IncreasesPlayerDamageInCombat()
        {
            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.SetExplicitStats(baseDamage: 20, runeBonus: 0, armorValue: 0);

            var enemyObj = new GameObject("Enemy");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(hp: 200, baseArmor: 0, attack: 0, interval: 10f);

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, null, modManager);

            // Baseline tick: 20 Base Damage -> 20 DMG dealt
            combat.StartCombat();
            combat.Tick(1.5f);
            Assert.AreEqual(180, enemy.CurrentHp);

            // Reset and Add Sharpened Runes (+15% Damage) + Glass Cannon (+50% Damage) = +65% (1.65x)
            combat.ResetCombat();
            enemy.ResetHpToFull();
            player.ResetCooldown();

            modManager.AddModifierById("mod_sharpened_runes");
            modManager.AddModifierById("mod_glass_cannon");

            combat.StartCombat();
            combat.Tick(1.5f);

            // 20 * 1.65 = 33 DMG -> Enemy HP = 200 - 33 = 167
            Assert.AreEqual(167, enemy.CurrentHp);
        }

        [Test]
        public void ElementalDamageBonus_ScalesActiveRuneBonus()
        {
            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.SetExplicitStats(baseDamage: 10, runeBonus: 10, armorValue: 0);

            var enemyObj = new GameObject("Enemy");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(hp: 200, baseArmor: 0, attack: 0, interval: 10f);

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();
            modManager.AddModifierById("mod_elemental_surge"); // +25% Elemental Damage

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, null, modManager);

            // Base 10 + (10 * 1.25 = 12.5 -> 13 or 12) -> ~22-23 DMG dealt
            combat.StartCombat();
            combat.Tick(1.5f);

            // 10 base + 13 rune = 23 DMG -> 200 - 23 = 177
            Assert.LessOrEqual(enemy.CurrentHp, 178);
        }

        [Test]
        public void CurseOfVulnerability_ReducesEffectivePlayerArmor()
        {
            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.SetExplicitStats(baseDamage: 0, runeBonus: 0, armorValue: 10); // 10 Armor

            var enemyObj = new GameObject("Enemy");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(hp: 100, baseArmor: 0, attack: 20, interval: 0.1f);

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();
            modManager.AddModifierById("mod_curse_vulnerability"); // -20% Defense -> Effective armor = 10 * 0.8 = 8

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, null, modManager);

            combat.StartCombat();
            combat.Tick(1.5f);

            // Enemy Attack: 20 - 8 armor = 12 DMG -> Player HP = 100 - 12 = 88
            Assert.AreEqual(88, player.CurrentHp);
        }

        [Test]
        public void GoldMultiplier_ScalesEncounterVictoryGold()
        {
            var runManager = _holder.AddComponent<RunManager>();
            var combat = _holder.AddComponent<CombatSystem>();
            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();
            modManager.AddModifierById("mod_midas_touch"); // +50% Gold Multiplier (1.5x)

            var playerObj = new GameObject("Player");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var enemyObj = new GameObject("Enemy");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(10, 0, 1, 10f);

            combat.Initialize(player, enemy, null, modManager);
            runManager.Initialize(DungeonDefinitionSO.Create10FloorCursedSewersDungeon(), combat, null, player, enemy, null, null, modManager);
            runManager.StartRun();

            // Clear encounter by defeat
            runManager.StartEncounterCombat();
            enemy.TakeDirectDamage(100); // Enemy perishes
            combat.Tick(0.1f); // Resolves victory

            // Base gold for non-elite is 10-15 -> 1.5x gives 15-22
            Assert.GreaterOrEqual(runManager.CurrentGold, 15);
        }
    }
}
