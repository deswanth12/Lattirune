using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    public enum RuneElement
    {
        Fire = 0,
        Ice = 1,
        Lightning = 2,
        Poison = 3,
        Void = 4,
        Light = 5,
        Force = 6,
        Earth = 7,
        Shadow = 8,
        Wind = 9
    }

    /// <summary>
    /// Data-driven ScriptableObject definition for a magical Rune.
    /// Stores core directional conduit properties, elemental affinity, and combat bonuses for the 5x5 LatticeGrid.
    /// Supports Cardinal, Crossfire (Cross), Refracting (Split), and Omnidirectional (Omni) emitter modes.
    /// Derived strictly from PLAN.md Section 5.1.
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

        [Header("Combat & Status Attributes (PLAN.md Section 5.1)")]
        [SerializeField] private int flatDamageBonus = 0;
        [SerializeField] private float burnDamagePerSec = 0f;
        [SerializeField] private float burnDuration = 0f;
        [SerializeField] private float speedReductionPercent = 0f;
        [SerializeField] private float chainChance = 0f;
        [SerializeField] private int poisonStacksPerSec = 0;
        [SerializeField] private int shieldBonus = 0;
        [SerializeField] private float lifestealPercent = 0f;
        [SerializeField] private float hastePercent = 0f;

        public string RuneId => runeId;
        public string DisplayName => displayName;
        public string RuneName => displayName;
        public ElementType Element => element;
        public ConduitDirection Direction => direction;
        public int Range => range;
        public bool IsActive => isActive;

        public int FlatDamageBonus => flatDamageBonus;
        public float BurnDamagePerSec => burnDamagePerSec;
        public float BurnDuration => burnDuration;
        public float SpeedReductionPercent => speedReductionPercent;
        public float ChainChance => chainChance;
        public int PoisonStacksPerSec => poisonStacksPerSec;
        public int ShieldBonus => shieldBonus;
        public int StartingShieldBonus => shieldBonus;
        public float LifestealPercent => lifestealPercent;
        public float LifestealRatio => lifestealPercent;
        public float HastePercent => hastePercent;
        public float AttackSpeedBonus => hastePercent;

        public void Initialize(
            string id, 
            string name, 
            ConduitDirection dir, 
            ElementType elem = ElementType.Fire, 
            int maxRange = 5, 
            bool active = true,
            int damageBonus = 0,
            float burnDmg = 0f,
            float burnDur = 0f,
            float speedReduction = 0f,
            float chain = 0f,
            int poisonRate = 0,
            int shield = 0,
            float lifesteal = 0f,
            float haste = 0f)
        {
            runeId = id;
            displayName = name;
            direction = dir;
            element = elem;
            range = Mathf.Clamp(maxRange, 1, 5);
            isActive = active;

            flatDamageBonus = Mathf.Max(0, damageBonus);
            burnDamagePerSec = Mathf.Max(0f, burnDmg);
            burnDuration = Mathf.Max(0f, burnDur);
            speedReductionPercent = Mathf.Clamp01(speedReduction);
            chainChance = Mathf.Clamp01(chain);
            poisonStacksPerSec = Mathf.Max(0, poisonRate);
            shieldBonus = Mathf.Max(0, shield);
            lifestealPercent = Mathf.Clamp01(lifesteal);
            hastePercent = Mathf.Clamp01(haste);
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
