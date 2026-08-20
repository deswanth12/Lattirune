using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Items
{
    /// <summary>
    /// Orchestrates pointer drag-and-drop interactions, 90-degree rotations, grid coordinate validation,
    /// dynamic snapping, and placement/removal delegation with the LatticeGrid for ItemInstances.
    /// </summary>
    public class ItemDragController : MonoBehaviour
    {
        [SerializeField] private GridView gridView;
        [SerializeField] private bool enableDebugLogs = true;

        private LatticeGrid _grid;
        private ItemInstance _activeDraggedItem;
        private Vector3 _dragOffset;
        private bool _wasPlacedBeforeDrag;
        private Vector2Int _previousGridPos;
        private int _previousRotation;
        private float _lastTapTime;
        private string _lastTappedItemId;

        public ItemInstance ActiveDraggedItem => _activeDraggedItem;

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

        private void Update()
        {
            // Optional PC test shortcut: 'R' key rotates currently dragged item
            if (_activeDraggedItem != null && Input.GetKeyDown(KeyCode.R))
            {
                RotateActiveItem();
            }
        }

        [SerializeField] private UI.ScreenNavigationController navigation;

        public void BindNavigation(UI.ScreenNavigationController nav)
        {
            navigation = nav;
        }

        private void OnGUI()
        {
            if (navigation == null || (navigation.CurrentScreen != UI.ScreenState.GRID_BUILD && navigation.CurrentScreen != UI.ScreenState.COMBAT)) return;
            if (_activeDraggedItem == null) return;

            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 24;
            btnStyle.fontStyle = FontStyle.Bold;

            float offsetY = (Screen.height / scale - 1920f) * 0.5f;
            Color oldColor = GUI.color;
            GUI.color = Color.cyan;
            if (GUI.Button(new Rect(540f - 180f, 1550f + offsetY, 360f, 75f), "[?] ROTATE (90°)", btnStyle))
            {
                RotateActiveItem();
            }
            GUI.color = oldColor;

            GUI.matrix = oldMatrix;
        }

        public bool RotateActiveItem()
        {
            if (_activeDraggedItem == null) return false;

            bool rotated = _activeDraggedItem.Rotate90();
            if (rotated && gridView != null && _grid != null)
            {
                // Refresh grid highlight with new rotated dimensions
                bool inBounds = GridCoordinateUtility.WorldToGridCoordinate(
                    _activeDraggedItem.transform.position,
                    gridView.GridOrigin,
                    out Vector2Int anchorCoord,
                    gridView.CellSize,
                    gridView.CellSpacing
                );

                bool isValidPlacement = inBounds && _grid.CanPlaceItem(anchorCoord, _activeDraggedItem.CurrentDimensions);

                if (inBounds)
                {
                    gridView.SetFootprintHighlight(anchorCoord, _activeDraggedItem.CurrentDimensions, isValidPlacement);
                }
                else
                {
                    gridView.ClearHighlight();
                }

                _activeDraggedItem.SetVisualState(isDragging: true, isValidDrop: isValidPlacement);
            }

            return rotated;
        }

        private void HandlePointerDown(Vector2 screenPos, Vector3 worldPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                ItemInstance item = hit.collider.GetComponent<ItemInstance>();
                if (item != null)
                {
                    // Double-tap in-place rotation detection (< 0.35s)
                    float now = Time.time;
                    if (now - _lastTapTime < 0.35f && _lastTappedItemId == item.InstanceId)
                    {
                        item.RotateClockwise();
                    }
                    _lastTapTime = now;
                    _lastTappedItemId = item.InstanceId;

                    StartDraggingItem(item, worldPos);
                }
            }
        }

        public void StartDraggingItem(ItemInstance item, Vector3 pointerWorldPos)
        {
            _activeDraggedItem = item;
            _dragOffset = item.transform.position - pointerWorldPos;
            _wasPlacedBeforeDrag = item.IsPlacedOnGrid;
            _previousGridPos = item.CurrentGridPosition;
            _previousRotation = item.CurrentRotationDegrees;

            // If the item was placed, remove its occupancy temporarily while dragging
            if (_wasPlacedBeforeDrag)
            {
                _grid.RemoveItem(item.InstanceId, _previousGridPos, item.CurrentDimensions);
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

            bool isValidPlacement = inBounds && _grid.CanPlaceItem(anchorCoord, _activeDraggedItem.CurrentDimensions);

            if (inBounds)
            {
                gridView.SetFootprintHighlight(anchorCoord, _activeDraggedItem.CurrentDimensions, isValidPlacement);
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

            bool isValidPlacement = inBounds && _grid.CanPlaceItem(anchorCoord, _activeDraggedItem.CurrentDimensions);

            if (isValidPlacement)
            {
                // Place item in LatticeGrid using its unique InstanceId
                bool placed = _grid.PlaceItem(_activeDraggedItem.InstanceId, anchorCoord, _activeDraggedItem.CurrentDimensions);
                if (placed)
                {
                    Vector3 snapPos = GridCoordinateUtility.GetFootprintWorldCenter(
                        anchorCoord, 
                        _activeDraggedItem.CurrentDimensions, 
                        gridView.GridOrigin, 
                        gridView.CellSize, 
                        gridView.CellSpacing
                    );

                    _activeDraggedItem.OnPlaced(anchorCoord, snapPos);

                    if (enableDebugLogs)
                    {
                        Debug.Log($"[Lattirune] Item '{_activeDraggedItem.InstanceId}' ({_activeDraggedItem.CurrentDimensions.x}x{_activeDraggedItem.CurrentDimensions.y}) PLACED at ({anchorCoord.x},{anchorCoord.y}).");
                    }
                }
            }
            else
            {
                // Invalid drop -> return to previous valid position/rotation
                if (_wasPlacedBeforeDrag)
                {
                    _grid.PlaceItem(_activeDraggedItem.InstanceId, _previousGridPos, _activeDraggedItem.CurrentDimensions);
                    Vector3 restorePos = GridCoordinateUtility.GetFootprintWorldCenter(
                        _previousGridPos, 
                        _activeDraggedItem.CurrentDimensions, 
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
                    Debug.Log($"[Lattirune] Invalid placement for '{_activeDraggedItem.InstanceId}'. Item returned.");
                }
            }

            gridView.ClearHighlight();
            _activeDraggedItem = null;
        }

        /// <summary>
        /// Removes a placed item from the grid and returns it to staging.
        /// </summary>
        public void RemovePlacedItem(ItemInstance item, Vector3 returnStagingPos)
        {
            if (item == null || !item.IsPlacedOnGrid || _grid == null) return;

            bool removed = _grid.RemoveItem(item.InstanceId, item.CurrentGridPosition, item.CurrentDimensions);
            if (removed)
            {
                item.OnRemoved(returnStagingPos);
                if (enableDebugLogs)
                {
                    Debug.Log($"[Lattirune] Item '{item.InstanceId}' REMOVED from grid.");
                }
            }
        }
    }
}
