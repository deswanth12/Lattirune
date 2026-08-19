using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Items;

namespace Lattirune.Inventory
{
    /// <summary>
    /// Master runtime controller for the player's spatial bag inventory and procedural expansion.
    /// Operates completely independently from the combat LatticeGrid.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryDefinitionSO inventoryDefinition;

        private InventoryGrid _grid;
        private readonly List<ItemInstance> _storedItems = new List<ItemInstance>();
        private int _expansionStep = 0;

        public event Action<ItemInstance, Vector2Int> OnItemAdded;
        public event Action<ItemInstance> OnItemRemoved;
        public event Action<ItemInstance, Vector2Int> OnItemMoved;
        public event Action<Vector2Int> OnInventoryExpanded;
        public event Action OnInventoryChanged;

        public InventoryGrid Grid => _grid;
        public IReadOnlyList<ItemInstance> StoredItems => _storedItems;
        public int StoredItemCount => _storedItems.Count;
        public int Capacity => _grid != null ? _grid.UnlockedCellCount : 0;
        public int TotalCapacity => _grid != null ? _grid.TotalCellCount : 0;
        public int UnlockedCount => _grid != null ? _grid.UnlockedCellCount : 0;
        public int LockedCount => _grid != null ? (_grid.TotalCellCount - _grid.UnlockedCellCount) : 0;
        public int ExpansionStep => _expansionStep;
        public bool CanExpand => inventoryDefinition != null && _expansionStep < inventoryDefinition.MaxExpansionCount;

        private void Awake()
        {
            EnsureDefaultDefinition();
        }

        public void EnsureDefaultDefinition()
        {
            if (inventoryDefinition == null)
            {
                inventoryDefinition = InventoryDefinitionSO.CreateDefaultDefinition();
            }
        }

        public void Initialize(InventoryDefinitionSO def = null)
        {
            if (def != null)
            {
                inventoryDefinition = def;
            }
            EnsureDefaultDefinition();

            _grid = new InventoryGrid(
                inventoryDefinition.Width,
                inventoryDefinition.Height,
                inventoryDefinition.InitialUnlockedCells
            );

            _storedItems.Clear();
            _expansionStep = 0;
        }

        public bool CanPlaceItem(Vector2Int origin, Vector2Int dimensions)
        {
            if (_grid == null) return false;
            return _grid.CanPlaceItem(origin, dimensions);
        }

        public bool AddItem(ItemInstance item, Vector2Int? preferredPosition = null)
        {
            if (item == null || _grid == null) return false;

            Vector2Int dims = item.CurrentDimensions;

            if (preferredPosition.HasValue)
            {
                Vector2Int pos = preferredPosition.Value;
                if (_grid.CanPlaceItem(pos, dims))
                {
                    if (_grid.PlaceItem(item.InstanceId, pos, dims))
                    {
                        item.OnPlaced(pos, Vector3.zero);
                        _storedItems.Add(item);
                        OnItemAdded?.Invoke(item, pos);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
                return false;
            }

            // Auto-find first available footprint
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    Vector2Int candidate = new Vector2Int(x, y);
                    if (_grid.CanPlaceItem(candidate, dims))
                    {
                        if (_grid.PlaceItem(item.InstanceId, candidate, dims))
                        {
                            item.OnPlaced(candidate, Vector3.zero);
                            _storedItems.Add(item);
                            OnItemAdded?.Invoke(item, candidate);
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            return false; // Inventory full / no suitable footprint
        }

        public bool RemoveItem(ItemInstance item)
        {
            if (item == null || _grid == null || !_storedItems.Contains(item)) return false;

            bool removed = _grid.RemoveItem(item.InstanceId, item.GridPosition, item.CurrentDimensions);
            if (removed)
            {
                item.OnPickedUp();
                _storedItems.Remove(item);
                OnItemRemoved?.Invoke(item);
                OnInventoryChanged?.Invoke();
            }
            return removed;
        }

        public bool MoveItem(ItemInstance item, Vector2Int newPosition)
        {
            if (item == null || _grid == null || !_storedItems.Contains(item)) return false;

            Vector2Int oldPos = item.GridPosition;
            Vector2Int dims = item.CurrentDimensions;

            // Clear old placement
            _grid.RemoveItem(item.InstanceId, oldPos, dims);

            if (_grid.CanPlaceItem(newPosition, dims))
            {
                _grid.PlaceItem(item.InstanceId, newPosition, dims);
                item.OnPlaced(newPosition, Vector3.zero);
                OnItemMoved?.Invoke(item, newPosition);
                OnInventoryChanged?.Invoke();
                return true;
            }

            // Rollback old placement
            _grid.PlaceItem(item.InstanceId, oldPos, dims);
            return false;
        }

        public bool ExpandBag()
        {
            if (!CanExpand || _grid == null) return false;

            Vector2Int nextCoord = inventoryDefinition.ExpansionOrder[_expansionStep];
            if (_grid.UnlockCell(nextCoord.x, nextCoord.y))
            {
                _expansionStep++;
                OnInventoryExpanded?.Invoke(nextCoord);
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        public bool ExpandStorage()
        {
            return ExpandBag();
        }

        public void RestoreState(IEnumerable<Vector2Int> unlockedCoords, int expansionStep)
        {
            EnsureDefaultDefinition();
            _grid = new InventoryGrid(inventoryDefinition.Width, inventoryDefinition.Height, unlockedCoords);
            _expansionStep = expansionStep;
            OnInventoryChanged?.Invoke();
        }

        public void ClearInventory()
        {
            _storedItems.Clear();
            if (inventoryDefinition != null)
            {
                Initialize(inventoryDefinition);
            }
        }
    }
}
