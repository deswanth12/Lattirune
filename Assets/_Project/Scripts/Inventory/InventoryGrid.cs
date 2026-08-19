using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Inventory
{
    /// <summary>
    /// Discrete 2D spatial grid dedicated to bag inventory storage and procedural expansion.
    /// Operates completely independently from the 5x5 combat LatticeGrid.
    /// </summary>
    public class InventoryGrid
    {
        private readonly int _width;
        private readonly int _height;
        private readonly InventoryCell[,] _cells;

        public int Width => _width;
        public int Height => _height;
        public int TotalCellCount => _width * _height;

        public int UnlockedCellCount
        {
            get
            {
                int count = 0;
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (!_cells[x, y].IsLocked) count++;
                    }
                }
                return count;
            }
        }

        public InventoryGrid(int width = 4, int height = 4, IEnumerable<Vector2Int> initialUnlocked = null)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _cells = new InventoryCell[_width, _height];

            // Initialize all cells as locked initially
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y] = new InventoryCell(x, y, locked: true);
                }
            }

            // Unlock initial configured set
            if (initialUnlocked != null)
            {
                foreach (var coord in initialUnlocked)
                {
                    UnlockCell(coord.x, coord.y);
                }
            }
            else
            {
                // Default 2x3 initial bag space (6 cells)
                for (int x = 0; x < Mathf.Min(3, _width); x++)
                {
                    for (int y = 0; y < Mathf.Min(2, _height); y++)
                    {
                        UnlockCell(x, y);
                    }
                }
            }
        }

        public bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public bool IsValidCoordinate(Vector2Int coord)
        {
            return IsValidCoordinate(coord.x, coord.y);
        }

        public InventoryCell GetCell(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return null;
            return _cells[x, y];
        }

        public bool IsCellLocked(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return true;
            return _cells[x, y].IsLocked;
        }

        public bool IsCellOccupied(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            return _cells[x, y].IsOccupied;
        }

        public bool UnlockCell(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            if (!_cells[x, y].IsLocked) return false; // Already unlocked

            _cells[x, y].SetLocked(false);
            return true;
        }

        public int ExpandCapacity(int count = 1)
        {
            int expanded = 0;
            for (int y = 0; y < _height && expanded < count; y++)
            {
                for (int x = 0; x < _width && expanded < count; x++)
                {
                    if (_cells[x, y].IsLocked)
                    {
                        if (UnlockCell(x, y)) expanded++;
                    }
                }
            }
            return UnlockedCellCount;
        }

        public bool LockCell(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;
            if (_cells[x, y].IsLocked) return false;

            _cells[x, y].SetLocked(true);
            return true;
        }

        public bool CanPlaceItem(Vector2Int origin, Vector2Int dimensions)
        {
            if (dimensions.x <= 0 || dimensions.y <= 0) return false;

            for (int dx = 0; dx < dimensions.x; dx++)
            {
                for (int dy = 0; dy < dimensions.y; dy++)
                {
                    int tx = origin.x + dx;
                    int ty = origin.y + dy;

                    if (!IsValidCoordinate(tx, ty)) return false;

                    InventoryCell cell = _cells[tx, ty];
                    if (cell.IsLocked || cell.IsOccupied) return false;
                }
            }

            return true;
        }

        public bool PlaceItem(string itemId, Vector2Int origin, Vector2Int dimensions)
        {
            if (string.IsNullOrEmpty(itemId) || !CanPlaceItem(origin, dimensions))
            {
                return false;
            }

            for (int dx = 0; dx < dimensions.x; dx++)
            {
                for (int dy = 0; dy < dimensions.y; dy++)
                {
                    _cells[origin.x + dx, origin.y + dy].SetOccupant(itemId);
                }
            }

            return true;
        }

        public bool RemoveItem(string itemId, Vector2Int origin, Vector2Int dimensions)
        {
            if (string.IsNullOrEmpty(itemId) || dimensions.x <= 0 || dimensions.y <= 0) return false;

            bool removedAny = false;
            for (int dx = 0; dx < dimensions.x; dx++)
            {
                for (int dy = 0; dy < dimensions.y; dy++)
                {
                    int tx = origin.x + dx;
                    int ty = origin.y + dy;

                    if (IsValidCoordinate(tx, ty))
                    {
                        InventoryCell cell = _cells[tx, ty];
                        if (cell.OccupantItemId == itemId)
                        {
                            cell.ClearOccupant();
                            removedAny = true;
                        }
                    }
                }
            }

            return removedAny;
        }

        public List<Vector2Int> GetUnlockedCoordinates()
        {
            List<Vector2Int> unlocked = new List<Vector2Int>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (!_cells[x, y].IsLocked)
                    {
                        unlocked.Add(new Vector2Int(x, y));
                    }
                }
            }
            return unlocked;
        }
    }
}
