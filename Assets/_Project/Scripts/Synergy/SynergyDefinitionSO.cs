using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Data-driven ScriptableObject defining an immutable static elemental synergy rule.
    /// Maps a source ElementType and target ItemCategory (or specific target ItemId) to a unique Synergy ID and combat bonuses.
    /// Derived strictly from PLAN.md Section 7.1.
    /// </summary>
    [CreateAssetMenu(fileName = "Synergy_", menuName = "Lattirune/Data/Synergy Definition")]
    public class SynergyDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string synergyId = "fire_sword";
        [SerializeField] private string displayName = "Flamebound Edge";
        [SerializeField] [TextArea(2, 4)] private string description = "Fire Rune connected to a Weapon adds +5 Rune Bonus damage.";

        [Header("Matrix & Target Conditions")]
        [SerializeField] private ElementType requiredElement = ElementType.Fire;
        [SerializeField] private ItemCategory requiredCategory = ItemCategory.Weapon;
        [SerializeField] private string targetItemId = null; // Specific item targeting (e.g. "item_iron_broadsword")
        [SerializeField] private int runeBonus = 5;
        [SerializeField] private int priority = 0;

        [Header("Visual Feedback")]
        [SerializeField] private Color synergyColor = new Color(1f, 0.45f, 0.1f, 1f);

        public string SynergyId => synergyId;
        public string DisplayName => displayName;
        public string Description => description;
        public ElementType RequiredElement => requiredElement;
        public ItemCategory RequiredCategory => requiredCategory;
        public string TargetItemId => targetItemId;
        public int RuneBonus => runeBonus;
        public int Priority => priority;
        public Color SynergyColor => synergyColor;
        public bool IsSpecificItemSynergy => !string.IsNullOrEmpty(targetItemId);

        public void Initialize(
            string id, 
            string name, 
            string desc, 
            ElementType elem, 
            ItemCategory cat, 
            int bonus, 
            Color color,
            int prio = 0,
            string specificItem = null)
        {
            synergyId = id;
            displayName = name;
            description = desc;
            requiredElement = elem;
            requiredCategory = cat;
            runeBonus = bonus;
            synergyColor = color;
            targetItemId = specificItem;
            // Default priority: specific item synergies get 100 priority, generic category synergies get 0
            priority = prio != 0 ? prio : (!string.IsNullOrEmpty(specificItem) ? 100 : 0);
        }

        public bool IsMatch(RuneData rune, ItemDataSO itemData)
        {
            if (rune == null || itemData == null) return false;
            if (rune.Element != requiredElement) return false;

            if (!string.IsNullOrEmpty(targetItemId))
            {
                return itemData.ItemId == targetItemId;
            }

            return itemData.Category == requiredCategory;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(synergyId))
            {
                error = "Synergy ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (runeBonus < 0)
            {
                error = "Rune Bonus cannot be negative.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
