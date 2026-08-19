using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Progression
{
    public enum EnemyTier
    {
        Normal,
        Elite,
        Boss
    }

    /// <summary>
    /// ScriptableObject defining an Enemy Bestiary entry with lore, combat stats,
    /// grid-disrupting mechanics, and counter strategies.
    /// Strictly adheres to PLAN.md Section 10.
    /// </summary>
    [CreateAssetMenu(fileName = "Bestiary_", menuName = "Lattirune/Progression/Bestiary Entry")]
    public class BestiaryEntrySO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyId = "enemy_sewer_rat";
        [SerializeField] private string enemyName = "Sewer Rat";
        [SerializeField] private EnemyTier tier = EnemyTier.Normal;
        [SerializeField] [TextArea(2, 4)] private string description = "A mutated rodent thriving in the toxic runoff of the Cursed Sewers.";

        [Header("Combat Profile")]
        [SerializeField] private int baseHp = 35;
        [SerializeField] private float attackSpeed = 1.2f;
        [SerializeField] private int baseArmor = 0;
        [SerializeField] private int baseAttack = 4;

        [Header("Mechanics & Strategy")]
        [SerializeField] private string uniqueMechanic = "Fast melee bites; tests opening burst DPS.";
        [SerializeField] private string counterStrategy = "High shield or fast daggers.";

        public string EnemyId => enemyId;
        public string EnemyName => enemyName;
        public EnemyTier Tier => tier;
        public string Description => description;
        public int BaseHp => baseHp;
        public float AttackSpeed => attackSpeed;
        public int BaseArmor => baseArmor;
        public int BaseAttack => baseAttack;
        public string UniqueMechanic => uniqueMechanic;
        public string CounterStrategy => counterStrategy;

        public void Initialize(
            string id,
            string name,
            EnemyTier enemyTier,
            string desc,
            int hp,
            float speed,
            int armor,
            int attack,
            string mechanic,
            string counter)
        {
            this.enemyId = id;
            this.enemyName = name;
            this.tier = enemyTier;
            this.description = desc;
            this.baseHp = Mathf.Max(1, hp);
            this.attackSpeed = Mathf.Max(0.1f, speed);
            this.baseArmor = Mathf.Max(0, armor);
            this.baseAttack = Mathf.Max(1, attack);
            this.uniqueMechanic = mechanic;
            this.counterStrategy = counter;
        }
    }
}
