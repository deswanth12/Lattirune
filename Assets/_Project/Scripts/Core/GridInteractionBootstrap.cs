using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Core
{
    /// <summary>
    /// Bootstraps the physical 5x5 LatticeGrid interaction & Rune Conduit prototype.
    /// Instantiates grid visualization, drag controller, test items, development runes, and debug conduit lines.
    /// [DEVELOPMENT / PROTOTYPE ENTRY POINT]
    /// </summary>
    public class GridInteractionBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GridView gridView;
        [SerializeField] private ItemDragController dragController;
        [SerializeField] private RuneConduitDebugView conduitDebugView;
        [SerializeField] private Transform stagingAreaParent;

        [Header("Staging Layout")]
        [SerializeField] private Vector3 stagingOrigin = new Vector3(-2f, -4f, 0f);
        [SerializeField] private float itemSpacing = 1.5f;

        [Header("Development Runes & Targets (TASK-004 Demo)")]
        [SerializeField] private bool enableConduitDemo = true;

        private LatticeGrid _grid;
        private readonly List<(Vector2Int origin, ConduitDirection dir, int range)> _activeRunes = new List<(Vector2Int, ConduitDirection, int)>();
        private readonly List<ConduitTarget> _activeTargets = new List<ConduitTarget>();

        public LatticeGrid Grid => _grid;
        public GridView View => gridView;

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

            // 5. Spawn Test Items for Interactive Footprint Testing
            SpawnTestItems();

            // 6. Setup Development Rune Conduits Demo if enabled
            if (enableConduitDemo)
            {
                SetupDevelopmentRunesAndTargets();
                RecalculateAndRenderConduits();
            }

            // Hook into item placement/removal events to dynamically recalculate conduits
            _grid.OnItemPlaced += (id, origin, size) => RecalculateAndRenderConduits();
            _grid.OnItemRemoved += (id, origin, size) => RecalculateAndRenderConduits();
        }

        private void SetupDevelopmentRunesAndTargets()
        {
            // Demo Rune A: Position (2,1) emitting North with range 3
            _activeRunes.Add((new Vector2Int(2, 1), ConduitDirection.North, 3));

            // Demo Rune B: Position (3,3) emitting West with range 3
            _activeRunes.Add((new Vector2Int(3, 3), ConduitDirection.West, 3));

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

            // Target detection predicate
            bool IsTarget(Vector2Int coord)
            {
                foreach (var t in _activeTargets)
                {
                    if (t != null && t.GridPosition == coord) return true;
                }
                return false;
            }

            List<RuneConduitResult> results = RuneConduitEngine.CalculateMultipleConduits(_grid, _activeRunes, IsTarget);
            conduitDebugView.RenderConduits(results);
        }

        private void SpawnTestItems()
        {
            if (stagingAreaParent == null)
            {
                GameObject stagingObj = new GameObject("StagingArea");
                stagingObj.transform.SetParent(transform);
                stagingAreaParent = stagingObj.transform;
            }

            // Item 1: 1x1 Dagger (Amber)
            CreateTestItem("item_1x1_dagger", new Vector2Int(1, 1), new Color(0.9f, 0.5f, 0.1f), stagingOrigin);

            // Item 2: 1x2 Sword (Copper)
            Vector3 pos2 = stagingOrigin + new Vector3(itemSpacing * 1.0f, 0f, 0f);
            CreateTestItem("item_1x2_sword", new Vector2Int(1, 2), new Color(0.7f, 0.4f, 0.2f), pos2);

            // Item 3: 2x1 Bow (Emerald)
            Vector3 pos3 = stagingOrigin + new Vector3(itemSpacing * 2.2f, 0f, 0f);
            CreateTestItem("item_2x1_bow", new Vector2Int(2, 1), new Color(0.2f, 0.7f, 0.4f), pos3);

            // Item 4: 2x2 Shield (Cobalt)
            Vector3 pos4 = stagingOrigin + new Vector3(itemSpacing * 3.8f, 0f, 0f);
            CreateTestItem("item_2x2_shield", new Vector2Int(2, 2), new Color(0.2f, 0.4f, 0.8f), pos4);
        }

        private TestItem CreateTestItem(string id, Vector2Int dims, Color color, Vector3 position)
        {
            GameObject obj = new GameObject(id);
            obj.transform.SetParent(stagingAreaParent);
            obj.transform.position = position;

            TestItem item = obj.AddComponent<TestItem>();
            item.Initialize(id, dims, color, position);
            return item;
        }
    }
}
