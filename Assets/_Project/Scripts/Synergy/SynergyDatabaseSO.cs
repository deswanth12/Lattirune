using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Data-driven database holding all registered elemental synergy rules.
    /// Provides fast, deterministic rule evaluation and validation against duplicate IDs.
    /// </summary>
    [CreateAssetMenu(fileName = "SynergyDatabase", menuName = "Lattirune/Data/Synergy Database")]
    public class SynergyDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<SynergyDefinitionSO> synergyDefinitions = new List<SynergyDefinitionSO>();

        public IReadOnlyList<SynergyDefinitionSO> Definitions => synergyDefinitions;
        public int Count => synergyDefinitions != null ? synergyDefinitions.Count : 0;

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

        /// <summary>
        /// Creates a complete prototype 5-element synergy matrix matching PLAN.md Phase 2 requirements.
        /// </summary>
        public static SynergyDatabaseSO CreateDefaultDatabase()
        {
            SynergyDatabaseSO db = ScriptableObject.CreateInstance<SynergyDatabaseSO>();
            List<SynergyDefinitionSO> list = new List<SynergyDefinitionSO>();

            // 1. Fire + Weapon (Flamebound Edge)
            SynergyDefinitionSO fireSword = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            fireSword.Initialize(
                "fire_sword", 
                "Flamebound Edge", 
                "A Fire Rune connected to a Weapon adds +5 Rune Bonus damage.", 
                ElementType.Fire, 
                ItemCategory.Weapon, 
                5, 
                new Color(1f, 0.45f, 0.1f, 1f)
            );
            list.Add(fireSword);

            // 2. Ice + Shield (Glacial Bastion)
            SynergyDefinitionSO iceShield = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            iceShield.Initialize(
                "ice_shield", 
                "Glacial Bastion", 
                "An Ice Rune connected to a Shield adds +4 Armor defense.", 
                ElementType.Ice, 
                ItemCategory.Shield, 
                4, 
                new Color(0.2f, 0.75f, 1.0f, 1f)
            );
            list.Add(iceShield);

            // 3. Lightning + Weapon (Storm Surge)
            SynergyDefinitionSO lightningWeapon = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            lightningWeapon.Initialize(
                "lightning_weapon", 
                "Storm Surge", 
                "A Lightning Rune connected to a Weapon adds +8 Shock damage.", 
                ElementType.Lightning, 
                ItemCategory.Weapon, 
                8, 
                new Color(0.95f, 0.85f, 0.15f, 1f)
            );
            list.Add(lightningWeapon);

            // 4. Poison + Weapon (Venomous Strike)
            SynergyDefinitionSO poisonBlade = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            poisonBlade.Initialize(
                "poison_blade", 
                "Venomous Strike", 
                "A Poison Rune connected to a Weapon adds +3 Poison damage.", 
                ElementType.Poison, 
                ItemCategory.Weapon, 
                3, 
                new Color(0.15f, 0.85f, 0.25f, 1f)
            );
            list.Add(poisonBlade);

            // 5. Light + Relic (Radiant Dawn)
            SynergyDefinitionSO lightRelic = ScriptableObject.CreateInstance<SynergyDefinitionSO>();
            lightRelic.Initialize(
                "light_relic", 
                "Radiant Dawn", 
                "A Light Rune connected to a Relic adds +4 Radiant power.", 
                ElementType.Light, 
                ItemCategory.Relic, 
                4, 
                new Color(1f, 0.92f, 0.45f, 1f)
            );
            list.Add(lightRelic);

            db.Initialize(list);
            return db;
        }
    }
}
