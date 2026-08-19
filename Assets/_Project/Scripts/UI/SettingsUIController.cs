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
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 920f;
            float panelHeight = 1100f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (1920f - panelHeight) * 0.5f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.96f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 50, panelWidth - 80, panelHeight - 100));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 36;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.3f, 0.8f, 1f); // Arcane Cyan

            GUILayout.Label("⚙️ AUDIO & HAPTICS ⚙️", titleStyle);
            GUILayout.Space(24);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 22;
            labelStyle.normal.textColor = Color.white;

            GUILayout.Label($"Master Volume: <b>{Mathf.RoundToInt(masterVolume * 100)}%</b>", labelStyle);
            masterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f, GUILayout.Height(30));
            GUILayout.Space(16);

            GUILayout.Label($"SFX Volume: <b>{Mathf.RoundToInt(sfxVolume * 100)}%</b>", labelStyle);
            sfxVolume = GUILayout.HorizontalSlider(sfxVolume, 0f, 1f, GUILayout.Height(30));
            GUILayout.Space(24);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 22;
            btnStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button(isMuted ? "🔇 UNMUTE AUDIO" : "🔊 MUTE AUDIO", btnStyle, GUILayout.Height(65)))
            {
                ToggleMute();
            }
            GUILayout.Space(14);

            if (GUILayout.Button(hapticsEnabled ? "📳 DISABLE HAPTICS" : "📳 ENABLE HAPTICS", btnStyle, GUILayout.Height(65)))
            {
                ToggleHaptics();
            }
            GUILayout.Space(24);

            if (GUILayout.Button("SAVE & RETURN", btnStyle, GUILayout.Height(65)))
            {
                CloseSettings();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
