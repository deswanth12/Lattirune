using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Runes;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Deterministic 2-beam crossing intersection detection engine.
    /// Consumes RuneConduitResult outputs and identifies valid crossing points on the 5x5 grid.
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
        /// Evaluates all active conduits and extracts all unique 2-beam crossing intersections.
        /// </summary>
        public static List<BeamIntersection> FindIntersections(
            IReadOnlyList<(RuneData rune, Vector2Int origin, RuneConduitResult conduit)> activeConduits)
        {
            List<BeamIntersection> results = new List<BeamIntersection>();
            if (activeConduits == null || activeConduits.Count < 2)
            {
                return results;
            }

            HashSet<BeamIntersection> uniqueIntersections = new HashSet<BeamIntersection>();

            for (int i = 0; i < activeConduits.Count; i++)
            {
                var (runeA, originA, conduitA) = activeConduits[i];
                if (runeA == null || !runeA.IsActive || conduitA == null || conduitA.TraversalLength == 0) continue;

                for (int j = i + 1; j < activeConduits.Count; j++)
                {
                    var (runeB, originB, conduitB) = activeConduits[j];
                    if (runeB == null || !runeB.IsActive || conduitB == null || conduitB.TraversalLength == 0) continue;

                    // Reject self-intersection
                    if (runeA.RuneId == runeB.RuneId) continue;

                    // Reject non-crossing (parallel / collinear) beams
                    if (!AreDirectionsCrossing(runeA.Direction, runeB.Direction)) continue;

                    // Find overlapping cell coordinates between both conduit paths
                    for (int ca = 0; ca < conduitA.TraversalLength; ca++)
                    {
                        Vector2Int cellA = conduitA.TraversedCells[ca];

                        for (int cb = 0; cb < conduitB.TraversalLength; cb++)
                        {
                            Vector2Int cellB = conduitB.TraversedCells[cb];

                            if (cellA == cellB)
                            {
                                BeamIntersection intersection = new BeamIntersection(
                                    coordinate: cellA,
                                    runeAId: runeA.RuneId,
                                    runeBId: runeB.RuneId,
                                    elementA: runeA.Element,
                                    elementB: runeB.Element,
                                    directionA: runeA.Direction,
                                    directionB: runeB.Direction
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
    }
}
