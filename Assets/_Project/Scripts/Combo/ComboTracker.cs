using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Reactions;

namespace Lattirune.Combo
{
    /// <summary>
    /// Event-driven combat combo and consecutive reaction tracker for Lattirune 1.1.
    /// Tracks current combo, highest combo, reaction chains, and calculates the dynamic damage multiplier.
    /// Does not directly mutate base combatant statistics.
    /// </summary>
    public class ComboTracker : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float multiplierStep = 0.05f; // +5% per combo
        [SerializeField] private float maxMultiplier = 2.5f;   // 2.5x cap
        [SerializeField] private float comboTimeoutSeconds = 3.0f; // Timeout in live combat

        private int _currentCombo = 0;
        private int _highestCombo = 0;
        private int _consecutiveReactions = 0;
        private float _timeSinceLastComboAction = 0f;
        private bool _isTimerActive = false;

        public event Action<int, float> OnComboIncremented;
        public event Action<int> OnComboBroken;
        public event Action<int> OnHighestComboUpdated;
        public event Action<int> OnReactionChainIncremented;

        public int CurrentCombo => _currentCombo;
        public int HighestCombo => _highestCombo;
        public int ConsecutiveReactions => _consecutiveReactions;
        public float ComboMultiplier => Mathf.Min(maxMultiplier, 1.0f + (_currentCombo * multiplierStep));
        public float MultiplierStep => multiplierStep;
        public float MaxMultiplier => maxMultiplier;
        public float ComboTimeoutSeconds => comboTimeoutSeconds;

        public void Initialize(float step = 0.05f, float maxMult = 2.5f, float timeout = 3.0f)
        {
            multiplierStep = step;
            maxMultiplier = maxMult;
            comboTimeoutSeconds = timeout;
            ResetCombo();
            _highestCombo = 0;
        }

        public void RecordHit()
        {
            _currentCombo++;
            _timeSinceLastComboAction = 0f;
            _isTimerActive = true;

            if (_currentCombo > _highestCombo)
            {
                _highestCombo = _currentCombo;
                OnHighestComboUpdated?.Invoke(_highestCombo);
            }

            OnComboIncremented?.Invoke(_currentCombo, ComboMultiplier);
        }

        public void RecordReaction(ElementalReactionResult reaction = null)
        {
            _consecutiveReactions++;
            RecordHit();
            OnReactionChainIncremented?.Invoke(_consecutiveReactions);
        }

        public void ResetCombo()
        {
            if (_currentCombo > 0 || _consecutiveReactions > 0)
            {
                int previous = _currentCombo;
                _currentCombo = 0;
                _consecutiveReactions = 0;
                _timeSinceLastComboAction = 0f;
                _isTimerActive = false;
                OnComboBroken?.Invoke(previous);
            }
        }

        public void BindCombatSystem(CombatSystem combat)
        {
            if (combat == null) return;
            combat.OnAttackExecuted += HandleAttackExecuted;
            combat.OnVictory += ResetCombo;
            combat.OnDefeat += ResetCombo;
        }

        public void UnbindCombatSystem(CombatSystem combat)
        {
            if (combat == null) return;
            combat.OnAttackExecuted -= HandleAttackExecuted;
            combat.OnVictory -= ResetCombo;
            combat.OnDefeat -= ResetCombo;
        }

        public void BindReactionSystem(ElementalReactionSystem reactionSystem)
        {
            if (reactionSystem == null) return;
            reactionSystem.OnReactionActivated += HandleReactionActivated;
        }

        public void UnbindReactionSystem(ElementalReactionSystem reactionSystem)
        {
            if (reactionSystem == null) return;
            reactionSystem.OnReactionActivated -= HandleReactionActivated;
        }

        private void HandleAttackExecuted(DamageResult damage)
        {
            if (damage.FinalDamage > 0)
            {
                RecordHit();
            }
            else
            {
                ResetCombo();
            }
        }

        private void HandleReactionActivated(ElementalReactionResult reaction)
        {
            RecordReaction(reaction);
        }

        public void UpdateTimer(float deltaTime)
        {
            if (!_isTimerActive || _currentCombo == 0) return;

            _timeSinceLastComboAction += deltaTime;
            if (_timeSinceLastComboAction >= comboTimeoutSeconds)
            {
                ResetCombo();
            }
        }

        private void Update()
        {
            UpdateTimer(Time.deltaTime);
        }

        public void RestoreHighestCombo(int savedHighest)
        {
            _highestCombo = Mathf.Max(_highestCombo, savedHighest);
        }
    }
}
