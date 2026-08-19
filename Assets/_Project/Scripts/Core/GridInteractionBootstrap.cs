using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Combat;
using Lattirune.Combat.Effects;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Reactions;
using Lattirune.Runes;
using Lattirune.Save;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Core
{
    /// <summary>
    /// Bootstraps the complete Phase 2 Prototype: 5x5 LatticeGrid, data-driven items,
    /// Prism Rune refraction & beam splitting, 5-Element Synergy system, 2-Beam Elemental Reactions,
    /// Combat Effect / Status Framework, 1v1 Combat loop, Reward selection, Audio/Haptics,
    /// and Encrypted Local Save persistence.
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
        [SerializeField] private RewardService rewardService;
        [SerializeField] private CombatEncounterUI combatEncounterUI;
        [SerializeField] private AudioController audioController;
        [SerializeField] private HapticFeedback hapticFeedback;
        [SerializeField] private InteractionFeedbackCoordinator feedbackCoordinator;
        [SerializeField] private SaveSystem saveSystem;
        [SerializeField] private Transform stagingAreaParent;

        [Header("Staging Layout")]
        [SerializeField] private Vector3 stagingOrigin = new Vector3(-2.2f, -4f, 0f);
        [SerializeField] private float itemSpacing = 1.1f;

        [Header("Item Catalogue (TASK-005 Prototype Items)")]
        [SerializeField] private List<ItemDataSO> prototypeItemCatalogue = new List<ItemDataSO>();

        [Header("Development Runes & Targets (TASK-004, TASK-006, TASK-013 & TASK-015 Demo)")]
        [SerializeField] private bool enableConduitDemo = true;
        [SerializeField] private PrismRuneDataSO defaultPrismData;

        private LatticeGrid _grid;
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
        public RewardService Rewards => rewardService;
        public CombatEncounterUI EncounterUI => combatEncounterUI;
        public AudioController Audio => audioController;
        public HapticFeedback Haptics => hapticFeedback;
        public InteractionFeedbackCoordinator Feedback => feedbackCoordinator;
        public SaveSystem Save => saveSystem;
        public PlayerCombatant Player => _playerCombatant;
        public EnemyCombatant Enemy => _enemyCombatant;
        public IReadOnlyList<ItemInstance> SpawnedItems => _spawnedItemInstances;
        public IReadOnlyDictionary<Vector2Int, PrismRuneDataSO> PlacedPrisms => _placedPrisms;

        private void Start()
        {
            InitializePrototype();
        }

        public void InitializePrototype()
        {
            // 1. Create Core 5x5 Grid Data Structure
            _grid = new LatticeGrid(initializeDefaultLayout: true);

            // 2. Ensure GridView exists and initialize
            if (gridView == null)
            {
                GameObject gridViewObj = new GameObject("GridView");
                gridViewObj.transform.SetParent(transform);
                gridView = gridViewObj.AddComponent<GridView>();
            }
            gridView.Initialize(_grid);

            // 3. Ensure ItemDragController exists and initialize
            if (dragController == null)
            {
                GameObject dragCtrlObj = new GameObject("ItemDragController");
                dragCtrlObj.transform.SetParent(transform);
                dragController = dragCtrlObj.AddComponent<ItemDragController>();
            }
            dragController.Initialize(_grid, gridView);

            // 4. Ensure RuneConduitDebugView exists and initialize
            if (conduitDebugView == null)
            {
                GameObject conduitViewObj = new GameObject("RuneConduitDebugView");
                conduitViewObj.transform.SetParent(transform);
                conduitDebugView = conduitViewObj.AddComponent<RuneConduitDebugView>();
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
            }
            reactionSystem.EnsureDefaultDefinitions();

            // 7. Ensure Catalogue Exists
            if (prototypeItemCatalogue == null || prototypeItemCatalogue.Count == 0)
            {
                BuildDefaultItemDefinitions();
            }

            // 8. Setup Save System (TASK-010)
            if (saveSystem == null)
            {
                GameObject saveObj = new GameObject("SaveSystem");
                saveObj.transform.SetParent(transform);
                saveSystem = saveObj.AddComponent<SaveSystem>();
            }

            // 9. Spawn Prototype Items or Restore from Save
            LoadOrCreateState();

            // 10. Setup Development Runes & Prism Refraction (TASK-015)
            if (enableConduitDemo && _activeRunesWithData.Count == 0)
            {
                SetupDevelopmentRunesAndTargets();
            }

            // 11. Setup Combat Entities, Effects & Encounter UI
            SetupCombatAndRewardEncounter();

            // 12. Setup Audio, Haptics & Feedback Coordinator
            SetupFeedbackSystem();

            // 13. Initial recalculation of conduits, synergies, reactions, and player stats
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
                stagingObj.transform.SetParent(transform);
                stagingAreaParent = stagingObj.transform;
            }

            LoadResult loadResult = saveSystem.Load();
            SaveData data = loadResult.Data ?? SaveData.CreateDefault();

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
                rewardService
            );
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
            combatSystem.Initialize(_playerCombatant, _enemyCombatant, combatEffectSystem);

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

            // Reward Service (TASK-008)
            if (rewardService == null)
            {
                GameObject rewardServiceObj = new GameObject("RewardService");
                rewardServiceObj.transform.SetParent(transform);
                rewardService = rewardServiceObj.AddComponent<RewardService>();
            }

            // Combat Encounter UI (TASK-008)
            if (combatEncounterUI == null)
            {
                GameObject uiObj = new GameObject("CombatEncounterUI");
                uiObj.transform.SetParent(transform);
                combatEncounterUI = uiObj.AddComponent<CombatEncounterUI>();
            }
            combatEncounterUI.Initialize(combatSystem, synergySystem, rewardService, prototypeItemCatalogue, stagingAreaParent);

            // Register newly rewarded items and trigger auto-save
            rewardService.OnRewardApplied += (option, instance) =>
            {
                if (instance != null && !_spawnedItemInstances.Contains(instance))
                {
                    _spawnedItemInstances.Add(instance);
                    SaveCurrentState();
                }
            };
        }

        private void BuildDefaultItemDefinitions()
        {
            prototypeItemCatalogue = new List<ItemDataSO>();

            // 1. Training Sword (Weapon, 1x2, Rotatable)
            ItemDataSO sword = ScriptableObject.CreateInstance<ItemDataSO>();
            sword.Initialize("item_training_sword", "Training Sword", "A reliable iron training sword.", ItemCategory.Weapon, new Vector2Int(1, 2), true, new Color(0.9f, 0.5f, 0.1f));
            prototypeItemCatalogue.Add(sword);

            // 2. Ember Blade (Weapon, 2x1, Rotatable)
            ItemDataSO ember = ScriptableObject.CreateInstance<ItemDataSO>();
            ember.Initialize("item_ember_blade", "Ember Blade", "A blade glowing with stored heat.", ItemCategory.Weapon, new Vector2Int(2, 1), true, new Color(0.91f, 0.3f, 0.24f));
            prototypeItemCatalogue.Add(ember);

            // 3. Guard Plate (Shield, 2x2, Rotatable)
            ItemDataSO plate = ScriptableObject.CreateInstance<ItemDataSO>();
            plate.Initialize("item_guard_plate", "Guard Plate", "A reinforced defensive chestplate.", ItemCategory.Shield, new Vector2Int(2, 2), true, new Color(0.2f, 0.6f, 0.86f));
            prototypeItemCatalogue.Add(plate);

            // 4. Arcane Relic (Relic, 1x1, Fixed)
            ItemDataSO relic = ScriptableObject.CreateInstance<ItemDataSO>();
            relic.Initialize("item_arcane_relic", "Arcane Relic", "Ancient artifact vibrating with energy.", ItemCategory.Relic, new Vector2Int(1, 1), false, new Color(0.61f, 0.35f, 0.71f));
            prototypeItemCatalogue.Add(relic);

            // 5. Vital Flask (Consumable, 1x1, Fixed)
            ItemDataSO flask = ScriptableObject.CreateInstance<ItemDataSO>();
            flask.Initialize("item_vital_flask", "Vital Flask", "Restorative dungeon potion.", ItemCategory.Consumable, new Vector2Int(1, 1), false, new Color(0.18f, 0.8f, 0.44f));
            prototypeItemCatalogue.Add(flask);
        }

        private void SetupDevelopmentRunesAndTargets()
        {
            // 1. Demo Fire Rune: Position (2,1) emitting North with range 3
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);
            _activeRunesWithData.Add((fireRune, new Vector2Int(2, 1), ConduitDirection.North, 3));

            // 2. Demo Ice Rune: Position (0,3) emitting East with range 4
            RuneData iceRune = ScriptableObject.CreateInstance<RuneData>();
            iceRune.Initialize("ice_rune_01", "Ice Rune", ConduitDirection.East, ElementType.Ice, 4);
            _activeRunesWithData.Add((iceRune, new Vector2Int(0, 3), ConduitDirection.East, 4));

            // Demo Target Receptor at (2,4)
            GameObject targetObj = new GameObject("Target_2_4");
            targetObj.transform.SetParent(transform);
            ConduitTarget target = targetObj.AddComponent<ConduitTarget>();
            target.Initialize("target_dummy_boss", new Vector2Int(2, 4));
            _activeTargets.Add(target);

            // Demo Prism setup
            defaultPrismData = ScriptableObject.CreateInstance<PrismRuneDataSO>();
            defaultPrismData.Initialize("prism_demo", "Prism Rune", branchCount: 2, maxDepth: 3);
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
                List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                    _grid, 
                    rune, 
                    origin, 
                    dir, 
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

        private void OnGUI()
        {
            // Development persistence & reaction telemetry in top right
            GUILayout.BeginArea(new Rect(Screen.width - 180, 20, 160, 290), GUI.skin.box);
            GUILayout.Label("<size=11><b>DEV CONTROLS</b></size>");
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

            GUILayout.Space(4);
            if (reactionSystem != null && reactionSystem.ActiveReactionCount > 0)
            {
                GUILayout.Label("<size=10><b>REACTIONS:</b></size>");
                foreach (var r in reactionSystem.ActiveReactions)
                {
                    GUILayout.Label($"<size=9>• {r.ReactionName} @ [{r.GridCoordinate.x},{r.GridCoordinate.y}]</size>");
                }
            }

            if (combatEffectSystem != null && _enemyCombatant != null)
            {
                var effects = combatEffectSystem.GetActiveEffects(_enemyCombatant);
                if (effects.Count > 0)
                {
                    GUILayout.Label("<size=10><b>ACTIVE EFFECTS:</b></size>");
                    foreach (var eff in effects)
                    {
                        GUILayout.Label($"<size=9>• {eff.Definition.DisplayName} ({eff.RemainingDuration:F1}s)</size>");
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
