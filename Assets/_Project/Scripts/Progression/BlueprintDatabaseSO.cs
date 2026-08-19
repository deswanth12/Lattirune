using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Progression
{
    /// <summary>
    /// Centralized ScriptableObject database containing all Forge blueprints.
    /// Provides fast, deterministic lookup, duplicate ID validation, and canonical catalogue generation.
    /// </summary>
    [CreateAssetMenu(fileName = "BlueprintDatabase", menuName = "Lattirune/Progression/Blueprint Database")]
    public class BlueprintDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<BlueprintDefinitionSO> blueprints = new List<BlueprintDefinitionSO>();

        private readonly Dictionary<string, BlueprintDefinitionSO> _lookup = new Dictionary<string, BlueprintDefinitionSO>();

        public IReadOnlyList<BlueprintDefinitionSO> AllBlueprints => blueprints;
        public int TotalBlueprintCount => blueprints != null ? blueprints.Count : 0;

        public void Initialize(List<BlueprintDefinitionSO> list)
        {
            blueprints = list ?? new List<BlueprintDefinitionSO>();
            BuildLookupTable();
        }

        private void OnEnable()
        {
            BuildLookupTable();
        }

        public void BuildLookupTable()
        {
            _lookup.Clear();
            if (blueprints == null) return;

            foreach (var bp in blueprints)
            {
                if (bp != null && !string.IsNullOrEmpty(bp.BlueprintId))
                {
                    if (!_lookup.ContainsKey(bp.BlueprintId))
                    {
                        _lookup.Add(bp.BlueprintId, bp);
                    }
                }
            }
        }

        public BlueprintDefinitionSO GetBlueprint(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_lookup.Count != (blueprints != null ? blueprints.Count : 0))
            {
                BuildLookupTable();
            }

            if (_lookup.TryGetValue(id, out var bp))
            {
                return bp;
            }

            if (blueprints != null)
            {
                return blueprints.Find(x => x != null && x.BlueprintId == id);
            }

            return null;
        }

        public bool HasBlueprint(string id)
        {
            return GetBlueprint(id) != null;
        }

        public bool IsValid(out string error)
        {
            if (blueprints == null || blueprints.Count == 0)
            {
                error = "Blueprint database cannot be empty.";
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>();
            for (int i = 0; i < blueprints.Count; i++)
            {
                var bp = blueprints[i];
                if (bp == null)
                {
                    error = $"Null blueprint entry at index {i}.";
                    return false;
                }
                if (!bp.IsValid(out string bpErr))
                {
                    error = $"Blueprint '{bp.BlueprintId}' at index {i} is invalid: {bpErr}";
                    return false;
                }
                if (seenIds.Contains(bp.BlueprintId))
                {
                    error = $"Duplicate Blueprint ID '{bp.BlueprintId}' detected.";
                    return false;
                }
                seenIds.Add(bp.BlueprintId);
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Creates the canonical MVP Blueprint Database matching PLAN.md specifications.
        /// </summary>
        public static BlueprintDatabaseSO CreateCanonicalBlueprintDatabase()
        {
            BlueprintDatabaseSO db = ScriptableObject.CreateInstance<BlueprintDatabaseSO>();
            List<BlueprintDefinitionSO> list = new List<BlueprintDefinitionSO>();

            // 1. Shortbow Blueprint (50 Embers)
            BlueprintDefinitionSO bpShortbow = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpShortbow.Initialize("bp_shortbow", "Shortbow Blueprint", "Unlocks the armor-piercing Shortbow into dungeon reward pools.",
                BlueprintCategory.Weapon, cost: 50, targetId: "item_shortbow");
            list.Add(bpShortbow);

            // 2. Apprentice Wand Blueprint (60 Embers)
            BlueprintDefinitionSO bpWand = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpWand.Initialize("bp_apprentice_wand", "Apprentice Wand Blueprint", "Unlocks the Apprentice Wand (+50% rune damage) into reward pools.",
                BlueprintCategory.Weapon, cost: 60, targetId: "item_apprentice_wand");
            list.Add(bpWand);

            // 3. Battleaxe Blueprint (80 Embers)
            BlueprintDefinitionSO bpAxe = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpAxe.Initialize("bp_battleaxe", "Battleaxe Blueprint", "Unlocks the heavy L-shaped Battleaxe into reward pools.",
                BlueprintCategory.Weapon, cost: 80, targetId: "item_battleaxe");
            list.Add(bpAxe);

            // 4. Phalanx Spear Blueprint (75 Embers)
            BlueprintDefinitionSO bpSpear = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpSpear.Initialize("bp_phalanx_spear", "Phalanx Spear Blueprint", "Unlocks the 1x3 Phalanx Spear into reward pools.",
                BlueprintCategory.Weapon, cost: 75, targetId: "item_phalanx_spear");
            list.Add(bpSpear);

            // 5. Iron Tower Shield Blueprint (65 Embers)
            BlueprintDefinitionSO bpTower = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpTower.Initialize("bp_iron_tower_shield", "Iron Tower Shield Blueprint", "Unlocks the 2x2 Iron Tower Shield (25 Shield) into reward pools.",
                BlueprintCategory.Shield, cost: 65, targetId: "item_iron_tower_shield");
            list.Add(bpTower);

            // 6. Spiked Buckler Blueprint (55 Embers)
            BlueprintDefinitionSO bpSpiked = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpSpiked.Initialize("bp_spiked_buckler", "Spiked Buckler Blueprint", "Unlocks the reflective Spiked Buckler into reward pools.",
                BlueprintCategory.Shield, cost: 55, targetId: "item_spiked_buckler");
            list.Add(bpSpiked);

            // 7. Chainmail Coat Blueprint (70 Embers)
            BlueprintDefinitionSO bpChainmail = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpChainmail.Initialize("bp_chainmail_coat", "Chainmail Coat Blueprint", "Unlocks the damage-reducing Chainmail Coat into reward pools.",
                BlueprintCategory.Armor, cost: 70, targetId: "item_chainmail_coat");
            list.Add(bpChainmail);

            // 8. Ruby Ring Blueprint (50 Embers)
            BlueprintDefinitionSO bpRuby = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpRuby.Initialize("bp_ruby_ring", "Ruby Ring Blueprint", "Unlocks the Fire-boosting Ruby Ring into reward pools.",
                BlueprintCategory.Relic, cost: 50, targetId: "item_ruby_ring");
            list.Add(bpRuby);

            // 9. Sapphire Ring Blueprint (50 Embers)
            BlueprintDefinitionSO bpSapphire = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpSapphire.Initialize("bp_sapphire_ring", "Sapphire Ring Blueprint", "Unlocks the Ice-boosting Sapphire Ring into reward pools.",
                BlueprintCategory.Relic, cost: 50, targetId: "item_sapphire_ring");
            list.Add(bpSapphire);

            // 10. Lucky Clover Blueprint (60 Embers)
            BlueprintDefinitionSO bpClover = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpClover.Initialize("bp_lucky_clover", "Lucky Clover Blueprint", "Unlocks the Lucky Clover (+10% Crit Chance) into reward pools.",
                BlueprintCategory.Relic, cost: 60, targetId: "item_lucky_clover");
            list.Add(bpClover);

            // 11. Crossfire Rune Blueprint (80 Embers)
            BlueprintDefinitionSO bpCrossfire = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpCrossfire.Initialize("bp_rune_crossfire", "Crossfire Rune Blueprint", "Unlocks the 4-way Crossfire Rune into reward pools.",
                BlueprintCategory.Rune, cost: 80, targetId: "rune_crossfire");
            list.Add(bpCrossfire);

            // 12. Haste Rune Blueprint (90 Embers)
            BlueprintDefinitionSO bpHaste = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpHaste.Initialize("bp_rune_haste", "Haste Rune Blueprint", "Unlocks the speed-enhancing Haste Rune into reward pools.",
                BlueprintCategory.Rune, cost: 90, targetId: "rune_haste");
            list.Add(bpHaste);

            db.Initialize(list);
            return db;
        }
    }
}
