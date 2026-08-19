using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Combat.Effects
{
    /// <summary>
    /// Data-driven database mapping elemental reaction IDs to static combat effect definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatEffectDatabase", menuName = "Lattirune/Data/Combat Effect Database")]
    public class CombatEffectDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<CombatEffectDefinitionSO> effectDefinitions = new List<CombatEffectDefinitionSO>();

        public IReadOnlyList<CombatEffectDefinitionSO> Definitions => effectDefinitions;
        public int Count => effectDefinitions != null ? effectDefinitions.Count : 0;

        public void Initialize(List<CombatEffectDefinitionSO> list)
        {
            effectDefinitions = list ?? new List<CombatEffectDefinitionSO>();
        }

        public void Register(CombatEffectDefinitionSO def)
        {
            if (def == null) return;
            if (effectDefinitions == null) effectDefinitions = new List<CombatEffectDefinitionSO>();

            if (!effectDefinitions.Exists(d => d != null && d.EffectId == def.EffectId))
            {
                effectDefinitions.Add(def);
            }
        }

        public CombatEffectDefinitionSO GetByEffectId(string effectId)
        {
            if (string.IsNullOrEmpty(effectId) || effectDefinitions == null) return null;
            return effectDefinitions.Find(d => d != null && d.EffectId == effectId);
        }

        public CombatEffectDefinitionSO GetByReactionId(string reactionId)
        {
            if (string.IsNullOrEmpty(reactionId) || effectDefinitions == null) return null;
            return effectDefinitions.Find(d => d != null && d.MappedReactionId == reactionId);
        }

        public bool ValidateDatabase(out List<string> errors)
        {
            errors = new List<string>();
            if (effectDefinitions == null || effectDefinitions.Count == 0)
            {
                errors.Add("CombatEffectDatabase has no registered definitions.");
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>();
            for (int i = 0; i < effectDefinitions.Count; i++)
            {
                CombatEffectDefinitionSO def = effectDefinitions[i];
                if (def == null)
                {
                    errors.Add($"Null effect definition at index {i}.");
                    continue;
                }

                if (!def.IsValid(out string defErr))
                {
                    errors.Add($"Effect definition at index {i} is invalid: {defErr}");
                }

                if (seenIds.Contains(def.EffectId))
                {
                    errors.Add($"Duplicate Effect ID '{def.EffectId}' found at index {i}.");
                }
                else
                {
                    seenIds.Add(def.EffectId);
                }
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Creates a complete prototype combat effect database implementing the 5 canonical PLAN.md reactions.
        /// </summary>
        public static CombatEffectDatabaseSO CreateDefaultDatabase()
        {
            CombatEffectDatabaseSO db = ScriptableObject.CreateInstance<CombatEffectDatabaseSO>();
            List<CombatEffectDefinitionSO> list = new List<CombatEffectDefinitionSO>();

            // 1. Steam -> 25% Blind/Miss
            CombatEffectDefinitionSO steam = ScriptableObject.CreateInstance<CombatEffectDefinitionSO>();
            steam.Initialize(
                "effect_steam_blind",
                "Steam Blind",
                "reaction_steam",
                CombatEffectType.AttackModifier,
                4.0f,
                1.0f,
                0.25f,
                "Inflicts 25% Enemy Blind/Miss chance.",
                new Color(0.85f, 0.95f, 1f, 0.9f)
            );
            list.Add(steam);

            // 2. Plasma -> 18 Dmg/s Continuous Ray (9 dmg every 0.5s)
            CombatEffectDefinitionSO plasma = ScriptableObject.CreateInstance<CombatEffectDefinitionSO>();
            plasma.Initialize(
                "effect_plasma_ray",
                "Plasma Ray",
                "reaction_plasma",
                CombatEffectType.DamageOverTime,
                3.0f,
                0.5f,
                9.0f,
                "Continuous plasma stream dealing 18 Dmg/s.",
                new Color(1f, 0.25f, 0.75f, 0.9f)
            );
            list.Add(plasma);

            // 3. Toxic Flame -> Detonates Poison (2x burst)
            CombatEffectDefinitionSO toxicFlame = ScriptableObject.CreateInstance<CombatEffectDefinitionSO>();
            toxicFlame.Initialize(
                "effect_toxic_detonation",
                "Toxic Flame Detonation",
                "reaction_toxic_flame",
                CombatEffectType.DirectDamage,
                0f, // Instant
                0f,
                20.0f,
                "Detonates toxic vapors for 20 instant burst damage.",
                new Color(0.6f, 1f, 0.2f, 0.9f)
            );
            list.Add(toxicFlame);

            // 4. Superconductor -> -40% Enemy Resistance / Armor
            CombatEffectDefinitionSO superconductor = ScriptableObject.CreateInstance<CombatEffectDefinitionSO>();
            superconductor.Initialize(
                "effect_superconductor_shred",
                "Superconductor Resistance Shred",
                "reaction_superconductor",
                CombatEffectType.ArmorModifier,
                4.0f,
                1.0f,
                0.40f,
                "Shreds enemy armor and resistance by 40%.",
                new Color(0.3f, 0.85f, 1f, 0.9f)
            );
            list.Add(superconductor);

            // 5. Frostbite -> +50% Poison Tick / Damage Vulnerability
            CombatEffectDefinitionSO frostbite = ScriptableObject.CreateInstance<CombatEffectDefinitionSO>();
            frostbite.Initialize(
                "effect_frostbite_vulnerability",
                "Frostbite Vulnerability",
                "reaction_frostbite",
                CombatEffectType.DamageModifier,
                4.0f,
                1.0f,
                0.50f,
                "Crystallizes target, increasing incoming damage by 50%.",
                new Color(0.2f, 0.95f, 0.75f, 0.9f)
            );
            list.Add(frostbite);

            db.Initialize(list);
            return db;
        }
    }
}
