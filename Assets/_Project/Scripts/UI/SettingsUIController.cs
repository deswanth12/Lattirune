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
        [SerializeField] private bool isMuted = false;
        [SerializeField] private bool hapticsEnabled = true;

        public float MasterVolume => masterVolume;
        public float SfxVolume => sfxVolume;
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
            float modalWidth = 360f;
            float modalHeight = 360f;
            float startX = 20f;
            float startY = 120f;

            GUIStyle modalStyle = new GUIStyle(GUI.skin.box);
            modalStyle.fontSize = 13;
            modalStyle.alignment = TextAnchor.UpperCenter;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(new Rect(startX, startY, modalWidth, modalHeight), modalStyle);

            GUILayout.Label("AUDIO & HAPTICS SETTINGS", titleStyle);
            GUILayout.Space(12);

            GUILayout.Label($"Master Volume: {Mathf.RoundToInt(masterVolume * 100)}%");
            masterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f);
            GUILayout.Space(8);

            GUILayout.Label($"SFX Volume: {Mathf.RoundToInt(sfxVolume * 100)}%");
            sfxVolume = GUILayout.HorizontalSlider(sfxVolume, 0f, 1f);
            GUILayout.Space(8);

            if (GUILayout.Button(isMuted ? "UNMUTE AUDIO" : "MUTE AUDIO", GUILayout.Height(40)))
            {
                ToggleMute();
            }
            GUILayout.Space(6);

            if (GUILayout.Button(hapticsEnabled ? "DISABLE HAPTICS" : "ENABLE HAPTICS", GUILayout.Height(40)))
            {
                ToggleHaptics();
            }
            GUILayout.Space(12);

            if (GUILayout.Button("SAVE & RETURN", GUILayout.Height(52)))
            {
                CloseSettings();
            }

            GUILayout.EndArea();
        }
    }
}
