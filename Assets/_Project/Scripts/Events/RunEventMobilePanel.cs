using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Economy;
using Lattirune.Modifiers;
using Lattirune.UI;

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

        [SerializeField] private ScreenNavigationController _navigation;

        public void BindNavigation(ScreenNavigationController nav)
        {
            _navigation = nav;
        }

        private void OnGUI()
        {
            if (_navigation != null && _navigation.CurrentScreen != ScreenState.EVENT) return;
            if (!isVisible || _activeEvent == null) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1200f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), _activeEvent.Title);

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader(_activeEvent.Title, "A mysterious encounter unfolds in the shadow of the lattice.");
            GUILayout.Space(12);

            // Resource indicators
            int currentGold = _economyManager != null ? _economyManager.GoldBalance : 0;
            int currentHp = _playerCombatant != null ? _playerCombatant.CurrentHp : 100;
            int maxHp = _playerCombatant != null ? _playerCombatant.MaxHp : 100;
            LattiruneUITheme.DrawProgressBar(currentHp, maxHp, $"HERO HP: {currentHp}/{maxHp}   |   [?] GOLD: {currentGold}", LattiruneUITheme.ColorGreenHealth, 28f);
            GUILayout.Space(16);

            GUIStyle loreStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            loreStyle.fontSize = 18;
            loreStyle.wordWrap = true;
            loreStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;
            GUILayout.Label(_activeEvent.Description, loreStyle);
            GUILayout.Space(20);

            if (!_isResolved)
            {
                // Choice options list
                for (int i = 0; i < _activeEvent.Choices.Count; i++)
                {
                    var choice = _activeEvent.Choices[i];
                    if (choice == null) continue;

                    string buttonText = $"⚡ {choice.DisplayName}: {choice.Description}";
                    if (LattiruneUITheme.DrawPrimaryButton(buttonText, 70f))
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
                GUIStyle successStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                successStyle.fontSize = 20;
                successStyle.fontStyle = FontStyle.Bold;
                successStyle.normal.textColor = LattiruneUITheme.ColorGreenHealth;
                successStyle.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label(_outcomeFeedback, successStyle);
                GUILayout.Space(24);

                if (LattiruneUITheme.DrawPrimaryButton("[SWORD]️ CONTINUE DUNGEON EXPLORATION ➔", 75f))
                {
                    Hide();
                }
            }

            if (!_isResolved && !string.IsNullOrEmpty(_outcomeFeedback))
            {
                GUILayout.Space(10);
                GUIStyle warnStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                warnStyle.fontSize = 18;
                warnStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
                warnStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(_outcomeFeedback, warnStyle);
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
