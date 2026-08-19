using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Definition of a valid elemental synergy between a Rune and an Item.
    /// </summary>
    [System.Serializable]
    public class SynergyDefinition
    {
        [SerializeField] private string synergyId = "fire_sword";
        [SerializeField] private string displayName = "Flamebound Edge";
        [SerializeField] private string description = "A Fire Rune connected to a Sword activates the Flamebound Edge synergy.";
        [SerializeField] private ElementType requiredElement = ElementType.Fire;
        [SerializeField] private ItemCategory requiredCategory = ItemCategory.Weapon;
        [SerializeField] private Color synergyColor = new Color(1f, 0.45f, 0.1f, 1f); // Flame Orange

        public string SynergyId => synergyId;
        public string DisplayName => displayName;
        public string Description => description;
        public ElementType RequiredElement => requiredElement;
        public ItemCategory RequiredCategory => requiredCategory;
        public Color SynergyColor => synergyColor;

        public SynergyDefinition(
            string id, 
            string name, 
            string desc, 
            ElementType elem, 
            ItemCategory cat, 
            Color color)
        {
            synergyId = id;
            displayName = name;
            description = desc;
            requiredElement = elem;
            requiredCategory = cat;
            synergyColor = color;
        }

        public bool IsMatch(RuneData rune, ItemDataSO itemData)
        {
            if (rune == null || itemData == null) return false;
            return rune.Element == requiredElement && itemData.Category == requiredCategory;
        }

        /// <summary>
        /// Prototype default Fire Sword (Flamebound Edge) synergy definition.
        /// </summary>
        public static SynergyDefinition CreateDefaultFireSword()
        {
            return new SynergyDefinition(
                "fire_sword",
                "Flamebound Edge",
                "A Fire Rune connected to a Sword activates the Flamebound Edge synergy.",
                ElementType.Fire,
                ItemCategory.Weapon,
                new Color(1f, 0.45f, 0.1f, 1f)
            );
        }
    }
}
