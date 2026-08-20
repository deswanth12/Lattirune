using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for selecting and unlocking Hero Classes.
    /// Strictly adheres to PLAN.md Section 12, 15, and 16.
    /// </summary>
    public class HeroClassSelectionUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HeroClassManager classManager;
        [SerializeField] private MetaProgressionManager metaProgression;
        [SerializeField] private ScreenNavigationController navigation;

        [Header("State")]
        [SerializeField] private RunManager runManager;
        [SerializeField] private DungeonMapScreenController mapController;
        [SerializeField] private bool isVisible = false;
        private int _selectedPreviewIndex = 0;
        private string _feedbackMessage = "";

        public bool IsVisible => isVisible;

        public void Initialize(
            HeroClassManager heroManager,
            MetaProgressionManager meta,
            ScreenNavigationController nav = null,
            RunManager run = null,
            DungeonMapScreenController map = null)
        {
            classManager = heroManager;
            metaProgression = meta;
            navigation = nav;
            runManager = run;
            mapController = map;
            _selectedPreviewIndex = 0;
            _feedbackMessage = "Choose your Champion for the descent.";

            if (navigation != null)
            {
                navigation.OnScreenChanged += HandleScreenChanged;
            }
        }

        private void OnDestroy()
        {
            if (navigation != null)
            {
                navigation.OnScreenChanged -= HandleScreenChanged;
            }
        }

        private void HandleScreenChanged(ScreenState prev, ScreenState next)
        {
            if (next == ScreenState.HERO_SELECTION)
            {
                Show();
            }
            else if (prev == ScreenState.HERO_SELECTION)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            _feedbackMessage = "Choose your Champion for the descent.";
        }

        public void Hide()
        {
            isVisible = false;
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.HERO_SELECTION) return;
            if (!isVisible || classManager == null || classManager.Database == null) return;

            var classes = classManager.Database.AllClasses;
            if (classes == null || classes.Count == 0) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "HERO CLASS SELECTION");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("HERO CLASS SELECTION", "Choose your Champion for the subterranean descent.");
            GUILayout.Space(12);

            int embers = metaProgression != null ? metaProgression.CurrentEmbers : 0;
            LattiruneUITheme.DrawBadge($"Persistent Embers: {embers}", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(16);

            // Class Selector 2x2 Grid for Perfect Mobile Fit
            int count = classes.Count;
            for (int r = 0; r < count; r += 2)
            {
                GUILayout.BeginHorizontal();
                for (int c = 0; c < 2; c++)
                {
                    int idx = r + c;
                    if (idx < count)
                    {
                        var def = classes[idx];
                        bool isSelectedPreview = (idx == _selectedPreviewIndex);
                        bool isUnlocked = classManager.IsClassUnlocked(def.ClassId);
                        bool isActiveHero = (def.ClassId == classManager.SelectedClassId);

                        string tabText = def.ClassName;
                        if (isActiveHero) tabText = $"★ {def.ClassName}";
                        else if (!isUnlocked) tabText = $"[LOCKED] {def.ClassName}";

                        if (isSelectedPreview)
                        {
                            if (LattiruneUITheme.DrawPrimaryButton(tabText, 55f))
                            {
                                _selectedPreviewIndex = idx;
                            }
                        }
                        else
                        {
                            if (LattiruneUITheme.DrawSecondaryButton(tabText, 55f))
                            {
                                _selectedPreviewIndex = idx;
                            }
                        }
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                    }
                    if (c == 0) GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
            }

            GUILayout.Space(14);

            // Active Preview Card
            var currentDef = classes[Mathf.Clamp(_selectedPreviewIndex, 0, classes.Count - 1)];
            if (currentDef != null)
            {
                bool isUnlocked = classManager.IsClassUnlocked(currentDef.ClassId);
                bool isActive = (currentDef.ClassId == classManager.SelectedClassId);

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUIStyle headerStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                headerStyle.fontSize = 24;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = isActive ? LattiruneUITheme.ColorGoldBright : (isUnlocked ? LattiruneUITheme.ColorTextPrimary : LattiruneUITheme.ColorTextMuted);

                string badge = isActive ? " [ACTIVE CHAMPION]" : (isUnlocked ? " [UNLOCKED]" : $" [LOCKED: {currentDef.EmbersCost} Embers]");
                GUILayout.Label($"{currentDef.ClassName}{badge}", headerStyle);
                GUILayout.Space(6);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 17;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(currentDef.Description, descStyle);
                GUILayout.Space(12);

                // Stats Matrix
                GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                statStyle.fontSize = 17;
                statStyle.fontStyle = FontStyle.Bold;
                statStyle.normal.textColor = LattiruneUITheme.ColorCyanArcane;

                GUILayout.Label($"Base Stats:  HP: {currentDef.BaseHp}  |  DEF: {currentDef.BaseArmor}  |  ATK: {currentDef.BaseAttack}  |  Speed: {currentDef.AttackInterval:0.0}s", statStyle);
                GUILayout.Space(10);

                // Starting Loadout
                GUIStyle loadoutStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                loadoutStyle.fontSize = 15;
                loadoutStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;

                string itemsList = string.Join(", ", currentDef.StartingItemIds);
                string runesList = string.Join(", ", currentDef.StartingRuneIds);
                GUILayout.Label($"Starting Items: {itemsList}", loadoutStyle);
                GUILayout.Label($"Starting Runes: {runesList}", loadoutStyle);

                GUILayout.Space(16);

                // Select / Unlock Action
                if (!isUnlocked)
                {
                    bool canAfford = metaProgression != null && metaProgression.CurrentEmbers >= currentDef.EmbersCost;
                    GUI.enabled = canAfford;

                    if (LattiruneUITheme.DrawPrimaryButton($"UNLOCK CHAMPION ({currentDef.EmbersCost} Embers)", 60f))
                    {
                        if (classManager.UnlockClass(currentDef.ClassId, metaProgression))
                        {
                            _feedbackMessage = $"Unlocked {currentDef.ClassName}!";
                        }
                    }

                    GUI.enabled = true;
                }
                else if (!isActive)
                {
                    if (LattiruneUITheme.DrawPrimaryButton("SET AS ACTIVE CHAMPION", 60f))
                    {
                        classManager.SelectClass(currentDef.ClassId);
                        _feedbackMessage = $"Selected {currentDef.ClassName} as active champion!";
                    }
                }
                else
                {
                    GUI.enabled = false;
                    LattiruneUITheme.DrawSecondaryButton("ACTIVE CHAMPION READY", 60f);
                    GUI.enabled = true;
                }

                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            // Feedback Dialogue
            if (!string.IsNullOrEmpty(_feedbackMessage))
            {
                GUIStyle feedbackStyle = new GUIStyle(GUI.skin.label);
                feedbackStyle.fontSize = 16;
                feedbackStyle.fontStyle = FontStyle.Italic;
                feedbackStyle.alignment = TextAnchor.MiddleCenter;
                feedbackStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
                GUILayout.Label(_feedbackMessage, feedbackStyle);
                GUILayout.Space(8);
            }

            // Bottom Navigation Row
            GUILayout.BeginHorizontal();
            if (LattiruneUITheme.DrawPrimaryButton("DESCEND INTO DUNGEON", 75f))
            {
                Hide();
                if (mapController != null)
                {
                    mapController.ResetMapForNewRun();
                }
                if (runManager != null)
                {
                    runManager.StartRun(metaProgression);
                }
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }
            GUILayout.Space(14);
            if (LattiruneUITheme.DrawSecondaryButton("RETURN", 75f))
            {
                Hide();
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.MAIN_MENU);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
