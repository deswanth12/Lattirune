using System;
using UnityEngine;

namespace Lattirune.Inventory
{
    /// <summary>
    /// Discrete cell in the spatial inventory grid.
    /// Tracks lock status and spatial item occupant identity.
    /// </summary>
    [Serializable]
    public class InventoryCell
    {
        [SerializeField] private Vector2Int coordinate;
        [SerializeField] private bool isLocked = true;
        [SerializeField] private string occupantItemId = null;

        public Vector2Int Coordinate => coordinate;
        public bool IsLocked => isLocked;
        public bool IsOccupied => !string.IsNullOrEmpty(occupantItemId);
        public string OccupantItemId => occupantItemId;

        public InventoryCell(int x, int y, bool locked = true)
        {
            coordinate = new Vector2Int(x, y);
            isLocked = locked;
            occupantItemId = null;
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
        }

        public void SetOccupant(string itemId)
        {
            occupantItemId = itemId;
        }

        public void ClearOccupant()
        {
            occupantItemId = null;
        }
    }
}
