using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    /// <summary>
    /// Data-driven definition for a magical Rune.
    /// Stores core directional conduit properties and elemental affinity for the 5x5 LatticeGrid.
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
    }
}
