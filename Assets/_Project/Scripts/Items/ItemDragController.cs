using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Items
{
    /// <summary>
    /// Orchestrates pointer drag-and-drop interactions, grid coordinate validation,
    /// dynamic snapping, and placement/removal delegation with the LatticeGrid.
    /// </summary>
    public class ItemDragController : MonoBehaviour
    {
        [SerializeField] private GridView gridView;
        [SerializeField] private bool enableDebugLogs = true;

        private LatticeGrid _grid;
        private TestItem _activeDraggedItem;
        private Vector3 _dragOffset;
        private bool _wasPlacedBeforeDrag;
        private Vector2Int _previousGridPos;

        public void Initialize(LatticeGrid grid, GridView view)
        {
            _grid = grid;
            gridView = view;

            if (TouchController.Instance != null)
            {
                TouchController.Instance.OnPointerDown += HandlePointerDown;
                TouchController.Instance.OnPointerDrag += HandlePointerDrag;
                TouchController.Instance.OnPointerUp += HandlePointerUp;
            }
        }

        private void OnDestroy()
        {
            if (TouchController.Instance != null)
            {
                TouchController.Instance.OnPointerDown -= HandlePointerDown;
                TouchController.Instance.OnPointerDrag -= HandlePointerDrag;
                TouchController.Instance.OnPointerUp -= HandlePointerUp;
            }
        }

        private void HandlePointerDown(Vector2 screenPos, Vector3 worldPos)
        {
            // Raycast in 2D to find if a TestItem was selected
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                TestItem item = hit.collider.GetComponent<TestItem>();
                if (item != null)
                {
                    StartDraggingItem(item, worldPos);
                }
            }
        }

        public void StartDraggingItem(TestItem item, Vector3 pointerWorldPos)
        {
            _activeDraggedItem = item;
            _dragOffset = item.transform.position - pointerWorldPos;
            _wasPlacedBeforeDrag = item.IsPlacedOnGrid;
            _previousGridPos = item.CurrentGridPosition;

            // If the item was placed, remove its occupancy temporarily while dragging
            if (_wasPlacedBeforeDrag)
            {
                _grid.RemoveItem(item.ItemId, _previousGridPos, item.Dimensions);
            }

            _activeDraggedItem.SetVisualState(isDragging: true, isValidDrop: true);
        }

        private void HandlePointerDrag(Vector2 screenPos, Vector3 worldPos)
        {
            if (_activeDraggedItem == null || gridView == null || _grid == null) return;

            Vector3 newPos = worldPos + _dragOffset;
            _activeDraggedItem.transform.position = newPos;

            // Calculate anchor grid coordinate
            bool inBounds = GridCoordinateUtility.WorldToGridCoordinate(
                newPos, 
                gridView.GridOrigin, 
                out Vector2Int anchorCoord, 
                gridView.CellSize, 
                gridView.CellSpacing
            );

            bool isValidPlacement = inBounds && _grid.CanPlaceItem(anchorCoord, _activeDraggedItem.Dimensions);

            if (inBounds)
            {
                gridView.SetFootprintHighlight(anchorCoord, _activeDraggedItem.Dimensions, isValidPlacement);
            }
            else
            {
                gridView.ClearHighlight();
            }

            _activeDraggedItem.SetVisualState(isDragging: true, isValidDrop: isValidPlacement);
        }

        private void HandlePointerUp(Vector2 screenPos, Vector3 worldPos)
        {
            if (_activeDraggedItem == null || gridView == null || _grid == null) return;

            Vector3 dropPosition = _activeDraggedItem.transform.position;
            bool inBounds = GridCoordinateUtility.WorldToGridCoordinate(
                dropPosition, 
                gridView.GridOrigin, 
                out Vector2Int anchorCoord, 
                gridView.CellSize, 
                gridView.CellSpacing
            );

            bool isValidPlacement = inBounds && _grid.CanPlaceItem(anchorCoord, _activeDraggedItem.Dimensions);

            if (isValidPlacement)
            {
                // Place item in LatticeGrid
                bool placed = _grid.PlaceItem(_activeDraggedItem.ItemId, anchorCoord, _activeDraggedItem.Dimensions);
                if (placed)
                {
                    Vector3 snapPos = GridCoordinateUtility.GetFootprintWorldCenter(
                        anchorCoord, 
                        _activeDraggedItem.Dimensions, 
                        gridView.GridOrigin, 
                        gridView.CellSize, 
                        gridView.CellSpacing
                    );

                    _activeDraggedItem.OnPlaced(anchorCoord, snapPos);

                    if (enableDebugLogs)
                    {
                        Debug.Log($"[Lattirune] Item '{_activeDraggedItem.ItemId}' ({_activeDraggedItem.Dimensions.x}x{_activeDraggedItem.Dimensions.y}) PLACED at ({anchorCoord.x},{anchorCoord.y}).");
                    }
                }
            }
            else
            {
                // Invalid drop -> return to previous valid position
                if (_wasPlacedBeforeDrag)
                {
                    // Restore previous placement
                    _grid.PlaceItem(_activeDraggedItem.ItemId, _previousGridPos, _activeDraggedItem.Dimensions);
                    Vector3 restorePos = GridCoordinateUtility.GetFootprintWorldCenter(
                        _previousGridPos, 
                        _activeDraggedItem.Dimensions, 
                        gridView.GridOrigin, 
                        gridView.CellSize, 
                        gridView.CellSpacing
                    );
                    _activeDraggedItem.OnPlaced(_previousGridPos, restorePos);
                }
                else
                {
                    _activeDraggedItem.ReturnToOriginalPosition();
                }

                if (enableDebugLogs)
                {
                    Debug.Log($"[Lattirune] Invalid placement at ({anchorCoord.x},{anchorCoord.y}). Item returned.");
                }
            }

            gridView.ClearHighlight();
            _activeDraggedItem = null;
        }

        /// <summary>
        /// Development-only method to remove an item from the grid and return it to staging.
        /// </summary>
        public void RemovePlacedItem(TestItem item, Vector3 returnStagingPos)
        {
            if (item == null || !item.IsPlacedOnGrid || _grid == null) return;

            bool removed = _grid.RemoveItem(item.ItemId, item.CurrentGridPosition, item.Dimensions);
            if (removed)
            {
                item.OnRemoved(returnStagingPos);
                if (enableDebugLogs)
                {
                    Debug.Log($"[Lattirune] Item '{item.ItemId}' REMOVED from grid.");
                }
            }
        }
    }
}
