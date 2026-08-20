using System.Collections.Generic;
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
using Lattirune.Progression;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Modifiers;
using Lattirune.Monetization;
using Lattirune.Combo;
using Lattirune.Tutorial;

namespace Lattirune.Core
{
    /// <summary>
    /// Bootstraps the complete Phase 2 Prototype: 5x5 LatticeGrid, data-driven items,
    /// Spatial Bag Inventory & Procedural Expansion, Crossfire Multi-Directional Emitters,
    /// Prism Refraction, 5-Element Synergy system, 2-Beam Elemental Reactions, Combat Effect / Status Framework,
    /// Multi-Floor Run Progression State Machine, Multi-Phase Boss (The Lich Lord), 1v1 Combat loop,
    /// Reward selection, Audio/Haptics, and Encrypted Local Save persistence.
    /// [DEVELOPMENT / PROTOTYPE ENTRY POINT]
    /// </summary>
    public class GridInteractionBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GridView gridView;
        [SerializeField] private ItemDragController dragController;
        [SerializeField] private RuneConduitDebugView conduitDebugView;
        [SerializeField] private SynergySystem synergySystem;
        [SerializeField] private ElementalReactionSystem reactionSystem;
        [SerializeField] private CombatEffectSystem combatEffectSystem;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private BossSystem bossSystem;
        [SerializeField] private RunManager runManager;
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private CombatEncounterUI combatEncounterUI;
        [SerializeField] private AudioController audioController;
        [SerializeField] private HapticFeedback hapticFeedback;
        [SerializeField] private InteractionFeedbackCoordinator feedbackCoordinator;
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private Transform stagingAreaParent;

        [Header("UI Screen Controllers (PLAN.md Section 14 & 19)")]
        [SerializeField] private ScreenNavigationController navigationController;
        [SerializeField] private MainMenuController mainMenuController;
        [SerializeField] private CampfireHubController campfireHubController;
        [SerializeField] private SettingsUIController settingsUIController;
        [SerializeField] private BlueprintForgeController blueprintForgeController;
        [SerializeField] private MetaProgressionManager metaProgressionManager;

        [Header("Staging Layout")]
        [SerializeField] private Vector3 stagingOrigin = new Vector3(-2.2f, -4f, 0f);
        [SerializeField] private float itemSpacing = 1.1f;

        public float ItemSpacing => itemSpacing;

        [Header("Item Catalogue (TASK-005 Prototype Items)")]
        [SerializeField] private List<ItemDataSO> prototypeItemCatalogue = new List<ItemDataSO>();

        [Header("Development Runes & Targets (TASK-004, TASK-006, TASK-013, TASK-015 & TASK-016 Demo)")]
        [SerializeField] private bool enableConduitDemo = true;
        [SerializeField] private PrismRuneDataSO defaultPrismData;

        private LatticeGrid _grid;
        private GameObject _worldGameplayContainer;
        private PlayerCombatant _playerCombatant;
        private EnemyCombatant _enemyCombatant;
        private readonly List<ItemInstance> _spawnedItemInstances = new List<ItemInstance>();
        private readonly List<(RuneData rune, Vector2Int origin, ConduitDirection dir, int range)> _activeRunesWithData = new List<(RuneData, Vector2Int, ConduitDirection, int)>();
        private readonly List<ConduitTarget> _activeTargets = new List<ConduitTarget>();
        private readonly Dictionary<Vector2Int, PrismRuneDataSO> _placedPrisms = new Dictionary<Vector2Int, PrismRuneDataSO>();

        public LatticeGrid Grid => _grid;
        public GridView View => gridView;
        public SynergySystem Synergy => synergySystem;
        public ElementalReactionSystem Reactions => reactionSystem;
        public CombatEffectSystem Effects => combatEffectSystem;
        public CombatSystem Combat => combatSystem;
        public BossSystem Boss => bossSystem;
        public RunManager Run => runManager;
        public InventorySystem Inventory => inventorySystem;
        public RewardService Rewards => rewardService;
        public CombatEncounterUI EncounterUI => combatEncounterUI;
        public AudioController Audio => audioController;
        public HapticFeedback Haptics => hapticFeedback;
        public InteractionFeedbackCoordinator Feedback => feedbackCoordinator;
        public SaveSystem Save => saveSystem;
        public ScreenNavigationController Navigation => navigationController;
        public MainMenuController MainMenu => mainMenuController;
        public CampfireHubController CampfireHub => campfireHubController;
        public SettingsUIController SettingsUI => settingsUIController;
        public BlueprintForgeController BlueprintForge => blueprintForgeController;
        public MetaProgressionManager MetaProgression => metaProgressionManager;
        public PlayerCombatant Player => _playerCombatant;
        public EnemyCombatant Enemy => _enemyCombatant;
        public IReadOnlyList<ItemInstance> SpawnedItems => _spawnedItemInstances;
        public IReadOnlyDictionary<Vector2Int, PrismRuneDataSO> PlacedPrisms => _placedPrisms;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            InitializePrototype();
        }

        public void InitializePrototype()
        {
            // 1. Create Core 5x5 Grid Data Structure
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            // Create World Gameplay Container to group all 3D world space visuals
            if (_worldGameplayContainer == null)
            {
                _worldGameplayContainer = new GameObject("WorldGameplayContainer");
                _worldGameplayContainer.transform.SetParent(transform);
            }

            // 2. Ensure GridView exists and initialize
            if (gridView == null)
            {
                GameObject gridViewObj = new GameObject("GridView");
                gridViewObj.transform.SetParent(_worldGameplayContainer.transform);
                gridView = gridViewObj.AddComponent<GridView>();
            }
            else
            {
                gridView.transform.SetParent(_worldGameplayContainer.transform);
            }
            gridView.Initialize(_grid);

            // 3. Ensure ItemDragController exists and initialize
            if (dragController == null)
            {
                GameObject dragCtrlObj = new GameObject("ItemDragController");
                dragCtrlObj.transform.SetParent(_worldGameplayContainer.transform);
                dragController = dragCtrlObj.AddComponent<ItemDragController>();
            }
            else
            {
                dragController.transform.SetParent(_worldGameplayContainer.transform);
            }
            dragController.Initialize(_grid, gridView);

            // 4. Ensure RuneConduitDebugView exists and initialize
            if (conduitDebugView == null)
            {
                GameObject conduitViewObj = new GameObject("RuneConduitDebugView");
                conduitViewObj.transform.SetParent(_worldGameplayContainer.transform);
                conduitDebugView = conduitViewObj.AddComponent<RuneConduitDebugView>();
            }
            else
            {
                conduitDebugView.transform.SetParent(_worldGameplayContainer.transform);
            }
            conduitDebugView.Initialize(gridView);

            // 5. Ensure SynergySystem exists and initialize
            if (synergySystem == null)
            {
                GameObject synergyObj = new GameObject("SynergySystem");
                synergyObj.transform.SetParent(transform);
                synergySystem = synergyObj.AddComponent<SynergySystem>();
            }
            synergySystem.EnsureDefaultDefinitions();

            // 6. Ensure ElementalReactionSystem exists and initialize
            if (reactionSystem == null)
            {
                GameObject reactionObj = new GameObject("ElementalReactionSystem");
                reactionObj.transform.SetParent(transform);
                reactionSystem = reactionObj.AddComponent<ElementalReactionSystem>();
                var reactionVFX = reactionObj.AddComponent<ElementalReactionVFXController>();
                reactionVFX.Initialize(reactionSystem, gridView, navigationController);
            }
            reactionSystem.EnsureDefaultDefinitions();

            // 7. Ensure InventorySystem exists and initialize (TASK-019)
            if (inventorySystem == null)
            {
                GameObject invObj = new GameObject("InventorySystem");
                invObj.transform.SetParent(transform);
                inventorySystem = invObj.AddComponent<InventorySystem>();
            }
            inventorySystem.Initialize();

            // 8. Ensure Catalogue Exists
            if (prototypeItemCatalogue == null || prototypeItemCatalogue.Count == 0)
            {
                BuildDefaultItemDefinitions();
            }

            // 9. Setup Save System (TASK-010)
            if (saveSystem == null)
            {
                GameObject saveObj = new GameObject("SaveSystem");
                saveObj.transform.SetParent(transform);
                saveSystem = saveObj.AddComponent<SaveSystem>();
            }

            // 10. Spawn Prototype Items or Restore from Save
            LoadOrCreateState();

            // 11. Setup Meta Progression and UI Navigation Flow (PLAN.md Section 14 & 19)
            SetupUINavigationFlow();

            // 12. Setup Development Runes, Crossfire & Prism Refraction (TASK-015 & TASK-016)
            if (enableConduitDemo && _activeRunesWithData.Count == 0)
            {
                SetupDevelopmentRunesAndTargets();
            }

            // 13. Setup Combat Entities, Effects, Boss & Run Manager (TASK-017 & TASK-018)
            SetupCombatAndRewardEncounter();

            // 14. Setup Audio, Haptics & Feedback Coordinator
            SetupFeedbackSystem();

            // 15. Initial recalculation of conduits, synergies, reactions, and player stats
            RecalculateAndRenderConduits();

            // Hook into item placement/removal events to dynamically recalculate conduits, synergies, and combat stats
            _grid.OnItemPlaced += (id, origin, size) => RecalculateAndRenderConduits();
            _grid.OnItemRemoved += (id, origin, size) => RecalculateAndRenderConduits();
        }

        private void LoadOrCreateState()
        {
            if (stagingAreaParent == null)
            {
                GameObject stagingObj = new GameObject("StagingArea");
                stagingObj.transform.SetParent(_worldGameplayContainer != null ? _worldGameplayContainer.transform : transform);
                stagingAreaParent = stagingObj.transform;
            }
            else if (_worldGameplayContainer != null)
            {
                stagingAreaParent.SetParent(_worldGameplayContainer.transform);
            }

            LoadResult loadResult = saveSystem.Load();
            SaveData data = loadResult.Data ?? SaveData.CreateDefault();

            // Restore inventory expansion state
            if (data.inventory != null && inventorySystem != null)
            {
                var coords = data.inventory.GetCoordinates();
                if (coords != null && coords.Count > 0)
                {
                    inventorySystem.RestoreState(coords, data.inventory.expansionStep);
                }
            }

            // Clear any previous spawned items
            for (int i = _spawnedItemInstances.Count - 1; i >= 0; i--)
            {
                if (_spawnedItemInstances[i] != null)
                {
                    DestroyImmediate(_spawnedItemInstances[i].gameObject);
                }
            }
            _spawnedItemInstances.Clear();

            // Restore items
            foreach (var savedItem in data.items)
            {
                ItemDataSO itemData = prototypeItemCatalogue.Find(x => x != null && x.ItemId == savedItem.itemId);
                if (itemData == null) continue;

                Vector3 spawnPos = savedItem.isPlacedOnGrid 
                    ? GridCoordinateUtility.GridToWorld(new Vector2Int(savedItem.gridX, savedItem.gridY))
                    : new Vector3(savedItem.stagingPosX, savedItem.stagingPosY, 0f);

                ItemInstance instance = ItemFactory.CreateInstance(itemData, spawnPos, stagingAreaParent);
                if (instance != null)
                {
                    if (savedItem.rotationDegrees > 0)
                    {
                        int steps = savedItem.rotationDegrees / 90;
                        for (int s = 0; s < steps; s++) instance.RotateClockwise();
                    }

                    if (savedItem.isPlacedOnGrid)
                    {
                        Vector2Int gridCoord = new Vector2Int(savedItem.gridX, savedItem.gridY);
                        if (_grid.PlaceItem(instance.InstanceId, gridCoord, instance.CurrentDimensions))
                        {
                            instance.OnPlaced(gridCoord, GridCoordinateUtility.GridToWorld(gridCoord));
                        }
                    }

                    _spawnedItemInstances.Add(instance);
                }
            }

            // Restore settings if present
            if (data.settings != null)
            {
                if (audioController != null)
                {
                    audioController.SetMasterVolume(data.settings.masterVolume);
                    audioController.SetSfxVolume(data.settings.sfxVolume);
                    audioController.SetMuted(data.settings.isMuted);
                }
                if (hapticFeedback != null)
                {
                    hapticFeedback.HapticsEnabled = data.settings.hapticsEnabled;
                }
            }
        }

        public void SaveCurrentState()
        {
            if (saveSystem == null) return;

            SaveData data = new SaveData();

            // Save Items
            foreach (var item in _spawnedItemInstances)
            {
                if (item == null || item.Data == null) continue;

                SavedItemData sItem = new SavedItemData(
                    id: item.Data.ItemId,
                    x: item.IsPlacedOnGrid ? item.GridPosition.x : -1,
                    y: item.IsPlacedOnGrid ? item.GridPosition.y : -1,
                    rot: item.CurrentRotationAngle,
                    placed: item.IsPlacedOnGrid,
                    stageX: item.transform.position.x,
                    stageY: item.transform.position.y
                );
                data.items.Add(sItem);
            }

            // Save Runes
            foreach (var runeTuple in _activeRunesWithData)
            {
                data.runes.Add(new SavedRuneData(
                    id: runeTuple.rune.RuneId,
                    x: runeTuple.origin.x,
                    y: runeTuple.origin.y,
                    dir: (int)runeTuple.dir,
                    elem: (int)runeTuple.rune.Element,
                    r: runeTuple.range
                ));
            }

            // Save Run Progress (TASK-017)
            if (runManager != null)
            {
                data.run = new SavedRunData(
                    active: runManager.CurrentState != RunState.NotStarted,
                    floorIdx: runManager.CurrentFloorIndex,
                    encIdx: runManager.CurrentEncounterIndex,
                    state: (int)runManager.CurrentState
                );
            }

            // Save Inventory Expansion (TASK-019)
            if (inventorySystem != null && inventorySystem.Grid != null)
            {
                data.inventory = new SavedInventoryData(
                    inventorySystem.ExpansionStep,
                    inventorySystem.Grid.GetUnlockedCoordinates()
                );
            }

            // Save Settings
            float masterVol = audioController != null ? audioController.MasterVolume : 1.0f;
            float sfxVol = audioController != null ? audioController.SfxVolume : 1.0f;
            bool muted = audioController != null ? audioController.IsMuted : false;
            bool haptics = hapticFeedback != null ? hapticFeedback.HapticsEnabled : true;
            data.settings = new SavedSettingsData(masterVol, sfxVol, muted, haptics);

            saveSystem.Save(data);
        }

        private void SetupFeedbackSystem()
        {
            if (audioController == null)
            {
                GameObject audioObj = new GameObject("AudioController");
                audioObj.transform.SetParent(transform);
                audioController = audioObj.AddComponent<AudioController>();
            }

            if (hapticFeedback == null)
            {
                GameObject hapticsObj = new GameObject("HapticFeedback");
                hapticsObj.transform.SetParent(transform);
                hapticFeedback = hapticsObj.AddComponent<HapticFeedback>();
            }

            if (feedbackCoordinator == null)
            {
                GameObject coordObj = new GameObject("InteractionFeedbackCoordinator");
                coordObj.transform.SetParent(transform);
                feedbackCoordinator = coordObj.AddComponent<InteractionFeedbackCoordinator>();
            }

            feedbackCoordinator.Initialize(
                audioController, 
                hapticFeedback, 
                _grid, 
                synergySystem, 
                combatSystem, 
                rewardService,
                reactionSystem
            );

            audioController.PlayBgm(AudioCueType.BgmDungeonLoop);
        }

        private void SetupCombatAndRewardEncounter()
        {
            // Player entity
            GameObject playerObj = new GameObject("PlayerCombatant");
            playerObj.transform.SetParent(transform);
            _playerCombatant = playerObj.AddComponent<PlayerCombatant>();
            _playerCombatant.SetupDefaultPlayer(initialHp: 100);

            // Enemy entity (Training Dummy)
            GameObject enemyObj = new GameObject("TrainingDummy");
            enemyObj.transform.SetParent(transform);
            _enemyCombatant = enemyObj.AddComponent<EnemyCombatant>();
            _enemyCombatant.SetupTrainingDummy(hp: 50, baseArmor: 2, attack: 4, interval: 1.5f);

            // Combat Effect System
            if (combatEffectSystem == null)
            {
                GameObject effectObj = new GameObject("CombatEffectSystem");
                effectObj.transform.SetParent(transform);
                combatEffectSystem = effectObj.AddComponent<CombatEffectSystem>();
            }
            combatEffectSystem.EnsureDefaultDatabase();

            // Combat System coordinator
            if (combatSystem == null)
            {
                GameObject combatSystemObj = new GameObject("CombatSystem");
                combatSystemObj.transform.SetParent(transform);
                combatSystem = combatSystemObj.AddComponent<CombatSystem>();
            }

            GameObject comboObj = new GameObject("ComboTracker");
            comboObj.transform.SetParent(transform);
            var comboTracker = comboObj.AddComponent<Lattirune.Combo.ComboTracker>();
            comboTracker.Initialize();
            comboTracker.BindCombatSystem(combatSystem);
            if (reactionSystem != null) comboTracker.BindReactionSystem(reactionSystem);

            combatSystem.Initialize(_playerCombatant, _enemyCombatant, combatEffectSystem, null, comboTracker);

            // Boss System coordinator (TASK-018)
            if (bossSystem == null)
            {
                GameObject bossObj = new GameObject("BossSystem");
                bossObj.transform.SetParent(transform);
                bossSystem = bossObj.AddComponent<BossSystem>();
            }
            bossSystem.Initialize(BossDefinitionSO.CreateLichLordDefinition(), _enemyCombatant, combatEffectSystem);

            // Reward Service (TASK-008)
            if (rewardService == null)
            {
                GameObject rewardServiceObj = new GameObject("RewardService");
                rewardServiceObj.transform.SetParent(transform);
                rewardService = rewardServiceObj.AddComponent<RewardService>();
            }

            // Run Manager Master State Machine (TASK-017 & TASK-018)
            if (runManager == null)
            {
                GameObject runObj = new GameObject("RunManager");
                runObj.transform.SetParent(transform);
                runManager = runObj.AddComponent<RunManager>();
            }
            runManager.Initialize(
                DungeonDefinitionSO.Create10FloorCursedSewersDungeon(),
                combatSystem,
                rewardService,
                _playerCombatant,
                _enemyCombatant,
                bossSystem
            );

            // Apply active elemental reactions to combat when battle starts
            combatSystem.OnStateChanged += (state) =>
            {
                if (state == CombatState.Fighting && reactionSystem != null && combatEffectSystem != null)
                {
                    foreach (var reaction in reactionSystem.ActiveReactions)
                    {
                        var effectInstance = ReactionEffectResolver.ResolveEffect(reaction, combatEffectSystem.Database, _enemyCombatant);
                        if (effectInstance != null)
                        {
                            combatEffectSystem.ApplyEffect(effectInstance);
                        }
                    }
                }
            };

            // Combat Encounter UI (TASK-008, TASK-017 & TASK-018)
            if (combatEncounterUI == null)
            {
                GameObject uiObj = new GameObject("CombatEncounterUI");
                uiObj.transform.SetParent(transform);
                combatEncounterUI = uiObj.AddComponent<CombatEncounterUI>();
            }
            if (GetComponent<CombatStageVisualController>() == null)
            {
                gameObject.AddComponent<CombatStageVisualController>();
            }
            combatEncounterUI.Initialize(combatSystem, synergySystem, rewardService, prototypeItemCatalogue, stagingAreaParent, navigationController, runManager);

            // Reward applied -> auto-save
            rewardService.OnRewardApplied += (option, instance) =>
            {
                if (instance != null && !_spawnedItemInstances.Contains(instance))
                {
                    _spawnedItemInstances.Add(instance);
                    SaveCurrentState();
                }
            };

            // Procedural Run Events Subsystem (TASK-051 & TASK-052)
            GameObject eventObj = new GameObject("RunEventIntegration");
            eventObj.transform.SetParent(transform);
            var eventIntegration = eventObj.AddComponent<RunEventIntegration>();
            eventIntegration.Initialize(runManager, runManager, _playerCombatant, combatSystem);

            // Merchant Stall Subsystem (TASK-056)
            GameObject merchantObj = new GameObject("MerchantSystem");
            merchantObj.transform.SetParent(transform);
            var merchantSystem = merchantObj.AddComponent<MerchantSystem>();
            merchantSystem.Initialize(ItemDatabaseSO.CreateCanonicalDatabase(), RuneDatabaseSO.CreateCanonicalDatabase());

            GameObject merchantUiObj = new GameObject("MerchantStallUIController");
            merchantUiObj.transform.SetParent(transform);
            var merchantUI = merchantUiObj.AddComponent<MerchantStallUIController>();
            merchantUI.Initialize(merchantSystem, runManager, inventorySystem, _grid, _playerCombatant, runManager, navigationController);

            if (feedbackCoordinator != null)
            {
                feedbackCoordinator.Initialize(audioController, hapticFeedback, _grid, synergySystem, combatSystem, rewardService, reactionSystem, merchantSystem, comboTracker, bossSystem);
            }

            // Dungeon Map Topology Subsystem (TASK-058)
            GameObject mapUiObj = new GameObject("DungeonMapScreenController");
            mapUiObj.transform.SetParent(transform);
            var mapUI = mapUiObj.AddComponent<DungeonMapScreenController>();
            mapUI.Initialize(runManager, navigationController);

            merchantUI.BindMapController(mapUI);

            // Hero Classes & Loadouts Subsystem (TASK-057)
            GameObject heroObj = new GameObject("HeroClassManager");
            heroObj.transform.SetParent(transform);
            var heroClassManager = heroObj.AddComponent<HeroClassManager>();
            heroClassManager.Initialize();

            GameObject heroUiObj = new GameObject("HeroClassSelectionUIController");
            heroUiObj.transform.SetParent(transform);
            var heroClassUI = heroUiObj.AddComponent<HeroClassSelectionUIController>();
            heroClassUI.Initialize(heroClassManager, metaProgressionManager, navigationController, runManager, mapUI);

            // Bestiary & Codex Subsystem (TASK-059)
            GameObject codexObj = new GameObject("CodexManager");
            codexObj.transform.SetParent(transform);
            var codexManager = codexObj.AddComponent<CodexManager>();
            codexManager.Initialize(BestiaryDatabaseSO.CreateCanonicalDatabase());

            if (synergySystem != null)
            {
                synergySystem.OnSynergyActivated += (syn) => codexManager.RecordSynergyDiscovered(syn.SynergyId);
            }
            if (reactionSystem != null)
            {
                reactionSystem.OnReactionActivated += (rxn) => codexManager.RecordReactionTriggered(rxn.ReactionId);
            }
            if (combatSystem != null)
            {
                combatSystem.OnVictory += () =>
                {
                    if (_enemyCombatant != null && !string.IsNullOrEmpty(_enemyCombatant.CombatantName))
                    {
                        codexManager.RecordEnemyDefeat(_enemyCombatant.CombatantName);
                    }
                };
            }

            GameObject codexUiObj = new GameObject("CodexUIController");
            codexUiObj.transform.SetParent(transform);
            var codexUI = codexUiObj.AddComponent<CodexUIController>();
            codexUI.Initialize(codexManager, navigationController);

            // Combat Juice: Floating Text & Camera Shake (TASK-060)
            GameObject floatyObj = new GameObject("FloatingCombatTextPool");
            floatyObj.transform.SetParent(transform);
            var floatyPool = floatyObj.AddComponent<FloatingCombatTextPool>();
            floatyPool.Initialize(combatSystem, reactionSystem);
            floatyPool.BindNavigation(navigationController);
            floatyPool.BindComboTracker(comboTracker);

            GameObject shakeObj = new GameObject("CombatCameraShakeController");
            shakeObj.transform.SetParent(transform);
            var shakeController = shakeObj.AddComponent<CombatCameraShakeController>();
            shakeController.Initialize(Camera.main, combatSystem);

            // Offline Monetization Service (TASK-061)
            GameObject monObj = new GameObject("OfflineMonetizationService");
            monObj.transform.SetParent(transform);
            var monetizationService = monObj.AddComponent<OfflineMonetizationService>();
            monetizationService.Initialize();

            // Campfire Rest UI Controller (TASK-063)
            GameObject restUiObj = new GameObject("CampfireRestUIController");
            restUiObj.transform.SetParent(transform);
            var campfireRestUI = restUiObj.AddComponent<CampfireRestUIController>();
            campfireRestUI.Initialize(runManager, _playerCombatant, null, navigationController);
            campfireRestUI.BindMapController(mapUI);

            // Run Event UI Controller
            GameObject eventUiObj = new GameObject("RunEventUIController");
            eventUiObj.transform.SetParent(transform);
            var eventUI = eventUiObj.AddComponent<RunEventUIController>();
            eventUI.Initialize(eventIntegration.EventService, runManager, _playerCombatant, eventIntegration.ModifierManager, navigationController, runManager, mapUI);

            // Run Complete & Summary Controller
            GameObject completeObj = new GameObject("RunCompleteController");
            completeObj.transform.SetParent(transform);
            var runCompleteUI = completeObj.AddComponent<RunCompleteController>();
            runCompleteUI.Initialize(navigationController, runManager, metaProgressionManager);

            combatEncounterUI.BindControllers(mapUI, runCompleteUI);

            runManager.OnRunCompleted += () =>
            {
                runCompleteUI.SetupSummary(victory: true, floors: 10, gold: runManager.CurrentGold, embers: runManager.CurrentEmbers);
                if (navigationController != null) navigationController.NavigateTo(ScreenState.VICTORY);
            };

            runManager.OnRunDefeated += () =>
            {
                if (!runManager.CanRevivePlayer)
                {
                    int cleared = Mathf.Max(0, runManager.CurrentFloorNumber - 1);
                    runCompleteUI.SetupSummary(victory: false, floors: cleared, gold: runManager.CurrentGold, embers: runManager.CurrentEmbers);
                    if (navigationController != null) navigationController.NavigateTo(ScreenState.DEATH);
                }
            };

            // Tutorial System Subsystem (TASK-063)
            GameObject tutObj = new GameObject("TutorialManager");
            tutObj.transform.SetParent(transform);
            var tutorialManager = tutObj.AddComponent<TutorialManager>();
            tutorialManager.Initialize(alreadyCompleted: false);

            _grid.OnItemPlaced += (id, origin, size) => tutorialManager.AdvanceStep(TutorialStep.DragWeaponToGrid);
            if (synergySystem != null)
            {
                synergySystem.OnSynergyActivated += (syn) => tutorialManager.AdvanceStep(TutorialStep.ConnectRuneLaser);
            }
            if (combatSystem != null)
            {
                combatSystem.OnStateChanged += (state) =>
                {
                    if (state == CombatState.Fighting)
                    {
                        tutorialManager.AdvanceStep(TutorialStep.StartFirstBattle);
                    }
                };
            }

            // Bind ScreenNavigationController state manager
            if (navigationController != null)
            {
                navigationController.BindWorldGrid(_worldGameplayContainer != null ? _worldGameplayContainer : (gridView != null ? gridView.gameObject : null));
                navigationController.RegisterScreenController(ScreenState.MAIN_MENU, mainMenuController);
                navigationController.RegisterScreenController(ScreenState.CAMPFIRE_HUB, campfireHubController);
                navigationController.RegisterScreenController(ScreenState.BLUEPRINT_FORGE, blueprintForgeController);
                navigationController.RegisterScreenController(ScreenState.SETTINGS, settingsUIController);
                navigationController.RegisterScreenController(ScreenState.HERO_SELECTION, heroClassUI);
                navigationController.RegisterScreenController(ScreenState.DUNGEON_MAP, mapUI);
                navigationController.RegisterScreenController(ScreenState.RUN_START, mapUI);
                navigationController.RegisterScreenController(ScreenState.GRID_BUILD, combatEncounterUI);
                navigationController.RegisterScreenController(ScreenState.COMBAT, combatEncounterUI);
                navigationController.RegisterScreenController(ScreenState.REWARD_SELECTION, combatEncounterUI);
                navigationController.RegisterScreenController(ScreenState.MERCHANT, merchantUI);
                navigationController.RegisterScreenController(ScreenState.CAMPFIRE_REST, campfireRestUI);
                navigationController.RegisterScreenController(ScreenState.EVENT, eventUI);
                navigationController.RegisterScreenController(ScreenState.VICTORY, runCompleteUI);
                navigationController.RegisterScreenController(ScreenState.DEATH, runCompleteUI);
                navigationController.RegisterScreenController(ScreenState.RUN_COMPLETE, runCompleteUI);
                navigationController.RegisterScreenController(ScreenState.CODEX, codexUI);

                if (dragController != null) dragController.BindNavigation(navigationController);
                navigationController.Initialize(ScreenState.MAIN_MENU);
            }

            if (reactionSystem != null)
            {
                var reactionVFX = reactionSystem.GetComponent<ElementalReactionVFXController>();
                if (reactionVFX != null)
                {
                    reactionVFX.Initialize(reactionSystem, gridView, navigationController);
                }
            }
        }

        private void BuildDefaultItemDefinitions()
        {
            ItemDatabaseSO db = ItemDatabaseSO.CreateCanonicalDatabase();
            prototypeItemCatalogue = new List<ItemDataSO>(db.AllItems);
        }

                private void SetupDevelopmentRunesAndTargets()
        {
            // 1. Demo Fire Rune: Position (2,1) emitting North with range 3
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);
            _activeRunesWithData.Add((fireRune, new Vector2Int(2, 1), ConduitDirection.North, 3));
            SpawnRuneVisualObject(fireRune, new Vector2Int(2, 1));

            // 2. Demo Ice Rune: Position (0,3) emitting East with range 4
            RuneData iceRune = ScriptableObject.CreateInstance<RuneData>();
            iceRune.Initialize("ice_rune_01", "Ice Rune", ConduitDirection.East, ElementType.Ice, 4);
            _activeRunesWithData.Add((iceRune, new Vector2Int(0, 3), ConduitDirection.East, 4));
            SpawnRuneVisualObject(iceRune, new Vector2Int(0, 3));

            // Demo Target Receptor at (2,4)
            GameObject targetObj = new GameObject("Target_2_4");
            targetObj.transform.SetParent(_worldGameplayContainer != null ? _worldGameplayContainer.transform : transform);
            ConduitTarget target = targetObj.AddComponent<ConduitTarget>();
            target.Initialize("target_dummy_boss", new Vector2Int(2, 4));
            _activeTargets.Add(target);

            // Demo Prism setup
            defaultPrismData = ScriptableObject.CreateInstance<PrismRuneDataSO>();
            defaultPrismData.Initialize("prism_demo", "Prism Rune", branchCount: 2, maxDepth: 3);
        }

        private void SpawnRuneVisualObject(RuneData rune, Vector2Int gridPos)
        {
            if (rune == null) return;
            GameObject runeObj = new GameObject($"RuneVisual_{rune.RuneName}_{gridPos.x}_{gridPos.y}");
            runeObj.transform.SetParent(_worldGameplayContainer != null ? _worldGameplayContainer.transform : transform);
            runeObj.transform.position = GridCoordinateUtility.GridToWorld(gridPos);
            runeObj.transform.localScale = new Vector3(GridCoordinateUtility.DEFAULT_CELL_SIZE * 0.85f, GridCoordinateUtility.DEFAULT_CELL_SIZE * 0.85f, 1f);

            SpriteRenderer sr = runeObj.AddComponent<SpriteRenderer>();
            Texture2D runeTex = VisualAssetProvider.GetRuneTexture(rune.Element);
            if (runeTex != null)
            {
                sr.sprite = Sprite.Create(runeTex, new Rect(0, 0, runeTex.width, runeTex.height), new Vector2(0.5f, 0.5f), 128);
            }
            sr.sortingOrder = 5;
        }

        public void PlacePrismAt(Vector2Int coord, PrismRuneDataSO data = null)
        {
            _placedPrisms[coord] = data ?? defaultPrismData ?? ScriptableObject.CreateInstance<PrismRuneDataSO>();
            RecalculateAndRenderConduits();
        }

        public void RemovePrismAt(Vector2Int coord)
        {
            if (_placedPrisms.Remove(coord))
            {
                RecalculateAndRenderConduits();
            }
        }

        public void AddRuneEmitter(RuneData rune, Vector2Int origin, int range = 5)
        {
            if (rune != null)
            {
                _activeRunesWithData.Add((rune, origin, rune.Direction, range));
                RecalculateAndRenderConduits();
            }
        }

        public void RecalculateAndRenderConduits()
        {
            if (conduitDebugView == null || _grid == null) return;

            bool IsTarget(Vector2Int coord)
            {
                foreach (var t in _activeTargets)
                {
                    if (t != null && t.GridPosition == coord) return true;
                }
                return false;
            }

            (bool isPrism, PrismRuneDataSO data) GetPrism(Vector2Int coord)
            {
                if (_placedPrisms.TryGetValue(coord, out var pData))
                {
                    return (true, pData);
                }
                return (false, null);
            }

            List<ConduitBeamPath> allBeams = new List<ConduitBeamPath>();

            for (int i = 0; i < _activeRunesWithData.Count; i++)
            {
                var (rune, origin, dir, range) = _activeRunesWithData[i];
                List<ConduitBeamPath> paths = MultiDirectionalEmitter.EmitBeams(
                    _grid, 
                    rune, 
                    origin, 
                    range, 
                    GetPrism, 
                    IsTarget, 
                    stopOnTarget: false
                );
                allBeams.AddRange(paths);
            }

            // Render all beam paths including refracted branches
            conduitDebugView.RenderBeamPaths(allBeams);

            // Update Synergies (Rune + Item)
            if (synergySystem != null)
            {
                synergySystem.UpdateSynergies(allBeams, _spawnedItemInstances);
            }

            // Update Elemental Reactions (Rune Beam x Rune Beam)
            if (reactionSystem != null)
            {
                reactionSystem.UpdateReactions(allBeams);
            }

            // Update Player Combat Stats from the Grid build
            if (_playerCombatant != null)
            {
                _playerCombatant.UpdateStatsFromBuild(_spawnedItemInstances);
            }
        }

        private void SetupUINavigationFlow()
        {
            if (metaProgressionManager == null)
            {
                GameObject metaObj = new GameObject("MetaProgressionManager");
                metaObj.transform.SetParent(transform);
                metaProgressionManager = metaObj.AddComponent<MetaProgressionManager>();
            }
            metaProgressionManager.Initialize();
            if (saveSystem != null && saveSystem.HasSaveFile())
            {
                SaveData loaded = saveSystem.Load();
                if (loaded != null && loaded.meta != null)
                {
                    metaProgressionManager.ImportMetaData(loaded.meta);
                }
            }

            if (navigationController == null)
            {
                GameObject navObj = new GameObject("ScreenNavigationController");
                navObj.transform.SetParent(transform);
                navigationController = navObj.AddComponent<ScreenNavigationController>();
            }
            navigationController.Initialize(ScreenState.MAIN_MENU);

            if (blueprintForgeController == null)
            {
                GameObject forgeObj = new GameObject("BlueprintForgeController");
                forgeObj.transform.SetParent(transform);
                blueprintForgeController = forgeObj.AddComponent<BlueprintForgeController>();
            }
            blueprintForgeController.Initialize(metaProgressionManager);

            if (campfireHubController == null)
            {
                GameObject campObj = new GameObject("CampfireHubController");
                campObj.transform.SetParent(transform);
                campfireHubController = campObj.AddComponent<CampfireHubController>();
            }
            campfireHubController.Initialize(navigationController, metaProgressionManager, blueprintForgeController);

            if (settingsUIController == null)
            {
                GameObject setObj = new GameObject("SettingsUIController");
                setObj.transform.SetParent(transform);
                settingsUIController = setObj.AddComponent<SettingsUIController>();
            }
            settingsUIController.Initialize(navigationController, audioController, saveSystem);

            if (mainMenuController == null)
            {
                GameObject menuObj = new GameObject("MainMenuController");
                menuObj.transform.SetParent(transform);
                mainMenuController = menuObj.AddComponent<MainMenuController>();
            }
            mainMenuController.Initialize(navigationController, runManager, metaProgressionManager, saveSystem);

            if (combatEncounterUI != null)
            {
                combatEncounterUI.Initialize(combatSystem, synergySystem, rewardService, prototypeItemCatalogue, stagingAreaParent, navigationController, runManager);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool showDevControlsOverlay = false;

        private void OnGUI()
        {
            if (!showDevControlsOverlay) return;

            // Only show developer HUD overlay during active gameplay (GRID_BUILD / COMBAT)
            if (navigationController != null && navigationController.CurrentScreen != ScreenState.GRID_BUILD && navigationController.CurrentScreen != ScreenState.COMBAT)
            {
                return;
            }

            // Development persistence, inventory & run telemetry in top right
            GUILayout.BeginArea(new Rect(Screen.width - 210, 20, 200, 420), GUI.skin.box);
            GUILayout.Label("<size=11><b>DEV CONTROLS & HUD</b></size>");

            if (runManager != null)
            {
                GUILayout.Label($"<size=10><b>Floor:</b> {runManager.CurrentFloorNumber} / {runManager.TotalFloors}</size>");
                GUILayout.Label($"<size=10><b>State:</b> {runManager.CurrentState}</size>");

                if (bossSystem != null && bossSystem.IsBossActive)
                {
                    BossTelemetry telem = bossSystem.GetTelemetry();
                    GUILayout.Label($"<size=10><b>BOSS:</b> {telem.BossName}</size>");
                    GUILayout.Label($"<size=9>HP: {telem.CurrentHp}/{telem.MaxHp} ({telem.HpPercentage:P0})</size>");
                    GUILayout.Label($"<size=9>Phase: {telem.CurrentPhaseIndex + 1}/{bossSystem.TotalPhases} ({telem.PhaseName})</size>");
                }

                if (runManager.CurrentState == RunState.NotStarted || runManager.IsRunFinished)
                {
                    if (GUILayout.Button("START RUN"))
                    {
                        runManager.StartRun();
                    }
                }
                else if (runManager.CurrentState == RunState.FloorPreparing)
                {
                    if (GUILayout.Button("FIGHT ENCOUNTER"))
                    {
                        runManager.StartEncounterCombat();
                    }
                }
                else if (runManager.CurrentState == RunState.RewardSelection)
                {
                    if (GUILayout.Button("CONTINUE"))
                    {
                        runManager.ContinueAfterReward();
                    }
                }
            }

            GUILayout.Space(4);
            if (inventorySystem != null)
            {
                GUILayout.Label("<size=10><b>BAG INVENTORY:</b></size>");
                GUILayout.Label($"<size=9>Capacity: {inventorySystem.Capacity} / {inventorySystem.TotalCapacity}</size>");
                GUILayout.Label($"<size=9>Unlocked: {inventorySystem.UnlockedCount} | Locked: {inventorySystem.LockedCount}</size>");
                if (inventorySystem.CanExpand)
                {
                    if (GUILayout.Button("EXPAND BAG"))
                    {
                        inventorySystem.ExpandBag();
                    }
                }
            }

            GUILayout.Space(4);
            GUILayout.Label("<size=10><b>SAVE ACTIONS</b></size>");
            if (GUILayout.Button("SAVE"))
            {
                SaveCurrentState();
            }
            if (GUILayout.Button("LOAD"))
            {
                LoadOrCreateState();
                RecalculateAndRenderConduits();
            }
            if (GUILayout.Button("RESET SAVE"))
            {
                saveSystem.DeleteSave();
                LoadOrCreateState();
                RecalculateAndRenderConduits();
            }

            GUILayout.EndArea();
        }
#endif

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveCurrentState();
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentState();
        }
    }
}
