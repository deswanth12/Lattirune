using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Economy;
using Lattirune.Modifiers;

namespace Lattirune.Choices
{
    /// <summary>
    /// Service managing validation, payment, and application of risk/reward choices during procedural runs.
    /// </summary>
    public class RunChoiceService : MonoBehaviour
    {
        private readonly HashSet<string> _consumedChoiceIds = new HashSet<string>();

        public event Action<RunChoiceDefinitionSO> OnChoiceApplied;
        public event Action<RunChoiceDefinitionSO, string> OnChoiceFailed;

        public IReadOnlyCollection<string> ConsumedChoiceIds => _consumedChoiceIds;

        public bool CanApplyChoice(
            RunChoiceDefinitionSO choice, 
            int currentGold, 
            PlayerCombatant player, 
            RunModifierManager modifierManager, 
            out string reason)
        {
            if (choice == null || string.IsNullOrEmpty(choice.ChoiceId))
            {
                reason = "Choice definition is null or invalid.";
                return false;
            }

            if (choice.IsOneTimeUse && _consumedChoiceIds.Contains(choice.ChoiceId))
            {
                reason = "This choice has already been consumed for this run.";
                return false;
            }

            if (choice.GoldCost > 0 && currentGold < choice.GoldCost)
            {
                reason = $"Insufficient gold (requires {choice.GoldCost}, have {currentGold}).";
                return false;
            }

            if (choice.HealthCostPercentage > 0f && player != null)
            {
                int hpCost = Mathf.RoundToInt(player.MaxHp * choice.HealthCostPercentage);
                if (player.CurrentHp <= hpCost)
                {
                    reason = "Hero health is too low to survive the sacrifice.";
                    return false;
                }
            }

            if (choice.GrantedModifier != null && modifierManager != null && modifierManager.HasModifier(choice.GrantedModifier.ModifierId))
            {
                reason = "Hero already has the granted modifier active.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool ApplyChoice(
            RunChoiceDefinitionSO choice, 
            EconomyManager economy, 
            PlayerCombatant player, 
            RunModifierManager modifierManager)
        {
            int currentGold = economy != null ? economy.GoldBalance : 0;
            if (!CanApplyChoice(choice, currentGold, player, modifierManager, out string reason))
            {
                OnChoiceFailed?.Invoke(choice, reason);
                return false;
            }

            // 1. Deduct Gold cost
            if (choice.GoldCost > 0 && economy != null)
            {
                economy.SpendGold(choice.GoldCost);
            }

            // 2. Apply Health sacrifice
            if (choice.HealthCostPercentage > 0f && player != null)
            {
                int hpCost = Mathf.RoundToInt(player.MaxHp * choice.HealthCostPercentage);
                player.TakeDirectDamage(hpCost);
            }

            // 3. Grant positive modifier
            if (choice.GrantedModifier != null && modifierManager != null)
            {
                modifierManager.AddModifier(choice.GrantedModifier);
            }

            // 4. Apply curse modifier
            if (choice.CurseModifier != null && modifierManager != null)
            {
                modifierManager.AddModifier(choice.CurseModifier);
            }

            // 5. Mark consumed if one-time
            if (choice.IsOneTimeUse)
            {
                _consumedChoiceIds.Add(choice.ChoiceId);
            }

            OnChoiceApplied?.Invoke(choice);
            return true;
        }

        public void ResetChoices()
        {
            _consumedChoiceIds.Clear();
        }
    }
}
