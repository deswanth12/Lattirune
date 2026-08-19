using System;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Grid
{
    /// <summary>
    /// Core 2D spatial data structure representing the 5x5 Lattice inventory.
    /// Manages coordinate validation, tile states, footprint placement, and occupancy queries.
    /// </summary>
    public class LatticeGrid
    {
        public const int WIDTH = 5;
        public const int HEIGHT = 5;
        public const int GRID_WIDTH = WIDTH;
        public const int GRID_HEIGHT = HEIGHT;
        public const int TOTAL_CELLS = WIDTH * HEIGHT; // 25

        private readonly GridCell[,] _cells = new GridCell[WIDTH, HEIGHT];

        public int ActiveCellCount
        {
            get
            {
                int count = 0;
                for (int x = 0; x < WIDTH; x++)
                {
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        if (_cells[x, y] != null && _cells[x, y].State != TileState.Locked)
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
        }

        public int LockedCellCount => TOTAL_CELLS - ActiveCellCount;

        public int GetActiveCellCount() => ActiveCellCount;
        public int GetLockedCellCount() => LockedCellCount;

        public event Action<Vector2Int, TileState> OnCellStateChanged;
        public event Action<string, Vector2Int, Vector2Int> OnItemPlaced;
        public event Action<string, Vector2Int, Vector2Int> OnItemRemoved;

        public LatticeGrid(bool initializeDefaultLayout = true)
        {
            Initialize(initializeDefaultLayout);
        }

        /// <summary>
        /// Initializes the 5x5 grid.
        /// Default configuration sets 17 active diamond-square tiles and 8 locked perimeter tiles.
        /// </summary>
        public void Initialize(bool useDefaultLayout = true)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    TileState initialState = TileState.Active;

                    if (useDefaultLayout && IsDefaultLockedTile(x, y))
                    {
                        initialState = TileState.Locked;
                    }

                    _cells[x, y] = new GridCell(x, y, initialState);
                }
            }
        }

        /// <summary>
        /// Defines the 8 locked perimeter tiles in the 5x5 diamond-square layout.
        /// </summary>
        public static bool IsDefaultLockedTile(int x, int y)
        {
            // 4 outer corner pairs:
            // (0,0), (4,0), (0,4), (4,4)
            // (0,1), (4,1), (0,3), (4,3)
            if ((x == 0 || x == 4) && (y == 0 || y == 1 || y == 3 || y == 4))
            {
                return true;
            }

            return false;
        }

        public bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
        }

        public bool IsValidCoordinate(Vector2Int coord)
        {
            return IsValidCoordinate(coord.x, coord.y);
        }

        public GridCell GetCell(int x, int y)
        {
            if (!IsValidCoordinate(x, y))
            {
                return null;
            }

            return _cells[x, y];
        }

        public GridCell GetCell(Vector2Int coord)
        {
            return GetCell(coord.x, coord.y);
        }

        /// <summary>
        /// Checks if a rectangular item footprint of (size.x, size.y) can be placed at origin (origin.x, origin.y).
        /// All footprint cells must be inside grid bounds, Active, and Unoccupied.
        /// </summary>
        public bool CanPlaceItem(Vector2Int origin, Vector2Int size)
        {
            if (size.x <= 0 || size.y <= 0)
            {
                return false;
            }

            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    int targetX = origin.x + dx;
                    int targetY = origin.y + dy;

                    if (!IsValidCoordinate(targetX, targetY))
                    {
                        return false;
                    }

                    GridCell cell = _cells[targetX, targetY];
                    if (!cell.IsAvailable())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Places an item on the grid across its footprint.
        /// Returns true if placement was successful, false if invalid.
        /// </summary>
        public bool PlaceItem(string itemId, Vector2Int origin, Vector2Int size)
        {
            if (string.IsNullOrEmpty(itemId) || !CanPlaceItem(origin, size))
            {
                return false;
            }

            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    int targetX = origin.x + dx;
                    int targetY = origin.y + dy;
                    _cells[targetX, targetY].Occupy(itemId);
                    OnCellStateChanged?.Invoke(new Vector2Int(targetX, targetY), TileState.Occupied);
                }
            }

            OnItemPlaced?.Invoke(itemId, origin, size);
            return true;
        }

        /// <summary>
        /// Removes an item footprint from the grid, freeing its occupied cells.
        /// </summary>
        public bool RemoveItem(string itemId, Vector2Int origin, Vector2Int size)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            bool anyRemoved = false;

            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    int targetX = origin.x + dx;
                    int targetY = origin.y + dy;

                    if (IsValidCoordinate(targetX, targetY))
                    {
                        GridCell cell = _cells[targetX, targetY];
                        if (cell.OccupyingItemId == itemId)
                        {
                            cell.Clear();
                            OnCellStateChanged?.Invoke(new Vector2Int(targetX, targetY), TileState.Active);
                            anyRemoved = true;
                        }
                    }
                }
            }

            if (anyRemoved)
            {
                OnItemRemoved?.Invoke(itemId, origin, size);
            }

            return anyRemoved;
        }

        /// <summary>
        /// Unlocks a locked tile, making it available for item placement.
        /// </summary>
        public bool UnlockTile(int x, int y)
        {
            if (!IsValidCoordinate(x, y))
            {
                return false;
            }

            GridCell cell = _cells[x, y];
            if (cell.IsLocked())
            {
                cell.Unlock();
                OnCellStateChanged?.Invoke(new Vector2Int(x, y), TileState.Active);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unlocks the first found locked tile in the grid.
        /// </summary>
        public bool UnlockFirstAvailableLockedSlot()
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (_cells[x, y].IsLocked())
                    {
                        return UnlockTile(x, y);
                    }
                }
            }
            return false;
        }

        public int GetActiveCount()
        {
            int count = 0;
            foreach (var cell in _cells)
            {
                if (cell.IsAvailable()) count++;
            }
            return count;
        }

        public int GetLockedCount()
        {
            int count = 0;
            foreach (var cell in _cells)
            {
                if (cell.IsLocked()) count++;
            }
            return count;
        }

        public int GetOccupiedCount()
        {
            int count = 0;
            foreach (var cell in _cells)
            {
                if (cell.IsOccupied()) count++;
            }
            return count;
        }
    }
}
