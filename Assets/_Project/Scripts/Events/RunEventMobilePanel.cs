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
        private IEconomyService _economyManager;
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
            IEconomyService economy,
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
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.98f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 36;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.95f, 0.8f, 0.2f); // Gold

            GUIStyle loreStyle = new GUIStyle(GUI.skin.label);
            loreStyle.fontSize = 20;
            loreStyle.wordWrap = true;
            loreStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            GUIStyle statusStyle = new GUIStyle(GUI.skin.box);
            statusStyle.fontSize = 20;
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label(_activeEvent.Title, titleStyle);
            GUILayout.Space(12);

            // Resource indicators
            int currentGold = _economyManager != null ? _economyManager.GoldBalance : 0;
            int currentHp = _playerCombatant != null ? _playerCombatant.CurrentHp : 100;
            int maxHp = _playerCombatant != null ? _playerCombatant.MaxHp : 100;
            GUILayout.Box($"❤️ HP: {currentHp}/{maxHp}   |   💰 GOLD: {currentGold}", statusStyle, GUILayout.Height(44));
            GUILayout.Space(16);

            GUILayout.Label(_activeEvent.Description, loreStyle);
            GUILayout.Space(24);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 20;
            btnStyle.fontStyle = FontStyle.Bold;

            if (!_isResolved)
            {
                // Choice options list
                for (int i = 0; i < _activeEvent.Choices.Count; i++)
                {
                    var choice = _activeEvent.Choices[i];
                    if (choice == null) continue;

                    string buttonText = $"<b>{choice.DisplayName}</b>\n<size=16>{choice.Description}</size>";
                    if (GUILayout.Button(buttonText, btnStyle, GUILayout.MinHeight(75)))
                    {
                        if (_eventService != null)
                        {
                            bool success = _eventService.SelectChoice(choice.ChoiceId, _economyManager, _playerCombatant, _modifierManager);
                            if (success)
                            {
                                SetOutcomeFeedback($"✓ Outcome applied: {choice.DisplayName}", resolved: true);
                            }
                            else
                            {
                                SetOutcomeFeedback($"✕ Cannot choose: Resource requirement not met.", resolved: false);
                            }
                        }
                    }
                    GUILayout.Space(10);
                }
            }
            else
            {
                // Resolution view with Continue button
                GUIStyle successStyle = new GUIStyle(GUI.skin.label);
                successStyle.fontSize = 22;
                successStyle.fontStyle = FontStyle.Bold;
                successStyle.normal.textColor = Color.green;
                successStyle.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label(_outcomeFeedback, successStyle);
                GUILayout.Space(24);

                GUI.color = Color.cyan;
                if (GUILayout.Button("CONTINUE DUNGEON EXPLORATION", btnStyle, GUILayout.Height(65)))
                {
                    Hide();
                }
                GUI.color = oldColor;
            }

            if (!_isResolved && !string.IsNullOrEmpty(_outcomeFeedback))
            {
                GUILayout.Space(10);
                GUIStyle warnStyle = new GUIStyle(GUI.skin.label);
                warnStyle.fontSize = 18;
                warnStyle.normal.textColor = Color.yellow;
                warnStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(_outcomeFeedback, warnStyle);
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
