using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;

namespace Lattirune.Combat
{
    /// <summary>
    /// Player combatant that dynamically derives attack power, armor, and rune bonuses from items placed on the LatticeGrid.
    /// Strictly adheres to PLAN.md Section 6.1 and Section 9.2.
    /// </summary>
    public class PlayerCombatant : Combatant
    {
        [Header("Player Combat Stats")]
        [SerializeField] private int baseAttackDamage = 10;
        [SerializeField] private int activeRuneBonus = 0;
        [SerializeField] private bool hasActiveSynergy = false;
        [SerializeField] private float activeCritBonus = 0f;

        public const int DEFAULT_FIRE_SYNERGY_BONUS = 5;
        public const int DEFAULT_BASE_WEAPON_DAMAGE = 10;
        public const int DEFAULT_GUARD_PLATE_ARMOR = 4;

        public int BaseAttackDamage => baseAttackDamage;
        public int ActiveRuneBonus => activeRuneBonus;
        public bool HasActiveSynergy => hasActiveSynergy;
        public float ActiveCritBonus => activeCritBonus;

        public void SetupDefaultPlayer(int initialHp = 100)
        {
            Initialize("Hero", initialHp, baseArmor: 0, interval: 1.0f);
        }

        /// <summary>
        /// Recomputes combat statistics from the items currently placed on the LatticeGrid.
        /// </summary>
        public void UpdateStatsFromBuild(IReadOnlyList<ItemInstance> items)
        {
            int calculatedDamage = 0;
            int calculatedArmor = 0;
            int calculatedRuneBonus = 0;
            float calculatedCrit = 0f;
            int calculatedHpBonus = 0;
            bool foundSynergy = false;

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item == null || !item.IsPlacedOnGrid || item.Data == null) continue;

                    // Weapon Damage & Synergy Bonus
                    if (item.Data.Category == ItemCategory.Weapon)
                    {
                        int weaponDmg = item.Data.BaseDamage > 0 ? item.Data.BaseDamage : DEFAULT_BASE_WEAPON_DAMAGE;
                        calculatedDamage += weaponDmg;

                        if (item.HasActiveSynergy)
                        {
                            foundSynergy = true;
                            if (item.ActiveSynergyId == "fire_sword")
                            {
                                calculatedRuneBonus += DEFAULT_FIRE_SYNERGY_BONUS;
                            }
                            else if (item.ActiveSynergyId == "lightning_weapon")
                            {
                                calculatedRuneBonus += 8;
                            }
                            else if (item.ActiveSynergyId == "poison_blade")
                            {
                                calculatedRuneBonus += 3;
                            }
                        }
                    }
                    // Shield Defense
                    else if (item.Data.Category == ItemCategory.Shield)
                    {
                        int shieldDef = item.Data.ShieldValue > 0 ? item.Data.ShieldValue : DEFAULT_GUARD_PLATE_ARMOR;
                        calculatedArmor += shieldDef;

                        if (item.HasActiveSynergy)
                        {
                            foundSynergy = true;
                            if (item.ActiveSynergyId == "ice_shield")
                            {
                                calculatedArmor += 4;
                            }
                        }
                    }
                    // Armor / Robes
                    else if (item.Data.Category == ItemCategory.Armor)
                    {
                        calculatedHpBonus += item.Data.MaxHpBonus;
                        if (item.Data.DamageTakenReduction > 0)
                        {
                            calculatedArmor += item.Data.DamageTakenReduction;
                        }
                    }
                    // Relic Stat Modifiers
                    else if (item.Data.Category == ItemCategory.Relic)
                    {
                        calculatedDamage += item.Data.FlatDamageBonus;
                        calculatedCrit += item.Data.CritBonus;
                    }
                }
            }

            // If no weapon placed, default to baseline 10 hero combat damage
            baseAttackDamage = Mathf.Max(10, calculatedDamage);
            activeRuneBonus = calculatedRuneBonus;
            activeCritBonus = calculatedCrit;
            hasActiveSynergy = foundSynergy;

            SetStats(MaxHp, calculatedArmor, AttackInterval);
        }

        public void SetExplicitStats(int baseDamage, int runeBonus, int armorValue, float interval = 1.2f)
        {
            baseAttackDamage = baseDamage;
            activeRuneBonus = runeBonus;
            hasActiveSynergy = runeBonus > 0;
            SetStats(MaxHp, armorValue, interval);
        }
    }
}
