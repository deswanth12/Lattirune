using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Inventory
{
    /// <summary>
    /// Static ScriptableObject defining initial bag dimensions, starting unlocked cells,
    /// and deterministic spatial expansion sequence.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryDefinition_", menuName = "Lattirune/Inventory/Inventory Definition")]
    public class InventoryDefinitionSO : ScriptableObject
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int width = 4;
        [SerializeField] private int height = 4;

        [Header("Starting Unlocked Cells")]
        [SerializeField] private List<Vector2Int> initialUnlockedCells = new List<Vector2Int>();

        [Header("Deterministic Expansion Order")]
        [SerializeField] private List<Vector2Int> expansionOrder = new List<Vector2Int>();

        public int Width => width;
        public int Height => height;
        public IReadOnlyList<Vector2Int> InitialUnlockedCells => initialUnlockedCells;
        public IReadOnlyList<Vector2Int> ExpansionOrder => expansionOrder;
        public int MaxExpansionCount => expansionOrder != null ? expansionOrder.Count : 0;

        public void Initialize(
            int w,
            int h,
            List<Vector2Int> startingCells,
            List<Vector2Int> expansionSequence)
        {
            width = Mathf.Max(1, w);
            height = Mathf.Max(1, h);
            initialUnlockedCells = startingCells ?? new List<Vector2Int>();
            expansionOrder = expansionSequence ?? new List<Vector2Int>();
        }

        public bool IsValid(out string error)
        {
            if (width <= 0 || height <= 0)
            {
                error = "Inventory dimensions must be positive integers.";
                return false;
            }
            if (initialUnlockedCells == null || initialUnlockedCells.Count == 0)
            {
                error = "Initial unlocked cells cannot be empty.";
                return false;
            }

            error = null;
            return true;
        }

        public static InventoryDefinitionSO CreateDefaultDefinition()
        {
            InventoryDefinitionSO def = ScriptableObject.CreateInstance<InventoryDefinitionSO>();

            List<Vector2Int> initial = new List<Vector2Int>
            {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)
            };

            List<Vector2Int> expansions = new List<Vector2Int>
            {
                new Vector2Int(3, 0),
                new Vector2Int(3, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3),
                new Vector2Int(3, 3)
            };

            def.Initialize(4, 4, initial, expansions);
            return def;
        }
    }
}
