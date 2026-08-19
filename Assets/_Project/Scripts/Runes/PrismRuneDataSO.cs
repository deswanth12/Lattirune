using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    /// <summary>
    /// Static ScriptableObject defining the Prism Rune refraction and beam-splitting rules.
    /// Splits an incoming orthogonal beam into two perpendicular branches.
    /// </summary>
    [CreateAssetMenu(fileName = "PrismRune_", menuName = "Lattirune/Data/Prism Rune")]
    public class PrismRuneDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string prismId = "prism_rune";
        [SerializeField] private string displayName = "Prism Rune";
        [SerializeField] private ElementType element = ElementType.Light;
        [SerializeField] private ConduitDirection direction = ConduitDirection.Split;

        [Header("Refraction Properties")]
        [SerializeField] private int maxBranchCount = 2;
        [SerializeField] private int maxRefractionDepth = 3;

        public string PrismId => prismId;
        public string DisplayName => displayName;
        public ElementType Element => element;
        public ConduitDirection Direction => direction;
        public int MaxBranchCount => maxBranchCount;
        public int MaxRefractionDepth => maxRefractionDepth;

        public void Initialize(
            string id = "prism_rune",
            string name = "Prism Rune",
            int branchCount = 2,
            int maxDepth = 3)
        {
            prismId = id;
            displayName = name;
            element = ElementType.Light;
            direction = ConduitDirection.Split;
            maxBranchCount = branchCount;
            maxRefractionDepth = maxDepth;
        }

        /// <summary>
        /// Maps an incoming cardinal beam direction to two refracted perpendicular branch directions.
        /// </summary>
        public (ConduitDirection branchA, ConduitDirection branchB) GetSplitDirections(ConduitDirection inputDirection)
        {
            switch (inputDirection)
            {
                case ConduitDirection.East:
                case ConduitDirection.West:
                    return (ConduitDirection.North, ConduitDirection.South);

                case ConduitDirection.North:
                case ConduitDirection.South:
                    return (ConduitDirection.East, ConduitDirection.West);

                default:
                    return (ConduitDirection.North, ConduitDirection.South);
            }
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(prismId))
            {
                error = "Prism ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (maxBranchCount <= 0)
            {
                error = "Branch count must be at least 1.";
                return false;
            }
            if (maxRefractionDepth <= 0)
            {
                error = "Max refraction depth must be at least 1.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
