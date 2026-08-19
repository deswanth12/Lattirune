using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Runes;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Deterministic 2-beam crossing intersection detection engine.
    /// Consumes RuneConduitResult or ConduitBeamPath outputs and identifies valid crossing points on the 5x5 grid.
    /// Supports Prism-generated split branches seamlessly.
    /// </summary>
    public static class ElementalIntersectionEngine
    {
        public static bool IsHorizontal(ConduitDirection dir) => dir == ConduitDirection.East || dir == ConduitDirection.West;
        public static bool IsVertical(ConduitDirection dir) => dir == ConduitDirection.North || dir == ConduitDirection.South;

        /// <summary>
        /// Detects whether two conduit directions are genuinely crossing (orthogonal).
        /// Rejects parallel (e.g. East & East, East & West, North & North) overlaps.
        /// </summary>
        public static bool AreDirectionsCrossing(ConduitDirection dirA, ConduitDirection dirB)
        {
            return (IsHorizontal(dirA) && IsVertical(dirB)) || (IsVertical(dirA) && IsHorizontal(dirB));
        }

        /// <summary>
        /// Evaluates all active conduit beam paths (including refracted branches) and extracts unique 2-beam crossing intersections.
        /// </summary>
        public static List<BeamIntersection> FindIntersections(IReadOnlyList<ConduitBeamPath> activeBeams)
        {
            List<BeamIntersection> results = new List<BeamIntersection>();
            if (activeBeams == null || activeBeams.Count < 2)
            {
                return results;
            }

            HashSet<BeamIntersection> uniqueIntersections = new HashSet<BeamIntersection>();

            for (int i = 0; i < activeBeams.Count; i++)
            {
                ConduitBeamPath beamA = activeBeams[i];
                if (beamA == null || beamA.TraversalLength == 0) continue;

                for (int j = i + 1; j < activeBeams.Count; j++)
                {
                    ConduitBeamPath beamB = activeBeams[j];
                    if (beamB == null || beamB.TraversalLength == 0) continue;

                    // Reject self-intersection from same root rune
                    if (beamA.SourceRuneId == beamB.SourceRuneId) continue;

                    // Reject non-crossing directions
                    if (!AreDirectionsCrossing(beamA.Direction, beamB.Direction)) continue;

                    for (int ca = 0; ca < beamA.TraversalLength; ca++)
                    {
                        Vector2Int cellA = beamA.TraversedCells[ca];

                        for (int cb = 0; cb < beamB.TraversalLength; cb++)
                        {
                            Vector2Int cellB = beamB.TraversedCells[cb];

                            if (cellA == cellB)
                            {
                                BeamIntersection intersection = new BeamIntersection(
                                    coordinate: cellA,
                                    runeAId: beamA.SourceRuneId,
                                    runeBId: beamB.SourceRuneId,
                                    elementA: beamA.Element,
                                    elementB: beamB.Element,
                                    directionA: beamA.Direction,
                                    directionB: beamB.Direction
                                );

                                if (uniqueIntersections.Add(intersection))
                                {
                                    results.Add(intersection);
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Legacy overload evaluating standard single-beam conduit results.
        /// </summary>
        public static List<BeamIntersection> FindIntersections(
            IReadOnlyList<(RuneData rune, Vector2Int origin, RuneConduitResult conduit)> activeConduits)
        {
            List<ConduitBeamPath> beamPaths = new List<ConduitBeamPath>();
            if (activeConduits != null)
            {
                for (int i = 0; i < activeConduits.Count; i++)
                {
                    var (rune, origin, conduit) = activeConduits[i];
                    if (rune == null || !rune.IsActive || conduit == null) continue;

                    beamPaths.Add(new ConduitBeamPath(
                        beamId: $"beam_{rune.RuneId}_{i}",
                        sourceRuneId: rune.RuneId,
                        element: rune.Element,
                        origin: origin,
                        direction: rune.Direction,
                        requestedRange: conduit.RequestedRange,
                        traversedCells: new List<Vector2Int>(conduit.TraversedCells),
                        terminationReason: conduit.TerminationReason,
                        targetCell: conduit.TargetCell
                    ));
                }
            }

            return FindIntersections(beamPaths);
        }
    }
}
