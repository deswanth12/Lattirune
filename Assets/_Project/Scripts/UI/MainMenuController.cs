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
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 920f;
            float panelHeight = 1100f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.96f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 50, panelWidth - 80, panelHeight - 100));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 44;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.77f, 0.61f, 0.15f); // Burnished Brass

            GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 20;
            subtitleStyle.fontStyle = FontStyle.Italic;
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            subtitleStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            GUILayout.Label("⚔️ LATTIRUNE ⚔️", titleStyle);
            GUILayout.Space(6);
            GUILayout.Label("Align the Lattice. Awaken the Runes.", subtitleStyle);
            GUILayout.Space(36);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 24;
            btnStyle.fontStyle = FontStyle.Bold;

            // Minimum touch target height 52dp compliant (65px in 1080x1920 portrait)
            if (hasSavedRun)
            {
                GUI.color = Color.cyan;
                if (GUILayout.Button("RESUME ACTIVE DESCENT", btnStyle, GUILayout.Height(65)))
                {
                    ContinueRun();
                }
                GUI.color = oldColor;
                GUILayout.Space(14);
            }

            GUI.color = Color.yellow;
            if (GUILayout.Button("START NEW RUN", btnStyle, GUILayout.Height(65)))
            {
                StartNewRun();
            }
            GUI.color = oldColor;
            GUILayout.Space(14);

            if (GUILayout.Button("CAMPFIRE META-HUB", btnStyle, GUILayout.Height(65)))
            {
                OpenCampfireHub();
            }
            GUILayout.Space(14);

            if (GUILayout.Button("AUDIO & SETTINGS", btnStyle, GUILayout.Height(65)))
            {
                OpenSettings();
            }
            GUILayout.Space(14);

            if (GUILayout.Button("EXIT GAME", btnStyle, GUILayout.Height(65)))
            {
                ExitGame();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
