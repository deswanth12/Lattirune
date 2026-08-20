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
                navigation.NavigateTo(ScreenState.HERO_SELECTION);
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
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 920f;
            float panelHeight = 1100f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "⚔️ LATTIRUNE ⚔️");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 50, panelWidth - 80, panelHeight - 100));

            LattiruneUITheme.DrawHeader("⚔️ LATTIRUNE ⚔️", "Align the Lattice. Awaken the Runes.");
            GUILayout.Space(36);

            if (hasSavedRun)
            {
                if (LattiruneUITheme.DrawPrimaryButton("⚔️ RESUME ACTIVE DESCENT ⚔️", 75f))
                {
                    ContinueRun();
                }
                GUILayout.Space(16);
            }

            if (!hasSavedRun)
            {
                if (LattiruneUITheme.DrawPrimaryButton("🔥 START NEW RUN 🔥", 75f))
                {
                    StartNewRun();
                }
            }
            else
            {
                if (LattiruneUITheme.DrawSecondaryButton("⚡ START NEW RUN (DISCARD SAVE)", 65f))
                {
                    StartNewRun();
                }
            }
            GUILayout.Space(16);

            if (LattiruneUITheme.DrawSecondaryButton("🏕️ CAMPFIRE META-HUB", 65f))
            {
                OpenCampfireHub();
            }
            GUILayout.Space(16);

            if (LattiruneUITheme.DrawSecondaryButton("🔊 AUDIO & SETTINGS", 65f))
            {
                OpenSettings();
            }
            GUILayout.Space(16);

            if (LattiruneUITheme.DrawDangerButton("🚪 EXIT GAME", 65f))
            {
                ExitGame();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
