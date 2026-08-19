using UnityEngine;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Encapsulates the evaluation outcome of a conduit-to-item synergy relationship.
    /// </summary>
    public class SynergyResult
    {
        public bool IsSynergyActive { get; private set; }
        public string SynergyId { get; private set; }
        public string SynergyName { get; private set; }
        public string RuneId { get; private set; }
        public string TargetItemId { get; private set; }
        public string TargetInstanceId { get; private set; }
        public Vector2Int SourcePosition { get; private set; }
        public Vector2Int TargetPosition { get; private set; }
        public Color SynergyColor { get; private set; }
        public int RuneBonus { get; private set; }

        public SynergyResult(
            bool isActive,
            string synergyId,
            string synergyName,
            string runeId,
            string targetItemId,
            string targetInstanceId,
            Vector2Int sourcePos,
            Vector2Int targetPos,
            Color color,
            int bonus = 0)
        {
            IsSynergyActive = isActive;
            SynergyId = synergyId;
            SynergyName = synergyName;
            RuneId = runeId;
            TargetItemId = targetItemId;
            TargetInstanceId = targetInstanceId;
            SourcePosition = sourcePos;
            TargetPosition = targetPos;
            SynergyColor = color;
            RuneBonus = bonus;
        }

        public static SynergyResult CreateActive(
            SynergyDefinition definition,
            string runeId,
            string targetItemId,
            string targetInstanceId,
            Vector2Int sourcePos,
            Vector2Int targetPos)
        {
            return new SynergyResult(
                isActive: true,
                synergyId: definition.SynergyId,
                synergyName: definition.DisplayName,
                runeId: runeId,
                targetItemId: targetItemId,
                targetInstanceId: targetInstanceId,
                sourcePos: sourcePos,
                targetPos: targetPos,
                color: definition.SynergyColor,
                bonus: 5 // Default fire bonus
            );
        }

        public static SynergyResult CreateActive(
            SynergyDefinitionSO definition,
            string runeId,
            string targetItemId,
            string targetInstanceId,
            Vector2Int sourcePos,
            Vector2Int targetPos)
        {
            return new SynergyResult(
                isActive: true,
                synergyId: definition.SynergyId,
                synergyName: definition.DisplayName,
                runeId: runeId,
                targetItemId: targetItemId,
                targetInstanceId: targetInstanceId,
                sourcePos: sourcePos,
                targetPos: targetPos,
                color: definition.SynergyColor,
                bonus: definition.RuneBonus
            );
        }

        public static SynergyResult CreateInactive(string runeId, string targetItemId, string targetInstanceId)
        {
            return new SynergyResult(
                isActive: false,
                synergyId: null,
                synergyName: null,
                runeId: runeId,
                targetItemId: targetItemId,
                targetInstanceId: targetInstanceId,
                sourcePos: new Vector2Int(-1, -1),
                targetPos: new Vector2Int(-1, -1),
                color: Color.clear,
                bonus: 0
            );
        }
    }
}
