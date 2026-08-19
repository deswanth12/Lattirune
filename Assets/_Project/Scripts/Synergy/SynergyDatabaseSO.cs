using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Data-driven database holding all registered elemental synergy rules and master item combinations.
    /// Provides fast, deterministic rule evaluation and priority resolution (specific item combos override generic categories).
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyDatabase", menuName = "Lattirune/Data/Synergy Database")]
    public class SynergyDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<SynergyDefinitionSO> synergyDefinitions = new List<SynergyDefinitionSO>();

        public IReadOnlyList<SynergyDefinitionSO> Definitions => synergyDefinitions;
        public IReadOnlyList<SynergyDefinitionSO> AllSynergies => synergyDefinitions;
        public int Count => synergyDefinitions != null ? synergyDefinitions.Count : 0;
        public int TotalSynergyCount => Count;

        public void Initialize(List<SynergyDefinitionSO> definitions)
        {
            synergyDefinitions = definitions ?? new List<SynergyDefinitionSO>();
        }

        public void Register(SynergyDefinitionSO def)
        {
            if (def == null) return;
            if (synergyDefinitions == null) synergyDefinitions = new List<SynergyDefinitionSO>();

            if (!synergyDefinitions.Exists(d => d != null && d.SynergyId == def.SynergyId))
            {
                synergyDefinitions.Add(def);
            }
        }

        public SynergyDefinitionSO GetById(string synergyId)
        {
            if (string.IsNullOrEmpty(synergyId) || synergyDefinitions == null) return null;
            return synergyDefinitions.Find(d => d != null && d.SynergyId == synergyId);
        }

        public bool HasSynergy(string synergyId)
        {
            return GetById(synergyId) != null;
        }

        public SynergyDefinitionSO FindMatchingDefinition(RuneData rune, ItemDataSO itemData)
        {
            if (rune == null || itemData == null || synergyDefinitions == null) return null;

            SynergyDefinitionSO bestMatch = null;
            for (int i = 0; i < synergyDefinitions.Count; i++)
            {
                SynergyDefinitionSO def = synergyDefinitions[i];
                if (def != null && def.IsMatch(rune, itemData))
                {
                    if (bestMatch == null || def.Priority > bestMatch.Priority)
                    {
                        bestMatch = def;
                    }
                }
            }

            return bestMatch;
        }

        public bool ValidateDatabase(out List<string> errors)
        {
            errors = new List<string>();
            if (synergyDefinitions == null || synergyDefinitions.Count == 0)
            {
                errors.Add("SynergyDatabase has no registered definitions.");
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>();
            for (int i = 0; i < synergyDefinitions.Count; i++)
            {
                SynergyDefinitionSO def = synergyDefinitions[i];
                if (def == null)
                {
                    errors.Add($"Null synergy definition at index {i}.");
                    continue;
                }

                if (!def.IsValid(out string defErr))
                {
                    errors.Add($"Definition at index {i} is invalid: {defErr}");
                }

                if (seenIds.Contains(def.SynergyId))
                {
                    errors.Add($"Duplicate Synergy ID '{def.SynergyId}' found at index {i}.");
                }
                else
                {
                    seenIds.Add(def.SynergyId);
                }
            }

            return errors.Count == 0;
        }

        public bool IsValid() => ValidateDatabase(out _);
        public bool IsValid(out string error)
        {
            bool valid = ValidateDatabase(out var list);
            error = valid ? null : string.Join("; ", list);
            return valid;
        }

        public static SynergyDatabaseSO CreateCanonicalDatabase() => CreateDefaultDatabase();
        public static SynergyDatabaseSO CreateCanonicalSynergyDatabase() => CreateDefaultDatabase();

        /// <summary>
        /// Creates a complete prototype 5-element synergy matrix + 5 Master Item Combinations matching PLAN.md Section 7.1.
        /// </summary>
        public static SynergyDatabaseSO CreateDefaultDatabase()
        {
            SynergyDatabaseSO db = ScriptableObject.CreateInstance<SynergyDatabaseSO>();
            List<SynergyDefinitionSO> list = new List<SynergyDefinitionSO>();

            // ==========================================

            // ==========================================
            // 1. MASTER ITEM COMBINATIONS (PLAN.md Section 7.1)
            // ==========================================

            // Combo 1: Flaming Blade (Ember Rune + Iron Broadsword)
            SynergyDefinitionSO flamingBlade = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            flamingBlade.Initialize(
                "combo_flaming_blade",
                "Flaming Blade",
                "Ember Rune + Iron Broadsword: Deals +6 Fire Damage and applies Burn.",
                ElementType.Fire,
                ItemCategory.Weapon,
                6,
                new Color(1f, 0.35f, 0.05f, 1f),
                prio: 100,
                specificItem: "item_iron_broadsword"
            );
            list.Add(flamingBlade);

            // Combo 2: Venom Shiv (Venom Rune + Rusty Dagger)
            SynergyDefinitionSO venomShiv = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            venomShiv.Initialize(
                "combo_venom_shiv",
                "Venom Shiv",
                "Venom Rune + Rusty Dagger: Applies 2 Poison stacks every 0.8s.",
                ElementType.Poison,
                ItemCategory.Weapon,
                3,
                new Color(0.1f, 0.9f, 0.2f, 1f),
                prio: 100,
                specificItem: "item_rusty_dagger"
            );
            list.Add(venomShiv);

            // Combo 3: Thunder Bow (Spark Rune + Shortbow)
            SynergyDefinitionSO thunderBow = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            thunderBow.Initialize(
                "combo_thunder_bow",
                "Thunder Bow",
                "Spark Rune + Shortbow: Arrows chain 8 Lightning Damage to backline targets.",
                ElementType.Lightning,
                ItemCategory.Weapon,
                8,
                new Color(0.95f, 0.9f, 0.1f, 1f),
                prio: 100,
                specificItem: "item_shortbow"
            );
            list.Add(thunderBow);

            // Combo 4: Molten Wall (Ember Rune + Iron Tower Shield)
            SynergyDefinitionSO moltenWall = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            moltenWall.Initialize(
                "combo_molten_wall",
                "Molten Wall",
                "Ember Rune + Iron Tower Shield: Attackers take 8 Burn Damage upon striking shield.",
                ElementType.Fire,
                ItemCategory.Shield,
                8,
                new Color(1f, 0.25f, 0.1f, 1f),
                prio: 100,
                specificItem: "item_iron_tower_shield"
            );
            list.Add(moltenWall);

            // Combo 5: Shatterstrike (Frost Rune + Battleaxe)
            SynergyDefinitionSO shatterstrike = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            shatterstrike.Initialize(
                "combo_shatterstrike",
                "Shatterstrike",
                "Frost Rune + Battleaxe: Axe deals 2x damage against chilled/frozen targets.",
                ElementType.Ice,
                ItemCategory.Weapon,
                6,
                new Color(0.3f, 0.8f, 1f, 1f),
                prio: 100,
                specificItem: "item_battleaxe"
            );
            list.Add(shatterstrike);

            // ==========================================
            // 2. GENERIC 5-ELEMENT MATRIX FALLBACKS
            // ==========================================

            // Fire + Weapon (Flamebound Edge)
            SynergyDefinitionSO fireSword = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            fireSword.Initialize(
                "fire_sword", 
                "Flamebound Edge", 
                "A Fire Rune connected to a Weapon adds +5 Rune Bonus damage.", 
                ElementType.Fire, 
                ItemCategory.Weapon, 
                5, 
                new Color(1f, 0.45f, 0.1f, 1f),
                prio: 0
            );
            list.Add(fireSword);

            // Ice + Shield (Glacial Bastion)
            SynergyDefinitionSO iceShield = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            iceShield.Initialize(
                "ice_shield", 
                "Glacial Bastion", 
                "An Ice Rune connected to a Shield adds +4 Armor defense.", 
                ElementType.Ice, 
                ItemCategory.Shield, 
                4, 
                new Color(0.2f, 0.75f, 1.0f, 1f),
                prio: 0
            );
            list.Add(iceShield);

            // Lightning + Weapon (Storm Surge)
            SynergyDefinitionSO lightningWeapon = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            lightningWeapon.Initialize(
                "lightning_weapon", 
                "Storm Surge", 
                "A Lightning Rune connected to a Weapon adds +8 Shock damage.", 
                ElementType.Lightning, 
                ItemCategory.Weapon, 
                8, 
                new Color(0.95f, 0.85f, 0.15f, 1f),
                prio: 0
            );
            list.Add(lightningWeapon);

            // Poison + Weapon (Venomous Strike)
            SynergyDefinitionSO poisonBlade = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            poisonBlade.Initialize(
                "poison_blade", 
                "Venomous Strike", 
                "A Poison Rune connected to a Weapon adds +3 Poison damage.", 
                ElementType.Poison, 
                ItemCategory.Weapon, 
                3, 
                new Color(0.15f, 0.85f, 0.25f, 1f),
                prio: 0
            );
            list.Add(poisonBlade);

            // Light + Relic (Radiant Dawn)
            SynergyDefinitionSO lightRelic = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            lightRelic.Initialize(
                "light_relic", 
                "Radiant Dawn", 
                "A Light Rune connected to a Relic adds +4 Radiant power.", 
                ElementType.Light, 
                ItemCategory.Relic, 
                4, 
                new Color(1f, 0.92f, 0.45f, 1f),
                prio: 0
            );
            list.Add(lightRelic);

            db.Initialize(list);
            return db;
        }
    }
}
