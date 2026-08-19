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
    /// Master integration component attaching and coordinating all procedural event subsystems with the live dungeon run loop.
    /// </summary>
    public class RunEventIntegration : MonoBehaviour
    {
        [Header("Subsystems")]
        [SerializeField] private RunManager runManager;
        [SerializeField] private RunEventService eventService;
        [SerializeField] private RunEventTrigger eventTrigger;
        [SerializeField] private RunEventPresenter eventPresenter;
        [SerializeField] private RunEventMobilePanel mobilePanel;
        [SerializeField] private RunModifierManager modifierManager;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private CombatSystem combatSystem;

        private IEconomyService _economyService;

        public RunEventService EventService => eventService;
        public RunEventTrigger EventTrigger => eventTrigger;
        public RunEventPresenter EventPresenter => eventPresenter;
        public RunEventMobilePanel MobilePanel => mobilePanel;

        public void Initialize(
            RunManager manager,
            IEconomyService economy,
            PlayerCombatant player,
            CombatSystem combat,
            RunModifierManager modifiers = null,
            IRandomSource random = null)
        {
            runManager = manager;
            _economyService = economy ?? (manager as IEconomyService);
            playerCombatant = player;
            combatSystem = combat;

            // 1. Ensure RunModifierManager
            if (modifierManager == null)
            {
                modifierManager = GetComponent<RunModifierManager>() ?? gameObject.AddComponent<RunModifierManager>();
                modifierManager.Initialize();
            }

            // 2. Ensure RunEventService
            if (eventService == null)
            {
                eventService = GetComponent<RunEventService>() ?? gameObject.AddComponent<RunEventService>();
                eventService.Initialize(random: random);
            }

            // 3. Ensure RunEventTrigger
            if (eventTrigger == null)
            {
                eventTrigger = GetComponent<RunEventTrigger>() ?? gameObject.AddComponent<RunEventTrigger>();
                eventTrigger.Configure(0.60f, true);
            }

            // 4. Ensure RunEventMobilePanel
            if (mobilePanel == null)
            {
                mobilePanel = GetComponent<RunEventMobilePanel>() ?? gameObject.AddComponent<RunEventMobilePanel>();
            }

            // 5. Ensure RunEventPresenter
            if (eventPresenter == null)
            {
                eventPresenter = GetComponent<RunEventPresenter>() ?? gameObject.AddComponent<RunEventPresenter>();
            }

            eventPresenter.Initialize(
                runManager,
                eventService,
                eventTrigger,
                mobilePanel,
                combatSystem,
                _economyService,
                playerCombatant,
                modifierManager,
                random
            );

            // 6. Hook into RunManager
            if (runManager != null)
            {
                runManager.OnFloorCompleted += HandleFloorCompleted;
            }
        }

        private void OnDestroy()
        {
            if (runManager != null)
            {
                runManager.OnFloorCompleted -= HandleFloorCompleted;
            }
        }

        private void HandleFloorCompleted(int completedFloorNumber)
        {
            if (eventPresenter != null && runManager != null && !runManager.IsRunFinished)
            {
                eventPresenter.TryTriggerBetweenEncounterEvent(runManager.CurrentFloorIndex, runManager.CurrentEncounterIndex);
            }
        }
    }
}
