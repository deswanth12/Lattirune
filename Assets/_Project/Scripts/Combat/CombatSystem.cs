using System;
using UnityEngine;
using Lattirune.Combat.Effects;

namespace Lattirune.Combat
{
    /// <summary>
    /// Coordinates 1v1 auto-battle encounters, execution cooldowns, damage application,
    /// dynamic runtime stat modifiers from CombatEffectSystem, and victory/defeat resolution.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("Encounter References")]
        [SerializeField] private PlayerCombatant player;
        [SerializeField] private EnemyCombatant enemy;
        [SerializeField] private CombatEffectSystem effectSystem;

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
        public CombatEffectSystem Effects => effectSystem;

        public void Initialize(
            PlayerCombatant playerCombatant, 
            EnemyCombatant enemyCombatant, 
            CombatEffectSystem effects = null)
        {
            player = playerCombatant;
            enemy = enemyCombatant;
            effectSystem = effects;
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

            // 0. Update Active Combat Status Effects & DoTs
            if (effectSystem != null)
            {
                effectSystem.UpdateEffects(deltaTime);

                if (!enemy.IsAlive)
                {
                    ResolveVictory();
                    return;
                }
                if (!player.IsAlive)
                {
                    ResolveDefeat();
                    return;
                }
            }

            // 1. Player Attack Turn
            if (player.IsAlive && player.TickCooldown(deltaTime))
            {
                int effectiveEnemyArmor = enemy.Armor;
                float damageModifier = 1.0f;
                int effectivePlayerAttack = player.BaseAttackDamage;

                if (effectSystem != null)
                {
                    effectiveEnemyArmor = Mathf.RoundToInt(enemy.Armor * effectSystem.GetArmorMultiplier(enemy));
                    damageModifier *= effectSystem.GetDamageIntakeMultiplier(enemy);
                    effectivePlayerAttack = Mathf.RoundToInt(player.BaseAttackDamage * effectSystem.GetAttackMultiplier(player));
                }

                DamageResult playerDamage = DamageCalculator.CalculateDamage(
                    sourceName: player.CombatantName,
                    targetName: enemy.CombatantName,
                    baseDamage: effectivePlayerAttack,
                    runeBonus: player.ActiveRuneBonus,
                    targetArmor: effectiveEnemyArmor,
                    isCritical: false,
                    damageModifier: damageModifier
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
                int effectivePlayerArmor = player.Armor;
                float damageModifier = 1.0f;
                int effectiveEnemyAttack = enemy.BaseAttackDamage;

                if (effectSystem != null)
                {
                    effectivePlayerArmor = Mathf.RoundToInt(player.Armor * effectSystem.GetArmorMultiplier(player));
                    damageModifier *= effectSystem.GetDamageIntakeMultiplier(player);
                    effectiveEnemyAttack = Mathf.RoundToInt(enemy.BaseAttackDamage * effectSystem.GetAttackMultiplier(enemy));
                }

                DamageResult enemyDamage = DamageCalculator.CalculateDamage(
                    sourceName: enemy.CombatantName,
                    targetName: player.CombatantName,
                    baseDamage: effectiveEnemyAttack,
                    runeBonus: 0,
                    targetArmor: effectivePlayerArmor,
                    isCritical: false,
                    damageModifier: damageModifier
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
            if (effectSystem != null) effectSystem.ClearAllEffects();

            OnStateChanged?.Invoke(CombatState.Preparing);
        }

        private void ResolveVictory()
        {
            currentState = CombatState.Victory;
            if (effectSystem != null) effectSystem.ClearAllEffects();
            OnStateChanged?.Invoke(CombatState.Victory);
            OnVictory?.Invoke();
        }

        private void ResolveDefeat()
        {
            currentState = CombatState.Defeat;
            if (effectSystem != null) effectSystem.ClearAllEffects();
            OnStateChanged?.Invoke(CombatState.Defeat);
            OnDefeat?.Invoke();
        }
    }
}
