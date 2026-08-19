using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Baseline prototype enemy combatant.
    /// Executes automatic attacks against the player on a fixed cooldown interval.
    /// </summary>
    public class EnemyCombatant : Combatant
    {
        [Header("Enemy Attack Stats")]
        [SerializeField] private int baseAttackDamage = 4;

        public int BaseAttackDamage => baseAttackDamage;

        public void SetupTrainingDummy(int hp = 50, int baseArmor = 2, int attack = 4, float interval = 1.5f)
        {
            baseAttackDamage = attack;
            Initialize("Training Dummy", hp, baseArmor, interval);
        }

        public void SetupCustom(string name, int hp, int baseArmor, int attack, float interval)
        {
            baseAttackDamage = attack;
            Initialize(name, hp, baseArmor, interval);
        }

        public void SetEffectiveStats(int newArmor, int newAttack, float newInterval)
        {
            baseAttackDamage = newAttack;
            SetStats(MaxHp, newArmor, newInterval);
        }
    }
}
