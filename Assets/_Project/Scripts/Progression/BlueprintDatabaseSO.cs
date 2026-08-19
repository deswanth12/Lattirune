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
                BlueprintCategory.Weapon, cost: 50, targetId: "item_shortbow", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpShortbow);

            // 2. Apprentice Wand Blueprint (60 Embers)
            BlueprintDefinitionSO bpWand = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpWand.Initialize("bp_apprentice_wand", "Apprentice Wand Blueprint", "Unlocks the Apprentice Wand (+50% rune damage) into reward pools.",
                BlueprintCategory.Weapon, cost: 60, targetId: "item_apprentice_wand", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpWand);

            // 3. Battleaxe Blueprint (80 Embers)
            BlueprintDefinitionSO bpAxe = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpAxe.Initialize("bp_battleaxe", "Battleaxe Blueprint", "Unlocks the heavy L-shaped Battleaxe into reward pools.",
                BlueprintCategory.Weapon, cost: 80, targetId: "item_battleaxe", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpAxe);

            // 4. Phalanx Spear Blueprint (75 Embers)
            BlueprintDefinitionSO bpSpear = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpSpear.Initialize("bp_phalanx_spear", "Phalanx Spear Blueprint", "Unlocks the 1x3 Phalanx Spear into reward pools.",
                BlueprintCategory.Weapon, cost: 75, targetId: "item_phalanx_spear", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpSpear);

            // 5. Iron Tower Shield Blueprint (65 Embers)
            BlueprintDefinitionSO bpTower = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpTower.Initialize("bp_iron_tower_shield", "Iron Tower Shield Blueprint", "Unlocks the 2x2 Iron Tower Shield (25 Shield) into reward pools.",
                BlueprintCategory.Shield, cost: 65, targetId: "item_iron_tower_shield", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpTower);

            // 6. Spiked Buckler Blueprint (55 Embers)
            BlueprintDefinitionSO bpSpiked = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpSpiked.Initialize("bp_spiked_buckler", "Spiked Buckler Blueprint", "Unlocks the reflective Spiked Buckler into reward pools.",
                BlueprintCategory.Shield, cost: 55, targetId: "item_spiked_buckler", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpSpiked);

            // 7. Chainmail Coat Blueprint (70 Embers)
            BlueprintDefinitionSO bpChainmail = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpChainmail.Initialize("bp_chainmail_coat", "Chainmail Coat Blueprint", "Unlocks the damage-reducing Chainmail Coat into reward pools.",
                BlueprintCategory.Armor, cost: 70, targetId: "item_chainmail_coat", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpChainmail);

            // 8. Ruby Ring Blueprint (50 Embers)
            BlueprintDefinitionSO bpRuby = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpRuby.Initialize("bp_ruby_ring", "Ruby Ring Blueprint", "Unlocks the Fire-boosting Ruby Ring into reward pools.",
                BlueprintCategory.Relic, cost: 50, targetId: "item_ruby_ring", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpRuby);

            // 9. Sapphire Ring Blueprint (50 Embers)
            BlueprintDefinitionSO bpSapphire = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpSapphire.Initialize("bp_sapphire_ring", "Sapphire Ring Blueprint", "Unlocks the Ice-boosting Sapphire Ring into reward pools.",
                BlueprintCategory.Relic, cost: 50, targetId: "item_sapphire_ring", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpSapphire);

            // 10. Lucky Clover Blueprint (60 Embers)
            BlueprintDefinitionSO bpClover = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpClover.Initialize("bp_lucky_clover", "Lucky Clover Blueprint", "Unlocks the Lucky Clover (+10% Crit Chance) into reward pools.",
                BlueprintCategory.Relic, cost: 60, targetId: "item_lucky_clover", effect: BlueprintEffectType.UnlockItemInRewardPool);
            list.Add(bpClover);

            // 11. Crossfire Rune Blueprint (80 Embers)
            BlueprintDefinitionSO bpCrossfire = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpCrossfire.Initialize("bp_rune_crossfire", "Crossfire Rune Blueprint", "Unlocks the 4-way Crossfire Rune into reward pools.",
                BlueprintCategory.Rune, cost: 80, targetId: "rune_crossfire", effect: BlueprintEffectType.UnlockRuneInRewardPool);
            list.Add(bpCrossfire);

            // 12. Haste Rune Blueprint (90 Embers)
            BlueprintDefinitionSO bpHaste = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpHaste.Initialize("bp_rune_haste", "Haste Rune Blueprint", "Unlocks the speed-enhancing Haste Rune into reward pools.",
                BlueprintCategory.Rune, cost: 90, targetId: "rune_haste", effect: BlueprintEffectType.UnlockRuneInRewardPool);
            list.Add(bpHaste);

            // 13. Mercenary Purse (Permanent Starting Gold +15) (45 Embers)
            BlueprintDefinitionSO bpGold = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpGold.Initialize("bp_mercenary_purse", "Mercenary Purse", "Permanently grants +15 Starting Gold on every new dungeon run.",
                BlueprintCategory.Utility, cost: 45, targetId: "stat_starting_gold", effect: BlueprintEffectType.PermanentStartingGoldBonus, value: 15);
            list.Add(bpGold);

            // 14. Vitality Infusion (Permanent Starting Max HP +20) (55 Embers)
            BlueprintDefinitionSO bpHp = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpHp.Initialize("bp_vitality_infusion", "Vitality Infusion", "Permanently grants +20 Starting Max HP on every new dungeon run.",
                BlueprintCategory.Utility, cost: 55, targetId: "stat_starting_hp", effect: BlueprintEffectType.PermanentStartingHpBonus, value: 20);
            list.Add(bpHp);

            // 15. Celestial Compass (70 Embers)
            BlueprintDefinitionSO bpCompass = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpCompass.Initialize("bp_celestial_compass", "Celestial Compass", "Attunes with ancient ley lines, revealing secret shrines on the Dungeon Map.",
                BlueprintCategory.Relic, cost: 70, targetId: "relic_celestial_compass", effect: BlueprintEffectType.MapVision);
            list.Add(bpCompass);

            // 16. Alchemist's Flask (60 Embers)
            BlueprintDefinitionSO bpFlask = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpFlask.Initialize("bp_alchemists_flask", "Alchemist's Flask", "Distills potent healing draughts, increasing emergency potion heals to 45 HP.",
                BlueprintCategory.Utility, cost: 60, targetId: "relic_alchemists_flask", effect: BlueprintEffectType.PotionHealBonus, value: 20);
            list.Add(bpFlask);

            // 17. Vampiric Edge (85 Embers)
            BlueprintDefinitionSO bpVampire = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpVampire.Initialize("bp_vampiric_edge", "Vampiric Edge", "Infuses weapons with sanguine energy, restoring 10% of attack damage as Health.",
                BlueprintCategory.Relic, cost: 85, targetId: "relic_vampiric_edge", effect: BlueprintEffectType.VampirismBonus, value: 10);
            list.Add(bpVampire);

            // 18. Prism Core (95 Embers)
            BlueprintDefinitionSO bpPrism = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpPrism.Initialize("bp_prism_core", "Prism Core", "Unlocks hyper-refractive Prism Runes in reward pools with extra beam branching.",
                BlueprintCategory.Rune, cost: 95, targetId: "rune_prism_core", effect: BlueprintEffectType.UnlockRuneInRewardPool);
            list.Add(bpPrism);

            // 19. Eternal Embers (80 Embers)
            BlueprintDefinitionSO bpEternal = ScriptableObject.CreateInstance<BlueprintDefinitionSO>();
            bpEternal.Initialize("bp_eternal_embers", "Eternal Embers", "Harvests lingering dungeon spirits, granting +5 bonus Embers on every floor clear.",
                BlueprintCategory.Utility, cost: 80, targetId: "stat_bonus_embers", effect: BlueprintEffectType.BonusEmberReward, value: 5);
            list.Add(bpEternal);

            db.Initialize(list);
            return db;
        }
    }
}
