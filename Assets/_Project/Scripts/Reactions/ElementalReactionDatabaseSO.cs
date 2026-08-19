using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Reactions
{
    /// <summary>
    /// Data-driven database holding all registered 2-beam elemental reaction rules.
    /// Provides symmetric element pair lookup and validation against duplicate IDs.
    /// </summary>
    [CreateAssetMenu(fileName = "ReactionDatabase", menuName = "Lattirune/Data/Reaction Database")]
    public class ElementalReactionDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<ElementalReactionDefinitionSO> reactionDefinitions = new List<ElementalReactionDefinitionSO>();

        public IReadOnlyList<ElementalReactionDefinitionSO> Definitions => reactionDefinitions;
        public IReadOnlyList<ElementalReactionDefinitionSO> AllReactions => reactionDefinitions;
        public int Count => reactionDefinitions != null ? reactionDefinitions.Count : 0;
        public int TotalReactionCount => Count;

        public void Initialize(List<ElementalReactionDefinitionSO> definitions)
        {
            reactionDefinitions = definitions ?? new List<ElementalReactionDefinitionSO>();
        }

        public void Register(ElementalReactionDefinitionSO def)
        {
            if (def == null) return;
            if (reactionDefinitions == null) reactionDefinitions = new List<ElementalReactionDefinitionSO>();

            if (!reactionDefinitions.Exists(d => d != null && d.ReactionId == def.ReactionId))
            {
                reactionDefinitions.Add(def);
            }
        }

        public ElementalReactionDefinitionSO GetById(string reactionId)
        {
            if (string.IsNullOrEmpty(reactionId) || reactionDefinitions == null) return null;
            return reactionDefinitions.Find(d => d != null && d.ReactionId == reactionId);
        }

        public ElementalReactionDefinitionSO FindReaction(ElementType a, ElementType b)
        {
            if (reactionDefinitions == null) return null;

            ElementalReactionDefinitionSO bestMatch = null;
            for (int i = 0; i < reactionDefinitions.Count; i++)
            {
                ElementalReactionDefinitionSO def = reactionDefinitions[i];
                if (def != null && def.IsMatch(a, b))
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
            if (reactionDefinitions == null || reactionDefinitions.Count == 0)
            {
                errors.Add("ElementalReactionDatabase has no registered definitions.");
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>();
            HashSet<(ElementType, ElementType)> seenPairs = new HashSet<(ElementType, ElementType)>();

            for (int i = 0; i < reactionDefinitions.Count; i++)
            {
                ElementalReactionDefinitionSO def = reactionDefinitions[i];
                if (def == null)
                {
                    errors.Add($"Null reaction definition at index {i}.");
                    continue;
                }

                if (!def.IsValid(out string defErr))
                {
                    errors.Add($"Reaction at index {i} is invalid: {defErr}");
                }

                if (seenIds.Contains(def.ReactionId))
                {
                    errors.Add($"Duplicate Reaction ID '{def.ReactionId}' found at index {i}.");
                }
                else
                {
                    seenIds.Add(def.ReactionId);
                }

                // Normalize pair key
                var pairKey = def.ElementA <= def.ElementB 
                    ? (def.ElementA, def.ElementB) 
                    : (def.ElementB, def.ElementA);

                if (seenPairs.Contains(pairKey))
                {
                    errors.Add($"Duplicate element pair ({def.ElementA}, {def.ElementB}) found for reaction '{def.ReactionId}'.");
                }
                else
                {
                    seenPairs.Add(pairKey);
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
        public bool IsValid(out List<string> errors) => ValidateDatabase(out errors);

        public ElementalReactionDefinitionSO GetReaction(ElementType a, ElementType b) => FindReaction(a, b);
        public ElementalReactionDefinitionSO GetReaction(Lattirune.Runes.RuneElement a, Lattirune.Runes.RuneElement b) => FindReaction((ElementType)(int)a, (ElementType)(int)b);
        public bool HasReaction(string id) => GetById(id) != null;

        public static ElementalReactionDatabaseSO CreateCanonicalDatabase() => CreateDefaultDatabase();
        public static ElementalReactionDatabaseSO CreateCanonicalReactionDatabase() => CreateDefaultDatabase();

        /// <summary>
        /// Creates a complete prototype Elemental Reaction database implementing PLAN.md Section 7.
        /// </summary>
        public static ElementalReactionDatabaseSO CreateDefaultDatabase()
        {
            ElementalReactionDatabaseSO db = ScriptableObject.CreateInstance<ElementalReactionDatabaseSO>();
            List<ElementalReactionDefinitionSO> list = new List<ElementalReactionDefinitionSO>();

            // 1. Steam (Fire + Ice) -> 25% Enemy Blind/Miss
            ElementalReactionDefinitionSO steam = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            steam.Initialize(
                "reaction_steam", 
                "Steam", 
                "Fire Beam + Ice Beam generates dense steam, inflicting 25% Enemy Blind/Miss.", 
                ElementType.Fire, 
                ElementType.Ice, 
                new Color(0.85f, 0.95f, 1f, 0.85f)
            );
            list.Add(steam);

            // 2. Plasma (Fire + Lightning) -> 18 Dmg/s Continuous Ray
            ElementalReactionDefinitionSO plasma = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            plasma.Initialize(
                "reaction_plasma", 
                "Plasma", 
                "Fire Beam + Lightning Beam fuses into a superheated plasma stream dealing 18 Dmg/s.", 
                ElementType.Fire, 
                ElementType.Lightning, 
                new Color(1f, 0.25f, 0.75f, 0.9f)
            );
            list.Add(plasma);

            // 3. Toxic Flame (Fire + Poison) -> Detonates Poison (2x)
            ElementalReactionDefinitionSO toxicFlame = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            toxicFlame.Initialize(
                "reaction_toxic_flame", 
                "Toxic Flame", 
                "Fire Beam + Poison Beam combusts toxic vapors, detonating poison stacks for 2x burst damage.", 
                ElementType.Fire, 
                ElementType.Poison, 
                new Color(0.6f, 1f, 0.2f, 0.9f)
            );
            list.Add(toxicFlame);

            // 4. Superconductor (Lightning + Ice) -> -40% Enemy Resistance
            ElementalReactionDefinitionSO superconductor = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            superconductor.Initialize(
                "reaction_superconductor", 
                "Superconductor", 
                "Lightning Beam + Ice Beam creates hyper-conductive freeze, shredding enemy resistance by 40%.", 
                ElementType.Lightning, 
                ElementType.Ice, 
                new Color(0.3f, 0.85f, 1f, 0.9f)
            );
            list.Add(superconductor);

            // 5. Frostbite (Ice + Poison) -> +50% Poison Tick Dmg
            ElementalReactionDefinitionSO frostbite = ScriptableObject.CreateInstance<ElementalReactionDefinitionSO>();
            frostbite.Initialize(
                "reaction_frostbite", 
                "Frostbite", 
                "Ice Beam + Poison Beam crystallizes toxins inside the target, increasing poison tick damage by 50%.", 
                ElementType.Ice, 
                ElementType.Poison, 
                new Color(0.2f, 0.95f, 0.75f, 0.9f)
            );
            list.Add(frostbite);

            db.Initialize(list);
            return db;
        }
    }
}
