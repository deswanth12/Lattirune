using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Dungeon;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for the Run Complete / Victory / Defeat end-of-run summary.
    /// Displays victory trophies, crypt tombstones, gold, and ember rewards (0 emoji, 0 placeholders).
    /// </summary>
    public class RunCompleteController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;
        [SerializeField] private MetaProgressionManager metaProgression;

        [Header("State")]
        [SerializeField] private bool isVictory = false;
        [SerializeField] private int floorsCleared = 0;
        [SerializeField] private int goldEarned = 0;
        [SerializeField] private int embersEarned = 0;

        public event Action OnHubReturned;

        public bool IsVictory => isVictory;
        public int FloorsCleared => floorsCleared;
        public int GoldEarned => goldEarned;
        public int EmbersEarned => embersEarned;

        public void Initialize(
            ScreenNavigationController nav,
            RunManager run,
            MetaProgressionManager meta)
        {
            navigation = nav;
            runManager = run;
            metaProgression = meta;
        }

        public void SetupSummary(bool victory, int floors, int gold, int embers)
        {
            isVictory = victory;
            floorsCleared = Mathf.Max(0, floors);
            goldEarned = Mathf.Max(0, gold);
            embersEarned = Mathf.Max(0, embers);

            if (isVictory)
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                HapticFeedback.Trigger(HapticFeedbackType.Success);
            }
            else
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                HapticFeedback.Trigger(HapticFeedbackType.Failure);
            }
        }

        public void ReturnToCampfireHub()
        {
            if (runManager != null)
            {
                runManager.ResetRun();
            }

            OnHubReturned?.Invoke();

            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.CAMPFIRE_HUB);
            }
        }

        public void StartNewRun()
        {
            if (runManager != null)
            {
                runManager.ResetRun();
                runManager.StartRun(metaProgression);
            }

            if (metaProgression != null)
            {
                metaProgression.RecordRunAttempt();
            }

            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.GRID_BUILD);
            }
        }

        public void ContinueEndlessMode()
        {
            if (runManager != null)
            {
                runManager.EnableEndlessMode();
                runManager.AdvanceFloor();
            }

            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.DUNGEON_MAP);
            }
        }

        private void OnGUI()
        {
            if (navigation == null || (navigation.CurrentScreen != ScreenState.RUN_COMPLETE && navigation.CurrentScreen != ScreenState.VICTORY && navigation.CurrentScreen != ScreenState.DEATH)) return;

            DrawRunCompleteWindow();
        }

        private void DrawRunCompleteWindow()
        {
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

            Texture2D icon = isVictory ? VisualAssetProvider.GetUIIcon("ui_icon_unlock") : VisualAssetProvider.GetUIIcon("ui_icon_death");
            if (icon != null)
            {
                GUI.DrawTexture(new Rect(padX + 18f, topY + 18f, 84f, 84f), icon, ScaleMode.ScaleToFit);
            }

            string headerTitle = isVictory ? "TRIUMPH OVER DARKNESS" : "FALLEN IN THE DEPTHS";
            string headerSubtitle = isVictory ? "The Lich Lord is slain. The catacombs are cleansed." : "Your soul returns to the sacred campfire to forge anew.";

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = isVictory ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorRedDanger;
            GUI.Label(new Rect(padX + 116f, topY + 18f, contentW - 130f, 26f), headerTitle, titleStyle);

            GUIStyle subStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            subStyle.fontSize = 13;
            subStyle.fontStyle = FontStyle.Italic;
            subStyle.alignment = TextAnchor.MiddleLeft;
            subStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(padX + 116f, topY + 48f, contentW - 130f, 40f), headerSubtitle, subStyle);

            // =================================================================
            // 2. EXPEDITION TELEMETRY & ARTWORK
            // =================================================================
            float contentY = topY + topH + 16f;
            float botBtnH = 75f;
            float botMargin = 25f;
            float act2Y = virtualH - botBtnH - botMargin;
            float act1Y = act2Y - botBtnH - 12f;
            float areaH = act1Y - contentY - 16f;

            Rect areaRect = new Rect(padX, contentY, contentW, areaH);
            LattiruneUITheme.DrawCard(areaRect);

            // Telemetry Cards
            float tY = contentY + 24f;
            GUIStyle sectionTitle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            sectionTitle.fontSize = 22;
            sectionTitle.fontStyle = FontStyle.Bold;
            sectionTitle.alignment = TextAnchor.MiddleCenter;
            sectionTitle.normal.textColor = isVictory ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorRedDanger;
            GUI.Label(new Rect(padX + 20f, tY, contentW - 40f, 30f), "EXPEDITION SUMMARY", sectionTitle);

            float rowY = tY + 60f;
            float rowH = 75f;
            float rowW = contentW - 48f;

            // Stat 1: Floors Cleared
            Rect r1 = new Rect(padX + 24f, rowY, rowW, rowH);
            LattiruneUITheme.DrawCard(r1);
            Texture2D floorIcon = VisualAssetProvider.GetUIIcon("ui_icon_floor");
            if (floorIcon != null) GUI.DrawTexture(new Rect(r1.x + 16f, r1.y + 14f, 48f, 48f), floorIcon, ScaleMode.ScaleToFit);
            GUIStyle sStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            sStyle.fontSize = 18;
            sStyle.fontStyle = FontStyle.Bold;
            sStyle.alignment = TextAnchor.MiddleLeft;
            sStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(r1.x + 76f, r1.y + 22f, 300f, 30f), $"Floors Cleared: {floorsCleared} / 10", sStyle);

            // Stat 2: Gold Collected
            rowY += rowH + 14f;
            Rect r2 = new Rect(padX + 24f, rowY, rowW, rowH);
            LattiruneUITheme.DrawCard(r2);
            Texture2D goldIcon = VisualAssetProvider.GetUIIcon("ui_icon_gold");
            if (goldIcon != null) GUI.DrawTexture(new Rect(r2.x + 16f, r2.y + 14f, 48f, 48f), goldIcon, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(r2.x + 76f, r2.y + 22f, 300f, 30f), $"Gold Collected: {goldEarned} Gold", sStyle);

            // Stat 3: Embers Awarded
            rowY += rowH + 14f;
            Rect r3 = new Rect(padX + 24f, rowY, rowW, rowH);
            LattiruneUITheme.DrawCard(r3);
            Texture2D embIcon = VisualAssetProvider.GetUIIcon("ui_icon_embers");
            if (embIcon != null) GUI.DrawTexture(new Rect(r3.x + 16f, r3.y + 14f, 48f, 48f), embIcon, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(r3.x + 76f, r3.y + 22f, 300f, 30f), $"Embers Harvested: +{embersEarned} Embers", sStyle);

            // =================================================================
            // 3. BOTTOM ACTION BUTTONS
            // =================================================================
            Rect act1Rect = new Rect(padX, act1Y, contentW, botBtnH);
            Rect act2Rect = new Rect(padX, act2Y, contentW, botBtnH);

            if (isVictory)
            {
                if (GUI.Button(act1Rect, "ENTER ENDLESS DESCENT", LattiruneUITheme.StylePrimaryBtn))
                {
                    AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                    ContinueEndlessMode();
                }
            }
            else
            {
                if (GUI.Button(act1Rect, "START NEW RUN", LattiruneUITheme.StylePrimaryBtn))
                {
                    AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                    StartNewRun();
                }
            }

            if (GUI.Button(act2Rect, "RETURN TO CAMPFIRE HUB", LattiruneUITheme.StyleSecondaryBtn))
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                ReturnToCampfireHub();
            }

            GUI.matrix = oldMatrix;
        }
    }
}

