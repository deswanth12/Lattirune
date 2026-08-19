using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Progression
{
    /// <summary>
    /// Centralized ScriptableObject database containing the 4 playable Hero Classes.
    /// Derived strictly from PLAN.md Section 12 and Section 26.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroClassDatabase", menuName = "Lattirune/Progression/Hero Class Database")]
    public class HeroClassDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<HeroClassDefinitionSO> classes = new List<HeroClassDefinitionSO>();

        private readonly Dictionary<string, HeroClassDefinitionSO> _classLookup = new Dictionary<string, HeroClassDefinitionSO>();

        public IReadOnlyList<HeroClassDefinitionSO> AllClasses => classes;
        public int TotalClassCount => classes != null ? classes.Count : 0;

        public void Initialize(List<HeroClassDefinitionSO> classList)
        {
            classes = classList ?? new List<HeroClassDefinitionSO>();
            BuildLookupTable();
        }

        private void OnEnable()
        {
            BuildLookupTable();
        }

        public void BuildLookupTable()
        {
            _classLookup.Clear();
            if (classes == null) return;

            foreach (var heroClass in classes)
            {
                if (heroClass != null && !string.IsNullOrEmpty(heroClass.ClassId))
                {
                    if (!_classLookup.ContainsKey(heroClass.ClassId))
                    {
                        _classLookup.Add(heroClass.ClassId, heroClass);
                    }
                }
            }
        }

        public HeroClassDefinitionSO GetClass(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return null;

            if (_classLookup.Count != (classes != null ? classes.Count : 0))
            {
                BuildLookupTable();
            }

            if (_classLookup.TryGetValue(classId, out var def))
            {
                return def;
            }

            if (classes != null)
            {
                return classes.Find(x => x != null && x.ClassId == classId);
            }

            return null;
        }

        public bool HasClass(string classId)
        {
            return GetClass(classId) != null;
        }

        /// <summary>
        /// Creates the canonical database of 4 playable Hero Classes per PLAN.md Section 12.
        /// </summary>
        public static HeroClassDatabaseSO CreateCanonicalDatabase()
        {
            var db = ScriptableObject.CreateInstance<HeroClassDatabaseSO>();
            var list = new List<HeroClassDefinitionSO>();

            // 1. Rune Knight (Default Melee)
            var knight = ScriptableObject.CreateInstance<HeroClassDefinitionSO>();
            knight.Initialize(
                id: ""class_rune_knight"",
                name: ""Rune Knight"",
                desc: ""Balanced frontline warrior channeling fire conduits through broadswords."",
                type: HeroClassType.RuneKnight,
                hp: 100,
                armor: 2,
                atk: 10,
                interval: 1.8f,
                items: new List<string> { ""item_iron_broadsword"", ""item_wooden_buckler"", ""item_health_potion"" },
                runes: new List<string> { ""fire_rune_01"" },
                cost: 0,
                unlocked: true
            );
            list.Add(knight);

            // 2. Elementalist (Mage)
            var mage = ScriptableObject.CreateInstance<HeroClassDefinitionSO>();
            mage.Initialize(
                id: ""class_elementalist"",
                name: ""Elementalist"",
                desc: ""Arcane spellcaster refracting lightning and ice beams across multi-rune matrices."",
                type: HeroClassType.Elementalist,
                hp: 85,
                armor: 0,
                atk: 8,
                interval: 1.5f,
                items: new List<string> { ""item_apprentice_wand"", ""item_lucky_clover"", ""item_stamina_flask"" },
                runes: new List<string> { ""spark_rune_01"", ""prism_rune_01"" },
                cost: 80,
                unlocked: false
            );
            list.Add(mage);

            // 3. Shadow Rogue (Speed & Crit)
            var rogue = ScriptableObject.CreateInstance<HeroClassDefinitionSO>();
            rogue.Initialize(
                id: ""class_shadow_rogue"",
                name: ""Shadow Rogue"",
                desc: ""Agile assassin delivering venomous rapid strikes from dark grid corners."",
                type: HeroClassType.ShadowRogue,
                hp: 90,
                armor: 1,
                atk: 12,
                interval: 1.0f,
                items: new List<string> { ""item_rusty_dagger"", ""item_shortbow"", ""item_poison_vial"" },
                runes: new List<string> { ""venom_rune_01"" },
                cost: 120,
                unlocked: false
            );
            list.Add(rogue);

            // 4. Iron Juggernaut (Tank Defense)
            var tank = ScriptableObject.CreateInstance<HeroClassDefinitionSO>();
            tank.Initialize(
                id: ""class_iron_juggernaut"",
                name: ""Iron Juggernaut"",
                desc: ""Heavily armored titan wielding massive tower shields and crushing battleaxes."",
                type: HeroClassType.IronJuggernaut,
                hp: 140,
                armor: 6,
                atk: 14,
                interval: 2.5f,
                items: new List<string> { ""item_iron_tower_shield"", ""item_battleaxe"", ""item_chainmail_coat"" },
                runes: new List<string> { ""iron_rune_01"" },
                cost: 150,
                unlocked: false
            );
            list.Add(tank);

            db.Initialize(list);
            return db;
        }
    }
}
