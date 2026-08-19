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
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunModifierManager modifierManager;

        private IEconomyService _economyService;
        private RunEventDefinitionSO _activeEvent;
        private string _lastOutcomeMessage = string.Empty;
        private bool _isShowingModal = false;

        public bool IsShowingModal => _isShowingModal;
        public RunEventDefinitionSO ActiveEvent => _activeEvent;
        public string LastOutcomeMessage => _lastOutcomeMessage;

        public void Initialize(
            RunEventService service,
            IEconomyService economy,
            PlayerCombatant player,
            RunModifierManager modifiers)
        {
            eventService = service;
            _economyService = economy;
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
            _lastOutcomeMessage = $"Outcome: {choice.DisplayName} applied successfully.";
            _isShowingModal = false;
            _activeEvent = null;
        }

        private void HandleEventFailed(RunEventDefinitionSO ev, RunEventChoice choice, string reason)
        {
            _lastOutcomeMessage = $"Cannot proceed: {reason}";
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

            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 960f;
            float panelHeight = 1200f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (1920f - panelHeight) * 0.5f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.96f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 32;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.95f, 0.8f, 0.2f); // Gold

            GUILayout.Label($"✨ {_activeEvent.Title} ✨", titleStyle);
            GUILayout.Space(12);

            GUIStyle resourceStyle = new GUIStyle(GUI.skin.label);
            resourceStyle.fontSize = 20;
            resourceStyle.alignment = TextAnchor.MiddleCenter;
            resourceStyle.normal.textColor = Color.cyan;

            int currentGold = _economyService != null ? _economyService.GoldBalance : 0;
            int currentHp = playerCombatant != null ? playerCombatant.CurrentHp : 100;
            int maxHp = playerCombatant != null ? playerCombatant.MaxHp : 100;
            GUILayout.Label($"[ ❤️ HERO HP: {currentHp}/{maxHp}  |  💰 GOLD: {currentGold} ]", resourceStyle);
            GUILayout.Space(16);

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 20;
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            GUILayout.Label(_activeEvent.Description, bodyStyle);
            GUILayout.Space(24);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 20;
            btnStyle.fontStyle = FontStyle.Bold;

            // Choices list
            for (int i = 0; i < _activeEvent.Choices.Count; i++)
            {
                var choice = _activeEvent.Choices[i];
                if (choice == null) continue;

                string btnText = $"{choice.DisplayName}\n<size=16><i>{choice.Description}</i></size>";
                if (GUILayout.Button(btnText, btnStyle, GUILayout.MinHeight(75)))
                {
                    if (eventService != null)
                    {
                        eventService.SelectChoice(choice.ChoiceId, _economyService, playerCombatant, modifierManager);
                    }
                }
                GUILayout.Space(10);
            }

            if (!string.IsNullOrEmpty(_lastOutcomeMessage))
            {
                GUILayout.Space(12);
                GUIStyle alertStyle = new GUIStyle(GUI.skin.label);
                alertStyle.fontSize = 18;
                alertStyle.fontStyle = FontStyle.Italic;
                alertStyle.alignment = TextAnchor.MiddleCenter;
                alertStyle.normal.textColor = Color.yellow;
                GUILayout.Label(_lastOutcomeMessage, alertStyle);
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
