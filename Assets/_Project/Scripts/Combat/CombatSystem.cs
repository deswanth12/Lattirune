using System;
using UnityEngine;
using Lattirune.Combat.Effects;
using Lattirune.Combo;
using Lattirune.Modifiers;

namespace Lattirune.Combat
{
    /// <summary>
    /// Coordinates 1v1 auto-battle encounters, execution cooldowns, damage application,
    /// dynamic battle speed multipliers (1x, 2x, 3x), emergency consumable taps,
    /// dynamic runtime stat modifiers from CombatEffectSystem, RunModifierManager, and ComboTracker, and victory/defeat resolution.
    /// Derived strictly from PLAN.md Section 9.1 and Section 9.2.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        [Header("Encounter References")]
        [SerializeField] private PlayerCombatant player;
        [SerializeField] private EnemyCombatant enemy;
        [SerializeField] private CombatEffectSystem effectSystem;
        [SerializeField] private RunModifierManager modifierManager;
        [SerializeField] private ComboTracker comboTracker;

        [Header("State")]
        [SerializeField] private CombatState currentState = CombatState.Preparing;
        [SerializeField] private bool autoUpdateInMonoBehaviour = true;
        [SerializeField] private float speedMultiplier = 1.0f;

        public event Action<CombatState> OnStateChanged;
        public event Action<DamageResult> OnAttackExecuted;
        public event Action<float> OnSpeedMultiplierChanged;
        public event Action<int> OnEmergencyPotionUsed;
        public event Action OnVictory;
        public event Action OnDefeat;

        public CombatState CurrentState => currentState;
        public PlayerCombatant Player => player;
        public EnemyCombatant Enemy => enemy;
        public CombatEffectSystem Effects => effectSystem;
        public RunModifierManager Modifiers => modifierManager;
        public ComboTracker Combo => comboTracker;
        public float SpeedMultiplier => speedMultiplier;

        public void Initialize(
            PlayerCombatant playerCombatant, 
            EnemyCombatant enemyCombatant, 
            CombatEffectSystem effects = null,
            RunModifierManager modifiers = null,
            ComboTracker tracker = null)
        {
            player = playerCombatant;
            enemy = enemyCombatant;
            effectSystem = effects;
            modifierManager = modifiers;
            comboTracker = tracker;
            currentState = CombatState.Preparing;
            speedMultiplier = 1.0f;
        }

        /// <summary>
        /// Sets the battle simulation speed multiplier. Strictly accepts 1.0x, 2.0x, or 3.0x per PLAN.md Section 9.1.
        /// </summary>
        public bool SetSpeedMultiplier(float multiplier)
        {
            if (Mathf.Approximately(multiplier, 1.0f) || 
                Mathf.Approximately(multiplier, 2.0f) || 
                Mathf.Approximately(multiplier, 3.0f))
            {
                speedMultiplier = multiplier;
                OnSpeedMultiplierChanged?.Invoke(speedMultiplier);
                return true;
            }

            Debug.LogWarning($"[Lattirune.Combat] Unsupported combat speed multiplier: {multiplier}. Only 1.0x, 2.0x, and 3.0x are supported.");
            return false;
        }

        /// <summary>
        /// Manual emergency potion tap allowing player agency to immediately drink a consumable during combat.
        /// </summary>
        public bool UseEmergencyPotion(PlayerCombatant targetPlayer, int healAmount)
        {
            if (targetPlayer == null || !targetPlayer.IsAlive || healAmount <= 0)
            {
                return false;
            }

            targetPlayer.Heal(healAmount);
            OnEmergencyPotionUsed?.Invoke(healAmount);
            return true;
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

            enemy.TriggerEncounterStartTraits();

            OnStateChanged?.Invoke(CombatState.Fighting);
        }

        public void Tick(float dt)
        {
            UpdateCombat(dt);
        }

        public void UpdateCombat(float deltaTime)
        {
            if (currentState != CombatState.Fighting || player == null || enemy == null)
            {
                return;
            }

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

            float scaledDelta = deltaTime * speedMultiplier;

            // 0. Update Active Combat Status Effects & DoTs
            if (effectSystem != null)
            {
                effectSystem.UpdateEffects(scaledDelta);

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
            if (player.IsAlive && player.TickCooldown(scaledDelta))
            {
                int effectiveEnemyArmor = enemy.Armor;
                float damageModifier = 1.0f;
                int effectivePlayerAttack = player.BaseAttackDamage;
                int effectiveRuneBonus = player.ActiveRuneBonus;

                if (modifierManager != null)
                {
                    damageModifier *= modifierManager.GetAggregateMultiplier(RunModifierType.DamageMultiplier, 1.0f);
                    effectiveRuneBonus = Mathf.RoundToInt(player.ActiveRuneBonus * modifierManager.GetAggregateMultiplier(RunModifierType.ElementalDamageBonus, 1.0f));
                }

                if (comboTracker != null)
                {
                    damageModifier *= comboTracker.ComboMultiplier;
                }

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
                    runeBonus: effectiveRuneBonus,
                    targetArmor: effectiveEnemyArmor,
                    isCritical: false,
                    damageModifier: damageModifier
                );

                enemy.TakeDamage(playerDamage);
                player.ResetCooldown();

                if (comboTracker != null && playerDamage.FinalDamage > 0)
                {
                    comboTracker.RecordHit();
                }

                OnAttackExecuted?.Invoke(playerDamage);

                // Handle enemy damage reflection if trait exists
                int reflectedDmg = enemy.CalculateDamageReflect(playerDamage);
                if (reflectedDmg > 0 && player.IsAlive)
                {
                    DamageResult reflectResult = new DamageResult(
                        $"{enemy.CombatantName} (Thorns)",
                        player.CombatantName,
                        reflectedDmg,
                        0,
                        reflectedDmg,
                        false,
                        true
                    );
                    player.TakeDamage(reflectResult);
                    OnAttackExecuted?.Invoke(reflectResult);
                }

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

            // 2. Enemy Attack Turn
            if (enemy.IsAlive && enemy.TickCooldown(scaledDelta))
            {
                int effectivePlayerArmor = player.Armor;
                float damageModifier = 1.0f;
                int effectiveEnemyAttack = enemy.BaseAttackDamage;

                if (modifierManager != null)
                {
                    float defenseMultiplier = modifierManager.GetAggregateMultiplier(RunModifierType.CurseOfVulnerability, 1.0f);
                    effectivePlayerArmor = Mathf.Max(0, Mathf.RoundToInt(effectivePlayerArmor * defenseMultiplier));
                }

                if (effectSystem != null)
                {
                    effectivePlayerArmor = Mathf.RoundToInt(effectivePlayerArmor * effectSystem.GetArmorMultiplier(player));
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

                // Trigger enemy attack traits (Poison, Gold Steal, Minions)
                enemy.TriggerAttackTraits(player, enemyDamage);

                if (effectSystem != null && effectSystem.Database != null)
                {
                    for (int t = 0; t < enemy.ActiveTraits.Count; t++)
                    {
                        var trait = enemy.ActiveTraits[t];
                        if (trait != null && trait.TraitType == EnemyTraitType.ApplyPoisonOnHit)
                        {
                            var poisonDef = effectSystem.Database.GetByEffectId("effect_poison_dot");
                            if (poisonDef != null)
                            {
                                effectSystem.ApplyEffect(new CombatEffectInstance(poisonDef, player));
                            }
                        }
                    }
                }

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
            speedMultiplier = 1.0f;

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
