using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Encapsulates the resolved runtime outcome of an elemental 2-beam crossing reaction.
    /// </summary>
    public class ElementalReactionResult
    {
        public bool IsActive { get; private set; }
        public string ReactionId { get; private set; }
        public string ReactionName { get; private set; }
        public string Description { get; private set; }
        public Vector2Int GridCoordinate { get; private set; }
        public string RuneAId { get; private set; }
        public string RuneBId { get; private set; }
        public ElementType ElementA { get; private set; }
        public ElementType ElementB { get; private set; }
        public Color ReactionColor { get; private set; }

        public ElementalReactionResult(
            bool isActive,
            string reactionId,
            string reactionName,
            string desc,
            Vector2Int coordinate,
            string runeAId,
            string runeBId,
            ElementType elemA,
            ElementType elemB,
            Color color)
        {
            IsActive = isActive;
            ReactionId = reactionId;
            ReactionName = reactionName;
            Description = desc;
            GridCoordinate = coordinate;
            RuneAId = runeAId;
            RuneBId = runeBId;
            ElementA = elemA;
            ElementB = elemB;
            ReactionColor = color;
        }

        public static ElementalReactionResult CreateActive(
            ElementalReactionDefinitionSO definition,
            BeamIntersection intersection)
        {
            return new ElementalReactionResult(
                isActive: true,
                reactionId: definition.ReactionId,
                reactionName: definition.DisplayName,
                desc: definition.Description,
                coordinate: intersection.GridCoordinate,
                runeAId: intersection.RuneAId,
                runeBId: intersection.RuneBId,
                elemA: intersection.ElementA,
                elemB: intersection.ElementB,
                color: definition.ReactionColor
            );
        }

        public static ElementalReactionResult CreateInactive(BeamIntersection intersection)
        {
            return new ElementalReactionResult(
                isActive: false,
                reactionId: null,
                reactionName: null,
                desc: null,
                coordinate: intersection != null ? intersection.GridCoordinate : new Vector2Int(-1, -1),
                runeAId: intersection?.RuneAId,
                runeBId: intersection?.RuneBId,
                elemA: ElementType.Physical,
                elemB: ElementType.Physical,
                color: Color.clear
            );
        }
    }
}
