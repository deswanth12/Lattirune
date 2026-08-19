using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Events
{
    /// <summary>
    /// Immutable struct holding the mathematical breakdown and resulting delta of an event resolution.
    /// </summary>
    public readonly struct RunEventResolutionResult
    {
        public bool IsSuccess { get; }
        public int GoldDelta { get; }
        public int HpDelta { get; }
        public IReadOnlyList<string> GrantedModifierIds { get; }
        public IReadOnlyList<string> CurseModifierIds { get; }
        public string FailureReason { get; }

        private RunEventResolutionResult(
            bool success,
            int goldDelta,
            int hpDelta,
            List<string> grantedMods,
            List<string> curseMods,
            string failureReason)
        {
            IsSuccess = success;
            GoldDelta = goldDelta;
            HpDelta = hpDelta;
            GrantedModifierIds = grantedMods ?? new List<string>();
            CurseModifierIds = curseMods ?? new List<string>();
            FailureReason = failureReason ?? string.Empty;
        }

        public static RunEventResolutionResult CreateSuccess(
            int goldDelta,
            int hpDelta,
            List<string> grantedMods = null,
            List<string> curseMods = null)
        {
            return new RunEventResolutionResult(true, goldDelta, hpDelta, grantedMods, curseMods, null);
        }

        public static RunEventResolutionResult CreateFailure(string reason)
        {
            return new RunEventResolutionResult(false, 0, 0, null, null, reason);
        }
    }

    /// <summary>
    /// Pure mathematical evaluator for procedural run event choices.
    /// Does not mutate any external state or entities.
    /// </summary>
    public static class RunEventResolver
    {
        public static RunEventResolutionResult ResolveChoice(
            RunEventDefinitionSO eventDef,
            RunEventChoice choice,
            int currentGold,
            int currentHp,
            int maxHp,
            IReadOnlyCollection<string> activeModifierIds,
            IReadOnlyCollection<string> consumedChoiceIds = null)
        {
            if (eventDef == null)
            {
                return RunEventResolutionResult.CreateFailure(""Event definition is null."");
            }

            if (choice == null || string.IsNullOrEmpty(choice.ChoiceId))
            {
                return RunEventResolutionResult.CreateFailure(""Selected choice is null or invalid."");
            }

            if (consumedChoiceIds != null && choice.OneTimeUse && consumedChoiceIds.Contains(choice.ChoiceId))
            {
                return RunEventResolutionResult.CreateFailure(""This choice has already been consumed for this run."");
            }

            if (choice.GoldCost > 0 && currentGold < choice.GoldCost)
            {
                return RunEventResolutionResult.CreateFailure($""Insufficient gold (requires {choice.GoldCost}, have {currentGold})."");
            }

            if (choice.RequiredGold > 0 && currentGold < choice.RequiredGold)
            {
                return RunEventResolutionResult.CreateFailure($""Requires at least {choice.RequiredGold} gold in purse (have {currentGold})."");
            }

            if (maxHp <= 0) maxHp = 1;
            int hpCost = Mathf.RoundToInt(maxHp * choice.HealthCostPercentage);
            if (hpCost > 0 && currentHp <= hpCost)
            {
                return RunEventResolutionResult.CreateFailure(""Hero health is too low to survive the sacrifice."");
            }

            if (!string.IsNullOrEmpty(choice.GrantedModifierId) && activeModifierIds != null && activeModifierIds.Contains(choice.GrantedModifierId))
            {
                return RunEventResolutionResult.CreateFailure(""Hero already has the granted modifier active."");
            }

            if (!string.IsNullOrEmpty(choice.CurseModifierId) && activeModifierIds != null && activeModifierIds.Contains(choice.CurseModifierId))
            {
                return RunEventResolutionResult.CreateFailure(""Hero already has the curse active."");
            }

            int goldDelta = choice.GoldReward - choice.GoldCost;
            int hpRestore = Mathf.RoundToInt(maxHp * choice.HealthRestorePercentage);
            int hpDelta = hpRestore - hpCost;

            List<string> granted = new List<string>();
            if (!string.IsNullOrEmpty(choice.GrantedModifierId))
            {
                granted.Add(choice.GrantedModifierId);
            }

            List<string> curses = new List<string>();
            if (!string.IsNullOrEmpty(choice.CurseModifierId))
            {
                curses.Add(choice.CurseModifierId);
            }

            return RunEventResolutionResult.CreateSuccess(goldDelta, hpDelta, granted, curses);
        }
    }
}
