using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Grid
{
    /// <summary>
    /// Visualizes the 5x5 LatticeGrid in Unity World space.
    /// Manages cell visual tiles, state updates, and dynamic drag-and-drop feedback highlights.
    /// </summary>
    public class GridView : MonoBehaviour
    {
        [Header("Grid Layout Settings")]
        [SerializeField] private Vector2 gridCenter = new Vector2(0f, -1.8f);
        [SerializeField] private float cellSize = GridCoordinateUtility.DEFAULT_CELL_SIZE;
        [SerializeField] private float cellSpacing = GridCoordinateUtility.DEFAULT_CELL_SPACING;

        [Header("Cell Colors (Prototyping / Dark Neo-Arcane Palette)")]
        [SerializeField] private Color activeCellColor = new Color(0.12f, 0.15f, 0.22f, 1f);   // Deep Slate
        [SerializeField] private Color lockedCellColor = new Color(0.04f, 0.05f, 0.08f, 0.8f); // Locked Dark
        [SerializeField] private Color occupiedCellColor = new Color(0.18f, 0.22f, 0.32f, 1f); // Occupied
        [SerializeField] private Color validHighlightColor = new Color(0.18f, 0.80f, 0.44f, 0.6f); // Emerald Green
        [SerializeField] private Color invalidHighlightColor = new Color(0.91f, 0.30f, 0.24f, 0.6f); // Ruby Red

        private LatticeGrid _grid;
        private Vector2 _gridOrigin;
        private readonly SpriteRenderer[,] _cellRenderers = new SpriteRenderer[LatticeGrid.WIDTH, LatticeGrid.HEIGHT];
        private readonly HashSet<Vector2Int> _highlightedCells = new HashSet<Vector2Int>();
        private Sprite _defaultCellSprite;

        public Vector2 GridOrigin => _gridOrigin;
        public float CellSize => cellSize;
        public float CellSpacing => cellSpacing;

        public void Initialize(LatticeGrid grid)
        {
            _grid = grid;
            _gridOrigin = GridCoordinateUtility.CalculateGridOrigin(gridCenter, LatticeGrid.WIDTH, LatticeGrid.HEIGHT, cellSize, cellSpacing);

            CreateDefaultCellSprite();
            BuildVisualGrid();

            _grid.OnCellStateChanged += HandleCellStateChanged;
        }

        private void OnDestroy()
        {
            if (_grid != null)
            {
                _grid.OnCellStateChanged -= HandleCellStateChanged;
            }
        }

                private void CreateDefaultCellSprite()
        {
            if (_defaultCellSprite == null)
            {
                Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                Color border = new Color(0.85f, 0.65f, 0.2f, 0.9f); // Gold Arcane Border
                Color inner = new Color(0.10f, 0.12f, 0.18f, 0.95f); // Deep Slate
                Color corner = new Color(1.0f, 0.85f, 0.35f, 1.0f); // Bright Gold Corner

                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        bool isOuter = x == 0 || x == 63 || y == 0 || y == 63;
                        bool isInner = x == 1 || x == 62 || y == 1 || y == 62;
                        bool isCorner = (x <= 3 || x >= 60) && (y <= 3 || y >= 60);

                        if (isCorner) tex.SetPixel(x, y, corner);
                        else if (isOuter || isInner) tex.SetPixel(x, y, border);
                        else tex.SetPixel(x, y, inner);
                    }
                }
                tex.filterMode = FilterMode.Bilinear;
                tex.Apply();
                _defaultCellSprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
            }
        }

        private void BuildVisualGrid()
        {
            // Clear existing children if any
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            for (int x = 0; x < LatticeGrid.WIDTH; x++)
            {
                for (int y = 0; y < LatticeGrid.HEIGHT; y++)
                {
                    GameObject cellObj = new GameObject($"Cell_{x}_{y}");
                    cellObj.transform.SetParent(transform);
                    cellObj.transform.position = GridCoordinateUtility.GridToWorldPosition(x, y, _gridOrigin, cellSize, cellSpacing);
                    cellObj.transform.localScale = new Vector3(cellSize, cellSize, 1f);

                    SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
                    sr.sprite = _defaultCellSprite;
                    sr.sortingOrder = 0;

                    _cellRenderers[x, y] = sr;
                    UpdateCellVisual(x, y);
                }
            }
        }

        private void HandleCellStateChanged(Vector2Int pos, TileState newState)
        {
            if (_grid.IsValidCoordinate(pos.x, pos.y))
            {
                UpdateCellVisual(pos.x, pos.y);
            }
        }

        public void UpdateCellVisual(int x, int y)
        {
            if (_cellRenderers[x, y] == null || _grid == null) return;

            GridCell cell = _grid.GetCell(x, y);
            if (cell == null) return;

            Color baseColor;
            switch (cell.State)
            {
                case TileState.Locked:
                    baseColor = lockedCellColor;
                    break;
                case TileState.Occupied:
                    baseColor = occupiedCellColor;
                    break;
                case TileState.Active:
                default:
                    baseColor = activeCellColor;
                    break;
            }

            _cellRenderers[x, y].color = baseColor;
        }

        /// <summary>
        /// Applies valid/invalid highlight to cells covered by an item footprint.
        /// </summary>
        public void SetFootprintHighlight(Vector2Int origin, Vector2Int size, bool isValid)
        {
            ClearHighlight();

            Color highlightColor = isValid ? validHighlightColor : invalidHighlightColor;

            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    int targetX = origin.x + dx;
                    int targetY = origin.y + dy;

                    if (_grid.IsValidCoordinate(targetX, targetY))
                    {
                        _cellRenderers[targetX, targetY].color = highlightColor;
                        _highlightedCells.Add(new Vector2Int(targetX, targetY));
                    }
                }
            }
        }

        /// <summary>
        /// Clears all temporary drag highlights, restoring default cell state colors.
        /// </summary>
        public void ClearHighlight()
        {
            foreach (var pos in _highlightedCells)
            {
                if (_grid.IsValidCoordinate(pos.x, pos.y))
                {
                    UpdateCellVisual(pos.x, pos.y);
                }
            }
            _highlightedCells.Clear();
        }
    }
}
