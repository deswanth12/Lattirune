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
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.ItemPlaced);
                HapticFeedback.Trigger(HapticFeedbackType.Success);
            }
            else
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.InvalidPlacement);
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

            float panelWidth = 960f;
            float panelHeight = 1400f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            string windowTitle = isVictory ? "EXPEDITION TRIUMPH" : "EXPEDITION CONCLUDED";
            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), windowTitle);

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            string headerTitle = isVictory ? "TRIUMPH OVER DARKNESS" : "FALLEN IN THE DEPTHS";
            string headerSubtitle = isVictory ? "The Lich Lord is slain. The subterranean catacombs are cleansed." : "Your soul returns to the sacred campfire to forge anew.";
            LattiruneUITheme.DrawHeader(headerTitle, headerSubtitle);
            GUILayout.Space(12);

            // Large Center Emblem / Backdrop
            Texture2D endBg = isVictory ? VisualAssetProvider.GetBackdrop("bg_victory_hall") : VisualAssetProvider.GetBackdrop("bg_death_crypt");
            if (endBg != null)
            {
                Rect bgRect = GUILayoutUtility.GetRect(panelWidth - 80, 240f);
                GUI.DrawTexture(bgRect, endBg, ScaleMode.ScaleAndCrop);
                GUILayout.Space(16);
            }

            // Run Statistics Card
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

            GUIStyle sectionTitle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            sectionTitle.fontSize = 20;
            sectionTitle.fontStyle = FontStyle.Bold;
            sectionTitle.normal.textColor = isVictory ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorRedDanger;
            GUILayout.Label("EXPEDITION TELEMETRY", sectionTitle);
            GUILayout.Space(10);

            GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            statStyle.fontSize = 18;
            statStyle.fontStyle = FontStyle.Bold;
            statStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;

            GUILayout.Label($"Floors Cleared: {floorsCleared} / 10", statStyle);
            GUILayout.Label($"Gold Collected: {goldEarned}g", statStyle);
            GUILayout.Label($"Embers Awarded: +{embersEarned}", statStyle);

            GUILayout.EndVertical();
            GUILayout.Space(24);

            // Action Buttons
            if (isVictory)
            {
                if (LattiruneUITheme.DrawPrimaryButton("ENTER ENDLESS DESCENT", 75f))
                {
                    ContinueEndlessMode();
                }
                GUILayout.Space(12);
            }
            else
            {
                if (LattiruneUITheme.DrawPrimaryButton("START NEW RUN", 75f))
                {
                    StartNewRun();
                }
                GUILayout.Space(12);
            }

            if (LattiruneUITheme.DrawSecondaryButton("RETURN TO CAMPFIRE HUB", 75f))
            {
                ReturnToCampfireHub();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
