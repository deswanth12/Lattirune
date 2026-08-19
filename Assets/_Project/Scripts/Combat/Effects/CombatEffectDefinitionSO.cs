using UnityEngine;

namespace Lattirune.Combat.Effects
{
    /// <summary>
    /// Static ScriptableObject defining an immutable combat effect rule produced by an elemental reaction.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatEffect_", menuName = "Lattirune/Data/Combat Effect Definition")]
    public class CombatEffectDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string effectId = "effect_steam_blind";
        [SerializeField] private string displayName = "Steam Blind";
        [SerializeField] private string mappedReactionId = "reaction_steam";
        [SerializeField] [TextArea(2, 4)] private string description = "Inflicts 25% Enemy Blind/Miss chance.";

        [Header("Mechanics")]
        [SerializeField] private CombatEffectType effectType = CombatEffectType.AttackModifier;
        [SerializeField] private float duration = 4.0f;
        [SerializeField] private float tickInterval = 1.0f;
        [SerializeField] private float magnitude = 0.25f;

        [Header("Visuals")]
        [SerializeField] private Color effectColor = new Color(0.85f, 0.95f, 1f, 0.9f);

        public string EffectId => effectId;
        public string DisplayName => displayName;
        public string MappedReactionId => mappedReactionId;
        public string Description => description;
        public CombatEffectType EffectType => effectType;
        public float Duration => duration;
        public float TickInterval => tickInterval;
        public float Magnitude => magnitude;
        public Color EffectColor => effectColor;

        public void Initialize(
            string id,
            string name,
            string reactionId,
            CombatEffectType type,
            float dur,
            float interval,
            float mag,
            string desc,
            Color color)
        {
            effectId = id;
            displayName = name;
            mappedReactionId = reactionId;
            effectType = type;
            duration = dur;
            tickInterval = interval;
            magnitude = mag;
            description = desc;
            effectColor = color;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(effectId))
            {
                error = "Effect ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (duration < 0f)
            {
                error = "Duration cannot be negative.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
