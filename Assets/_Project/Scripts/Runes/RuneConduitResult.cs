using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    /// <summary>
    /// Encapsulates the deterministic outcome of a directional rune conduit calculation across the LatticeGrid.
    /// </summary>
    public class RuneConduitResult
    {
        public Vector2Int Origin { get; private set; }
        public ConduitDirection Direction { get; private set; }
        public int RequestedRange { get; private set; }
        public IReadOnlyList<Vector2Int> TraversedCells { get; private set; }
        public ConduitTerminationReason TerminationReason { get; private set; }
        public Vector2Int? TargetCell { get; private set; }

        public bool HasTarget => TargetCell.HasValue;
        public int TraversalLength => TraversedCells != null ? TraversedCells.Count : 0;

        public RuneConduitResult(
            Vector2Int origin,
            ConduitDirection direction,
            int requestedRange,
            List<Vector2Int> traversedCells,
            ConduitTerminationReason terminationReason,
            Vector2Int? targetCell = null)
        {
            Origin = origin;
            Direction = direction;
            RequestedRange = requestedRange;
            TraversedCells = traversedCells ?? new List<Vector2Int>();
            TerminationReason = terminationReason;
            TargetCell = targetCell;
        }

        public static RuneConduitResult CreateEmpty(Vector2Int origin, ConduitDirection direction, ConduitTerminationReason reason)
        {
            return new RuneConduitResult(origin, direction, 0, new List<Vector2Int>(), reason, null);
        }
    }
}
