using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Progression;
using Lattirune.Save;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for the main menu screen.
    /// Manages starting a new run, continuing a saved run, navigating to sub-screens, and quitting.
    /// Adheres to dark fantasy aesthetic with stylized artwork backdrops.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;
        [SerializeField] private MetaProgressionManager metaProgression;
        [SerializeField] private SaveSystem saveSystem;

        [Header("State")]
        [SerializeField] private bool hasSavedRun = false;

        public bool HasSavedRun => hasSavedRun;

        public void Initialize(ScreenNavigationController nav, RunManager run = null, MetaProgressionManager meta = null, SaveSystem save = null)
        {
            navigation = nav;
            runManager = run;
            metaProgression = meta;
            saveSystem = save;
            RefreshSaveState();
        }

        public void Initialize(ScreenNavigationController nav, SaveSystem save)
        {
            navigation = nav;
            saveSystem = save;
            RefreshSaveState();
        }

        public void RefreshSaveState()
        {
            hasSavedRun = saveSystem != null && saveSystem.HasSave();
        }

        public void StartNewRun()
        {
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            if (hasSavedRun && saveSystem != null)
            {
                saveSystem.DeleteSave();
                hasSavedRun = false;
            }

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
                navigation.NavigateTo(ScreenState.HERO_SELECTION);
            }
        }

        public void ContinueRun()
        {
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.DUNGEON_MAP);
            }
        }

        public void OpenCampfireHub()
        {
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.CAMPFIRE_HUB);
            }
        }

        public void OpenSettings()
        {
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
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
            LattiruneUITheme.DrawHeader("LATTIRUNE", "ALIGN THE LATTICE. AWAKEN THE RUNES.");
            GUILayout.Space(14);

            // Center Visual Arcane Citadel Backdrop
            Texture2D menuBg = VisualAssetProvider.GetBackdrop("bg_mainmenu");
            if (menuBg != null)
            {
                Rect bgRect = GUILayoutUtility.GetRect(panelWidth - 80, 200f);
                GUI.DrawTexture(bgRect, menuBg, ScaleMode.ScaleAndCrop);
                GUILayout.Space(16);
            }

            // Primary Action Button
            if (hasSavedRun)
            {
                if (LattiruneUITheme.DrawPrimaryButton("RESUME DESCENT", 80f))
                {
                    ContinueRun();
                }
                GUILayout.Space(14);
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

            GUILayout.Space(18);

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

            GUILayout.FlexibleSpace();

            // Bottom Version Footer
            GUIStyle versionStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            versionStyle.fontSize = 14;
            versionStyle.alignment = TextAnchor.MiddleCenter;
            versionStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUILayout.Label("Lattirune v1.0.0 (API 36 - Android 16)", versionStyle);

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
