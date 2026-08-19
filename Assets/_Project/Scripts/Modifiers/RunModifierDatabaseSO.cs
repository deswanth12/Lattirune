using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Modifiers
{
    /// <summary>
    /// Canonical ScriptableObject repository for all data-driven run modifiers in Lattirune 1.1.
    /// </summary>
    [CreateAssetMenu(fileName = "RunModifierDatabase", menuName = "Lattirune/Modifiers/Run Modifier Database")]
    public class RunModifierDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<RunModifierDefinitionSO> modifiers = new List<RunModifierDefinitionSO>();

        public IReadOnlyList<RunModifierDefinitionSO> AllModifiers => modifiers;
        public int Count => modifiers != null ? modifiers.Count : 0;

        public void Initialize(List<RunModifierDefinitionSO> list)
        {
            modifiers = list ?? new List<RunModifierDefinitionSO>();
        }

        public RunModifierDefinitionSO GetModifier(string id)
        {
            if (string.IsNullOrEmpty(id) || modifiers == null) return null;
            return modifiers.Find(m => m != null && m.ModifierId == id);
        }

        public bool HasModifier(string id)
        {
            return GetModifier(id) != null;
        }

        public static RunModifierDatabaseSO CreateCanonicalDatabase()
        {
            var db = CreateInstance<RunModifierDatabaseSO>();
            var list = new List<RunModifierDefinitionSO>();

            // 1. Sharpened Runes (Common, Positive, +15% damage)
            var m1 = CreateInstance<RunModifierDefinitionSO>();
            m1.Initialize("mod_sharpened_runes", "Sharpened Runes", "Increases all physical and rune attack damage by 15%.", RunModifierRarity.Common, RunModifierPolarity.Positive, RunModifierType.DamageMultiplier, 0.15f, Color.red);
            list.Add(m1);

            // 2. Elemental Surge (Uncommon, Positive, +25% elemental damage)
            var m2 = CreateInstance<RunModifierDefinitionSO>();
            m2.Initialize("mod_elemental_surge", "Elemental Surge", "Boosts elemental reactions and conduit damage by 25%.", RunModifierRarity.Uncommon, RunModifierPolarity.Positive, RunModifierType.ElementalDamageBonus, 0.25f, Color.cyan);
            list.Add(m2);

            // 3. Midas Touch (Rare, Positive, +50% Gold drops)
            var m3 = CreateInstance<RunModifierDefinitionSO>();
            m3.Initialize("mod_midas_touch", "Midas Touch", "Increases all in-run Gold rewards by 50%.", RunModifierRarity.Rare, RunModifierPolarity.Positive, RunModifierType.GoldMultiplier, 0.50f, Color.yellow);
            list.Add(m3);

            // 4. Glass Cannon (Epic, Hybrid, +50% Damage, +30% Enemy Health)
            var m4 = CreateInstance<RunModifierDefinitionSO>();
            m4.Initialize("mod_glass_cannon", "Glass Cannon", "Increases damage by 50%, but enemies have 30% more HP.", RunModifierRarity.Epic, RunModifierPolarity.Hybrid, RunModifierType.DamageMultiplier, 0.50f, Color.magenta);
            list.Add(m4);

            // 5. Curse of Vulnerability (Curse, Negative, -20% Defense)
            var m5 = CreateInstance<RunModifierDefinitionSO>();
            m5.Initialize("mod_curse_vulnerability", "Curse of Vulnerability", "Reduces hero defense and armor effectiveness by 20%.", RunModifierRarity.Curse, RunModifierPolarity.Negative, RunModifierType.CurseOfVulnerability, -0.20f, Color.gray);
            list.Add(m5);

            db.Initialize(list);
            return db;
        }
    }
}
