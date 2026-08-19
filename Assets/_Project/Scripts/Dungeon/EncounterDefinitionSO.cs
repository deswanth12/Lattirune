using UnityEngine;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Static ScriptableObject defining an encounter within a dungeon floor.
    /// Configures enemy statistics and battle parameters without holding runtime state.
    /// </summary>
    [CreateAssetMenu(fileName = "Encounter_", menuName = "Lattirune/Dungeon/Encounter Definition")]
    public class EncounterDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string encounterId = "enc_sewer_rat";
        [SerializeField] private string displayName = "Sewer Rat Skirmish";

        [Header("Enemy Stats")]
        [SerializeField] private string enemyName = "Sewer Rat";
        [SerializeField] private int enemyHp = 40;
        [SerializeField] private int enemyArmor = 1;
        [SerializeField] private int enemyAttack = 3;
        [SerializeField] private float attackInterval = 1.4f;
        [SerializeField] private bool isBoss = false;

        public string EncounterId => encounterId;
        public string DisplayName => displayName;
        public string EnemyName => enemyName;
        public int EnemyHp => enemyHp;
        public int EnemyArmor => enemyArmor;
        public int EnemyAttack => enemyAttack;
        public float AttackInterval => attackInterval;
        public bool IsBoss => isBoss;

        public void Initialize(
            string id,
            string name,
            string eName,
            int hp,
            int armor,
            int attack,
            float interval = 1.5f,
            bool boss = false)
        {
            encounterId = id;
            displayName = name;
            enemyName = eName;
            enemyHp = Mathf.Max(1, hp);
            enemyArmor = Mathf.Max(0, armor);
            enemyAttack = Mathf.Max(1, attack);
            attackInterval = Mathf.Max(0.2f, interval);
            isBoss = boss;
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
    }
}
