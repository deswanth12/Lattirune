using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Save;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for Game Settings (Audio Volume, Mute, Haptics).
    /// Interfaces directly with AudioController, HapticFeedback, and SaveSystem.
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private AudioController audioController;
        [SerializeField] private SaveSystem saveSystem;

        [Header("Runtime Settings")]
        [SerializeField] private float masterVolume = 1.0f;
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private bool isMuted = false;
        [SerializeField] private bool hapticsEnabled = true;

        public float MasterVolume => masterVolume;
        public float SfxVolume => sfxVolume;
        public float MusicVolume => musicVolume;
        public bool IsMuted => isMuted;
        public bool HapticsEnabled => hapticsEnabled;

        public void Initialize(
            ScreenNavigationController nav,
            AudioController audio = null,
            SaveSystem save = null)
        {
            navigation = nav;
            audioController = audio ?? AudioController.Instance;
            saveSystem = save;

            LoadSettings();
        }

        public void LoadSettings()
        {
            if (saveSystem != null && saveSystem.HasSaveFile())
            {
                SaveData data = saveSystem.Load();
                if (data != null && data.settings != null)
                {
                    masterVolume = data.settings.masterVolume;
                    sfxVolume = data.settings.sfxVolume;
                    isMuted = data.settings.isMuted;
                    hapticsEnabled = data.settings.hapticsEnabled;

                    ApplyToSystems();
                    return;
                }
            }

            masterVolume = 1.0f;
            sfxVolume = 1.0f;
            musicVolume = 0.7f;
            isMuted = false;
            hapticsEnabled = true;
            ApplyToSystems();
        }

        public void SetMasterVolume(float vol)
        {
            masterVolume = Mathf.Clamp01(vol);
            ApplyToSystems();
        }

        public void SetSfxVolume(float vol)
        {
            sfxVolume = Mathf.Clamp01(vol);
            ApplyToSystems();
        }

        public void SetMusicVolume(float vol)
        {
            musicVolume = Mathf.Clamp01(vol);
            ApplyToSystems();
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;
            ApplyToSystems();
        }

        public void ToggleHaptics()
        {
            hapticsEnabled = !hapticsEnabled;
            ApplyToSystems();
        }

        private void ApplyToSystems()
        {
            if (audioController != null)
            {
                audioController.SetMasterVolume(masterVolume);
                audioController.SetSfxVolume(sfxVolume);
                audioController.SetMusicVolume(musicVolume);
                audioController.SetMuted(isMuted);
            }
            HapticFeedback.SetHapticsEnabled(hapticsEnabled);
        }

        public void SaveSettings()
        {
            if (saveSystem != null)
            {
                SaveData data = saveSystem.HasSaveFile() ? saveSystem.Load() : SaveData.CreateDefault();
                data.settings = new SavedSettingsData(masterVolume, sfxVolume, isMuted, hapticsEnabled);
                saveSystem.Save(data);
            }
        }

        public void CloseSettings()
        {
            SaveSettings();
            if (navigation != null)
            {
                navigation.NavigateBack();
            }
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.SETTINGS) return;

            DrawSettingsWindow();
        }

        private void DrawSettingsWindow()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 920f;
            float panelHeight = 1100f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "⚙️ AUDIO & SETTINGS ⚙️");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("⚙️ AUDIO & SETTINGS ⚙️", "Configure audio levels, tactile haptics, and accessibility.");
            GUILayout.Space(20);

            GUIStyle labelStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            labelStyle.fontSize = 20;
            labelStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;

            GUILayout.Label($"Master Volume: <b>{Mathf.RoundToInt(masterVolume * 100)}%</b>", labelStyle);
            float newMaster = GUILayout.HorizontalSlider(masterVolume, 0f, 1f, GUILayout.Height(30));
            if (Mathf.Abs(newMaster - masterVolume) > 0.001f) SetMasterVolume(newMaster);
            GUILayout.Space(12);

            GUILayout.Label($"Music (BGM) Volume: <b>{Mathf.RoundToInt(musicVolume * 100)}%</b>", labelStyle);
            float newMusic = GUILayout.HorizontalSlider(musicVolume, 0f, 1f, GUILayout.Height(30));
            if (Mathf.Abs(newMusic - musicVolume) > 0.001f) SetMusicVolume(newMusic);
            GUILayout.Space(12);

            GUILayout.Label($"SFX Volume: <b>{Mathf.RoundToInt(sfxVolume * 100)}%</b>", labelStyle);
            float newSfx = GUILayout.HorizontalSlider(sfxVolume, 0f, 1f, GUILayout.Height(30));
            if (Mathf.Abs(newSfx - sfxVolume) > 0.001f) SetSfxVolume(newSfx);
            GUILayout.Space(20);

            if (LattiruneUITheme.DrawSecondaryButton(isMuted ? "🔇 UNMUTE AUDIO" : "🔊 MUTE AUDIO", 65f))
            {
                ToggleMute();
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton(hapticsEnabled ? "📳 DISABLE HAPTICS" : "📳 ENABLE HAPTICS", 65f))
            {
                ToggleHaptics();
            }
            GUILayout.Space(20);

            if (LattiruneUITheme.DrawPrimaryButton("⚔️ SAVE & RETURN ⚔️", 75f))
            {
                CloseSettings();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
