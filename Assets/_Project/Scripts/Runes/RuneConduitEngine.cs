using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Runes
{
    /// <summary>
    /// Deterministic 2D grid raycasting engine for directional Rune Conduits.
    /// Traverses the 5x5 LatticeGrid coordinates using discrete integer stepping (zero physics queries).
    /// </summary>
    public static class RuneConduitEngine
    {
        /// <summary>
        /// Maps a cardinal ConduitDirection enum to a discrete 2D integer step vector.
        /// </summary>
        public static Vector2Int GetDirectionVector(ConduitDirection direction)
        {
            switch (direction)
            {
                case ConduitDirection.North: return new Vector2Int(0, 1);
                case ConduitDirection.South: return new Vector2Int(0, -1);
                case ConduitDirection.East:  return new Vector2Int(1, 0);
                case ConduitDirection.West:  return new Vector2Int(-1, 0);
                default:                     return Vector2Int.zero;
            }
        }

        /// <summary>
        /// Calculates the discrete path of a single directional rune conduit across the LatticeGrid.
        /// </summary>
        /// <param name="grid">The authoritative 5x5 LatticeGrid instance.</param>
        /// <param name="origin">The coordinate of the emitter Rune (e.g. (2,2)).</param>
        /// <param name="direction">The emission direction (North, South, East, West).</param>
        /// <param name="range">The maximum tile step count (clamped to 1..5).</param>
        /// <param name="isTargetPredicate">Optional predicate to detect whether a cell contains a target receptor.</param>
        /// <param name="stopOnTarget">If true, finding a target terminates the conduit immediately.</param>
        /// <param name="stopOnOccupied">If true, hitting an occupied cell terminates the conduit (insulator behavior).</param>
        public static RuneConduitResult CalculateConduit(
            LatticeGrid grid,
            Vector2Int origin,
            ConduitDirection direction,
            int range = LatticeGrid.WIDTH,
            Func<Vector2Int, bool> isTargetPredicate = null,
            bool stopOnTarget = true,
            bool stopOnOccupied = false)
        {
            if (grid == null)
            {
                return RuneConduitResult.CreateEmpty(origin, direction, ConduitTerminationReason.None);
            }

            if (!grid.IsValidCoordinate(origin))
            {
                return RuneConduitResult.CreateEmpty(origin, direction, ConduitTerminationReason.InvalidOrigin);
            }

            Vector2Int stepVector = GetDirectionVector(direction);
            if (stepVector == Vector2Int.zero)
            {
                return RuneConduitResult.CreateEmpty(origin, direction, ConduitTerminationReason.UnsupportedDirection);
            }

            int clampedRange = Mathf.Clamp(range, 1, LatticeGrid.WIDTH);
            List<Vector2Int> traversed = new List<Vector2Int>(clampedRange);
            ConduitTerminationReason terminationReason = ConduitTerminationReason.RangeReached;
            Vector2Int? foundTarget = null;

            for (int step = 1; step <= clampedRange; step++)
            {
                int targetX = origin.x + (stepVector.x * step);
                int targetY = origin.y + (stepVector.y * step);
                Vector2Int currentCoord = new Vector2Int(targetX, targetY);

                // 1. Check Grid Boundary
                if (!grid.IsValidCoordinate(targetX, targetY))
                {
                    terminationReason = ConduitTerminationReason.GridBoundary;
                    break;
                }

                GridCell cell = grid.GetCell(targetX, targetY);

                // 2. Check Locked Cell (Conduits stop at locked cells without traversing past them)
                if (cell != null && cell.IsLocked())
                {
                    terminationReason = ConduitTerminationReason.LockedCell;
                    break;
                }

                // 3. Record Traversed Cell
                traversed.Add(currentCoord);

                // 4. Target Detection
                if (isTargetPredicate != null && isTargetPredicate(currentCoord))
                {
                    foundTarget = currentCoord;
                    if (stopOnTarget)
                    {
                        terminationReason = ConduitTerminationReason.TargetFound;
                        break;
                    }
                }

                // 5. Occupied Insulator Check
                if (stopOnOccupied && cell != null && cell.IsOccupied())
                {
                    terminationReason = ConduitTerminationReason.BlockedByOccupant;
                    break;
                }
            }

            return new RuneConduitResult(origin, direction, clampedRange, traversed, terminationReason, foundTarget);
        }

        /// <summary>
        /// Calculates multiple independent rune conduits across the grid in a single pass.
        /// </summary>
        public static List<RuneConduitResult> CalculateMultipleConduits(
            LatticeGrid grid,
            IEnumerable<(Vector2Int origin, ConduitDirection dir, int range)> runeSpecifications,
            Func<Vector2Int, bool> isTargetPredicate = null)
        {
            List<RuneConduitResult> results = new List<RuneConduitResult>();
            if (runeSpecifications == null || grid == null) return results;

            foreach (var (origin, dir, range) in runeSpecifications)
            {
                RuneConduitResult result = CalculateConduit(grid, origin, dir, range, isTargetPredicate);
                results.Add(result);
            }

            return results;
        }
    }
}
