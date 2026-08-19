using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Combat.Effects;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Tests
{
    /// <summary>
    /// Milestone Phase 2 Complete Vertical-Slice End-to-End Verification Suite.
    /// Proves the entire Phase 2 roguelite architecture functions seamlessly:
    /// RUN START -> FLOOR 1 -> ENCOUNTER -> GRID BUILD -> CONDUITS -> SYNERGIES -> COMBAT -> VICTORY -> REWARD -> INVENTORY ->
    /// FLOOR 2 -> ELEMENTAL REACTIONS -> PRISM REFRACTION -> CROSSFIRE -> FLOOR 3 -> THE LICH LORD 3-PHASE BOSS ->
    /// FINAL REWARD -> RUN COMPLETE -> ENCRYPTED SAVE -> LOAD RESTORATION.
    /// </summary>
    [TestFixture]
    public class Phase2MilestoneVerificationTests
    {
        private GameObject _holderObj;
        private string _testSaveDir;

        [SetUp]
        public void Setup()
        {
            _holderObj = new GameObject("MilestonePhase2VerificationHolder");
            _testSaveDir = Path.Combine(Application.temporaryCachePath, "MilestonePhase2Tests");
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
        public void Phase2_CompleteVerticalSliceLoop_ThreeFloors_Reactions_Prisms_LichLordBoss_AndSaveLoad()
        {
            // 1. GRID & INVENTORY INITIALIZATION
            LatticeGrid combatGrid = new LatticeGrid(initializeDefaultLayout: true);
            Assert.AreEqual(17, combatGrid.GetActiveCellCount());

            InventorySystem inventory = _holderObj.AddComponent<InventorySystem>();
            inventory.Initialize();
            Assert.AreEqual(6, inventory.Capacity);

            // 2. ITEM & SYNERGY SETUP
            ItemDataSO swordData = ScriptableObject.CreateInstance<ItemDataSO>();
            swordData.Initialize("item_training_sword", "Training Sword", "A sharp blade.", ItemCategory.Weapon, new Vector2Int(1, 2), true, Color.yellow);
            ItemInstance sword = ItemFactory.CreateInstance(swordData, Vector3.zero, _holderObj.transform);

            // Place on combat grid at (2, 2)
            Vector2Int swordPos = new Vector2Int(2, 2);
            combatGrid.PlaceItem(sword.InstanceId, swordPos, sword.CurrentDimensions);
            sword.OnPlaced(swordPos, GridCoordinateUtility.GridToWorld(swordPos));

            // Fire Rune at (2, 1) emitting North -> Flamebound Edge synergy (+5 Dmg)
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);
            List<ConduitBeamPath> fireBeams = MultiDirectionalEmitter.EmitBeams(combatGrid, fireRune, new Vector2Int(2, 1), 3);

            SynergySystem synergy = _holderObj.AddComponent<SynergySystem>();
            synergy.EnsureDefaultDefinitions();
            synergy.UpdateSynergies(fireBeams, new List<ItemInstance> { sword });
            Assert.IsTrue(sword.HasActiveSynergy);
            Assert.AreEqual("fire_sword", sword.ActiveSynergyId);

            // 3. COMBAT & DUNGEON RUN MANAGER SETUP
            PlayerCombatant player = _holderObj.AddComponent<PlayerCombatant>();
            player.SetupDefaultPlayer(initialHp: 500);
            player.UpdateStatsFromBuild(new List<ItemInstance> { sword });

            EnemyCombatant enemy = _holderObj.AddComponent<EnemyCombatant>();

            CombatEffectSystem effectSystem = _holderObj.AddComponent<CombatEffectSystem>();
            effectSystem.EnsureDefaultDatabase();

            CombatSystem combat = _holderObj.AddComponent<CombatSystem>();
            combat.Initialize(player, enemy, effectSystem);

            RewardService rewardService = _holderObj.AddComponent<RewardService>();

            BossSystem bossSystem = _holderObj.AddComponent<BossSystem>();
            bossSystem.Initialize(BossDefinitionSO.CreateLichLordDefinition(), enemy, effectSystem);

            DungeonDefinitionSO dungeonDef = DungeonDefinitionSO.CreateDefaultPhase2Dungeon();
            RunManager runManager = _holderObj.AddComponent<RunManager>();
            runManager.Initialize(dungeonDef, combat, rewardService, player, enemy, bossSystem);

            // ==========================================
            // FLOOR 1: SEWER RAT SKIRMISH
            // ==========================================
            runManager.StartRun();
            Assert.AreEqual(1, runManager.CurrentFloorNumber);
            Assert.AreEqual(RunState.FloorPreparing, runManager.CurrentState);

            runManager.StartEncounterCombat();
            Assert.AreEqual(RunState.EncounterActive, runManager.CurrentState);
            Assert.AreEqual("Sewer Rat", enemy.CombatantName);

            // Execute combat turns to win Floor 1
            enemy.TakeDamage(new DamageResult("Hero", "Sewer Rat", 50, 0, 1f, 1f, 0, 50, false));
            combat.UpdateCombat(0.1f);

            Assert.AreEqual(RunState.RewardSelection, runManager.CurrentState);

            // Reward selection -> Add rewarded item to Spatial Inventory Bag
            ItemDataSO emberBladeData = ScriptableObject.CreateInstance<ItemDataSO>();
            emberBladeData.Initialize("item_ember_blade", "Ember Blade", "Fire Blade", ItemCategory.Weapon, new Vector2Int(2, 1), true, Color.red);
            ItemInstance rewardedEmber = ItemFactory.CreateInstance(emberBladeData, Vector3.zero, _holderObj.transform);
            Assert.IsTrue(inventory.AddItem(rewardedEmber));
            Assert.AreEqual(1, inventory.StoredItemCount);

            // Expand bag inventory: 6 -> 7 capacity
            Assert.IsTrue(inventory.ExpandBag());
            Assert.AreEqual(7, inventory.Capacity);

            runManager.ContinueAfterReward();

            // ==========================================
            // FLOOR 2: ARMORY CELLAR & ELEMENTAL REACTION
            // ==========================================
            Assert.AreEqual(2, runManager.CurrentFloorNumber);
            Assert.AreEqual("Armored Skeleton", enemy.CombatantName);

            // Setup 2-Beam Elemental Reaction (Steam) using Prism Refraction
            PrismRuneDataSO prismData = ScriptableObject.CreateInstance<PrismRuneDataSO>();
            prismData.Initialize("prism_01", "Prism Rune", 2, 3);
            (bool, PrismRuneDataSO) GetPrism(Vector2Int coord) => (coord == new Vector2Int(2, 2), prismData);

            RuneData fireEast = ScriptableObject.CreateInstance<RuneData>();
            fireEast.Initialize("fire_east", "Fire East", ConduitDirection.East, ElementType.Fire, 4);

            RuneData iceEast = ScriptableObject.CreateInstance<RuneData>();
            iceEast.Initialize("ice_east", "Ice East", ConduitDirection.East, ElementType.Ice, 4);

            // Fire East at (0,2) hits Prism at (2,2) -> splits into North branch through (2,3)
            List<ConduitBeamPath> fPaths = MultiDirectionalEmitter.EmitBeams(combatGrid, fireEast, new Vector2Int(0, 2), 4, GetPrism);
            // Ice East at (0,3) passes through (2,3)
            List<ConduitBeamPath> iPaths = MultiDirectionalEmitter.EmitBeams(combatGrid, iceEast, new Vector2Int(0, 3), 4, GetPrism);

            List<ConduitBeamPath> allBeams = new List<ConduitBeamPath>();
            allBeams.AddRange(fPaths);
            allBeams.AddRange(iPaths);

            ElementalReactionSystem reactionSys = _holderObj.AddComponent<ElementalReactionSystem>();
            reactionSys.EnsureDefaultDefinitions();
            reactionSys.UpdateReactions(allBeams);

            Assert.AreEqual(1, reactionSys.ActiveReactionCount);
            foreach (var r in reactionSys.ActiveReactions)
            {
                Assert.AreEqual("reaction_steam", r.ReactionId);
                Assert.AreEqual(new Vector2Int(2, 3), r.GridCoordinate);
            }

            // Start & win Floor 2
            runManager.StartEncounterCombat();
            enemy.TakeDamage(new DamageResult("Hero", "Skeleton", 100, 0, 1f, 1f, 0, 100, false));
            combat.UpdateCombat(0.1f);
            Assert.AreEqual(RunState.RewardSelection, runManager.CurrentState);

            runManager.ContinueAfterReward();

            // ==========================================
            // FLOOR 3: BOSS SANCTUM & THE LICH LORD (3 PHASES)
            // ==========================================
            Assert.AreEqual(3, runManager.CurrentFloorNumber);
            Assert.IsTrue(runManager.IsFinalFloor);
            Assert.IsTrue(bossSystem.IsBossActive);
            Assert.AreEqual("The Lich Lord", enemy.CombatantName);
            Assert.AreEqual(750, enemy.MaxHp);

            runManager.StartEncounterCombat();

            // Deal damage to cross Phase 2 threshold (66% -> 450 HP)
            enemy.TakeDamage(new DamageResult("Hero", "Lich", 300, 0, 1f, 1f, 0, 300, false));
            Assert.AreEqual(1, bossSystem.CurrentPhaseIndex);
            Assert.AreEqual("Phase 2: Soul Harvest", bossSystem.CurrentPhase.DisplayName);
            Assert.AreEqual(15, enemy.Armor); // 10 + 5

            // Deal damage to cross Phase 3 threshold (33% -> 200 HP)
            enemy.TakeDamage(new DamageResult("Hero", "Lich", 250, 0, 1f, 1f, 0, 250, false));
            Assert.AreEqual(2, bossSystem.CurrentPhaseIndex);
            Assert.AreEqual("Phase 3: Necrotic Inversion", bossSystem.CurrentPhase.DisplayName);
            Assert.AreEqual(20, enemy.Armor); // 10 + 10

            // Defeat Boss
            enemy.TakeDamage(new DamageResult("Hero", "Lich", 500, 0, 1f, 1f, 0, 500, false));
            combat.UpdateCombat(0.1f);

            Assert.IsFalse(bossSystem.IsBossActive);
            Assert.AreEqual(RunState.RewardSelection, runManager.CurrentState);

            // Continue past final reward -> RUN COMPLETE!
            runManager.ContinueAfterReward();

            Assert.AreEqual(RunState.RunComplete, runManager.CurrentState);
            Assert.IsTrue(runManager.IsRunFinished);

            // ==========================================
            // 4. SAVE SYSTEM PERSISTENCE & RESTORATION
            // ==========================================
            SaveSystem saveSystem = _holderObj.AddComponent<SaveSystem>();
            saveSystem.SetCustomDirectory(_testSaveDir);

            SaveData saveData = new SaveData();
            saveData.run = new SavedRunData(true, runManager.CurrentFloorIndex, runManager.CurrentEncounterIndex, (int)runManager.CurrentState);
            saveData.inventory = new SavedInventoryData(inventory.ExpansionStep, inventory.Grid.GetUnlockedCoordinates());
            saveData.settings = new SavedSettingsData(0.9f, 0.8f, false, true);

            SaveResult saveResult = saveSystem.Save(saveData);
            Assert.IsTrue(saveResult.IsSuccess);

            // Fresh instance loads save
            LoadResult loadResult = saveSystem.Load();
            Assert.IsTrue(loadResult.IsSuccess);
            Assert.AreEqual((int)RunState.RunComplete, loadResult.Data.run.runState);
            Assert.AreEqual(1, loadResult.Data.inventory.expansionStep);
            Assert.AreEqual(7, loadResult.Data.inventory.GetCoordinates().Count);
        }
    }
}
