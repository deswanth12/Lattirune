using System;
using UnityEngine;
using Lattirune.Combat;
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
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunModifierManager modifierManager;

        private RunEventDefinitionSO _activeEvent;
        private string _lastOutcomeMessage = string.Empty;
        private bool _isShowingModal = false;

        public bool IsShowingModal => _isShowingModal;
        public RunEventDefinitionSO ActiveEvent => _activeEvent;
        public string LastOutcomeMessage => _lastOutcomeMessage;

        public void Initialize(
            RunEventService service,
            EconomyManager economy,
            PlayerCombatant player,
            RunModifierManager modifiers)
        {
            eventService = service;
            economyManager = economy;
            playerCombatant = player;
            modifierManager = modifiers;

            if (eventService != null)
            {
                eventService.OnEventPresented += HandleEventPresented;
                eventService.OnEventResolved += HandleEventResolved;
                eventService.OnEventFailed += HandleEventFailed;
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
        }

        private void HandleEventPresented(RunEventDefinitionSO ev)
        {
            _activeEvent = ev;
            _lastOutcomeMessage = string.Empty;
            _isShowingModal = true;
        }

        private void HandleEventResolved(RunEventDefinitionSO ev, RunEventChoice choice, RunEventResolutionResult result)
        {
            _lastOutcomeMessage = $""Outcome: {choice.DisplayName} applied successfully."";
            _isShowingModal = false;
            _activeEvent = null;
        }

        private void HandleEventFailed(RunEventDefinitionSO ev, RunEventChoice choice, string reason)
        {
            _lastOutcomeMessage = $""Cannot proceed: {reason}"";
        }

        public void CloseModal()
        {
            _isShowingModal = false;
            _activeEvent = null;
            if (eventService != null)
            {
                eventService.ClearActiveEvent();
            }
        }

        private void OnGUI()
        {
            if (!_isShowingModal || _activeEvent == null) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 14;
            boxStyle.alignment = TextAnchor.UpperLeft;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 13;
            bodyStyle.wordWrap = true;

            int panelWidth = Mathf.Min(480, Screen.width - 40);
            int panelHeight = 420;
            int posX = (Screen.width - panelWidth) / 2;
            int posY = (Screen.height - panelHeight) / 2;

            GUILayout.BeginArea(new Rect(posX, posY, panelWidth, panelHeight), boxStyle);

            GUILayout.Space(10);
            GUILayout.Label(_activeEvent.Title, titleStyle);
            GUILayout.Space(8);

            // Resources header
            int currentGold = economyManager != null ? economyManager.GoldBalance : 0;
            int currentHp = playerCombatant != null ? playerCombatant.CurrentHp : 100;
            int maxHp = playerCombatant != null ? playerCombatant.MaxHp : 100;
            GUILayout.Label($""[ HERO HP: {currentHp}/{maxHp} | GOLD: {currentGold} ]"", bodyStyle);

            GUILayout.Space(6);
            GUILayout.Label(_activeEvent.Description, bodyStyle);
            GUILayout.Space(12);

            // Choices list
            for (int i = 0; i < _activeEvent.Choices.Count; i++)
            {
                var choice = _activeEvent.Choices[i];
                if (choice == null) continue;

                string btnText = $"{choice.DisplayName}\n<size=11>{choice.Description}</size>";
                if (GUILayout.Button(btnText, GUILayout.MinHeight(54)))
                {
                    if (eventService != null)
                    {
                        eventService.SelectChoice(choice.ChoiceId, economyManager, playerCombatant, modifierManager);
                    }
                }
                GUILayout.Space(4);
            }

            if (!string.IsNullOrEmpty(_lastOutcomeMessage))
            {
                GUILayout.Space(6);
                GUIStyle alertStyle = new GUIStyle(GUI.skin.label);
                alertStyle.normal.textColor = Color.yellow;
                GUILayout.Label(_lastOutcomeMessage, alertStyle);
            }

            GUILayout.EndArea();
        }
    }
}
