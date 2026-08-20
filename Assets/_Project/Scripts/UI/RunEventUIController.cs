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
    /// Presentation component for procedural run events in Lattirune.
    /// Displays glowing mystery shrine artwork and choice cards (0 emoji, 0 placeholders).
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
            float panelHeight = 1350f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), _activeEvent.Title.ToUpper());

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader(_activeEvent.Title.ToUpper(), "A strange encounter stirs in the subterranean shadows.");
            GUILayout.Space(10);

            // Shrine Backdrop
            Texture2D shrineBg = VisualAssetProvider.GetBackdrop("bg_shrine_event");
            if (shrineBg != null)
            {
                Rect sRect = GUILayoutUtility.GetRect(panelWidth - 80, 200f);
                GUI.DrawTexture(sRect, shrineBg, ScaleMode.ScaleAndCrop);
                GUILayout.Space(12);
            }

            // Description
            GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            descStyle.fontSize = 16;
            descStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;
            GUILayout.Label(_activeEvent.Description, descStyle);
            GUILayout.Space(14);

            // Choice Cards
            if (!_isResolved && _activeEvent.Choices != null)
            {
                for (int i = 0; i < _activeEvent.Choices.Count; i++)
                {
                    var choice = _activeEvent.Choices[i];
                    if (choice == null) continue;

                    GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                    GUIStyle choiceTitle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                    choiceTitle.fontSize = 18;
                    choiceTitle.fontStyle = FontStyle.Bold;
                    choiceTitle.normal.textColor = LattiruneUITheme.ColorGoldBright;
                    GUILayout.Label(choice.DisplayName.ToUpper(), choiceTitle);

                    GUIStyle choiceDesc = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    choiceDesc.fontSize = 14;
                    choiceDesc.normal.textColor = LattiruneUITheme.ColorTextMuted;
                    GUILayout.Label(choice.Description, choiceDesc);
                    GUILayout.Space(8);

                    if (LattiruneUITheme.DrawPrimaryButton($"CHOOSE: {choice.DisplayName.ToUpper()}", 55f))
                    {
                        if (eventService != null)
                        {
                            eventService.SelectChoice(choice.ChoiceId, _economyService, playerCombatant, modifierManager);
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }

            if (!string.IsNullOrEmpty(_lastOutcomeMessage))
            {
                GUIStyle outcomeStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                outcomeStyle.fontSize = 16;
                outcomeStyle.fontStyle = FontStyle.Bold;
                outcomeStyle.alignment = TextAnchor.MiddleCenter;
                outcomeStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
                GUILayout.Label(_lastOutcomeMessage, outcomeStyle);
                GUILayout.Space(12);
            }

            GUILayout.FlexibleSpace();

            if (_isResolved)
            {
                if (LattiruneUITheme.DrawPrimaryButton("CONTINUE DESCENT", 75f))
                {
                    CloseModal();
                    if (mapController != null && mapController.MapGraph != null)
                    {
                        mapController.MapGraph.CompleteCurrentNode();
                    }
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
