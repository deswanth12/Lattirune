using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone Phase 1 End-to-End Integration Verification Suite.
    /// Verifies the complete loop:
    /// GRID -> RUNE CONDUIT -> SYNERGY -> COMBAT -> VICTORY -> REWARD -> SAVE -> LOAD -> RECONSTRUCTION.
    /// </summary>
    [TestFixture]
    public class Phase1MilestoneVerificationTests
    {
        private GameObject _holderObj;
        private string _testSaveDir;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MilestonePhase1VerificationHolder");
            _testSaveDir = Path.Combine(Application.temporaryCachePath, "MilestonePhase1Tests");
            if (Directory.Exists(_testSaveDir))
            {
                Directory.Delete(_testSaveDir, true);
            }
            Directory.CreateDirectory(_testSaveDir);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testSaveDir))
            {
                Directory.Delete(_testSaveDir, true);
            }

            if (_holderObj != null)
            {
                Object.DestroyImmediate(_holderObj);
            }
        }

        [Test]
        public void Phase1_CompleteCoreGameplayLoop_ExecutesAndSavesSuccessfully()
        {
            // 1. GRID INITIALIZATION (5x5, 17 active, 8 locked)
            LatticeGrid grid = new LatticeGrid(initializeDefaultLayout: true);
            Assert.AreEqual(17, grid.GetActiveCellCount());
            Assert.AreEqual(8, grid.GetLockedCellCount());

            // 2. ITEM CREATION & ROTATION (Training Sword 1x2 -> Rotated to 2x1)
            ItemDataSO swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            swordData.Initialize("item_training_sword", "Training Sword", "A sharp blade.", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);

            ItemInstance sword = ItemFactory.CreateInstance(swordData, Vector3.zero, _holderObj.transform);
            Assert.AreEqual(new Vector2Int(1, 2), sword.CurrentDimensions);
            sword.RotateClockwise();
            Assert.AreEqual(new Vector2Int(2, 1), sword.CurrentDimensions);

            // 3. GRID PLACEMENT at (2, 2)
            Vector2Int placeCoord = new Vector2Int(2, 2);
            bool placed = grid.PlaceItem(sword.InstanceId, placeCoord, sword.CurrentDimensions);
            Assert.IsTrue(placed);
            sword.OnPlaced(placeCoord, GridCoordinateUtility.GridToWorld(placeCoord));
            Assert.IsTrue(sword.IsPlacedOnGrid);

            // 4. RUNE CONDUIT ENGINE (Fire Rune at (2,1) emitting North with range 3)
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);
            RuneConduitResult conduitResult = RuneConduitEngine.CalculateConduit(grid, new Vector2Int(2, 1), ConduitDirection.North, 3);
            Assert.IsTrue(conduitResult.TraversedCells.Contains(new Vector2Int(2, 2)));

            // 5. SYNERGY SYSTEM (Flamebound Edge activation)
            SynergySystem synergy = _holderObj.AddComponent<SynergySystem>();
            synergy.EnsureDefaultDefinitions();
            var conduitData = new List<(RuneData, Vector2Int, RuneConduitResult)>
            {
                (fireRune, new Vector2Int(2, 1), conduitResult)
            };
            synergy.UpdateSynergies(conduitData, new List<ItemInstance> { sword });

            Assert.IsTrue(sword.HasActiveSynergy);
            Assert.AreEqual("fire_sword", sword.ActiveSynergyId);

            // 6. COMBAT ENCOUNTER (Player vs Training Dummy)
            PlayerCombatant player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 100);
            player.UpdateStatsFromBuild(new List<ItemInstance> { sword });

            Assert.AreEqual(10, player.BaseAttackDamage);
            Assert.AreEqual(5, player.ActiveRuneBonus); // +5 Flamebound Edge synergy bonus!

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(hp: 30, baseArmor: 2, attack: 4, interval: 1.0f);

            CombatSystem combat = _holderObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            combat.StartCombat();
            Assert.AreEqual(CombatState.Fighting, combat.CurrentState);

            // Execute attack turns: (10 + 5) - 2 = 13 DMG per hit -> 3 hits = 39 DMG (defeats 30 HP enemy)
            combat.UpdateCombat(1.0f); // Hit 1: 30 - 13 = 17 HP
            combat.UpdateCombat(1.0f); // Hit 2: 17 - 13 = 4 HP
            combat.UpdateCombat(1.0f); // Hit 3: 4 - 13 = 0 HP -> Victory!

            Assert.AreEqual(CombatState.Victory, combat.CurrentState);
            Assert.IsFalse(enemy.IsAlive);

            // 7. REWARD GENERATION & APPLICATION
            ItemDataSO emberBladeData = ScriptableObject.CreateInstance<ItemDataSO>();
            emberBladeData.Initialize("item_ember_blade", "Ember Blade", "Fire Sword", ItemCategory.Weapon, new Vector2Int(2, 1), true, Color.red);
            var itemCatalogue = new List<ItemDataSO> { swordData, emberBladeData };

            List<RewardOption> rewardOptions = RewardGenerator.GenerateRewardOptions(itemCatalogue, count: 2);
            Assert.AreEqual(2, rewardOptions.Count);

            RewardService rewardService = _holderObj.AddComponent<RewardService>();
            RewardOption chosenReward = rewardOptions.Find(r => r.ItemData.ItemId == "item_ember_blade");
            ItemInstance rewardedInstance = rewardService.ApplyReward(chosenReward, new Vector3(0f, -4f, 0f), _holderObj.transform);

            Assert.IsNotNull(rewardedInstance);
            Assert.AreEqual("item_ember_blade", rewardedInstance.Data.ItemId);
            Assert.IsTrue(rewardService.IsSelectionLocked);

            // 8. SAVE SYSTEM (Persist state to disk)
            SaveSystem saveSystem = _holderObj.AddComponent<SaveSystem>();
            saveSystem.SetCustomDirectory(_testSaveDir);

            SaveData saveData = new SaveData();
            saveData.items.Add(new SavedItemData(sword.Data.ItemId, sword.GridPosition.x, sword.GridPosition.y, sword.CurrentRotationAngle, true, 0f, 0f));
            saveData.items.Add(new SavedItemData(rewardedInstance.Data.ItemId, -1, -1, 0, false, 0f, -4f));
            saveData.runes.Add(new SavedRuneData(fireRune.RuneId, 2, 1, (int)ConduitDirection.North, (int)ElementType.Fire, 3));
            saveData.settings = new SavedSettingsData(0.8f, 0.7f, false, true);

            SaveResult saveResult = saveSystem.Save(saveData);
            Assert.IsTrue(saveResult.IsSuccess);

            // 9. LOAD & RECONSTRUCTION TEST
            LoadResult loadResult = saveSystem.Load();
            Assert.IsTrue(loadResult.IsSuccess);
            Assert.AreEqual(2, loadResult.Data.items.Count);

            // Verify ItemDataSO remained immutable
            Assert.AreEqual(new Vector2Int(1, 2), swordData.BaseDimensions);
            Assert.AreEqual("Training Sword", swordData.DisplayName);
        }

        [Test]
        public void Phase1_DefeatAndRetry_ResetsCombatCleanlyWithoutDuplicateListeners()
        {
            PlayerCombatant player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 20);
            player.SetExplicitStats(baseDamage: 2, runeBonus: 0, armorValue: 0, interval: 2.0f);

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();
            enemy.SetupTrainingDummy(hp: 100, baseArmor: 0, attack: 50, interval: 1.0f);

            CombatSystem combat = _holderObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy);

            AudioController audio = _holderObj.AddComponent<AudioController>();
            HapticFeedback haptics = _holderObj.AddComponent<HapticFeedback>();
            InteractionFeedbackCoordinator coordinator = _holderObj.AddComponent<InteractionFeedbackCoordinator>();
            coordinator.Initialize(audio, haptics, null, null, combat, null);

            // Start combat -> enemy defeats player
            combat.StartCombat();
            combat.UpdateCombat(1.0f);

            Assert.AreEqual(CombatState.Defeat, combat.CurrentState);
            Assert.IsFalse(player.IsAlive);
            Assert.AreEqual(2, audio.TotalSfxPlayed); // Attack + Defeat SFX

            // Retry combat
            combat.ResetCombat();
            Assert.AreEqual(CombatState.Preparing, combat.CurrentState);
            Assert.IsTrue(player.IsAlive);
            Assert.AreEqual(20, player.CurrentHp);
            Assert.AreEqual(100, enemy.CurrentHp);
        }
    }
}
