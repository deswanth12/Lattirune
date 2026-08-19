using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;

namespace Lattirune.Combat
{
    /// <summary>
    /// Player combatant that dynamically derives attack power, armor, and rune bonuses from items placed on the LatticeGrid.
    /// </summary>
    public class PlayerCombatant : Combatant
    {
        [Header("Player Combat Stats")]
        [SerializeField] private int baseAttackDamage = 10;
        [SerializeField] private int activeRuneBonus = 0;
        [SerializeField] private bool hasActiveSynergy = false;

        public const int DEFAULT_FIRE_SYNERGY_BONUS = 5;
        public const int DEFAULT_BASE_WEAPON_DAMAGE = 10;
        public const int DEFAULT_GUARD_PLATE_ARMOR = 4;

        public int BaseAttackDamage => baseAttackDamage;
        public int ActiveRuneBonus => activeRuneBonus;
        public bool HasActiveSynergy => hasActiveSynergy;

        public void SetupDefaultPlayer(int initialHp = 100)
        {
            Initialize("Hero", initialHp, baseArmor: 0, interval: 1.2f);
        }

        /// <summary>
        /// Recomputes combat statistics from the items currently placed on the LatticeGrid.
        /// </summary>
        public void UpdateStatsFromBuild(IReadOnlyList<ItemInstance> items)
        {
            int calculatedDamage = 0;
            int calculatedArmor = 0;
            int calculatedRuneBonus = 0;
            bool foundSynergy = false;

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item == null || !item.IsPlacedOnGrid || item.Data == null) continue;

                    // Weapon Damage & Synergy Bonus
                    if (item.Data.Category == ItemCategory.Weapon)
                    {
                        calculatedDamage += DEFAULT_BASE_WEAPON_DAMAGE;

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
                        calculatedArmor += DEFAULT_GUARD_PLATE_ARMOR;

                        if (item.HasActiveSynergy)
                        {
                            foundSynergy = true;
                            if (item.ActiveSynergyId == "ice_shield")
                            {
                                calculatedArmor += 4;
                            }
                        }
                    }
                }
            }

            // If no weapon placed, default to 1 unarmed damage
            baseAttackDamage = Mathf.Max(1, calculatedDamage);
            activeRuneBonus = calculatedRuneBonus;
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
