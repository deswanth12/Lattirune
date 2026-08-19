using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Dungeon;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for the Main Menu entry flow.
    /// Coordinates Starting New Runs, Continuing Active Runs from encrypted save data,
    /// entering the persistent Campfire Meta-Hub, and accessing Game Settings.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;
        [SerializeField] private MetaProgressionManager metaProgression;
        [SerializeField] private SaveSystem saveSystem;

        [Header("State")]
        [SerializeField] private bool hasSavedRun = false;

        public event Action OnNewRunStarted;
        public event Action OnRunContinued;

        public ScreenNavigationController Navigation => navigation;
        public RunManager Run => runManager;
        public MetaProgressionManager Meta => metaProgression;
        public bool HasSavedRun => hasSavedRun;

        public void Initialize(
            ScreenNavigationController nav,
            RunManager run,
            MetaProgressionManager meta,
            SaveSystem save = null)
        {
            navigation = nav;
            runManager = run;
            metaProgression = meta;
            saveSystem = save;

            CheckSavedRun();
        }

        public void CheckSavedRun()
        {
            if (saveSystem != null && saveSystem.HasSaveFile())
            {
                SaveData data = saveSystem.Load();
                hasSavedRun = data != null && data.run != null && data.run.hasActiveRun;
            }
            else
            {
                hasSavedRun = false;
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

            OnNewRunStarted?.Invoke();

            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.GRID_BUILD);
            }
        }

        public void ContinueRun()
        {
            if (saveSystem != null && saveSystem.HasSaveFile())
            {
                SaveData data = saveSystem.Load();
                if (data != null && data.run != null && runManager != null)
                {
                    runManager.RestoreRunState(
                        data.run.currentFloorIndex,
                        data.run.currentEncounterIndex,
                        (RunState)data.run.runState
                    );

                    if (metaProgression != null && data.meta != null)
                    {
                        metaProgression.ImportMetaData(data.meta);
                    }

                    OnRunContinued?.Invoke();

                    if (navigation != null)
                    {
                        navigation.NavigateTo(ScreenState.GRID_BUILD);
                    }
                    return;
                }
            }

            // Fallback to start new run if no valid save was found
            StartNewRun();
        }

        public void OpenCampfireHub()
        {
            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.CAMPFIRE_HUB);
            }
        }

        public void OpenSettings()
        {
            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.SETTINGS);
            }
        }

        public void ExitGame()
        {
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.MAIN_MENU) return;

            DrawMainMenuWindow();
        }

        private void DrawMainMenuWindow()
        {
            float modalWidth = 360f;
            float modalHeight = 400f;
            float startX = 20f;
            float startY = 120f;

            GUIStyle modalStyle = new GUIStyle(GUI.skin.box);
            modalStyle.fontSize = 13;
            modalStyle.alignment = TextAnchor.UpperCenter;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 12;
            subtitleStyle.fontStyle = FontStyle.Italic;
            subtitleStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(new Rect(startX, startY, modalWidth, modalHeight), modalStyle);

            GUILayout.Label("LATTIRUNE", titleStyle);
            GUILayout.Label("Align the Lattice. Awaken the Runes.", subtitleStyle);
            GUILayout.Space(16);

            // Minimum touch target height 52dp compliant
            if (hasSavedRun)
            {
                if (GUILayout.Button("CONTINUE RUN", GUILayout.Height(52)))
                {
                    ContinueRun();
                }
                GUILayout.Space(6);
            }

            if (GUILayout.Button("START NEW RUN", GUILayout.Height(52)))
            {
                StartNewRun();
            }
            GUILayout.Space(6);

            if (GUILayout.Button("CAMPFIRE META-HUB", GUILayout.Height(52)))
            {
                OpenCampfireHub();
            }
            GUILayout.Space(6);

            if (GUILayout.Button("SETTINGS", GUILayout.Height(52)))
            {
                OpenSettings();
            }
            GUILayout.Space(6);

            if (GUILayout.Button("EXIT", GUILayout.Height(52)))
            {
                ExitGame();
            }

            GUILayout.EndArea();
        }
    }
}
