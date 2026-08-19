using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Modifiers;
using Lattirune.Save;
using Lattirune.Tutorial;
using Lattirune.UI;

namespace Lattirune.Tests
{
    [TestFixture]
    public class CampfireRestAndTutorialTests
    {
        private GameObject _holder;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject(""CampfireTutorialTestHolder"");
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
        public void CampfireRest_Heal40Percent_RestoresPlayerHealth()
        {
            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);
            player.TakeDirectDamage(60); // 40 HP remaining

            var runManager = _holder.AddComponent<RunManager>();
            var campfireUI = _holder.AddComponent<CampfireRestUIController>();
            campfireUI.Initialize(runManager, player);

            bool healed = campfireUI.ChooseRestAndHeal();
            Assert.IsTrue(healed);
            Assert.AreEqual(80, player.CurrentHp); // 40 + 40 = 80

            // Second choice fails
            bool secondChoice = campfireUI.ChooseUpgradeRune();
            Assert.IsFalse(secondChoice);
        }

        [Test]
        public void CampfireRest_CleanseCurse_RemovesVulnerabilityModifier()
        {
            var playerObj = new GameObject(""Player"");
            playerObj.transform.SetParent(_holder.transform);
            var player = playerObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(100);

            var modManager = _holder.AddComponent<RunModifierManager>();
            modManager.Initialize();
            modManager.AddModifierById(""mod_curse_vulnerability"");
            Assert.IsTrue(modManager.HasModifier(""mod_curse_vulnerability""));

            var runManager = _holder.AddComponent<RunManager>();
            var campfireUI = _holder.AddComponent<CampfireRestUIController>();
            campfireUI.Initialize(runManager, player, modManager);

            bool cleansed = campfireUI.ChooseCleanseCurse();
            Assert.IsTrue(cleansed);
            Assert.IsFalse(modManager.HasModifier(""mod_curse_vulnerability""));
        }

        [Test]
        public void TutorialManager_StepProgressionAndSkip()
        {
            var tutorial = _holder.AddComponent<TutorialManager>();
            tutorial.Initialize(alreadyCompleted: false);

            Assert.AreEqual(TutorialStep.DragWeaponToGrid, tutorial.CurrentStep);
            Assert.IsFalse(tutorial.IsTutorialCompleted);

            tutorial.AdvanceStep(TutorialStep.DragWeaponToGrid);
            Assert.AreEqual(TutorialStep.ConnectRuneLaser, tutorial.CurrentStep);

            tutorial.AdvanceStep(TutorialStep.ConnectRuneLaser);
            Assert.AreEqual(TutorialStep.StartFirstBattle, tutorial.CurrentStep);

            tutorial.AdvanceStep(TutorialStep.StartFirstBattle);
            Assert.AreEqual(TutorialStep.Completed, tutorial.CurrentStep);
            Assert.IsTrue(tutorial.IsTutorialCompleted);
        }

        [Test]
        public void TutorialSavePersistence_PreservesCompletedFlag()
        {
            var tutorial = _holder.AddComponent<TutorialManager>();
            tutorial.Initialize();
            tutorial.CompleteTutorial();

            SaveData save = SaveData.CreateDefault();
            save.settings.hasCompletedTutorial = tutorial.IsTutorialCompleted;

            string json = SaveSerializer.SerializeToJson(save);
            Assert.IsNotNull(json);

            SaveData loaded = SaveSerializer.DeserializeFromJson(json);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.version);
            Assert.IsTrue(loaded.settings.hasCompletedTutorial);
        }
    }
}
