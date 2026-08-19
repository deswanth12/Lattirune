using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Economy;
using Lattirune.Modifiers;

namespace Lattirune.Events
{
    /// <summary>
    /// Mobile-first portrait UI panel for procedural run events in Lattirune 1.1.
    /// Complies with PLAN.md Section 16 (1080x1920 portrait reference, >=52dp touch targets).
    /// </summary>
    public class RunEventMobilePanel : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private bool isVisible = false;

        private RunEventDefinitionSO _activeEvent;
        private RunEventService _eventService;
        private EconomyManager _economyManager;
        private PlayerCombatant _playerCombatant;
        private RunModifierManager _modifierManager;
        private string _outcomeFeedback = string.Empty;
        private bool _isResolved = false;

        public event Action OnPanelDismissed;
        public bool IsVisible => isVisible;
        public bool IsResolved => _isResolved;
        public RunEventDefinitionSO ActiveEvent => _activeEvent;

        public void Initialize(
            RunEventService service,
            EconomyManager economy,
            PlayerCombatant player,
            RunModifierManager modifiers)
        {
            _eventService = service;
            _economyManager = economy;
            _playerCombatant = player;
            _modifierManager = modifiers;
            isVisible = false;
            _isResolved = false;
            _outcomeFeedback = string.Empty;
        }

        public void Show(RunEventDefinitionSO eventDef)
        {
            _activeEvent = eventDef;
            isVisible = true;
            _isResolved = false;
            _outcomeFeedback = string.Empty;
        }

        public void Hide()
        {
            isVisible = false;
            _activeEvent = null;
            _isResolved = false;
            _outcomeFeedback = string.Empty;
            OnPanelDismissed?.Invoke();
        }

        public void SetOutcomeFeedback(string message, bool resolved = true)
        {
            _outcomeFeedback = message;
            _isResolved = resolved;
        }

        private void OnGUI()
        {
            if (!isVisible || _activeEvent == null) return;

            GUIStyle cardStyle = new GUIStyle(GUI.skin.box);
            cardStyle.fontSize = 14;
            cardStyle.alignment = TextAnchor.UpperLeft;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle loreStyle = new GUIStyle(GUI.skin.label);
            loreStyle.fontSize = 13;
            loreStyle.wordWrap = true;

            GUIStyle statusStyle = new GUIStyle(GUI.skin.box);
            statusStyle.fontSize = 12;
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.alignment = TextAnchor.MiddleCenter;

            int panelWidth = Mathf.Min(480, Screen.width - 32);
            int panelHeight = 440;
            int posX = (Screen.width - panelWidth) / 2;
            int posY = (Screen.height - panelHeight) / 2;

            GUILayout.BeginArea(new Rect(posX, posY, panelWidth, panelHeight), cardStyle);

            GUILayout.Space(8);
            GUILayout.Label(_activeEvent.Title, titleStyle);
            GUILayout.Space(6);

            // Resource indicators
            int currentGold = _economyManager != null ? _economyManager.GoldBalance : 0;
            int currentHp = _playerCombatant != null ? _playerCombatant.CurrentHp : 100;
            int maxHp = _playerCombatant != null ? _playerCombatant.MaxHp : 100;
            GUILayout.Box($""❤️ HP: {currentHp}/{maxHp}   |   💰 GOLD: {currentGold}"", statusStyle, GUILayout.Height(28));
            GUILayout.Space(8);

            GUILayout.Label(_activeEvent.Description, loreStyle);
            GUILayout.Space(12);

            if (!_isResolved)
            {
                // Choice options list
                for (int i = 0; i < _activeEvent.Choices.Count; i++)
                {
                    var choice = _activeEvent.Choices[i];
                    if (choice == null) continue;

                    string buttonText = $""<b>{choice.DisplayName}</b>\n<size=11>{choice.Description}</size>"";
                    if (GUILayout.Button(buttonText, GUILayout.MinHeight(54)))
                    {
                        if (_eventService != null)
                        {
                            bool success = _eventService.SelectChoice(choice.ChoiceId, _economyManager, _playerCombatant, _modifierManager);
                            if (success)
                            {
                                SetOutcomeFeedback($""✓ Outcome applied: {choice.DisplayName}"", resolved: true);
                            }
                            else
                            {
                                SetOutcomeFeedback($""✕ Cannot choose: Resource requirement not met."", resolved: false);
                            }
                        }
                    }
                    GUILayout.Space(4);
                }
            }
            else
            {
                // Resolution view with Continue button
                GUIStyle successStyle = new GUIStyle(GUI.skin.label);
                successStyle.fontSize = 14;
                successStyle.fontStyle = FontStyle.Bold;
                successStyle.normal.textColor = Color.green;
                successStyle.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label(_outcomeFeedback, successStyle);
                GUILayout.Space(16);

                if (GUILayout.Button(""CONTINUE DUNGEON EXPLORATION"", GUILayout.MinHeight(54)))
                {
                    Hide();
                }
            }

            if (!_isResolved && !string.IsNullOrEmpty(_outcomeFeedback))
            {
                GUILayout.Space(6);
                GUIStyle warnStyle = new GUIStyle(GUI.skin.label);
                warnStyle.normal.textColor = Color.yellow;
                warnStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(_outcomeFeedback, warnStyle);
            }

            GUILayout.EndArea();
        }
    }
}
