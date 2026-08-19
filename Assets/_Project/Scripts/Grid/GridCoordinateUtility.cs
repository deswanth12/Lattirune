using UnityEngine;

namespace Lattirune.Grid
{
    /// <summary>
    /// Deterministic mathematical conversion between screen, world, and 5x5 LatticeGrid coordinates.
    /// Standard origin (0,0) is at bottom-left; X is East (+1), Y is North (+1).
    /// </summary>
    public static class GridCoordinateUtility
    {
        public const float DEFAULT_CELL_SIZE = 1.2f;
        public const float DEFAULT_CELL_SPACING = 0.1f;

        /// <summary>
        /// Calculates the world position of the center of a specific grid cell (x, y).
        /// </summary>
        public static Vector3 GridToWorldPosition(int x, int y, Vector2 gridOrigin, float cellSize = DEFAULT_CELL_SIZE, float cellSpacing = DEFAULT_CELL_SPACING)
        {
            float step = cellSize + cellSpacing;
            float worldX = gridOrigin.x + (x * step) + (cellSize * 0.5f);
            float worldY = gridOrigin.y + (y * step) + (cellSize * 0.5f);
            return new Vector3(worldX, worldY, 0f);
        }

        /// <summary>
        /// Calculates the world position for a grid coordinate vector.
        /// </summary>
        public static Vector3 GridToWorld(Vector2Int gridCoord, Vector2 gridOrigin = default, float cellSize = DEFAULT_CELL_SIZE, float cellSpacing = DEFAULT_CELL_SPACING)
        {
            return GridToWorldPosition(gridCoord.x, gridCoord.y, gridOrigin, cellSize, cellSpacing);
        }

        /// <summary>
        /// Calculates the bottom-left world origin of the 5x5 grid such that the grid is centered at targetCenter.
        /// </summary>
        public static Vector2 CalculateGridOrigin(Vector2 targetCenter, int gridWidth = LatticeGrid.WIDTH, int gridHeight = LatticeGrid.HEIGHT, float cellSize = DEFAULT_CELL_SIZE, float cellSpacing = DEFAULT_CELL_SPACING)
        {
            float totalWidth = (gridWidth * cellSize) + ((gridWidth - 1) * cellSpacing);
            float totalHeight = (gridHeight * cellSize) + ((gridHeight - 1) * cellSpacing);
            return new Vector2(targetCenter.x - (totalWidth * 0.5f), targetCenter.y - (totalHeight * 0.5f));
        }

        /// <summary>
        /// Converts a world position to the nearest integer grid coordinate (x, y).
        /// Returns whether the coordinate is within grid bounds [0, 4].
        /// </summary>
        public static bool WorldToGridCoordinate(Vector3 worldPos, Vector2 gridOrigin, out Vector2Int gridCoord, float cellSize = DEFAULT_CELL_SIZE, float cellSpacing = DEFAULT_CELL_SPACING)
        {
            float step = cellSize + cellSpacing;
            float relativeX = worldPos.x - gridOrigin.x;
            float relativeY = worldPos.y - gridOrigin.y;

            int x = Mathf.FloorToInt(relativeX / step);
            int y = Mathf.FloorToInt(relativeY / step);

            gridCoord = new Vector2Int(x, y);
            return x >= 0 && x < LatticeGrid.WIDTH && y >= 0 && y < LatticeGrid.HEIGHT;
        }

        /// <summary>
        /// Computes the centered world position for an item footprint (e.g. 1x1, 1x2, 2x2) starting at originCoord.
        /// </summary>
        public static Vector3 GetFootprintWorldCenter(Vector2Int originCoord, Vector2Int size, Vector2 gridOrigin, float cellSize = DEFAULT_CELL_SIZE, float cellSpacing = DEFAULT_CELL_SPACING)
        {
            float step = cellSize + cellSpacing;
            float spanX = (size.x * cellSize) + ((size.x - 1) * cellSpacing);
            float spanY = (size.y * cellSize) + ((size.y - 1) * cellSpacing);

            float startX = gridOrigin.x + (originCoord.x * step);
            float startY = gridOrigin.y + (originCoord.y * step);

            float centerX = startX + (spanX * 0.5f);
            float centerY = startY + (spanY * 0.5f);

            return new Vector3(centerX, centerY, 0f);
        }
    }
}
