using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Items
{
    /// <summary>
    /// Development test item representing a draggable entity for spatial grid testing.
    /// Supports dynamic footprints (1x1, 1x2, 2x1, 2x2) and visual state feedback.
    /// [DEVELOPMENT ONLY]
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class TestItem : MonoBehaviour
    {
        [Header("Item Properties")]
        [SerializeField] private string itemId = "test_item_1x1";
        [SerializeField] private Vector2Int dimensions = new Vector2Int(1, 1);
        [SerializeField] private Color itemColor = new Color(0.85f, 0.45f, 0.15f, 1f); // Amber / Copper

        [Header("State")]
        [SerializeField] private bool isPlacedOnGrid;
        [SerializeField] private Vector2Int currentGridPosition;

        private Vector3 _originalPosition;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _collider;

        public string ItemId => itemId;
        public Vector2Int Dimensions => dimensions;
        public bool IsPlacedOnGrid => isPlacedOnGrid;
        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector3 OriginalPosition => _originalPosition;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _collider = GetComponent<BoxCollider2D>();
            _originalPosition = transform.position;

            CreatePlaceholderVisual();
        }

        public void Initialize(string id, Vector2Int dims, Color color, Vector3 startPosition)
        {
            itemId = id;
            dimensions = dims;
            itemColor = color;
            transform.position = startPosition;
            _originalPosition = startPosition;
            isPlacedOnGrid = false;

            CreatePlaceholderVisual();
        }

        public void CreatePlaceholderVisual()
        {
            float cellSize = GridCoordinateUtility.DEFAULT_CELL_SIZE;
            float cellSpacing = GridCoordinateUtility.DEFAULT_CELL_SPACING;

            float width = (dimensions.x * cellSize) + ((dimensions.x - 1) * cellSpacing);
            float height = (dimensions.y * cellSize) + ((dimensions.y - 1) * cellSpacing);

            Texture2D tex = new Texture2D(32, 32);
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    bool isBorder = x <= 1 || x >= 30 || y <= 1 || y >= 30;
                    tex.SetPixel(x, y, isBorder ? Color.white : itemColor);
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            _spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            _spriteRenderer.color = Color.white;
            _spriteRenderer.sortingOrder = 10; // In front of grid tiles

            transform.localScale = new Vector3(width, height, 1f);

            if (_collider != null)
            {
                _collider.size = Vector2.one;
            }
        }

        public void SetVisualState(bool isDragging, bool isValidDrop = true)
        {
            if (isDragging)
            {
                _spriteRenderer.color = isValidDrop ? new Color(0.2f, 1f, 0.4f, 0.85f) : new Color(1f, 0.2f, 0.2f, 0.85f);
                _spriteRenderer.sortingOrder = 20; // Float above everything during drag
            }
            else
            {
                _spriteRenderer.color = Color.white;
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
