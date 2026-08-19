using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Modifiers;

namespace Lattirune.Events
{
    /// <summary>
    /// Coordinates procedural event presentation, pausing dungeon progression during event resolution,
    /// and resuming the run smoothly upon panel dismissal.
    /// </summary>
    public class RunEventPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RunManager runManager;
        [SerializeField] private RunEventService eventService;
        [SerializeField] private RunEventTrigger eventTrigger;
        [SerializeField] private RunEventMobilePanel mobilePanel;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunModifierManager modifierManager;

        private IEconomyService _economyManager;
        private IRandomSource _randomSource;
        private bool _isHandlingEvent = false;

        public event Action<RunEventDefinitionSO> OnEventStarted;
        public event Action OnEventCompleted;

        public bool IsHandlingEvent => _isHandlingEvent;

        public void Initialize(
            RunManager manager,
            RunEventService service,
            RunEventTrigger trigger,
            RunEventMobilePanel panel,
            CombatSystem combat,
            IEconomyService economy,
            PlayerCombatant player,
            RunModifierManager modifiers,
            IRandomSource random = null)
        {
            runManager = manager;
            eventService = service;
            eventTrigger = trigger;
            mobilePanel = panel;
            combatSystem = combat;
            _economyManager = economy ?? (manager as IEconomyService);
            playerCombatant = player;
            modifierManager = modifiers;
            _randomSource = random ?? new SystemRandomSource();
            _isHandlingEvent = false;

            if (mobilePanel != null)
            {
                mobilePanel.Initialize(eventService, _economyManager, playerCombatant, modifierManager);
                mobilePanel.OnPanelDismissed += HandlePanelDismissed;
            }
        }

        private void OnDestroy()
        {
            if (mobilePanel != null)
            {
                mobilePanel.OnPanelDismissed -= HandlePanelDismissed;
            }
        }

        public void SetRandomSource(IRandomSource random)
        {
            _randomSource = random ?? new SystemRandomSource();
        }

        public bool TryTriggerBetweenEncounterEvent(int floorIndex, int encounterIndex)
        {
            if (eventTrigger == null || eventService == null || mobilePanel == null)
            {
                return false;
            }

            if (!eventTrigger.ShouldTriggerEvent(floorIndex, encounterIndex, combatSystem, _randomSource))
            {
                return false;
            }

            RunEventDefinitionSO ev = eventService.SelectEligibleEvent(floorIndex);
            if (ev == null)
            {
                return false;
            }

            _isHandlingEvent = true;

            if (runManager != null)
            {
                runManager.PauseForEvent();
            }

            eventService.PresentEvent(ev);
            mobilePanel.Show(ev);
            OnEventStarted?.Invoke(ev);
            return true;
        }

        private void HandlePanelDismissed()
        {
            if (!_isHandlingEvent) return;

            _isHandlingEvent = false;
            if (runManager != null)
            {
                runManager.ResumeFromEvent();
            }

            OnEventCompleted?.Invoke();
        }
    }
}
