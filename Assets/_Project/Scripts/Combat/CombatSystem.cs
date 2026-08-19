using System;
using UnityEngine;

namespace Lattirune.Combat
{
    /// <summary>
    /// Coordinates 1v1 auto-battle encounters, execution cooldowns, damage application, and victory/defeat resolution.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("Encounter References")]
        [SerializeField] private PlayerCombatant player;
        [SerializeField] private EnemyCombatant enemy;

        [Header("State")]
        [SerializeField] private CombatState currentState = CombatState.Preparing;
        [SerializeField] private bool autoUpdateInMonoBehaviour = true;

        public event Action<CombatState> OnStateChanged;
        public event Action<DamageResult> OnAttackExecuted;
        public event Action OnVictory;
        public event Action OnDefeat;

        public CombatState CurrentState => currentState;
        public PlayerCombatant Player => player;
        public EnemyCombatant Enemy => enemy;

        public void Initialize(PlayerCombatant playerCombatant, EnemyCombatant enemyCombatant)
        {
            player = playerCombatant;
            enemy = enemyCombatant;
            currentState = CombatState.Preparing;
        }

        private void Update()
        {
            if (autoUpdateInMonoBehaviour)
            {
                UpdateCombat(Time.deltaTime);
            }
        }

        public void StartCombat()
        {
            if (player == null || enemy == null)
            {
                Debug.LogWarning("[Lattirune.Combat] Cannot start combat: Missing player or enemy reference.");
                return;
            }

            currentState = CombatState.Fighting;
            player.ResetCooldown();
            enemy.ResetCooldown();

            OnStateChanged?.Invoke(CombatState.Fighting);
        }

        public void UpdateCombat(float deltaTime)
        {
            if (currentState != CombatState.Fighting || player == null || enemy == null)
            {
                return;
            }

            // 1. Player Attack Turn
            if (player.IsAlive && player.TickCooldown(deltaTime))
            {
                DamageResult playerDamage = DamageCalculator.CalculateDamage(
                    sourceName: player.CombatantName,
                    targetName: enemy.CombatantName,
                    baseDamage: player.BaseAttackDamage,
                    runeBonus: player.ActiveRuneBonus,
                    targetArmor: enemy.Armor,
                    isCritical: false
                );

                enemy.TakeDamage(playerDamage);
                player.ResetCooldown();
                OnAttackExecuted?.Invoke(playerDamage);

                if (!enemy.IsAlive)
                {
                    ResolveVictory();
                    return;
                }
            }

            // 2. Enemy Attack Turn
            if (enemy.IsAlive && enemy.TickCooldown(deltaTime))
            {
                DamageResult enemyDamage = DamageCalculator.CalculateDamage(
                    sourceName: enemy.CombatantName,
                    targetName: player.CombatantName,
                    baseDamage: enemy.BaseAttackDamage,
                    runeBonus: 0,
                    targetArmor: player.Armor,
                    isCritical: false
                );

                player.TakeDamage(enemyDamage);
                enemy.ResetCooldown();
                OnAttackExecuted?.Invoke(enemyDamage);

                if (!player.IsAlive)
                {
                    ResolveDefeat();
                    return;
                }
            }
        }

        public void ResetCombat()
        {
            currentState = CombatState.Preparing;

            if (player != null) player.ResetHpToFull();
            if (enemy != null) enemy.ResetHpToFull();

            OnStateChanged?.Invoke(CombatState.Preparing);
        }

        private void ResolveVictory()
        {
            currentState = CombatState.Victory;
            OnStateChanged?.Invoke(CombatState.Victory);
            OnVictory?.Invoke();
        }

        private void ResolveDefeat()
        {
            currentState = CombatState.Defeat;
            OnStateChanged?.Invoke(CombatState.Defeat);
            OnDefeat?.Invoke();
        }
    }
}
