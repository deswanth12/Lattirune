using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Grid;

namespace Lattirune.Runes
{
    /// <summary>
    /// Multi-directional conduit emitter engine.
    /// Resolves output emission directions for Crossfire, Omnidirectional, and single-cardinal runes.
    /// Generates discrete, deterministic ConduitBeamPath sets across the 5x5 LatticeGrid.
    /// </summary>
    public static class MultiDirectionalEmitter
    {
        private static readonly ConduitDirection[] FourCardinalDirections = new[]
        {
            ConduitDirection.North,
            ConduitDirection.South,
            ConduitDirection.East,
            ConduitDirection.West
        };

        /// <summary>
        /// Resolves the list of cardinal output directions emitted by a rune configuration.
        /// </summary>
        public static IReadOnlyList<ConduitDirection> GetOutputDirections(ConduitDirection direction)
        {
            switch (direction)
            {
                case ConduitDirection.Cross:
                case ConduitDirection.Omni:
                    return FourCardinalDirections;

                case ConduitDirection.North:
                    return new[] { ConduitDirection.North };

                case ConduitDirection.South:
                    return new[] { ConduitDirection.South };

                case ConduitDirection.East:
                    return new[] { ConduitDirection.East };

                case ConduitDirection.West:
                    return new[] { ConduitDirection.West };

                default:
                    return Array.Empty<ConduitDirection>();
            }
        }

        /// <summary>
        /// Calculates all emitted beams (and any resulting Prism refractions) for a single rune emitter.
        /// </summary>
        public static List<ConduitBeamPath> EmitBeams(
            LatticeGrid grid,
            RuneData rune,
            Vector2Int origin,
            int range = LatticeGrid.WIDTH,
            Func<Vector2Int, (bool isPrism, PrismRuneDataSO data)> getPrismAtCell = null,
            Func<Vector2Int, bool> isTargetPredicate = null,
            bool stopOnTarget = true,
            bool stopOnOccupied = false,
            int maxDepth = 3)
        {
            List<ConduitBeamPath> allBeams = new List<ConduitBeamPath>();
            if (grid == null || rune == null || !rune.IsActive || !grid.IsValidCoordinate(origin))
            {
                return allBeams;
            }

            IReadOnlyList<ConduitDirection> directions = GetOutputDirections(rune.Direction);
            for (int i = 0; i < directions.Count; i++)
            {
                ConduitDirection dir = directions[i];
                List<ConduitBeamPath> paths = RuneConduitEngine.CalculateConduitWithRefraction(
                    grid,
                    rune,
                    origin,
                    dir,
                    range,
                    getPrismAtCell,
                    isTargetPredicate,
                    stopOnTarget,
                    stopOnOccupied,
                    maxDepth
                );
                allBeams.AddRange(paths);
            }

            return allBeams;
        }

        /// <summary>
        /// Batch evaluates all active rune emitters on the grid.
        /// </summary>
        public static List<ConduitBeamPath> EmitAllActiveBeams(
            LatticeGrid grid,
            IEnumerable<(RuneData rune, Vector2Int origin, int range)> runeEmitters,
            Func<Vector2Int, (bool isPrism, PrismRuneDataSO data)> getPrismAtCell = null,
            Func<Vector2Int, bool> isTargetPredicate = null,
            bool stopOnTarget = true,
            bool stopOnOccupied = false,
            int maxDepth = 3)
        {
            List<ConduitBeamPath> combinedBeams = new List<ConduitBeamPath>();
            if (grid == null || runeEmitters == null) return combinedBeams;

            foreach (var (rune, origin, range) in runeEmitters)
            {
                List<ConduitBeamPath> beams = EmitBeams(
                    grid, 
                    rune, 
                    origin, 
                    range, 
                    getPrismAtCell, 
                    isTargetPredicate, 
                    stopOnTarget, 
                    stopOnOccupied, 
                    maxDepth
                );
                combinedBeams.AddRange(beams);
            }

            return combinedBeams;
        }
    }
}
