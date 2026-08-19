using UnityEngine;

namespace Lattirune.Runes
{
    /// <summary>
    /// Development marker component for entities that act as conduit receptors/targets on the LatticeGrid.
    /// [DEVELOPMENT ONLY]
    /// </summary>
    public class ConduitTarget : MonoBehaviour
    {
        [SerializeField] private string targetId = "target_dummy_01";
        [SerializeField] private Vector2Int gridPosition = new Vector2Int(2, 4);

        public string TargetId => targetId;
        public Vector2Int GridPosition => gridPosition;

        public void Initialize(string id, Vector2Int position)
        {
            targetId = id;
            gridPosition = position;
        }

        public void SetGridPosition(Vector2Int position)
        {
            gridPosition = position;
        }
    }
}
