using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Static ScriptableObject defining an encounter within a dungeon floor.
    /// Configures enemy statistics, traits, and battle parameters without holding runtime state.
    /// </summary>
    [CreateAssetMenu(fileName = "Encounter_", menuName = "Lattirune/Dungeon/Encounter Definition")]
    public class EncounterDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string encounterId = "enc_sewer_rat";
        [SerializeField] private string displayName = "Sewer Rat Skirmish";

        [Header("Enemy Stats")]
        [SerializeField] private string enemyName = "Sewer Rat";
        [SerializeField] private int enemyHp = 35;
        [SerializeField] private int enemyArmor = 0;
        [SerializeField] private int enemyAttack = 3;
        [SerializeField] private float attackInterval = 1.2f;
        [SerializeField] private bool isBoss = false;

        [Header("Enemy Traits (PLAN.md Section 10)")]
        [SerializeField] private List<EnemyTraitDefinitionSO> enemyTraits = new List<EnemyTraitDefinitionSO>();

        public string EncounterId => encounterId;
        public string DisplayName => displayName;
        public string EnemyName => enemyName;
        public int EnemyHp => enemyHp;
        public int EnemyArmor => enemyArmor;
        public int EnemyAttack => enemyAttack;
        public float AttackInterval => attackInterval;
        public bool IsBoss => isBoss;
        public IReadOnlyList<EnemyTraitDefinitionSO> EnemyTraits => enemyTraits;

        public void Initialize(
            string id,
            string name,
            string eName,
            int hp,
            int armor,
            int attack,
            float interval = 1.5f,
            bool boss = false,
            List<EnemyTraitDefinitionSO> traits = null)
        {
            encounterId = id;
            displayName = name;
            enemyName = eName;
            enemyHp = Mathf.Max(1, hp);
            enemyArmor = Mathf.Max(0, armor);
            enemyAttack = Mathf.Max(1, attack);
            attackInterval = Mathf.Max(0.2f, interval);
            isBoss = boss;
            enemyTraits = traits ?? new List<EnemyTraitDefinitionSO>();
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(encounterId))
            {
                error = "Encounter ID cannot be empty.";
                return false;
            }
            if (string.IsNullOrEmpty(enemyName))
            {
                error = "Enemy Name cannot be empty.";
                return false;
            }
            if (enemyHp <= 0)
            {
                error = "Enemy HP must be greater than 0.";
                return false;
            }
            error = null;
            return true;
        }

        // ==========================================
        // FACTORY METHODS FOR PLAN.MD BESTIARY
        // ==========================================

        public static EncounterDefinitionSO CreateSewerRat()
        {
            EncounterDefinitionSO enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc.Initialize("enc_sewer_rat", "Sewer Rat Skirmish", "Sewer Rat", hp: 35, armor: 0, attack: 3, interval: 1.2f, boss: false);
            return enc;
        }

        public static EncounterDefinitionSO CreateGoblinThief()
        {
            EncounterDefinitionSO enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            EnemyTraitDefinitionSO goldSteal = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            goldSteal.Initialize("trait_gold_steal", "Gold Steal", EnemyTraitType.GoldSteal, 3f);

            enc.Initialize("enc_goblin_thief", "Goblin Thief Ambush", "Goblin Thief", hp: 45, armor: 0, attack: 4, interval: 1.0f, boss: false,
                new List<EnemyTraitDefinitionSO> { goldSteal });
            return enc;
        }

        public static EncounterDefinitionSO CreateArmoredSkeleton()
        {
            EncounterDefinitionSO enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            EnemyTraitDefinitionSO reflect = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            reflect.Initialize("trait_reflect", "Damage Reflection", EnemyTraitType.DamageReflect, 0.20f);

            enc.Initialize("enc_armored_skeleton", "Armored Skeleton Guard", "Armored Skeleton", hp: 75, armor: 15, attack: 5, interval: 2.0f, boss: false,
                new List<EnemyTraitDefinitionSO> { reflect });
            return enc;
        }

        public static EncounterDefinitionSO CreateVenomousSpider()
        {
            EncounterDefinitionSO enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            EnemyTraitDefinitionSO poison = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            poison.Initialize("trait_poison", "Toxic Bite", EnemyTraitType.ApplyPoisonOnHit, 2f);

            enc.Initialize("enc_venomous_spider", "Venomous Spider Nest", "Venomous Spider", hp: 50, armor: 0, attack: 4, interval: 1.4f, boss: false,
                new List<EnemyTraitDefinitionSO> { poison });
            return enc;
        }

        public static EncounterDefinitionSO CreateAcidSlime()
        {
            EncounterDefinitionSO enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            EnemyTraitDefinitionSO acidSpit = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            acidSpit.Initialize("trait_acid_spit", "Acid Spit", EnemyTraitType.DisableBagSlot, 1f);

            enc.Initialize("enc_acid_slime", "Elite: Acid Slime", "Acid Slime", hp: 160, armor: 2, attack: 6, interval: 2.0f, boss: false,
                new List<EnemyTraitDefinitionSO> { acidSpit });
            return enc;
        }

        public static EncounterDefinitionSO CreateNecromancer()
        {
            EncounterDefinitionSO enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            EnemyTraitDefinitionSO summon = ScriptableObject.CreateInstance<EnemyTraitDefinitionSO>();
            summon.Initialize("trait_summon", "Raise Skeletons", EnemyTraitType.SummonMinions, 2f, interval: 4.0f);

            enc.Initialize("enc_necromancer", "Elite: Necromancer", "Necromancer", hp: 140, armor: 0, attack: 5, interval: 3.0f, boss: false,
                new List<EnemyTraitDefinitionSO> { summon });
            return enc;
        }
    }
}
