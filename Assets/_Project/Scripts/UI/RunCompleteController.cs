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

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.RUN_COMPLETE) return;

            DrawRunCompleteWindow();
        }

        private void DrawRunCompleteWindow()
        {
            float modalWidth = 360f;
            float modalHeight = 360f;
            float startX = 20f;
            float startY = 120f;

            GUIStyle modalStyle = new GUIStyle(GUI.skin.box);
            modalStyle.fontSize = 13;
            modalStyle.alignment = TextAnchor.UpperCenter;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 22;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(new Rect(startX, startY, modalWidth, modalHeight), modalStyle);

            string outcomeHeader = isVictory ? "<color=green>DUNGEON CLEARED!</color>" : "<color=red>RUN DEFEATED</color>";
            GUILayout.Label(outcomeHeader, titleStyle);
            GUILayout.Space(10);

            GUILayout.Label($"<b>Floors Reached:</b> {floorsCleared} / 10");
            GUILayout.Label($"<b>In-Run Gold Collected:</b> {goldEarned} 🪙");
            GUILayout.Label($"<b>Dungeon Embers Awarded:</b> {embersEarned} 🔥");
            GUILayout.Space(16);

            if (GUILayout.Button("RETURN TO CAMPFIRE HUB", GUILayout.Height(52)))
            {
                ReturnToCampfireHub();
            }
            GUILayout.Space(8);

            if (GUILayout.Button("START NEW RUN", GUILayout.Height(52)))
            {
                StartNewRun();
            }

            GUILayout.EndArea();
        }
    }
}
