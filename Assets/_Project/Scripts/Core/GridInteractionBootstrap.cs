using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Runes;
using Lattirune.Synergy;
using Lattirune.UI;

namespace Lattirune.Core
{
    /// <summary>
    /// Bootstraps the physical 5x5 LatticeGrid interaction, data-driven prototype items,
    /// Rune Conduit engine, Elemental Synergy system, 1v1 Combat loop, and post-battle Reward selection flow.
    /// [DEVELOPMENT / PROTOTYPE ENTRY POINT]
    /// </summary>
    public class GridInteractionBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GridView gridView;
        [SerializeField] private ItemDragController dragController;
        [SerializeField] private RuneConduitDebugView conduitDebugView;
        [SerializeField] private SynergySystem synergySystem;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private CombatEncounterUI combatEncounterUI;
        [SerializeField] private Transform stagingAreaParent;

        [Header("Staging Layout")]
        [SerializeField] private Vector3 stagingOrigin = new Vector3(-2.2f, -4f, 0f);
        [SerializeField] private float itemSpacing = 1.1f;

        [Header("Item Catalogue (TASK-005 Prototype Items)")]
        [SerializeField] private List<ItemDataSO> prototypeItemCatalogue = new List<ItemDataSO>();

        [Header("Development Runes & Targets (TASK-004 & TASK-006 Demo)")]
        [SerializeField] private bool enableConduitDemo = true;

        private LatticeGrid _grid;
        private PlayerCombatant _playerCombatant;
        private EnemyCombatant _enemyCombatant;
        private readonly List<ItemInstance> _spawnedItemInstances = new List<ItemInstance>();
        private readonly List<(RuneData rune, Vector2Int origin, ConduitDirection dir, int range)> _activeRunesWithData = new List<(RuneData, Vector2Int, ConduitDirection, int)>();
        private readonly List<ConduitTarget> _activeTargets = new List<ConduitTarget>();

        public LatticeGrid Grid => _grid;
        public GridView View => gridView;
        public SynergySystem Synergy => synergySystem;
        public CombatSystem Combat => combatSystem;
        public RewardService Rewards => rewardService;
        public CombatEncounterUI EncounterUI => combatEncounterUI;
        public PlayerCombatant Player => _playerCombatant;
        public EnemyCombatant Enemy => _enemyCombatant;
        public IReadOnlyList<ItemInstance> SpawnedItems => _spawnedItemInstances;

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

            // 6. Spawn Prototype Data-Driven Items
            SpawnPrototypeCatalogue();

            // 7. Setup Development Runes
            if (enableConduitDemo)
            {
                SetupDevelopmentRunesAndTargets();
            }

            // 8. Setup Combat Entities & Encounter UI (TASK-007 & TASK-008)
            SetupCombatAndRewardEncounter();

            // 9. Initial recalculation of conduits, synergies, and player stats
            RecalculateAndRenderConduits();

            // Hook into item placement/removal events to dynamically recalculate conduits, synergies, and combat stats
            _grid.OnItemPlaced += (id, origin, size) => RecalculateAndRenderConduits();
            _grid.OnItemRemoved += (id, origin, size) => RecalculateAndRenderConduits();
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

            // Combat System coordinator
            if (combatSystem == null)
            {
                GameObject combatSystemObj = new GameObject("CombatSystem");
                combatSystemObj.transform.SetParent(transform);
                combatSystem = combatSystemObj.AddComponent<CombatSystem>();
            }
            combatSystem.Initialize(_playerCombatant, _enemyCombatant);

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

            // Register newly rewarded items into active spawned item tracking
            rewardService.OnRewardApplied += (option, instance) =>
            {
                if (instance != null && !_spawnedItemInstances.Contains(instance))
                {
                    _spawnedItemInstances.Add(instance);
                }
            };
        }

        private void SpawnPrototypeCatalogue()
        {
            if (stagingAreaParent == null)
            {
                GameObject stagingObj = new GameObject("StagingArea");
                stagingObj.transform.SetParent(transform);
                stagingAreaParent = stagingObj.transform;
            }

            if (prototypeItemCatalogue == null || prototypeItemCatalogue.Count == 0)
            {
                BuildDefaultItemDefinitions();
            }

            for (int i = 0; i < prototypeItemCatalogue.Count; i++)
            {
                ItemDataSO data = prototypeItemCatalogue[i];
                if (data == null) continue;

                Vector3 spawnPos = stagingOrigin + new Vector3(i * itemSpacing, 0f, 0f);
                ItemInstance instance = ItemFactory.CreateInstance(data, spawnPos, stagingAreaParent);
                if (instance != null)
                {
                    _spawnedItemInstances.Add(instance);
                }
            }
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
            // Demo Fire Rune: Position (2,1) emitting North with range 3
            RuneData fireRune = ScriptableObject.CreateInstance<RuneData>();
            fireRune.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, 3);
            _activeRunesWithData.Add((fireRune, new Vector2Int(2, 1), ConduitDirection.North, 3));

            // Demo Target Receptor at (2,4)
            GameObject targetObj = new GameObject("Target_2_4");
            targetObj.transform.SetParent(transform);
            ConduitTarget target = targetObj.AddComponent<ConduitTarget>();
            target.Initialize("target_dummy_boss", new Vector2Int(2, 4));
            _activeTargets.Add(target);
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

            var runeSpecs = new List<(Vector2Int, ConduitDirection, int)>();
            var activeConduitData = new List<(RuneData, Vector2Int, RuneConduitResult)>();

            for (int i = 0; i < _activeRunesWithData.Count; i++)
            {
                var (rune, origin, dir, range) = _activeRunesWithData[i];
                RuneConduitResult result = RuneConduitEngine.CalculateConduit(_grid, origin, dir, range, IsTarget, stopOnTarget: false);
                activeConduitData.Add((rune, origin, result));
                runeSpecs.Add((origin, dir, range));
            }

            List<RuneConduitResult> results = new List<RuneConduitResult>();
            foreach (var item in activeConduitData) results.Add(item.Item3);

            conduitDebugView.RenderConduits(results);

            // Update Synergies
            if (synergySystem != null)
            {
                synergySystem.UpdateSynergies(activeConduitData, _spawnedItemInstances);
            }

            // Update Player Combat Stats from the Grid build
            if (_playerCombatant != null)
            {
                _playerCombatant.UpdateStatsFromBuild(_spawnedItemInstances);
            }
        }
    }
}
