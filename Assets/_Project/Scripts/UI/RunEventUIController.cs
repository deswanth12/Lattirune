using System;
using UnityEngine;
using Lattirune.Audio;
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

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 35f;
            float contentW = screenW - (padX * 2f);

            // =================================================================
            // 1. TOP HEADER & HUD BAR
            // =================================================================
            float topY = 45f;
            float topH = 120f;
            Rect topBarRect = new Rect(padX, topY, contentW, topH);
            LattiruneUITheme.DrawCard(topBarRect);

            Texture2D evIcon = VisualAssetProvider.GetUIIcon("ui_icon_event");
            if (evIcon != null)
            {
                GUI.DrawTexture(new Rect(padX + 18f, topY + 18f, 84f, 84f), evIcon, ScaleMode.ScaleToFit);
            }

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 116f, topY + 18f, contentW - 130f, 26f), _activeEvent.Title.ToUpper(), titleStyle);

            GUIStyle subtitleStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            subtitleStyle.fontSize = 13;
            subtitleStyle.fontStyle = FontStyle.Italic;
            subtitleStyle.alignment = TextAnchor.MiddleLeft;
            subtitleStyle.normal.textColor = LattiruneUITheme.ColorCyanArcane;
            GUI.Label(new Rect(padX + 116f, topY + 48f, contentW - 130f, 22f), "Mysterious Subterranean Encounter", subtitleStyle);

            // =================================================================
            // 2. STORY & CHOICES CARD
            // =================================================================
            float contentY = topY + topH + 16f;
            float botBtnH = 85f;
            float botMargin = 25f;
            float actY = virtualH - botBtnH - botMargin;
            float areaH = actY - contentY - 16f;

            Rect areaRect = new Rect(padX, contentY, contentW, areaH);
            LattiruneUITheme.DrawCard(areaRect);

            // Story Text Box
            float descY = contentY + 20f;
            GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            descStyle.fontSize = 16;
            descStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;
            GUI.Label(new Rect(padX + 24f, descY, contentW - 48f, 75f), _activeEvent.Description, descStyle);

            // Choices
            float choicesStartY = descY + 85f;
            if (!_isResolved && _activeEvent.Choices != null)
            {
                float cardH = (areaH - 120f) / Mathf.Max(1, _activeEvent.Choices.Count);
                for (int i = 0; i < _activeEvent.Choices.Count; i++)
                {
                    var choice = _activeEvent.Choices[i];
                    if (choice == null) continue;

                    Rect cRect = new Rect(padX + 16f, choicesStartY + (i * (cardH + 8f)), contentW - 32f, cardH);
                    Color oldC = GUI.color;
                    GUI.color = new Color(0.12f, 0.16f, 0.24f, 0.90f);
                    LattiruneUITheme.DrawCard(cRect);
                    GUI.color = oldC;
                    LattiruneUITheme.DrawBorder(cRect, 1.5f, LattiruneUITheme.ColorGoldPrimary);

                    float textX = cRect.x + 20f;
                    float textW = cRect.width - 240f;

                    GUIStyle choiceTitle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                    choiceTitle.fontSize = 18;
                    choiceTitle.fontStyle = FontStyle.Bold;
                    choiceTitle.normal.textColor = LattiruneUITheme.ColorGoldBright;
                    GUI.Label(new Rect(textX, cRect.y + 14f, textW, 24f), choice.DisplayName.ToUpper(), choiceTitle);

                    GUIStyle choiceDesc = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    choiceDesc.fontSize = 13;
                    choiceDesc.normal.textColor = LattiruneUITheme.ColorTextMuted;
                    GUI.Label(new Rect(textX, cRect.y + 40f, textW, 40f), choice.Description, choiceDesc);

                    float btnW = 200f;
                    float btnX = cRect.x + cRect.width - btnW - 16f;
                    float btnY = cRect.y + (cRect.height - 55f) * 0.5f;
                    if (GUI.Button(new Rect(btnX, btnY, btnW, 55f), "CHOOSE", LattiruneUITheme.StylePrimaryBtn))
                    {
                        AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                        HapticFeedback.Trigger(HapticFeedbackType.Medium);
                        eventService?.SelectChoice(choice.ChoiceId, _economyService, playerCombatant, modifierManager);
                    }
                }
            }

            if (!string.IsNullOrEmpty(_lastOutcomeMessage))
            {
                float outY = choicesStartY + 120f;
                GUIStyle outcomeStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                outcomeStyle.fontSize = 17;
                outcomeStyle.fontStyle = FontStyle.Bold;
                outcomeStyle.alignment = TextAnchor.MiddleCenter;
                outcomeStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
                GUI.Label(new Rect(padX + 24f, outY, contentW - 48f, 40f), _lastOutcomeMessage, outcomeStyle);
            }

            // =================================================================
            // 3. BOTTOM ACTION BUTTON
            // =================================================================
            if (_isResolved)
            {
                Rect actRect = new Rect(padX, actY, contentW, botBtnH);
                if (GUI.Button(actRect, "CONTINUE DESCENT", LattiruneUITheme.StylePrimaryBtn))
                {
                    AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                    CloseModal();
                    mapController?.MapGraph?.CompleteCurrentNode();
                    navigation?.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }

            GUI.matrix = oldMatrix;
        }
    }
}
