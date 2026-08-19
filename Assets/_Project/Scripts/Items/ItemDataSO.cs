using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Items
{
    /// <summary>
    /// Static ScriptableObject definition for an Item in Lattirune.
    /// Stores immutable catalog metadata and footprint definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "Item_", menuName = "Lattirune/Data/Item")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId = "item_sword_01";
        [SerializeField] private string displayName = "Training Sword";
        [SerializeField] [TextArea(2, 4)] private string description = "A reliable baseline weapon.";

        [Header("Classification & Shape")]
        [SerializeField] private ItemCategory category = ItemCategory.Weapon;
        [SerializeField] private Vector2Int baseDimensions = new Vector2Int(1, 2);
        [SerializeField] private bool rotationAllowed = true;

        [Header("Visual Representation")]
        [SerializeField] private Color placeholderColor = new Color(0.9f, 0.5f, 0.1f, 1f);
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public Vector2Int BaseDimensions => baseDimensions;
        public bool RotationAllowed => rotationAllowed;
        public Color PlaceholderColor => placeholderColor;
        public Sprite Icon => icon;

        public void Initialize(
            string id, 
            string name, 
            string desc, 
            ItemCategory cat, 
            Vector2Int dims, 
            bool canRotate, 
            Color color, 
            Sprite itemIcon = null)
        {
            itemId = id;
            displayName = name;
            description = desc;
            category = cat;
            baseDimensions = new Vector2Int(Mathf.Max(1, dims.x), Mathf.Max(1, dims.y));
            rotationAllowed = canRotate;
            placeholderColor = color;
            icon = itemIcon;
        }

        public bool IsValid(out string errorReason)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                errorReason = "Item ID cannot be null or empty.";
                return false;
            }

            if (baseDimensions.x <= 0 || baseDimensions.y <= 0)
            {
                errorReason = $"Invalid footprint dimensions ({baseDimensions.x}x{baseDimensions.y}). Dimensions must be >= 1.";
                return false;
            }

            if (baseDimensions.x > LatticeGrid.WIDTH || baseDimensions.y > LatticeGrid.HEIGHT)
            {
                errorReason = $"Footprint ({baseDimensions.x}x{baseDimensions.y}) exceeds grid dimensions (5x5).";
                return false;
            }

            errorReason = null;
            return true;
        }

        private void OnValidate()
        {
            if (baseDimensions.x < 1) baseDimensions.x = 1;
            if (baseDimensions.y < 1) baseDimensions.y = 1;
        }
    }
}
