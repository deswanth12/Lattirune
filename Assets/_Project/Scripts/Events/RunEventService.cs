using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Economy;
using Lattirune.Modifiers;

namespace Lattirune.Events
{
    /// <summary>
    /// Master runtime service coordinating procedural run event selection, presentation,
    /// resolution, and transactional outcome application across game systems.
    /// </summary>
    public class RunEventService : MonoBehaviour
    {
        [SerializeField] private RunEventDatabaseSO database;
        private IRandomSource _randomSource;
        private RunEventDefinitionSO _currentActiveEvent;
        private readonly HashSet<string> _consumedChoiceIds = new HashSet<string>();

        public event Action<RunEventDefinitionSO> OnEventPresented;
        public event Action<RunEventChoice> OnChoiceSelected;
        public event Action<RunEventDefinitionSO, RunEventChoice, RunEventResolutionResult> OnEventResolved;
        public event Action<RunEventDefinitionSO, RunEventChoice, string> OnEventFailed;

        public RunEventDatabaseSO Database => database;
        public RunEventDefinitionSO CurrentActiveEvent => _currentActiveEvent;
        public bool HasActiveEvent => _currentActiveEvent != null;
        public IReadOnlyCollection<string> ConsumedChoiceIds => _consumedChoiceIds;

        public void Initialize(RunEventDatabaseSO db = null, IRandomSource random = null)
        {
            database = db ?? RunEventDatabaseSO.CreateCanonicalEventDatabase();
            _randomSource = random ?? new SystemRandomSource();
            _currentActiveEvent = null;
            _consumedChoiceIds.Clear();
        }

        public void SetRandomSource(IRandomSource random)
        {
            _randomSource = random ?? new SystemRandomSource();
        }

        public RunEventDefinitionSO SelectEligibleEvent(int floorIndex)
        {
            if (database == null)
            {
                database = RunEventDatabaseSO.CreateCanonicalEventDatabase();
            }

            List<RunEventDefinitionSO> eligible = database.GetEligibleEventsForFloor(floorIndex);
            if (eligible == null || eligible.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < eligible.Count; i++)
            {
                if (eligible[i] != null && eligible[i].Weight > 0)
                {
                    totalWeight += eligible[i].Weight;
                }
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            if (_randomSource == null)
            {
                _randomSource = new SystemRandomSource();
            }

            int roll = _randomSource.Next(0, totalWeight);
            int accumulated = 0;

            for (int i = 0; i < eligible.Count; i++)
            {
                var ev = eligible[i];
                if (ev != null && ev.Weight > 0)
                {
                    accumulated += ev.Weight;
                    if (roll < accumulated)
                    {
                        return ev;
                    }
                }
            }

            return eligible[eligible.Count - 1];
        }

        public bool SelectAndPresentEventForFloor(int floorIndex)
        {
            RunEventDefinitionSO ev = SelectEligibleEvent(floorIndex);
            if (ev != null)
            {
                PresentEvent(ev);
                return true;
            }
            return false;
        }

        public void PresentEvent(RunEventDefinitionSO eventDef)
        {
            if (eventDef == null) return;
            _currentActiveEvent = eventDef;
            OnEventPresented?.Invoke(_currentActiveEvent);
        }

        public bool SelectChoice(
            string choiceId,
            EconomyManager economy,
            PlayerCombatant player,
            RunModifierManager modifierManager)
        {
            if (_currentActiveEvent == null)
            {
                OnEventFailed?.Invoke(null, null, ""No event is currently active."");
                return false;
            }

            RunEventChoice choice = _currentActiveEvent.GetChoice(choiceId);
            if (choice == null)
            {
                OnEventFailed?.Invoke(_currentActiveEvent, null, $""Choice '{choiceId}' not found in active event."");
                return false;
            }

            OnChoiceSelected?.Invoke(choice);

            int currentGold = economy != null ? economy.GoldBalance : 0;
            int currentHp = player != null ? player.CurrentHp : 100;
            int maxHp = player != null ? player.MaxHp : 100;
            IReadOnlyCollection<string> activeMods = modifierManager != null ? modifierManager.ExportActiveModifierIds() : new List<string>();

            RunEventResolutionResult result = RunEventResolver.ResolveChoice(
                _currentActiveEvent,
                choice,
                currentGold,
                currentHp,
                maxHp,
                activeMods,
                _consumedChoiceIds
            );

            if (!result.IsSuccess)
            {
                OnEventFailed?.Invoke(_currentActiveEvent, choice, result.FailureReason);
                return false;
            }

            // Apply outcomes
            if (result.GoldDelta > 0 && economy != null)
            {
                economy.AddGold(result.GoldDelta);
            }
            else if (result.GoldDelta < 0 && economy != null)
            {
                economy.SpendGold(-result.GoldDelta);
            }

            if (result.HpDelta > 0 && player != null)
            {
                player.Heal(result.HpDelta);
            }
            else if (result.HpDelta < 0 && player != null)
            {
                player.TakeDirectDamage(-result.HpDelta);
            }

            if (modifierManager != null)
            {
                for (int i = 0; i < result.GrantedModifierIds.Count; i++)
                {
                    modifierManager.AddModifierById(result.GrantedModifierIds[i]);
                }

                for (int i = 0; i < result.CurseModifierIds.Count; i++)
                {
                    modifierManager.AddModifierById(result.CurseModifierIds[i]);
                }
            }

            if (choice.OneTimeUse)
            {
                _consumedChoiceIds.Add(choice.ChoiceId);
            }

            var resolvedEvent = _currentActiveEvent;
            _currentActiveEvent = null;
            OnEventResolved?.Invoke(resolvedEvent, choice, result);
            return true;
        }

        public void ClearActiveEvent()
        {
            _currentActiveEvent = null;
        }

        public void ResetConsumedChoices()
        {
            _consumedChoiceIds.Clear();
        }
    }
}
