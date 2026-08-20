using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.UI;

namespace Lattirune.Items
{
    /// <summary>
    /// Runtime representation of an item in the game world / inventory.
    /// Renders high-resolution dark fantasy 2D artwork and manages rotation, footprint, and synergies.
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

        public void SetSynergyState(string synergyId, Color? synergyColor = null)
        {
            activeSynergyId = synergyId;
            hasActiveSynergy = !string.IsNullOrEmpty(synergyId);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = hasActiveSynergy 
                    ? (synergyColor ?? new Color(1f, 0.75f, 0.3f, 1f)) 
                    : Color.white;
            }
        }

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

            Vector2Int baseDims = itemData.BaseDimensions;
            float cellSize = GridCoordinateUtility.DEFAULT_CELL_SIZE;
            float cellSpacing = GridCoordinateUtility.DEFAULT_CELL_SPACING;

            float baseW = (baseDims.x * cellSize) + ((baseDims.x - 1) * cellSpacing);
            float baseH = (baseDims.y * cellSize) + ((baseDims.y - 1) * cellSpacing);

            Texture2D itemTex = VisualAssetProvider.GetItemTexture(itemData.ItemId);

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            if (itemTex != null)
            {
                float ppu = 256f;
                _spriteRenderer.sprite = Sprite.Create(itemTex, new Rect(0, 0, itemTex.width, itemTex.height), new Vector2(0.5f, 0.5f), ppu);

                float naturalW = itemTex.width / ppu;
                float naturalH = itemTex.height / ppu;

                transform.localScale = new Vector3(baseW / Mathf.Max(0.01f, naturalW), baseH / Mathf.Max(0.01f, naturalH), 1f);
            }

            transform.localRotation = Quaternion.Euler(0f, 0f, -currentRotationDegrees);

            _spriteRenderer.color = hasActiveSynergy ? new Color(1f, 0.85f, 0.4f, 1f) : Color.white;
            _spriteRenderer.sortingOrder = 10;

            if (_collider != null)
            {
                _collider.size = new Vector2(baseW, baseH);
                _collider.offset = Vector2.zero;
            }
        }

                public void SetVisualState(bool isDragging, bool isValidDrop)
        {
            if (_spriteRenderer != null)
            {
                if (isDragging)
                {
                    _spriteRenderer.color = isValidDrop 
                        ? new Color(0.6f, 1f, 0.6f, 0.85f) 
                        : new Color(1f, 0.5f, 0.5f, 0.85f);
                    _spriteRenderer.sortingOrder = 50;
                }
                else
                {
                    _spriteRenderer.color = hasActiveSynergy ? new Color(1f, 0.8f, 0.35f, 1f) : Color.white;
                    _spriteRenderer.sortingOrder = 10;
                }
            }
        }

        public void OnPickedUp()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = 50;
            }
        }

        public void ReturnToOriginalPosition()
        {
            transform.position = _originalPosition;
            SetVisualState(isDragging: false, isValidDrop: true);
        }

        public void OnRemoved()
        {
            isPlacedOnGrid = false;
            currentGridPosition = new Vector2Int(-1, -1);
            SetSynergyState(null);
            SetVisualState(isDragging: false, isValidDrop: true);
        }

        public void OnRemoved(Vector3 stagingPosition)
        {
            OnRemovedFromGrid(stagingPosition);
        }

        public void OnPlaced(Vector2Int gridCoord, Vector3 worldPosition)
        {
            isPlacedOnGrid = true;
            currentGridPosition = gridCoord;
            transform.position = worldPosition;
        }

        public void OnRemovedFromGrid(Vector3 stagingPosition)
        {
            isPlacedOnGrid = false;
            currentGridPosition = new Vector2Int(-1, -1);
            transform.position = stagingPosition;
            SetSynergyState(null);
        }

        public void ResetPosition()
        {
            transform.position = _originalPosition;
        }

        public void SetOriginalPosition(Vector3 newPos)
        {
            _originalPosition = newPos;
        }
    }
}
