using System;
using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Base entity managing health points, defense armor, attack cooldown timers, and damage events.
    /// </summary>
    public class Combatant : MonoBehaviour
    {
        [Header("Identity & Stats")]
        [SerializeField] private string combatantName = "Entity";
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int currentHp = 100;
        [SerializeField] private int armor = 0;
        [SerializeField] private float attackInterval = 1.5f;

        [Header("State")]
        [SerializeField] private float cooldownTimer = 0f;

        public event Action<DamageResult> OnDamaged;
        public event Action OnDied;
        public event Action OnHpChanged;

        public string CombatantName => combatantName;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public int Armor => armor;
        public float AttackInterval => attackInterval;
        public float CooldownTimer => cooldownTimer;
        public bool IsAlive => currentHp > 0;

        public virtual void Initialize(string name, int maxHealth, int baseArmor, float interval)
        {
            combatantName = name;
            maxHp = Mathf.Max(1, maxHealth);
            currentHp = maxHp;
            armor = Mathf.Max(0, baseArmor);
            attackInterval = Mathf.Max(0.1f, interval);
            cooldownTimer = 0f;
        }

        public void SetStats(int maxHealth, int baseArmor, float interval)
        {
            maxHp = Mathf.Max(1, maxHealth);
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            armor = Mathf.Max(0, baseArmor);
            attackInterval = Mathf.Max(0.1f, interval);
        }

        public bool TickCooldown(float deltaTime)
        {
            if (!IsAlive) return false;

            cooldownTimer -= deltaTime;
            if (cooldownTimer <= 0f)
            {
                return true;
            }
            return false;
        }

        public void ResetCooldown()
        {
            cooldownTimer = attackInterval;
        }

        public virtual void TakeDamage(DamageResult damage)
        {
            if (!IsAlive || damage == null) return;

            currentHp = Mathf.Max(0, currentHp - damage.FinalDamage);
            OnDamaged?.Invoke(damage);
            OnHpChanged?.Invoke();

            if (currentHp == 0)
            {
                OnDied?.Invoke();
            }
        }

        public void ResetHpToFull()
        {
            currentHp = maxHp;
            cooldownTimer = 0f;
            OnHpChanged?.Invoke();
        }
    }
}
