using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Grid
{
    /// <summary>
    /// Represents a single discrete cell in the 5x5 Lattice Grid.
    /// </summary>
    public class GridCell
    {
        public Vector2Int Position { get; private set; }
        public TileState State { get; private set; }
        public string OccupyingItemId { get; private set; }

        public GridCell(int x, int y, TileState initialState = TileState.Active)
        {
            Position = new Vector2Int(x, y);
            State = initialState;
            OccupyingItemId = null;
        }

        public bool IsAvailable()
        {
            return State == TileState.Active;
        }

        public bool IsOccupied()
        {
            return State == TileState.Occupied;
        }

        public bool IsLocked()
        {
            return State == TileState.Locked;
        }

        public bool Occupy(string itemId)
        {
            if (State != TileState.Active)
            {
                return false;
            }

            State = TileState.Occupied;
            OccupyingItemId = itemId;
            return true;
        }

        public void Clear()
        {
            if (State == TileState.Occupied)
            {
                State = TileState.Active;
                OccupyingItemId = null;
            }
        }

        public void Unlock()
        {
            if (State == TileState.Locked)
            {
                State = TileState.Active;
            }
        }

        public void Lock()
        {
            State = TileState.Locked;
            OccupyingItemId = null;
        }
    }
}
