using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    /// <summary>
    /// Data-driven definition for a magical Rune.
    /// Stores core directional conduit properties and elemental affinity for the 5x5 LatticeGrid.
    /// Supports Cardinal, Crossfire (Cross), Refracting (Split), and Omnidirectional (Omni) emitter modes.
    /// </summary>
    [CreateAssetMenu(fileName = "Rune_", menuName = "Lattirune/Data/Rune")]
    public class RuneData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string runeId = "fire_rune";
        [SerializeField] private string displayName = "Fire Rune";

        [Header("Elemental Affinity")]
        [SerializeField] private ElementType element = ElementType.Fire;

        [Header("Conduit Properties")]
        [SerializeField] private ConduitDirection direction = ConduitDirection.North;
        [SerializeField] [Range(1, 5)] private int range = 5;
        [SerializeField] private bool isActive = true;

        public string RuneId => runeId;
        public string DisplayName => displayName;
        public ElementType Element => element;
        public ConduitDirection Direction => direction;
        public int Range => range;
        public bool IsActive => isActive;

        public void Initialize(
            string id, 
            string name, 
            ConduitDirection dir, 
            ElementType elem = ElementType.Fire, 
            int maxRange = 5, 
            bool active = true)
        {
            runeId = id;
            displayName = name;
            direction = dir;
            element = elem;
            range = Mathf.Clamp(maxRange, 1, 5);
            isActive = active;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(runeId))
            {
                error = "Rune ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (direction == ConduitDirection.None)
            {
                error = "Conduit direction must be specified.";
                return false;
            }
            if (range < 1 || range > 5)
            {
                error = "Rune range must be between 1 and 5.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
