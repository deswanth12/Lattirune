using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Progression
{
    /// <summary>
    /// Centralized ScriptableObject database containing all 7 canonical enemies from PLAN.md Section 10.
    /// </summary>
    [CreateAssetMenu(fileName = "BestiaryDatabase", menuName = "Lattirune/Progression/Bestiary Database")]
    public class BestiaryDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<BestiaryEntrySO> entries = new List<BestiaryEntrySO>();

        private readonly Dictionary<string, BestiaryEntrySO> _lookup = new Dictionary<string, BestiaryEntrySO>();

        public IReadOnlyList<BestiaryEntrySO> AllEntries => entries;
        public int TotalCount => entries != null ? entries.Count : 0;

        public void Initialize(List<BestiaryEntrySO> entryList)
        {
            entries = entryList ?? new List<BestiaryEntrySO>();
            BuildLookup();
        }

        private void OnEnable()
        {
            BuildLookup();
        }

        public void BuildLookup()
        {
            _lookup.Clear();
            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.EnemyId))
                {
                    if (!_lookup.ContainsKey(entry.EnemyId))
                    {
                        _lookup.Add(entry.EnemyId, entry);
                    }
                }
            }
        }

        public BestiaryEntrySO GetEntry(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return null;

            if (_lookup.Count != (entries != null ? entries.Count : 0))
            {
                BuildLookup();
            }

            if (_lookup.TryGetValue(enemyId, out var entry)) return entry;
            return entries?.Find(x => x != null && x.EnemyId == enemyId);
        }

        public static BestiaryDatabaseSO CreateCanonicalDatabase()
        {
            var db = ScriptableObject.CreateInstance<BestiaryDatabaseSO>();
            var list = new List<BestiaryEntrySO>();

            // 1. Sewer Rat
            var rat = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            rat.Initialize(
                "enemy_sewer_rat",
                "Sewer Rat",
                EnemyTier.Normal,
                "A mutated sewer rodent with razor-sharp incisors.",
                hp: 35,
                speed: 1.2f,
                armor: 0,
                attack: 4,
                mechanic: "Fast melee bites; tests opening burst DPS.",
                counter: "High shield or fast daggers."
            );
            list.Add(rat);

            // 2. Goblin Thief
            var goblin = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            goblin.Initialize(
                "enemy_goblin_thief",
                "Goblin Thief",
                EnemyTier.Normal,
                "A nimble scavenger obsessed with shining dungeon coins.",
                hp: 45,
                speed: 1.0f,
                armor: 0,
                attack: 3,
                mechanic: "Steals 3 Gold on every hit taken!",
                counter: "Burst down before 5 seconds."
            );
            list.Add(goblin);

            // 3. Armored Skeleton
            var skeleton = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            skeleton.Initialize(
                "enemy_armored_skeleton",
                "Armored Skeleton",
                EnemyTier.Normal,
                "Ancient warrior bones encased in heavy steel plate.",
                hp: 75,
                speed: 2.0f,
                armor: 15,
                attack: 6,
                mechanic: "15 Armor; reflects 20% physical damage back to attacker.",
                counter: "Elemental Wands & Poison Runes."
            );
            list.Add(skeleton);

            // 4. Venomous Spider
            var spider = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            spider.Initialize(
                "enemy_venomous_spider",
                "Venomous Spider",
                EnemyTier.Normal,
                "A chitinous arachnid dripping corrosive paralytic venom.",
                hp: 50,
                speed: 1.4f,
                armor: 0,
                attack: 4,
                mechanic: "Inflicts 2 Poison stacks per strike (bypasses shields).",
                counter: "Sun Runes & Healing Potions."
            );
            list.Add(spider);

            // 5. Acid Slime
            var slime = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            slime.Initialize(
                "enemy_acid_slime",
                "Acid Slime",
                EnemyTier.Elite,
                "An enormous gelatinous mass dissolving everything in its path.",
                hp: 160,
                speed: 2.0f,
                armor: 4,
                attack: 10,
                mechanic: "Acid spit: Disables 1 random bag slot during combat.",
                counter: "Redundant weapon arrays."
            );
            list.Add(slime);

            // 6. Necromancer
            var necro = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            necro.Initialize(
                "enemy_necromancer",
                "Necromancer",
                EnemyTier.Elite,
                "A robed death-weaver animating fallen crypt remains.",
                hp: 140,
                speed: 3.0f,
                armor: 2,
                attack: 12,
                mechanic: "Summons 2 Skeleton adds every 4 seconds.",
                counter: "Lightning arc & piercing bows."
            );
            list.Add(necro);

            // 7. The Lich Lord
            var lich = ScriptableObject.CreateInstance<BestiaryEntrySO>();
            lich.Initialize(
                "enemy_lich_lord",
                "The Lich Lord",
                EnemyTier.Boss,
                "The immortal ruler of the Cursed Sewers wielding terrifying frost magic.",
                hp: 750,
                speed: 2.5f,
                armor: 10,
                attack: 8,
                mechanic: "Freezes top grid row; inverts rune laser conduit directions!",
                counter: "Horizontal cross-runes & Sun Rune cleanse."
            );
            list.Add(lich);

            db.Initialize(list);
            return db;
        }
    }
}
