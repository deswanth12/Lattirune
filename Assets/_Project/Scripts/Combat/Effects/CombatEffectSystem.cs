using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Combat.Effects
{
    /// <summary>
    /// Coordinates runtime combat effects and status lifecycles across combatants.
    /// Manages deterministic ticking, DoT damage application, runtime stat modifiers, and encounter cleanup.
    /// </summary>
    public class CombatEffectSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CombatEffectDatabaseSO effectDatabase;

        private readonly Dictionary<Combatant, List<CombatEffectInstance>> _activeEffectsByTarget = new Dictionary<Combatant, List<CombatEffectInstance>>();

        public event Action<CombatEffectInstance> OnEffectApplied;
        public event Action<CombatEffectInstance, float> OnEffectTicked;
        public event Action<CombatEffectInstance> OnEffectExpired;

        public CombatEffectDatabaseSO Database => effectDatabase;

        private void Awake()
        {
            EnsureDefaultDatabase();
        }

        public void Initialize(CombatEffectDatabaseSO db)
        {
            effectDatabase = db;
            EnsureDefaultDatabase();
        }

        public void EnsureDefaultDatabase()
        {
            if (effectDatabase == null)
            {
                effectDatabase = CombatEffectDatabaseSO.CreateDefaultDatabase();
            }
        }

        public void ApplyEffect(CombatEffectInstance instance)
        {
            if (instance == null || instance.Target == null || !instance.Target.IsAlive) return;

            Combatant target = instance.Target;

            // Direct instant damage bursts (e.g. Toxic Flame)
            if (instance.Definition.EffectType == CombatEffectType.DirectDamage)
            {
                DamageResult burstDamage = DamageCalculator.CalculateDamage(
                    sourceName: instance.Definition.DisplayName,
                    targetName: target.CombatantName,
                    baseDamage: Mathf.RoundToInt(instance.Definition.Magnitude),
                    runeBonus: 0,
                    targetArmor: target.Armor
                );
                target.TakeDamage(burstDamage);
                OnEffectApplied?.Invoke(instance);
                return;
            }

            if (!_activeEffectsByTarget.TryGetValue(target, out List<CombatEffectInstance> effectList))
            {
                effectList = new List<CombatEffectInstance>();
                _activeEffectsByTarget[target] = effectList;
            }

            // Check if same effect is already active on target -> refresh duration
            CombatEffectInstance existing = effectList.Find(e => e.Definition.EffectId == instance.Definition.EffectId);
            if (existing != null)
            {
                existing.RefreshDuration();
            }
            else
            {
                effectList.Add(instance);
                OnEffectApplied?.Invoke(instance);
            }
        }

        public void UpdateEffects(float deltaTime)
        {
            List<Combatant> deadTargets = new List<Combatant>();

            foreach (var kvp in _activeEffectsByTarget)
            {
                Combatant target = kvp.Key;
                List<CombatEffectInstance> effects = kvp.Value;

                if (target == null || !target.IsAlive)
                {
                    deadTargets.Add(target);
                    continue;
                }

                for (int i = effects.Count - 1; i >= 0; i--)
                {
                    CombatEffectInstance effect = effects[i];

                    // Tick duration and DoT
                    if (effect.Tick(deltaTime, out float periodicDamage))
                    {
                        if (periodicDamage > 0f && target.IsAlive)
                        {
                            DamageResult dotDamage = DamageCalculator.CalculateDamage(
                                sourceName: effect.Definition.DisplayName,
                                targetName: target.CombatantName,
                                baseDamage: Mathf.RoundToInt(periodicDamage),
                                runeBonus: 0,
                                targetArmor: 0 // DoT bypasses flat armor
                            );
                            target.TakeDamage(dotDamage);
                            OnEffectTicked?.Invoke(effect, periodicDamage);
                        }
                    }

                    if (effect.IsExpired || !target.IsAlive)
                    {
                        effects.RemoveAt(i);
                        OnEffectExpired?.Invoke(effect);
                    }
                }
            }

            // Clean up dead targets
            foreach (var dead in deadTargets)
            {
                _activeEffectsByTarget.Remove(dead);
            }
        }

        public float GetArmorMultiplier(Combatant target)
        {
            if (target == null || !_activeEffectsByTarget.TryGetValue(target, out var effects)) return 1.0f;

            float multiplier = 1.0f;
            foreach (var e in effects)
            {
                if (e.Definition.EffectType == CombatEffectType.ArmorModifier)
                {
                    multiplier *= Mathf.Clamp01(1.0f - e.Definition.Magnitude);
                }
            }
            return multiplier;
        }

        public float GetDamageIntakeMultiplier(Combatant target)
        {
            if (target == null || !_activeEffectsByTarget.TryGetValue(target, out var effects)) return 1.0f;

            float multiplier = 1.0f;
            foreach (var e in effects)
            {
                if (e.Definition.EffectType == CombatEffectType.DamageModifier)
                {
                    multiplier *= (1.0f + e.Definition.Magnitude);
                }
            }
            return multiplier;
        }

        public float GetAttackMultiplier(Combatant attacker)
        {
            if (attacker == null || !_activeEffectsByTarget.TryGetValue(attacker, out var effects)) return 1.0f;

            float multiplier = 1.0f;
            foreach (var e in effects)
            {
                if (e.Definition.EffectType == CombatEffectType.AttackModifier)
                {
                    multiplier *= Mathf.Clamp01(1.0f - e.Definition.Magnitude);
                }
            }
            return multiplier;
        }

        public IReadOnlyList<CombatEffectInstance> GetActiveEffects(Combatant target)
        {
            if (target != null && _activeEffectsByTarget.TryGetValue(target, out var list))
            {
                return list;
            }
            return Array.Empty<CombatEffectInstance>();
        }

        public int GetActiveEffectCount(Combatant target)
        {
            if (target != null && _activeEffectsByTarget.TryGetValue(target, out var list))
            {
                return list.Count;
            }
            return 0;
        }

        public void ClearAllEffects()
        {
            _activeEffectsByTarget.Clear();
        }
    }
}
