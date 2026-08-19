using System;
using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Enumeration of unique grid-disrupting and combat traits possessed by dungeon monsters.
    /// Derived strictly from PLAN.md Section 10 (Enemy & Boss Architecture).
    /// </summary>
    public enum EnemyTraitType
    {
        None,
        GoldSteal,          // Steals gold on hit (Goblin Thief)
        DamageReflect,      // Reflects percentage of received physical damage back to attacker (Armored Skeleton)
        ApplyPoisonOnHit,   // Inflicts ticking poison stacks bypassing HP/Armor (Venomous Spider)
        DisableBagSlot,     // Acid spit: temporarily disables 1 bag slot during battle (Acid Slime)
        SummonMinions       // Summons reinforcements periodically (Necromancer)
    }

    /// <summary>
    /// Specialized affixes randomly applied to Elite encounters and Endless mode champions.
    /// </summary>
    public enum EliteAffixType
    {
        None,
        Vampiric,     // Heals 25% of damage dealt
        Juggernaut,   // +40% Max HP, +10 Armor
        Frenzied,     // +35% Attack Speed (faster cooldown)
        MoltenAura,   // Reflects 25% damage back as flame thorns
        ToxicThorns   // Inflicts virulent poison on hit
    }

    /// <summary>
    /// Static ScriptableObject defining an enemy combat/grid trait.
    /// Configures mechanics and values without holding runtime state.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyTrait_", menuName = "Lattirune/Combat/Enemy Trait Definition")]
    public class EnemyTraitDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string traitId = "trait_reflect";
        [SerializeField] private string displayName = "Damage Reflection";

        [Header("Trait Configuration")]
        [SerializeField] private EnemyTraitType traitType = EnemyTraitType.DamageReflect;
        [SerializeField] private float traitValue = 0.20f; // e.g. 0.20 for 20% reflect, 3 for 3 gold, 2 for 2 poison stacks
        [SerializeField] private float triggerInterval = 4.0f; // Interval in seconds for periodic traits

        public string TraitId => traitId;
        public string DisplayName => displayName;
        public EnemyTraitType TraitType => traitType;
        public float TraitValue => traitValue;
        public float TriggerInterval => triggerInterval;

        public void Initialize(string id, string name, EnemyTraitType type, float value, float interval = 0f)
        {
            traitId = id;
            displayName = name;
            traitType = type;
            traitValue = value;
            triggerInterval = interval;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(traitId))
            {
                error = "Trait ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(displayName))
            {
                error = "Display Name cannot be empty.";
                return false;
            }
            if (traitType == EnemyTraitType.None)
            {
                error = "Trait type cannot be None.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
