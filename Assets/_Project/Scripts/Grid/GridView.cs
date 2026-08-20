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
        [SerializeField] private Vector2 gridCenter = new Vector2(0f, -2.1f);
        [SerializeField] private float cellSize = GridCoordinateUtility.DEFAULT_CELL_SIZE;
        [SerializeField] private float cellSpacing = GridCoordinateUtility.DEFAULT_CELL_SPACING;

        [Header("Cell Colors (Prototyping / Dark Neo-Arcane Palette)")]
        [SerializeField] private Color activeCellColor = Color.white;
        [SerializeField] private Color lockedCellColor = new Color(0.3f, 0.3f, 0.35f, 0.6f);
        [SerializeField] private Color occupiedCellColor = new Color(0.85f, 0.9f, 1f, 1f);
        [SerializeField] private Color validHighlightColor = new Color(0.2f, 1f, 0.5f, 0.75f);
        [SerializeField] private Color invalidHighlightColor = new Color(1f, 0.25f, 0.25f, 0.75f);

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
                Texture2D tex = Resources.Load<Texture2D>("Art/Runes/tile_rune_stone");
                if (tex != null)
                {
                    _defaultCellSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 256);
                }
                else
                {
                    Texture2D fallback = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                    Color border = new Color(0.85f, 0.65f, 0.2f, 0.9f);
                    Color inner = new Color(0.10f, 0.12f, 0.18f, 0.95f);
                    for (int x = 0; x < 64; x++)
                    {
                        for (int y = 0; y < 64; y++)
                        {
                            bool isBorder = x <= 1 || x >= 62 || y <= 1 || y >= 62;
                            fallback.SetPixel(x, y, isBorder ? border : inner);
                        }
                    }
                    fallback.Apply();
                    _defaultCellSprite = Sprite.Create(fallback, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
                }
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
