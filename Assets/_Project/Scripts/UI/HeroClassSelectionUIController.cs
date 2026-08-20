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
    /// Displays full high-contrast stylized Hero portraits, class emblems,
    /// base combat stats, and starting runic loadouts (0 emoji, 0 placeholders).
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
        private string _feedbackMessage = "Choose your Champion for the descent.";

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

            if (classManager == null)
            {
                classManager = UnityEngine.Object.FindFirstObjectByType<HeroClassManager>();
                if (classManager == null)
                {
                    classManager = gameObject.AddComponent<HeroClassManager>();
                }
            }
            if (classManager.Database == null)
            {
                classManager.Initialize();
            }

            var classes = classManager.Database != null ? classManager.Database.AllClasses : null;
            if (classes == null || classes.Count == 0) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "HERO CLASS SELECTION");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("HERO CLASS SELECTION", "Choose your Champion for the subterranean descent.");
            GUILayout.Space(10);

            int embers = metaProgression != null ? metaProgression.CurrentEmbers : 0;
            LattiruneUITheme.DrawBadge($"Persistent Embers: {embers}", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(14);

            // 2x2 Hero Selection Grid
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

                        string tabText = def.ClassName.ToUpper();
                        if (isActiveHero) tabText = $"[ACTIVE] {tabText}";
                        else if (!isUnlocked) tabText = $"[LOCKED] {tabText}";

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

            GUILayout.Space(12);

            // Selected Hero Showcase Card
            var currentDef = classes[Mathf.Clamp(_selectedPreviewIndex, 0, classes.Count - 1)];
            if (currentDef != null)
            {
                bool isUnlocked = classManager.IsClassUnlocked(currentDef.ClassId);
                bool isActive = (currentDef.ClassId == classManager.SelectedClassId);

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUILayout.BeginHorizontal();

                // 1. Large Hero Artwork Portrait
                Texture2D heroTex = VisualAssetProvider.GetHeroTexture(currentDef.ClassId);
                if (heroTex != null)
                {
                    Rect heroArtRect = GUILayoutUtility.GetRect(180f, 180f, GUILayout.Width(180f), GUILayout.Height(180f));
                    GUI.DrawTexture(heroArtRect, heroTex, ScaleMode.ScaleToFit);
                    GUILayout.Space(16);
                }

                // 2. Hero Info & Stats
                GUILayout.BeginVertical();
                GUIStyle headerStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                headerStyle.fontSize = 24;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = isActive ? LattiruneUITheme.ColorGoldBright : (isUnlocked ? LattiruneUITheme.ColorTextPrimary : LattiruneUITheme.ColorTextMuted);

                string badge = isActive ? " [ACTIVE CHAMPION]" : (isUnlocked ? " [UNLOCKED]" : $" [LOCKED: {currentDef.EmbersCost} Embers]");
                GUILayout.Label($"{currentDef.ClassName.ToUpper()}{badge}", headerStyle);
                GUILayout.Space(4);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 15;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(currentDef.Description, descStyle);
                GUILayout.Space(8);

                // Stats Row
                GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                statStyle.fontSize = 15;
                statStyle.fontStyle = FontStyle.Bold;
                statStyle.normal.textColor = LattiruneUITheme.ColorCyanArcane;
                GUILayout.Label($"HP: {currentDef.BaseHp}  |  ARMOR: {currentDef.BaseArmor}  |  ATK: {currentDef.BaseAttack}  |  SPD: {currentDef.AttackInterval:0.0}s", statStyle);

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                // Starting Runes & Items Row
                GUILayout.BeginHorizontal();
                for (int i = 0; i < currentDef.StartingRuneIds.Count; i++)
                {
                    Texture2D runeIcon = VisualAssetProvider.GetRuneTexture(ElementType.Fire);
                    Rect rRect = GUILayoutUtility.GetRect(48f, 48f, GUILayout.Width(48f), GUILayout.Height(48f));
                    if (runeIcon != null) GUI.DrawTexture(rRect, runeIcon, ScaleMode.ScaleToFit);
                    GUILayout.Space(6);
                }
                for (int i = 0; i < currentDef.StartingItemIds.Count; i++)
                {
                    Texture2D itemIcon = VisualAssetProvider.GetItemTexture(currentDef.StartingItemIds[i]);
                    Rect iRect = GUILayoutUtility.GetRect(48f, 48f, GUILayout.Width(48f), GUILayout.Height(48f));
                    if (itemIcon != null) GUI.DrawTexture(iRect, itemIcon, ScaleMode.ScaleToFit);
                    GUILayout.Space(6);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(14);

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

            // Bottom Navigation Buttons
            GUILayout.BeginHorizontal();

            if (LattiruneUITheme.DrawPrimaryButton("DESCEND INTO DUNGEON", 75f))
            {
                if (runManager != null)
                {
                    runManager.StartRun(metaProgression);
                }
                if (mapController != null)
                {
                    mapController.ResetMapForNewRun();
                }
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }

            GUILayout.Space(16);

            if (LattiruneUITheme.DrawSecondaryButton("RETURN", 75f))
            {
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
