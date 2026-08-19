using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Items
{
    /// <summary>
    /// Runtime representation of an item in the game world / inventory.
    /// References immutable ItemDataSO while managing instance-specific position, rotation, synergy, and visual states.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ItemInstance : MonoBehaviour
    {
        [Header("Data Reference")]
        [SerializeField] private ItemDataSO itemData;

        [Header("Runtime Instance State")]
        [SerializeField] private string instanceId;
        [SerializeField] private int currentRotationDegrees = 0;
        [SerializeField] private bool isPlacedOnGrid = false;
        [SerializeField] private Vector2Int currentGridPosition = new Vector2Int(-1, -1);

        [Header("Synergy State (Runtime Only)")]
        [SerializeField] private string activeSynergyId = null;
        [SerializeField] private bool hasActiveSynergy = false;

        private Vector3 _originalPosition;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;

        public ItemDataSO Data => itemData;
        public string InstanceId => instanceId;
        public int CurrentRotationDegrees => currentRotationDegrees;
        public int CurrentRotationAngle => currentRotationDegrees;
        public bool IsPlacedOnGrid => isPlacedOnGrid;
        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector2Int GridPosition => currentGridPosition;
        public Vector3 OriginalPosition => _originalPosition;
        public string ActiveSynergyId => activeSynergyId;
        public bool HasActiveSynergy => hasActiveSynergy;

        public Vector2Int CurrentDimensions => itemData != null 
            ? ItemRotationUtility.GetRotatedDimensions(itemData.BaseDimensions, currentRotationDegrees)
            : Vector2Int.one;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _collider = GetComponent<BoxCollider2D>();
            _originalPosition = transform.position;
        }

        public void Initialize(ItemDataSO data, string id, Vector3 startPosition, int initialRotation = 0)
        {
            itemData = data;
            instanceId = id;
            transform.position = startPosition;
            _originalPosition = startPosition;
            currentRotationDegrees = ItemRotationUtility.NormalizeRotation(initialRotation);
            isPlacedOnGrid = false;
            currentGridPosition = new Vector2Int(-1, -1);
            activeSynergyId = null;
            hasActiveSynergy = false;

            UpdateVisual();
        }

        public bool Rotate90()
        {
            if (itemData == null || !itemData.RotationAllowed)
            {
                return false;
            }

            currentRotationDegrees = ItemRotationUtility.GetNextRotation(currentRotationDegrees);
            UpdateVisual();
            return true;
        }

        public bool RotateClockwise()
        {
            return Rotate90();
        }

        /// <summary>
        /// Sets the runtime synergy state and visual feedback without modifying ItemDataSO.
        /// </summary>
        public void SetSynergyState(string synergyId, Color? synergyColor = null)
        {
            activeSynergyId = synergyId;
            hasActiveSynergy = !string.IsNullOrEmpty(synergyId);

            if (_spriteRenderer != null)
            {
                if (hasActiveSynergy)
                {
                    // Flame / Elemental glow tint
                    _spriteRenderer.color = synergyColor ?? new Color(1f, 0.45f, 0.1f, 1f);
                }
                else
                {
                    _spriteRenderer.color = Color.white;
                }
            }
        }

        /// <summary>
        /// Checks if a discrete grid coordinate is covered by this item instance's occupied footprint.
        /// </summary>
        public bool ContainsGridCoordinate(Vector2Int coord)
        {
            if (!isPlacedOnGrid) return false;

            Vector2Int dims = CurrentDimensions;
            return coord.x >= currentGridPosition.x && coord.x < currentGridPosition.x + dims.x &&
                   coord.y >= currentGridPosition.y && coord.y < currentGridPosition.y + dims.y;
        }

        public void UpdateVisual()
        {
            if (itemData == null) return;

            Vector2Int dims = CurrentDimensions;
            float cellSize = GridCoordinateUtility.DEFAULT_CELL_SIZE;
            float cellSpacing = GridCoordinateUtility.DEFAULT_CELL_SPACING;

            float width = (dims.x * cellSize) + ((dims.x - 1) * cellSpacing);
            float height = (dims.y * cellSize) + ((dims.y - 1) * cellSpacing);

            Texture2D tex = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    bool isBorder = x <= 1 || x >= 30 || y <= 1 || y >= 30;
                    tex.SetPixel(x, y, isBorder ? Color.white : itemData.PlaceholderColor);
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            _spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            _spriteRenderer.color = hasActiveSynergy ? new Color(1f, 0.45f, 0.1f, 1f) : Color.white;
            _spriteRenderer.sortingOrder = 10;

            transform.localScale = new Vector3(width, height, 1f);

            if (_collider != null)
            {
                _collider.size = Vector2.one;
            }
        }

        public void SetVisualState(bool isDragging, bool isValidDrop = true)
        {
            if (_spriteRenderer == null) return;

            if (isDragging)
            {
                _spriteRenderer.color = isValidDrop 
                    ? new Color(0.2f, 1f, 0.4f, 0.85f) 
                    : new Color(1f, 0.2f, 0.2f, 0.85f);
                _spriteRenderer.sortingOrder = 20;
            }
            else
            {
                _spriteRenderer.color = hasActiveSynergy ? new Color(1f, 0.45f, 0.1f, 1f) : Color.white;
                _spriteRenderer.sortingOrder = 10;
            }
        }

        public void OnPlaced(Vector2Int gridCoord, Vector3 worldSnapPosition)
        {
            isPlacedOnGrid = true;
            currentGridPosition = gridCoord;
            transform.position = worldSnapPosition;
            SetVisualState(false);
        }

        public void OnRemoved(Vector3 returnPosition)
        {
            isPlacedOnGrid = false;
            currentGridPosition = new Vector2Int(-1, -1);
            SetSynergyState(null);
            transform.position = returnPosition;
            _originalPosition = returnPosition;
            SetVisualState(false);
        }

        public void ReturnToOriginalPosition()
        {
            transform.position = _originalPosition;
            SetVisualState(false);
        }
    }
}
