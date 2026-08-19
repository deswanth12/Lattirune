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
        [SerializeField] private Vector2 gridCenter = Vector2.zero;
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
                Texture2D tex = new Texture2D(32, 32);
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        // Draw a simple bordered square
                        bool isBorder = x == 0 || x == 31 || y == 0 || y == 31;
                        tex.SetPixel(x, y, isBorder ? new Color(0.77f, 0.61f, 0.15f, 0.8f) : Color.white);
                    }
                }
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                _defaultCellSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
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
