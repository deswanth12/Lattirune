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

            float panelWidth = 940f;
            float panelHeight = 1350f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "LATTIRUNE");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            // Top Title & Subtitle Header
            LattiruneUITheme.DrawHeader("LATTIRUNE", "Align the Lattice. Awaken the Runes.");
            GUILayout.Space(20);

            // Center Visual Identity Emblem Container
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.Space(12);
            GUIStyle emblemStyle = new GUIStyle(LattiruneUITheme.StyleHeaderTitle);
            emblemStyle.fontSize = 42;
            emblemStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUILayout.Label("--- ❖ ---", emblemStyle);
            
            GUIStyle loreStyle = new GUIStyle(LattiruneUITheme.StyleHeaderSubtitle);
            loreStyle.fontSize = 16;
            loreStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("A Tactical Grid-Inventory Roguelite", loreStyle);
            GUILayout.Space(12);
            GUILayout.EndVertical();

            GUILayout.Space(36);

            // Primary Action Button
            if (hasSavedRun)
            {
                if (LattiruneUITheme.DrawPrimaryButton("RESUME DESCENT", 80f))
                {
                    ContinueRun();
                }
                GUILayout.Space(16);
                if (LattiruneUITheme.DrawSecondaryButton("START NEW RUN (DISCARD SAVE)", 65f))
                {
                    StartNewRun();
                }
            }
            else
            {
                if (LattiruneUITheme.DrawPrimaryButton("START NEW RUN", 85f))
                {
                    StartNewRun();
                }
            }

            GUILayout.Space(20);

            // Secondary Buttons
            if (LattiruneUITheme.DrawSecondaryButton("CAMPFIRE META-HUB", 65f))
            {
                OpenCampfireHub();
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton("ARCANE CODEX & BESTIARY", 65f))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.CODEX);
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton("SETTINGS", 65f))
            {
                OpenSettings();
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawDangerButton("EXIT GAME", 60f))
            {
                ExitGame();
            }

            GUILayout.FlexibleSpace();

            // Bottom Version Footer
            GUIStyle versionStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            versionStyle.fontSize = 15;
            versionStyle.alignment = TextAnchor.MiddleCenter;
            versionStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUILayout.Label("Lattirune v1.0.0 (API 36 - Android 16)", versionStyle);

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
