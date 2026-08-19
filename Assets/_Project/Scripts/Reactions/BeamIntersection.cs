using System;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Runes;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Encapsulates a valid 2-beam crossing intersection at a discrete grid coordinate.
    /// Uses normalized rune ordering to guarantee deterministic (A x B == B x A) equality.
    /// </summary>
    public class BeamIntersection : IEquatable<BeamIntersection>
    {
        public Vector2Int GridCoordinate { get; private set; }
        public string RuneAId { get; private set; }
        public string RuneBId { get; private set; }
        public ElementType ElementA { get; private set; }
        public ElementType ElementB { get; private set; }
        public ConduitDirection DirectionA { get; private set; }
        public ConduitDirection DirectionB { get; private set; }

        public BeamIntersection(
            Vector2Int coordinate,
            string runeAId,
            string runeBId,
            ElementType elementA,
            ElementType elementB,
            ConduitDirection directionA,
            ConduitDirection directionB)
        {
            GridCoordinate = coordinate;

            // Normalize order by rune ID so (A, B) and (B, A) produce identical keys
            if (string.CompareOrdinal(runeAId, runeBId) <= 0)
            {
                RuneAId = runeAId;
                RuneBId = runeBId;
                ElementA = elementA;
                ElementB = elementB;
                DirectionA = directionA;
                DirectionB = directionB;
            }
            else
            {
                RuneAId = runeBId;
                RuneBId = runeAId;
                ElementA = elementB;
                ElementB = elementA;
                DirectionA = directionB;
                DirectionB = directionA;
            }
        }

        public bool Equals(BeamIntersection other)
        {
            if (other == null) return false;
            return GridCoordinate == other.GridCoordinate &&
                   RuneAId == other.RuneAId &&
                   RuneBId == other.RuneBId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BeamIntersection);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + GridCoordinate.GetHashCode();
                hash = hash * 31 + (RuneAId != null ? RuneAId.GetHashCode() : 0);
                hash = hash * 31 + (RuneBId != null ? RuneBId.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
