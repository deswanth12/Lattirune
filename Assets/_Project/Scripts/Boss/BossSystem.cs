using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Combat.Effects;

namespace Lattirune.Boss
{
    /// <summary>
    /// Master controller for Multi-Phase Boss encounters.
    /// Evaluates deterministic health percentage thresholds, manages phase transitions,
    /// applies phase stat modifiers, and exposes telemetry.
    /// </summary>
    public class BossSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BossDefinitionSO bossDefinition;

        [Header("References")]
        [SerializeField] private EnemyCombatant enemyCombatant;
        [SerializeField] private CombatEffectSystem combatEffectSystem;

        [Header("Runtime State")]
        [SerializeField] private int currentPhaseIndex = 0;
        [SerializeField] private int phaseTransitionCount = 0;
        [SerializeField] private bool isBossActive = false;

        public event Action<int, BossPhaseDefinitionSO> OnPhaseChanged;
        public event Action OnBossEnraged;

        public BossDefinitionSO Definition => bossDefinition;
        public int CurrentPhaseIndex => currentPhaseIndex;
        public int CurrentPhaseNumber => currentPhaseIndex + 1;
        public int TotalPhases => bossDefinition != null ? bossDefinition.PhaseCount : 0;
        public BossPhaseDefinitionSO CurrentPhase => bossDefinition != null ? bossDefinition.GetPhase(currentPhaseIndex) : null;
        public int PhaseTransitionCount => phaseTransitionCount;
        public bool IsBossActive => isBossActive;

        private void Awake()
        {
            EnsureDefaultBossDefinition();
        }

        public void EnsureDefaultBossDefinition()
        {
            if (bossDefinition == null)
            {
                bossDefinition = BossDefinitionSO.CreateLichLordDefinition();
            }
        }

        public void Initialize(BossDefinitionSO boss, EnemyCombatant enemy, CombatEffectSystem effectSys = null)
        {
            bossDefinition = boss;
            EnsureDefaultBossDefinition();

            enemyCombatant = enemy;
            combatEffectSystem = effectSys;

            currentPhaseIndex = 0;
            phaseTransitionCount = 0;
            isBossActive = false;

            if (enemyCombatant != null)
            {
                enemyCombatant.OnHpChanged += HandleEnemyHpChanged;
                enemyCombatant.OnDied += HandleBossDied;
            }
        }

        private void OnDestroy()
        {
            if (enemyCombatant != null)
            {
                enemyCombatant.OnHpChanged -= HandleEnemyHpChanged;
                enemyCombatant.OnDied -= HandleBossDied;
            }
        }

        public void StartBossFight()
        {
            EnsureDefaultBossDefinition();
            if (bossDefinition == null || enemyCombatant == null) return;

            currentPhaseIndex = 0;
            phaseTransitionCount = 0;
            isBossActive = true;

            // Initialize enemy combatant with base stats
            enemyCombatant.SetupCustom(
                bossDefinition.DisplayName,
                bossDefinition.MaxHp,
                bossDefinition.BaseArmor,
                bossDefinition.BaseAttack,
                bossDefinition.BaseAttackInterval
            );

            // Apply Phase 1
            ApplyPhaseStats(currentPhaseIndex);
            OnPhaseChanged?.Invoke(currentPhaseIndex, CurrentPhase);
        }

        private void HandleEnemyHpChanged()
        {
            if (!isBossActive || enemyCombatant == null || !enemyCombatant.IsAlive) return;

            EvaluatePhaseTransitions();
        }

        public void EvaluatePhaseTransitions()
        {
            if (!isBossActive || bossDefinition == null || enemyCombatant == null || enemyCombatant.CurrentHp <= 0)
            {
                return;
            }

            float currentHpPct = (float)enemyCombatant.CurrentHp / enemyCombatant.MaxHp;

            // Determine deepest phase whose threshold is >= currentHpPct
            int highestEligiblePhase = currentPhaseIndex;
            for (int i = currentPhaseIndex + 1; i < bossDefinition.PhaseCount; i++)
            {
                BossPhaseDefinitionSO nextPhase = bossDefinition.GetPhase(i);
                if (nextPhase != null && currentHpPct <= nextPhase.HpThresholdPercentage)
                {
                    highestEligiblePhase = i;
                }
            }

            if (highestEligiblePhase > currentPhaseIndex)
            {
                TransitionToPhase(highestEligiblePhase);
            }
        }

        private void TransitionToPhase(int newPhaseIndex)
        {
            currentPhaseIndex = newPhaseIndex;
            phaseTransitionCount++;

            ApplyPhaseStats(currentPhaseIndex);
            OnPhaseChanged?.Invoke(currentPhaseIndex, CurrentPhase);

            if (currentPhaseIndex >= 1)
            {
                OnBossEnraged?.Invoke();
            }
        }

        private void ApplyPhaseStats(int phaseIdx)
        {
            if (bossDefinition == null || enemyCombatant == null) return;

            BossPhaseDefinitionSO phase = bossDefinition.GetPhase(phaseIdx);
            if (phase == null) return;

            int effectiveArmor = bossDefinition.BaseArmor + phase.BonusArmor;
            int effectiveAttack = bossDefinition.BaseAttack + phase.BonusAttack;
            float effectiveInterval = bossDefinition.BaseAttackInterval * phase.AttackIntervalMultiplier;

            enemyCombatant.SetEffectiveStats(effectiveArmor, effectiveAttack, effectiveInterval);

            // Optional Phase Status Effect
            if (phase.PhaseEffect != null && combatEffectSystem != null)
            {
                CombatEffectInstance instance = new CombatEffectInstance(
                    phase.PhaseEffect,
                    enemyCombatant,
                    enemyCombatant
                );
                combatEffectSystem.ApplyEffect(instance);
            }
        }

        private void HandleBossDied()
        {
            isBossActive = false;
        }

        public void StopBossFight()
        {
            isBossActive = false;
        }

        public void ResetBoss()
        {
            currentPhaseIndex = 0;
            phaseTransitionCount = 0;
            isBossActive = false;
        }

        public BossTelemetry GetTelemetry()
        {
            if (bossDefinition == null || enemyCombatant == null)
            {
                return default;
            }

            int currentHp = enemyCombatant.CurrentHp;
            int maxHp = enemyCombatant.MaxHp;
            float hpPct = maxHp > 0 ? (float)currentHp / maxHp : 0f;

            return new BossTelemetry(
                id: bossDefinition.BossId,
                name: bossDefinition.DisplayName,
                phaseIdx: currentPhaseIndex,
                phaseName: CurrentPhase != null ? CurrentPhase.DisplayName : "Unknown",
                hp: currentHp,
                maxHp: maxHp,
                hpPct: hpPct,
                atk: enemyCombatant.BaseAttackDamage,
                arm: enemyCombatant.Armor,
                interval: enemyCombatant.AttackInterval,
                transitions: phaseTransitionCount
            );
        }
    }
}
