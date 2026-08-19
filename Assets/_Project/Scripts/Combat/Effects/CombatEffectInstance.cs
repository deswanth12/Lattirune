using UnityEngine;

namespace Lattirune.Combat.Effects
{
    /// <summary>
    /// Runtime instance of an active combat effect bound to a specific target combatant.
    /// Tracks deterministic duration and periodic tick intervals without mutating static ScriptableObjects.
    /// </summary>
    public class CombatEffectInstance
    {
        public CombatEffectDefinitionSO Definition { get; private set; }
        public string SourceRuneAId { get; private set; }
        public string SourceRuneBId { get; private set; }
        public Combatant Target { get; private set; }
        public float RemainingDuration { get; private set; }
        public float TickTimer { get; private set; }
        public int StackCount { get; private set; }

        public bool IsExpired => Definition.Duration > 0f && RemainingDuration <= 0f;

        public CombatEffectInstance(
            CombatEffectDefinitionSO definition,
            string runeAId,
            string runeBId,
            Combatant target)
        {
            Definition = definition;
            SourceRuneAId = runeAId;
            SourceRuneBId = runeBId;
            Target = target;
            RemainingDuration = definition.Duration;
            TickTimer = definition.TickInterval;
            StackCount = 1;
        }

        public void RefreshDuration()
        {
            RemainingDuration = Definition.Duration;
            TickTimer = Definition.TickInterval;
        }

        public void AddStack()
        {
            StackCount++;
            RefreshDuration();
        }

        /// <summary>
        /// Advances the effect's duration and tick timers deterministically.
        /// Returns true if a periodic tick occurs on this frame/step.
        /// </summary>
        public bool Tick(float deltaTime, out float periodicDamage)
        {
            periodicDamage = 0f;

            if (Definition.Duration > 0f)
            {
                RemainingDuration -= deltaTime;
            }

            if (Definition.EffectType == CombatEffectType.DamageOverTime && Definition.TickInterval > 0f)
            {
                TickTimer -= deltaTime;
                if (TickTimer <= 0f)
                {
                    TickTimer = Definition.TickInterval;
                    periodicDamage = Definition.Magnitude;
                    return true;
                }
            }

            return false;
        }
    }
}
