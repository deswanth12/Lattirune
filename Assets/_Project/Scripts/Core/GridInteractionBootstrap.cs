using UnityEngine;
using Lattirune.Grid;
using Lattirune.Items;

namespace Lattirune.Core
{
    /// <summary>
    /// Bootstraps the physical 5x5 LatticeGrid interaction prototype.
    /// Instantiates grid visualization, drag controller, and initial test items.
    /// [DEVELOPMENT / PROTOTYPE ENTRY POINT]
    /// </summary>
    public class GridInteractionBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private GridView gridView;
        [SerializeField] private ItemDragController dragController;
        [SerializeField] private Transform stagingAreaParent;

        [Header("Staging Layout")]
        [SerializeField] private Vector3 stagingOrigin = new Vector3(-2f, -4f, 0f);
        [SerializeField] private float itemSpacing = 1.5f;

        private LatticeGrid _grid;

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

            // 4. Spawn Test Items for Interactive Footprint Testing
            SpawnTestItems();
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
