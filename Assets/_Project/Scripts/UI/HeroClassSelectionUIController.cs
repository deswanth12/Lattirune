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

        private void StartDescent()
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
            Audio.AudioController.Instance?.PlaySfx(Audio.AudioCueType.ButtonClick);
            Audio.HapticFeedback.Trigger(Audio.HapticFeedbackType.Heavy);
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

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 35f;
            float contentW = screenW - (padX * 2f);

            // =================================================================
            // 1. TOP HEADER & EMBERS PILL
            // =================================================================
            float topY = 25f;
            Rect topBarRect = new Rect(padX, topY, contentW, 70f);
            LattiruneUITheme.DrawCard(topBarRect);

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 20f, topY + 12f, 400f, 26f), "CHOOSE YOUR HERO", titleStyle);

            GUIStyle subTitleStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            subTitleStyle.fontSize = 13;
            subTitleStyle.fontStyle = FontStyle.Italic;
            subTitleStyle.alignment = TextAnchor.MiddleLeft;
            subTitleStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(padX + 20f, topY + 38f, 400f, 20f), "Select your champion for the subterranean descent", subTitleStyle);

            int embers = metaProgression != null ? metaProgression.CurrentEmbers : 0;
            Texture2D iconEmbers = VisualAssetProvider.GetUIIcon("ui_icon_embers");
            LattiruneUITheme.DrawIconValue(new Rect(padX + contentW - 200f, topY + 20f, 180f, 30f), iconEmbers, $"{embers} Embers", new Color(1f, 0.6f, 0.2f), 16);

            // =================================================================
            // 2. HERO SELECTOR CARDS (4 TABS)
            // =================================================================
            float tabY = topY + 80f;
            float tabW = (contentW - 30f) / 4f; // ~240px each
            float tabH = 80f;

            for (int i = 0; i < classes.Count && i < 4; i++)
            {
                var def = classes[i];
                bool isSelected = (i == _selectedPreviewIndex);
                bool isUnlocked = classManager.IsClassUnlocked(def.ClassId);
                bool isActive = (def.ClassId == classManager.SelectedClassId);

                Rect tabRect = new Rect(padX + (i * (tabW + 10f)), tabY, tabW, tabH);

                // Background Card
                Color tabBg = isSelected 
                    ? new Color(0.18f, 0.22f, 0.32f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.14f, 0.90f);
                GUI.color = tabBg;
                LattiruneUITheme.DrawCard(tabRect);
                GUI.color = Color.white;

                if (isSelected)
                {
                    LattiruneUITheme.DrawBorder(tabRect, 2.5f, LattiruneUITheme.ColorGoldBright);
                }
                else if (isActive)
                {
                    LattiruneUITheme.DrawBorder(tabRect, 1.5f, LattiruneUITheme.ColorCyanArcane);
                }

                // Emblem Icon
                Texture2D emblem = VisualAssetProvider.GetClassEmblem(def.ClassId);
                if (emblem != null)
                {
                    Rect embRect = new Rect(tabRect.x + 8f, tabRect.y + (tabH - 44f) * 0.5f, 44f, 44f);
                    GUI.DrawTexture(embRect, emblem, ScaleMode.ScaleToFit);
                }

                // Tab Title & Status
                float textX = tabRect.x + 56f;
                float textW = tabW - 60f;

                GUIStyle nameStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                nameStyle.fontSize = 13;
                nameStyle.fontStyle = FontStyle.Bold;
                nameStyle.alignment = TextAnchor.MiddleLeft;
                nameStyle.normal.textColor = isSelected ? LattiruneUITheme.ColorGoldBright : (isUnlocked ? Color.white : LattiruneUITheme.ColorTextMuted);
                GUI.Label(new Rect(textX, tabRect.y + 12f, textW, 22f), def.ClassName, nameStyle);

                string statusText = isActive ? "ACTIVE" : (isUnlocked ? "READY" : $"{def.EmbersCost} EMB");
                Color statusCol = isActive ? LattiruneUITheme.ColorCyanArcane : (isUnlocked ? new Color(0.4f, 0.9f, 0.4f) : new Color(1f, 0.5f, 0.3f));

                GUIStyle statSt = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                statSt.fontSize = 11;
                statSt.fontStyle = FontStyle.Bold;
                statSt.alignment = TextAnchor.MiddleLeft;
                statSt.normal.textColor = statusCol;
                GUI.Label(new Rect(textX, tabRect.y + 36f, textW, 20f), statusText, statSt);

                // Button overlay for click detection
                if (GUI.Button(tabRect, GUIContent.none, GUIStyle.none))
                {
                    if (_selectedPreviewIndex != i)
                    {
                        _selectedPreviewIndex = i;
                        Audio.AudioController.Instance?.PlaySfx(Audio.AudioCueType.ButtonClick);
                        Audio.HapticFeedback.Trigger(Audio.HapticFeedbackType.Light);
                    }
                }
            }

            // =================================================================
            // 3. CENTER SHOWCASE: LARGE SELECTED HERO ARTWORK & BREATHING
            // =================================================================
            var currentDef = classes[Mathf.Clamp(_selectedPreviewIndex, 0, classes.Count - 1)];
            if (currentDef != null)
            {
                bool isUnlocked = classManager.IsClassUnlocked(currentDef.ClassId);
                bool isActive = (currentDef.ClassId == classManager.SelectedClassId);

                float stageY = tabY + tabH + 8f;
                float stageH = 560f;
                Rect stageRect = new Rect(padX, stageY, contentW, stageH);

                // Breathing animation
                float breathY = Mathf.Sin(Time.time * 2.8f) * 6f;
                float scalePulse = 1f + Mathf.Sin(Time.time * 2.8f) * 0.02f;

                float heroW = 400f * scalePulse;
                float heroH = 490f * scalePulse;
                float heroCenterX = stageRect.x + stageRect.width * 0.5f;
                float groundY = stageRect.y + stageH - 15f;

                // Hero Artwork
                Texture2D heroTex = VisualAssetProvider.GetHeroTexture(currentDef.ClassId);
                if (heroTex != null)
                {
                    Rect heroRect = new Rect(heroCenterX - heroW * 0.5f, groundY - heroH + breathY, heroW, heroH);
                    Color oldC = GUI.color;
                    if (!isUnlocked)
                    {
                        GUI.color = new Color(0.55f, 0.55f, 0.65f, 0.75f);
                    }
                    GUI.DrawTexture(heroRect, heroTex, ScaleMode.ScaleToFit);
                    GUI.color = oldC;
                }

                // =================================================================
                // 4. STATS, ABILITIES & STARTING LOADOUT CARD
                // =================================================================
                float infoY = stageY + stageH + 8f;
                float infoH = 340f;
                Rect infoRect = new Rect(padX, infoY, contentW, infoH);
                LattiruneUITheme.DrawCard(infoRect);

                // 4a. Header
                GUIStyle cardTitleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                cardTitleStyle.fontSize = 22;
                cardTitleStyle.fontStyle = FontStyle.Bold;
                cardTitleStyle.alignment = TextAnchor.MiddleLeft;
                cardTitleStyle.normal.textColor = isActive ? LattiruneUITheme.ColorGoldBright : (isUnlocked ? Color.white : LattiruneUITheme.ColorTextMuted);

                string activeTag = isActive ? " [ACTIVE CHAMPION]" : (isUnlocked ? " [UNLOCKED]" : $" [LOCKED — {currentDef.EmbersCost} EMBERS]");
                GUI.Label(new Rect(padX + 20f, infoY + 12f, contentW - 40f, 28f), $"{currentDef.ClassName.ToUpper()}{activeTag}", cardTitleStyle);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 14;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUI.Label(new Rect(padX + 20f, infoY + 44f, contentW - 40f, 38f), currentDef.Description, descStyle);

                // 4b. Stat Pills Row (HP, ATK, ARMOR, SPD)
                float statY = infoY + 90f;
                float statW = (contentW - 40f) / 4f;
                Texture2D iconHp = VisualAssetProvider.GetUIIcon("ui_icon_hp");
                Texture2D iconAtk = VisualAssetProvider.GetUIIcon("ui_icon_attack");
                Texture2D iconArmor = VisualAssetProvider.GetUIIcon("ui_icon_armor");
                Texture2D iconBattle = VisualAssetProvider.GetUIIcon("ui_icon_battle");

                LattiruneUITheme.DrawIconValue(new Rect(padX + 20f, statY, statW, 26f), iconHp, $"HP: {currentDef.BaseHp}", new Color(0.3f, 0.9f, 0.4f), 15);
                LattiruneUITheme.DrawIconValue(new Rect(padX + 20f + statW, statY, statW, 26f), iconAtk, $"ATK: {currentDef.BaseAttack}", LattiruneUITheme.ColorTextPrimary, 15);
                LattiruneUITheme.DrawIconValue(new Rect(padX + 20f + statW * 2f, statY, statW, 26f), iconArmor, $"ARMOR: {currentDef.BaseArmor}", LattiruneUITheme.ColorCyanArcane, 15);
                LattiruneUITheme.DrawIconValue(new Rect(padX + 20f + statW * 3f, statY, statW, 26f), iconBattle, $"SPD: {currentDef.AttackInterval:0.0}s", LattiruneUITheme.ColorGoldPrimary, 15);

                // 4c. Starting Equipment & Runes Row
                float loadoutY = statY + 38f;
                GUIStyle loadoutTitleStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                loadoutTitleStyle.fontSize = 13;
                loadoutTitleStyle.fontStyle = FontStyle.Bold;
                loadoutTitleStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
                GUI.Label(new Rect(padX + 20f, loadoutY, 200f, 20f), "STARTING LOADOUT:", loadoutTitleStyle);

                float iconX = padX + 20f;
                float itemIconY = loadoutY + 24f;
                float itemSize = 48f;

                for (int i = 0; i < currentDef.StartingItemIds.Count; i++)
                {
                    Texture2D itex = VisualAssetProvider.GetItemTexture(currentDef.StartingItemIds[i]);
                    Rect itRect = new Rect(iconX, itemIconY, itemSize, itemSize);
                    GUI.DrawTexture(itRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                    LattiruneUITheme.DrawBorder(itRect, 1.5f, LattiruneUITheme.ColorGoldPrimary);
                    if (itex != null) GUI.DrawTexture(new Rect(itRect.x + 4, itRect.y + 4, itRect.width - 8, itRect.height - 8), itex, ScaleMode.ScaleToFit);
                    iconX += itemSize + 10f;
                }

                for (int i = 0; i < currentDef.StartingRuneIds.Count; i++)
                {
                    Texture2D rtex = VisualAssetProvider.GetRuneTexture(currentDef.StartingRuneIds[i]);
                    Rect rRect = new Rect(iconX, itemIconY, itemSize, itemSize);
                    GUI.DrawTexture(rRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                    LattiruneUITheme.DrawBorder(rRect, 1.5f, LattiruneUITheme.ColorCyanArcane);
                    if (rtex != null) GUI.DrawTexture(new Rect(rRect.x + 4, rRect.y + 4, rRect.width - 8, rRect.height - 8), rtex, ScaleMode.ScaleToFit);
                    iconX += itemSize + 10f;
                }

                // =================================================================
                // 5. BOTTOM ACTION BAR (DESCEND / SELECT / UNLOCK & RETURN)
                // =================================================================
                float botBarY = virtualH - 115f;
                float botBtnH = 85f;

                if (!isUnlocked)
                {
                    bool canAfford = metaProgression != null && metaProgression.CurrentEmbers >= currentDef.EmbersCost;
                    float unlockW = contentW - 220f;
                    Rect unlockRect = new Rect(padX, botBarY, unlockW, botBtnH);

                    GUI.enabled = canAfford;
                    if (GUI.Button(unlockRect, $"UNLOCK {currentDef.ClassName.ToUpper()} ({currentDef.EmbersCost} EMBERS)", LattiruneUITheme.StylePrimaryBtn))
                    {
                        if (classManager.UnlockClass(currentDef.ClassId, metaProgression))
                        {
                            _feedbackMessage = $"Unlocked {currentDef.ClassName}!";
                            Audio.AudioController.Instance?.PlaySfx(Audio.AudioCueType.RewardApplied);
                            Audio.HapticFeedback.Trigger(Audio.HapticFeedbackType.Success);
                        }
                    }
                    GUI.enabled = true;

                    Rect backRect = new Rect(padX + unlockW + 16f, botBarY, 204f, botBtnH);
                    if (GUI.Button(backRect, "RETURN", LattiruneUITheme.StyleSecondaryBtn))
                    {
                        navigation?.NavigateTo(ScreenState.MAIN_MENU);
                    }
                }
                else if (!isActive)
                {
                    float selectW = (contentW - 16f) * 0.5f;
                    Rect selectRect = new Rect(padX, botBarY, selectW, botBtnH);
                    if (GUI.Button(selectRect, "SELECT HERO", LattiruneUITheme.StylePrimaryBtn))
                    {
                        classManager.SelectClass(currentDef.ClassId);
                        Audio.AudioController.Instance?.PlaySfx(Audio.AudioCueType.ButtonClick);
                        Audio.HapticFeedback.Trigger(Audio.HapticFeedbackType.Medium);
                    }

                    Rect playRect = new Rect(padX + selectW + 16f, botBarY, selectW, botBtnH);
                    if (GUI.Button(playRect, "DESCEND", LattiruneUITheme.StylePrimaryBtn))
                    {
                        classManager.SelectClass(currentDef.ClassId);
                        StartDescent();
                    }
                }
                else
                {
                    float playW = contentW - 220f;
                    Rect playRect = new Rect(padX, botBarY, playW, botBtnH);
                    if (GUI.Button(playRect, "DESCEND INTO DUNGEON", LattiruneUITheme.StylePrimaryBtn))
                    {
                        StartDescent();
                    }

                    Rect backRect = new Rect(padX + playW + 16f, botBarY, 204f, botBtnH);
                    if (GUI.Button(backRect, "RETURN", LattiruneUITheme.StyleSecondaryBtn))
                    {
                        navigation?.NavigateTo(ScreenState.MAIN_MENU);
                    }
                }
            }

            GUI.matrix = oldMatrix;
        }
    }
}
