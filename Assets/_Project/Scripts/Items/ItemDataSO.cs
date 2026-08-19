using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Items
{
    /// <summary>
    /// Static ScriptableObject definition for an Item in Lattirune.
    /// Stores immutable catalog metadata, footprint definitions, and combat attributes.
    /// Derived strictly from PLAN.md Section 4.1 & Section 6.1.
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
        [SerializeField] private bool isLShape = false;
        [SerializeField] private bool isCursed = false;

        [Header("Combat & Stat Attributes (PLAN.md Section 6.1)")]
        [SerializeField] private int baseDamage = 0;
        [SerializeField] private float cooldown = 1.5f;
        [SerializeField] private int shieldValue = 0;
        [SerializeField] private int maxHpBonus = 0;
        [SerializeField] private int armorPierce = 0;
        [SerializeField] private int thornsDamage = 0;
        [SerializeField] private float critBonus = 0f;
        [SerializeField] private int flatDamageBonus = 0;
        [SerializeField] private float elementalRuneDamageModifier = 0f;
        [SerializeField] private int damageTakenReduction = 0;

        [Header("Visual Representation")]
        [SerializeField] private Color placeholderColor = new Color(0.9f, 0.5f, 0.1f, 1f);
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public Vector2Int BaseDimensions => baseDimensions;
        public bool RotationAllowed => rotationAllowed;
        public bool IsLShape => isLShape;
        public bool IsCursed => isCursed;
        public int BaseDamage => baseDamage;
        public float Cooldown => cooldown;
        public int ShieldValue => shieldValue;
        public int MaxHpBonus => maxHpBonus;
        public int ArmorPierce => armorPierce;
        public int ThornsDamage => thornsDamage;
        public float CritBonus => critBonus;
        public int FlatDamageBonus => flatDamageBonus;
        public float ElementalRuneDamageModifier => elementalRuneDamageModifier;
        public int DamageTakenReduction => damageTakenReduction;
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
            Sprite itemIcon = null,
            int damage = 0,
            float cd = 1.5f,
            int shield = 0,
            int hpBonus = 0,
            int pierce = 0,
            int thorns = 0,
            float crit = 0f,
            int flatBonus = 0,
            float runeMod = 0f,
            int dmgReduction = 0,
            bool lShape = false,
            bool cursed = false)
        {
            itemId = id;
            displayName = name;
            description = desc;
            category = cat;
            baseDimensions = new Vector2Int(Mathf.Max(1, dims.x), Mathf.Max(1, dims.y));
            rotationAllowed = canRotate;
            placeholderColor = color;
            icon = itemIcon;

            baseDamage = Mathf.Max(0, damage);
            cooldown = Mathf.Max(0.1f, cd);
            shieldValue = Mathf.Max(0, shield);
            maxHpBonus = Mathf.Max(0, hpBonus);
            armorPierce = Mathf.Max(0, pierce);
            thornsDamage = Mathf.Max(0, thorns);
            critBonus = Mathf.Max(0f, crit);
            flatDamageBonus = Mathf.Max(0, flatBonus);
            elementalRuneDamageModifier = runeMod;
            damageTakenReduction = Mathf.Max(0, dmgReduction);
            isLShape = lShape;
            isCursed = cursed;
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
