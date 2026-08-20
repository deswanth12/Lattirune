using System;
using UnityEngine;
using Lattirune.Core;
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
        [SerializeField] private bool isVisible = false;
        private int _selectedPreviewIndex = 0;
        private string _feedbackMessage = "";

        public bool IsVisible => isVisible;

        public void Initialize(
            HeroClassManager heroManager,
            MetaProgressionManager meta,
            ScreenNavigationController nav = null)
        {
            classManager = heroManager;
            metaProgression = meta;
            navigation = nav;
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
            if (navigation != null && navigation.CurrentScreen != ScreenState.HERO_SELECTION) return;
            if (!isVisible || classManager == null || classManager.Database == null) return;

            var classes = classManager.Database.AllClasses;
            if (classes == null || classes.Count == 0) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "🛡 HERO CLASS SELECTION 🛡");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("🛡 HERO CLASS SELECTION 🛡", "Choose your Champion for the subterranean descent.");
            GUILayout.Space(12);

            int embers = metaProgression != null ? metaProgression.CurrentEmbers : 0;
            LattiruneUITheme.DrawBadge($"🔥 Persistent Embers: {embers}", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(16);

            // Class Selector Tabs
            GUILayout.BeginHorizontal();
            for (int i = 0; i < classes.Count; i++)
            {
                var def = classes[i];
                if (def == null) continue;

                bool isSelectedPreview = (i == _selectedPreviewIndex);
                bool isUnlocked = classManager.IsClassUnlocked(def.ClassId);
                bool isActiveHero = (def.ClassId == classManager.SelectedClassId);

                string tabText = def.ClassName;
                if (isActiveHero) tabText = $"★ {def.ClassName}";
                else if (!isUnlocked) tabText = $"🔒 {def.ClassName}";

                if (isSelectedPreview)
                {
                    if (LattiruneUITheme.DrawPrimaryButton(tabText, 60f))
                    {
                        _selectedPreviewIndex = i;
                    }
                }
                else
                {
                    if (LattiruneUITheme.DrawSecondaryButton(tabText, 60f))
                    {
                        _selectedPreviewIndex = i;
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            // Active Preview Card
            if (_selectedPreviewIndex >= 0 && _selectedPreviewIndex < classes.Count)
            {
                var currentDef = classes[_selectedPreviewIndex];
                bool isUnlocked = classManager.IsClassUnlocked(currentDef.ClassId);
                bool isActive = (currentDef.ClassId == classManager.SelectedClassId);

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUIStyle headerStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                headerStyle.fontSize = 26;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = isActive ? LattiruneUITheme.ColorGreenHealth : (isUnlocked ? LattiruneUITheme.ColorTextPrimary : LattiruneUITheme.ColorTextMuted);

                string badge = isActive ? "[ACTIVE CHAMPION]" : (isUnlocked ? "[UNLOCKED]" : $"[LOCKED - {currentDef.EmbersCost} Embers]");
                GUILayout.Label($"{currentDef.ClassName}  {badge}", headerStyle);
                GUILayout.Space(8);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 18;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(currentDef.Description, descStyle);
                GUILayout.Space(14);

                // Stats Matrix
                GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                statStyle.fontSize = 18;
                statStyle.fontStyle = FontStyle.Bold;
                statStyle.normal.textColor = LattiruneUITheme.ColorCyanArcane;

                GUILayout.Label($"Base Stats: ❤️ HP: {currentDef.BaseHp}  |  🛡 Armor: {currentDef.BaseArmor}  |  ⚔ Attack: {currentDef.BaseAttack}  |  ⏱ Speed: {currentDef.AttackInterval:0.0}s", statStyle);
                GUILayout.Space(14);

                // Starting Loadout
                GUIStyle loadoutStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                loadoutStyle.fontSize = 16;
                loadoutStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;

                string itemsList = string.Join(", ", currentDef.StartingItemIds);
                string runesList = string.Join(", ", currentDef.StartingRuneIds);
                GUILayout.Label($"Starting Gear: {itemsList}", loadoutStyle);
                GUILayout.Label($"Starting Runes: {runesList}", loadoutStyle);

                GUILayout.Space(20);

                // Action Buttons
                if (!isUnlocked)
                {
                    bool canAfford = metaProgression != null && metaProgression.CurrentEmbers >= currentDef.EmbersCost;
                    GUI.enabled = canAfford;

                    if (LattiruneUITheme.DrawPrimaryButton($"🔥 UNLOCK CLASS ({currentDef.EmbersCost} Embers)", 65f))
                    {
                        if (classManager.UnlockClass(currentDef.ClassId, metaProgression))
                        {
                            classManager.SelectClass(currentDef.ClassId);
                            _feedbackMessage = $"Unlocked and selected {currentDef.ClassName}!";
                        }
                    }

                    GUI.enabled = true;
                }
                else if (!isActive)
                {
                    if (LattiruneUITheme.DrawPrimaryButton("⚔️ SELECT THIS HERO", 65f))
                    {
                        classManager.SelectClass(currentDef.ClassId);
                        _feedbackMessage = $"Selected {currentDef.ClassName} as active champion!";
                    }
                }
                else
                {
                    GUI.enabled = false;
                    LattiruneUITheme.DrawSecondaryButton("★ ACTIVE CHAMPION SELECTED", 65f);
                    GUI.enabled = true;
                }

                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            // Feedback Dialogue
            GUIStyle feedbackStyle = new GUIStyle(GUI.skin.label);
            feedbackStyle.fontSize = 18;
            feedbackStyle.fontStyle = FontStyle.Italic;
            feedbackStyle.alignment = TextAnchor.MiddleCenter;
            feedbackStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUILayout.Label(_feedbackMessage, feedbackStyle);
            GUILayout.Space(12);

            // Action / Navigation Buttons
            GUILayout.BeginHorizontal();

            if (LattiruneUITheme.DrawPrimaryButton("⚔️ DESCEND INTO DUNGEON ➔", 75f))
            {
                Hide();
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.RUN_START);
                }
            }

            GUILayout.Space(12);

            if (LattiruneUITheme.DrawSecondaryButton("↩ RETURN", 75f))
            {
                Hide();
                if (navigation != null)
                {
                    navigation.NavigateBack();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
