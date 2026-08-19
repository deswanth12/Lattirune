using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    /// <summary>
    /// Data-driven definition for a magical Rune.
    /// Stores core directional conduit properties for the 5x5 LatticeGrid.
    /// </summary>
    [CreateAssetMenu(fileName = "Rune_", menuName = "Lattirune/Data/Rune")]
    public class RuneData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string runeId = "rune_ember_east";
        [SerializeField] private string displayName = "Ember Rune";

        [Header("Conduit Properties")]
        [SerializeField] private ConduitDirection direction = ConduitDirection.East;
        [SerializeField] [Range(1, 5)] private int range = 5;
        [SerializeField] private bool isActive = true;

        public string RuneId => runeId;
        public string DisplayName => displayName;
        public ConduitDirection Direction => direction;
        public int Range => range;
        public bool IsActive => isActive;

        public void Initialize(string id, string name, ConduitDirection dir, int maxRange = 5, bool active = true)
        {
            runeId = id;
            displayName = name;
            direction = dir;
            range = Mathf.Clamp(maxRange, 1, 5);
            isActive = active;
        }
    }
}
