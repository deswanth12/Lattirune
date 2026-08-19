using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combo;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Modifiers;

namespace Lattirune.Tests
{
    [TestFixture]
    public class ComboGameplayIntegrationTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""ComboIntegrationTestHolder"");
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
        public void CombatSystem_IncrementsComboOnSuccessfulAttack()
        {
            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.SetExplicitStats(baseDamage: 10, runeBonus: 0, armorValue: 0);

            var enemyObj = new GameObject(""Enemy"");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(hp: 200, baseArmor: 0, attack: 0, interval: 10f);

            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize(step: 0.10f, maxMult: 2.0f);

            var combat = _holder.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, null, null, tracker);

            Assert.AreEqual(0, tracker.CurrentCombo);

            // Tick 1 -> Attack 1 (Base 10 * 1.0 = 10 DMG)
            combat.StartCombat();
            combat.Tick(1.5f);
            Assert.AreEqual(1, tracker.CurrentCombo);
            Assert.AreEqual(190, enemy.CurrentHp);

            // Tick 2 -> Attack 2 (Base 10 * (1.0 + 1 * 0.10 = 1.10) = 11 DMG)
            player.ResetCooldown();
            combat.Tick(1.5f);
            Assert.AreEqual(2, tracker.CurrentCombo);
            Assert.AreEqual(179, enemy.CurrentHp); // 190 - 11 = 179
        }

        [Test]
        public void RunManager_GrantsBonusGoldAndEmbersFromComboDepthOnVictory()
        {
            var tracker = _holder.AddComponent<ComboTracker>();
            tracker.Initialize();

            // Simulate high combo + reactions (e.g. 5 combo + 5 reactions = Legendary Cascade)
            for (int i = 0; i < 5; i++)
            {
                tracker.RecordReaction();
            }

            Assert.AreEqual(5, tracker.CurrentCombo);
            Assert.AreEqual(5, tracker.ConsecutiveReactions);

            var runManager = _holder.AddComponent<RunManager>();
            var combat = _holder.AddComponent<CombatSystem>();

            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var enemyObj = new GameObject(""Enemy"");
            enemyObj.transform.SetParent(_holder.transform);
            var enemy = enemyObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(10, 0, 1, 10f);

            combat.Initialize(player, enemy, null, null, tracker);
            runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                combat,
                null,
                player,
                enemy,
                null,
                null,
                null,
                tracker
            );
            runManager.StartRun();

            // Win battle
            runManager.StartEncounterCombat();
            enemy.TakeDirectDamage(100); // Defeats enemy
            combat.Tick(0.1f); // Resolves victory

            // Legendary Cascade grants at least 5 Embers and substantial bonus Gold
            Assert.GreaterOrEqual(runManager.CurrentEmbers, 5);
            Assert.GreaterOrEqual(runManager.CurrentGold, 30);
        }
    }
}
