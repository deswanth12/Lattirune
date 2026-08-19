using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Data-driven ScriptableObject defining an immutable static elemental reaction rule.
    /// Maps a 2-element crossing pair to a unique Reaction ID, combat effect description, and visual aura.
    /// </summary>
    [CreateAssetMenu(fileName = "Reaction_", menuName = "Lattirune/Data/Reaction Definition")]
    public class ElementalReactionDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string reactionId = "reaction_steam";
        [SerializeField] private string displayName = "Steam";
        [SerializeField] [TextArea(2, 4)] private string description = "Fire Beam + Ice Beam causes 25% Enemy Blind/Miss.";

        [Header("Element Pair Conditions")]
        [SerializeField] private ElementType elementA = ElementType.Fire;
        [SerializeField] private ElementType elementB = ElementType.Ice;
        [SerializeField] private int priority = 0;

        [Header("Visual Feedback")]
        [SerializeField] private Color reactionColor = new Color(0.8f, 0.9f, 1f, 0.9f);

        public string ReactionId => reactionId;
        public string DisplayName => displayName;
        public string Description => description;
        public ElementType ElementA => elementA;
        public ElementType ElementB => elementB;
        public int Priority => priority;
        public Color ReactionColor => reactionColor;

        public void Initialize(
            string id, 
            string name, 
            string desc, 
            ElementType elemA, 
            ElementType elemB, 
            Color color,
            int prio = 0)
        {
            reactionId = id;
            displayName = name;
            description = desc;
            elementA = elemA;
            elementB = elemB;
            reactionColor = color;
            priority = prio;
        }

        public bool IsMatch(ElementType a, ElementType b)
        {
            return (a == elementA && b == elementB) || (a == elementB && b == elementA);
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(reactionId))
            {
                error = "Reaction ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (elementA == elementB)
            {
                error = "Reaction requires two distinct elements.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
