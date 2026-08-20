using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Events;
using Lattirune.Modifiers;

namespace Lattirune.UI
{
    /// <summary>
    /// Lightweight presentation component for procedural run events in Lattirune 1.1.
    /// Renders the event modal, title, description, current resources (Gold/HP), choice buttons, and resolution outcome.
    /// </summary>
    public class RunEventUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RunEventService eventService;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunModifierManager modifierManager;
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;
        [SerializeField] private DungeonMapScreenController mapController;

        private IEconomyService _economyService;
        private RunEventDefinitionSO _activeEvent;
        private string _lastOutcomeMessage = string.Empty;
        private bool _isShowingModal = false;
        private bool _isResolved = false;

        public bool IsShowingModal => _isShowingModal;
        public RunEventDefinitionSO ActiveEvent => _activeEvent;
        public string LastOutcomeMessage => _lastOutcomeMessage;

        public void BindMapController(DungeonMapScreenController map)
        {
            mapController = map;
        }

        public void Initialize(
            RunEventService service,
            IEconomyService economy,
            PlayerCombatant player,
            RunModifierManager modifiers,
            ScreenNavigationController nav = null,
            RunManager run = null,
            DungeonMapScreenController map = null)
        {
            eventService = service;
            _economyService = economy;
            playerCombatant = player;
            modifierManager = modifiers;
            navigation = nav;
            runManager = run;
            mapController = map;

            if (eventService != null)
            {
                eventService.OnEventPresented += HandleEventPresented;
                eventService.OnEventResolved += HandleEventResolved;
                eventService.OnEventFailed += HandleEventFailed;
            }

            if (navigation != null)
            {
                navigation.OnScreenChanged += HandleScreenChanged;
            }
        }

        private void OnDestroy()
        {
            if (eventService != null)
            {
                eventService.OnEventPresented -= HandleEventPresented;
                eventService.OnEventResolved -= HandleEventResolved;
                eventService.OnEventFailed -= HandleEventFailed;
            }
            if (navigation != null)
            {
                navigation.OnScreenChanged -= HandleScreenChanged;
            }
        }

        private void HandleScreenChanged(ScreenState prev, ScreenState next)
        {
            if (next == ScreenState.EVENT)
            {
                if (_activeEvent == null && eventService != null)
                {
                    int floor = runManager != null ? runManager.CurrentFloorIndex : 0;
                    eventService.SelectAndPresentEventForFloor(floor);
                }
            }
        }

        private void HandleEventPresented(RunEventDefinitionSO ev)
        {
            _activeEvent = ev;
            _lastOutcomeMessage = string.Empty;
            _isShowingModal = true;
            _isResolved = false;
        }

        private void HandleEventResolved(RunEventDefinitionSO ev, RunEventChoice choice, RunEventResolutionResult result)
        {
            _lastOutcomeMessage = $"Choice resolved: {choice.DisplayName} applied successfully.";
            _isResolved = true;
        }

        private void HandleEventFailed(RunEventDefinitionSO ev, RunEventChoice choice, string reason)
        {
            _lastOutcomeMessage = $"Cannot proceed: {reason}";
        }

        public void CloseModal()
        {
            _isShowingModal = false;
            _activeEvent = null;
            _isResolved = false;
            if (eventService != null)
            {
                eventService.ClearActiveEvent();
            }
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.EVENT) return;
            if (!_isShowingModal || _activeEvent == null) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1300f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), _activeEvent.Title);

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader(_activeEvent.Title, "A mysterious encounter deep within the Cursed Sewers.");
            GUILayout.Space(12);

            int currentGold = _economyService != null ? _economyService.GoldBalance : 0;
            int currentHp = playerCombatant != null ? playerCombatant.CurrentHp : 100;
            int maxHp = playerCombatant != null ? playerCombatant.MaxHp : 100;
            LattiruneUITheme.DrawBadge($"Hero HP: {currentHp}/{maxHp}  |  Gold: {currentGold}g", LattiruneUITheme.ColorCyanArcane);
            GUILayout.Space(16);

            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUIStyle bodyStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            bodyStyle.fontSize = 18;
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;
            GUILayout.Label(_activeEvent.Description, bodyStyle);
            GUILayout.EndVertical();
            GUILayout.Space(20);

            // Choices list
            GUI.enabled = !_isResolved;
            for (int i = 0; i < _activeEvent.Choices.Count; i++)
            {
                var choice = _activeEvent.Choices[i];
                if (choice == null) continue;

                string btnText = $"{choice.DisplayName}  —  {choice.Description}";
                if (LattiruneUITheme.DrawPrimaryButton(btnText, 65f))
                {
                    if (eventService != null)
                    {
                        eventService.SelectChoice(choice.ChoiceId, _economyService, playerCombatant, modifierManager);
                    }
                }
                GUILayout.Space(10);
            }
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(_lastOutcomeMessage))
            {
                GUILayout.Space(12);
                GUIStyle alertStyle = new GUIStyle(GUI.skin.label);
                alertStyle.fontSize = 17;
                alertStyle.fontStyle = FontStyle.Italic;
                alertStyle.alignment = TextAnchor.MiddleCenter;
                alertStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
                GUILayout.Label(_lastOutcomeMessage, alertStyle);
            }

            if (_isResolved)
            {
                GUILayout.Space(16);
                if (LattiruneUITheme.DrawPrimaryButton("CONTINUE DESCENT", 75f))
                {
                    if (mapController != null && mapController.MapGraph != null)
                    {
                        mapController.MapGraph.CompleteCurrentNode();
                    }
                    if (runManager != null)
                    {
                        runManager.ContinueAfterReward();
                    }
                    CloseModal();
                    if (navigation != null)
                    {
                        navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                    }
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
