using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;

namespace Lattirune.Progression
{
    public enum HeroClassType
    {
        RuneKnight,
        Elementalist,
        ShadowRogue,
        IronJuggernaut
    }

    /// <summary>
    /// ScriptableObject defining a playable Hero Class, its base combat stats,
    /// starting item/rune loadout, and meta-hub unlock requirements.
    /// Derived strictly from PLAN.md Section 12 and Section 26.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroClass_", menuName = "Lattirune/Progression/Hero Class Definition")]
    public class HeroClassDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string classId = "class_rune_knight";
        [SerializeField] private string className = "Rune Knight";
        [SerializeField] [TextArea(2, 4)] private string description = "A balanced frontline warrior channeling elemental runes through blades.";
        [SerializeField] private HeroClassType classType = HeroClassType.RuneKnight;

        [Header("Base Combat Stats")]
        [SerializeField] private int baseHp = 100;
        [SerializeField] private int baseArmor = 2;
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private float attackInterval = 1.8f;

        [Header("Starting Loadout")]
        [SerializeField] private List<string> startingItemIds = new List<string>();
        [SerializeField] private List<string> startingRuneIds = new List<string>();

        [Header("Meta-Hub Unlock")]
        [SerializeField] private int embersCost = 0;
        [SerializeField] private bool defaultUnlocked = true;

        public string ClassId => classId;
        public string ClassName => className;
        public string Description => description;
        public HeroClassType ClassType => classType;
        public int BaseHp => baseHp;
        public int BaseArmor => baseArmor;
        public int BaseAttack => baseAttack;
        public float AttackInterval => attackInterval;
        public IReadOnlyList<string> StartingItemIds => startingItemIds;
        public IReadOnlyList<string> StartingRuneIds => startingRuneIds;
        public int EmbersCost => embersCost;
        public bool DefaultUnlocked => defaultUnlocked;

        public void Initialize(
            string id,
            string name,
            string desc,
            HeroClassType type,
            int hp,
            int armor,
            int atk,
            float interval,
            List<string> items,
            List<string> runes,
            int cost,
            bool unlocked)
        {
            this.classId = id;
            this.className = name;
            this.description = desc;
            this.classType = type;
            this.baseHp = Mathf.Max(1, hp);
            this.baseArmor = Mathf.Max(0, armor);
            this.baseAttack = Mathf.Max(1, atk);
            this.attackInterval = Mathf.Max(0.1f, interval);
            this.startingItemIds = items ?? new List<string>();
            this.startingRuneIds = runes ?? new List<string>();
            this.embersCost = Mathf.Max(0, cost);
            this.defaultUnlocked = unlocked;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(classId))
            {
                error = "Hero class ID cannot be null or empty.";
                return false;
            }
            if (string.IsNullOrEmpty(className))
            {
                error = "Hero class name cannot be null or empty.";
                return false;
            }
            if (baseHp <= 0)
            {
                error = "Base HP must be greater than zero.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
