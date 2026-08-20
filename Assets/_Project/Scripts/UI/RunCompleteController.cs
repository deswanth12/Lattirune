using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Dungeon;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for the Run Complete / Victory / Defeat end-of-run summary.
    /// Displays run statistics, gold earned, Embers awarded, and routes back to the Campfire Meta-Hub.
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

            float panelWidth = 920f;
            float panelHeight = 1100f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            string windowTitle = isVictory ? "VICTORY: DUNGEON CLEARED" : "DEFEAT: HERO FALLEN";
            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), windowTitle);

            GUILayout.BeginArea(new Rect(posX + 40, posY + 50, panelWidth - 80, panelHeight - 100));

            LattiruneUITheme.DrawHeader(windowTitle, isVictory ? "The Lich Sanctum is cleansed. Your legend echoes in the abyss." : "The dark forces of the Cursed Sewers proved overwhelming.");
            GUILayout.Space(24);

            GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            statStyle.fontSize = 18;
            statStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;

            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.Label($"<b>Floors Cleared:</b> {floorsCleared} / 10", statStyle);
            GUILayout.Space(6);
            GUILayout.Label($"<b>Gold Collected:</b> {goldEarned} Gold", statStyle);
            GUILayout.Space(6);
            GUILayout.Label($"<b>Embers Banked:</b> {embersEarned} Embers", statStyle);
            if (metaProgression != null)
            {
                GUILayout.Space(6);
                GUILayout.Label($"<b>Total Boss Clears:</b> {metaProgression.TotalBossClears}", statStyle);
                GUILayout.Label($"<b>Total Runs Attempted:</b> {metaProgression.TotalRunsAttempted}", statStyle);
            }
            GUILayout.EndVertical();

            GUILayout.Space(24);

            if (LattiruneUITheme.DrawPrimaryButton("START ANOTHER RUN", 75f))
            {
                StartNewRun();
            }
            GUILayout.Space(12);

            if (isVictory)
            {
                if (LattiruneUITheme.DrawSecondaryButton("CONTINUE IN ENDLESS MODE", 65f))
                {
                    ContinueEndlessMode();
                }
                GUILayout.Space(12);
            }

            if (LattiruneUITheme.DrawSecondaryButton("RETURN TO CAMPFIRE HUB", 65f))
            {
                ReturnToCampfireHub();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
