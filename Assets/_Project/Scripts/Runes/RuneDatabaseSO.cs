using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Runes
{
    /// <summary>
    /// Centralized ScriptableObject database containing the complete MVP 1.0 10-Rune Catalogue.
    /// Strictly adheres to PLAN.md Section 5.1.
    /// </summary>
    [CreateAssetMenu(fileName = "RuneDatabase", menuName = "Lattirune/Data/Rune Database")]
    public class RuneDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<RuneData> runes = new List<RuneData>();

        private readonly Dictionary<string, RuneData> _runeLookup = new Dictionary<string, RuneData>();

        public IReadOnlyList<RuneData> AllRunes => runes;
        public int TotalRuneCount => runes != null ? runes.Count : 0;

        public void Initialize(List<RuneData> runeList)
        {
            runes = runeList ?? new List<RuneData>();
            BuildLookupTable();
        }

        private void OnEnable()
        {
            BuildLookupTable();
        }

        public void BuildLookupTable()
        {
            _runeLookup.Clear();
            if (runes == null) return;

            foreach (var rune in runes)
            {
                if (rune != null && !string.IsNullOrEmpty(rune.RuneId))
                {
                    if (!_runeLookup.ContainsKey(rune.RuneId))
                    {
                        _runeLookup.Add(rune.RuneId, rune);
                    }
                }
            }
        }

        public RuneData GetRune(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (_runeLookup.Count != (runes != null ? runes.Count : 0))
            {
                BuildLookupTable();
            }

            if (_runeLookup.TryGetValue(id, out var rune))
            {
                return rune;
            }

            // Fallback search in list
            if (runes != null)
            {
                return runes.Find(x => x != null && x.RuneId == id);
            }

            return null;
        }

        public bool HasRune(string id)
        {
            return GetRune(id) != null;
        }

        public bool IsValid(out string error)
        {
            if (runes == null || runes.Count == 0)
            {
                error = "Rune database cannot be empty.";
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>();
            for (int i = 0; i < runes.Count; i++)
            {
                var r = runes[i];
                if (r == null)
                {
                    error = $"Null rune reference at index {i}.";
                    return false;
                }
                if (!r.IsValid(out string runeErr))
                {
                    error = $"Rune '{r.RuneId}' at index {i} is invalid: {runeErr}";
                    return false;
                }
                if (seenIds.Contains(r.RuneId))
                {
                    error = $"Duplicate rune ID detected: '{r.RuneId}'.";
                    return false;
                }
                seenIds.Add(r.RuneId);
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Creates the complete canonical MVP 1.0 10-Rune Database specified in PLAN.md Section 5.1.
        /// </summary>
        public static RuneDatabaseSO CreateCanonicalDatabase()
        {
            RuneDatabaseSO db = ScriptableObject.CreateInstance<RuneDatabaseSO>();
            List<RuneData> list = new List<RuneData>();

            // 1. Ember Rune (Fire, East, +6 Fire Dmg, Burn 3 dmg/s for 4s)
            RuneData ember = ScriptableObject.CreateInstance<RuneData>();
            ember.Initialize("rune_ember", "Ember Rune", ConduitDirection.East, ElementType.Fire, maxRange: 5, active: true,
                damageBonus: 6, burnDmg: 3f, burnDur: 4f);
            list.Add(ember);

            // 2. Frost Rune (Ice, South, +4 Ice Dmg, speed -15%)
            RuneData frost = ScriptableObject.CreateInstance<RuneData>();
            frost.Initialize("rune_frost", "Frost Rune", ConduitDirection.South, ElementType.Ice, maxRange: 5, active: true,
                damageBonus: 4, speedReduction: 0.15f);
            list.Add(frost);

            // 3. Spark Rune (Lightning, North, +8 Shock Dmg, 25% chain arc)
            RuneData spark = ScriptableObject.CreateInstance<RuneData>();
            spark.Initialize("rune_spark", "Spark Rune", ConduitDirection.North, ElementType.Lightning, maxRange: 5, active: true,
                damageBonus: 8, chain: 0.25f);
            list.Add(spark);

            // 4. Venom Rune (Poison, West, 2 Poison stacks/s)
            RuneData venom = ScriptableObject.CreateInstance<RuneData>();
            venom.Initialize("rune_venom", "Venom Rune", ConduitDirection.West, ElementType.Poison, maxRange: 5, active: true,
                poisonRate: 2);
            list.Add(venom);

            // 5. Crossfire Rune (Fire, Cross, +3 Fire Dmg in 4 directions)
            RuneData crossfire = ScriptableObject.CreateInstance<RuneData>();
            crossfire.Initialize("rune_crossfire", "Crossfire Rune", ConduitDirection.Cross, ElementType.Fire, maxRange: 5, active: true,
                damageBonus: 3);
            list.Add(crossfire);

            // 6. Prism Rune (Light, Split, splits incoming beam)
            RuneData prism = ScriptableObject.CreateInstance<RuneData>();
            prism.Initialize("rune_prism", "Prism Rune", ConduitDirection.Split, ElementType.Light, maxRange: 3, active: true);
            list.Add(prism);

            // 7. Amplifier Node (Force, Omni, doubles adjacent power)
            RuneData amplifier = ScriptableObject.CreateInstance<RuneData>();
            amplifier.Initialize("rune_amplifier", "Amplifier Node", ConduitDirection.Omni, ElementType.Force, maxRange: 1, active: true);
            list.Add(amplifier);

            // 8. Iron Rune (Earth, South, +15 Shield at battle start)
            RuneData iron = ScriptableObject.CreateInstance<RuneData>();
            iron.Initialize("rune_iron", "Iron Rune", ConduitDirection.South, ElementType.Earth, maxRange: 4, active: true,
                shield: 15);
            list.Add(iron);

            // 9. Vampire Rune (Shadow, North, heals for 12% dmg dealt)
            RuneData vampire = ScriptableObject.CreateInstance<RuneData>();
            vampire.Initialize("rune_vampire", "Vampire Rune", ConduitDirection.North, ElementType.Shadow, maxRange: 4, active: true,
                lifesteal: 0.12f);
            list.Add(vampire);

            // 10. Haste Rune (Wind, East, +25% attack speed)
            RuneData haste = ScriptableObject.CreateInstance<RuneData>();
            haste.Initialize("rune_haste", "Haste Rune", ConduitDirection.East, ElementType.Wind, maxRange: 4, active: true,
                haste: 0.25f);
            list.Add(haste);

            // ==========================================
            // BACKWARD-COMPATIBLE PROTOTYPE ALIASES
            // ==========================================
            RuneData protoFire = ScriptableObject.CreateInstance<RuneData>();
            protoFire.Initialize("fire_rune_01", "Fire Rune", ConduitDirection.North, ElementType.Fire, maxRange: 3, active: true, damageBonus: 5);
            list.Add(protoFire);

            RuneData protoIce = ScriptableObject.CreateInstance<RuneData>();
            protoIce.Initialize("ice_rune_01", "Ice Rune", ConduitDirection.East, ElementType.Ice, maxRange: 4, active: true, damageBonus: 4);
            list.Add(protoIce);

            RuneData protoOmni = ScriptableObject.CreateInstance<RuneData>();
            protoOmni.Initialize("rune_omni", "Omnidirectional Node", ConduitDirection.Omni, ElementType.Force, maxRange: 1, active: true);
            list.Add(protoOmni);

            RuneData protoPrism = ScriptableObject.CreateInstance<RuneData>();
            protoPrism.Initialize("prism_demo", "Prism Rune", ConduitDirection.Split, ElementType.Light, maxRange: 3, active: true);
            list.Add(protoPrism);

            db.Initialize(list);
            return db;
        }
    }
}
