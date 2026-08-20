using System;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for the persistent Campfire Meta-Hub.
    /// Manages persistent Ember balance display, progression stats, and Blueprint Forge navigation.
    /// Completely decoupled from in-run temporary state.
    /// </summary>
    public class CampfireHubController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private MetaProgressionManager metaManager;
        [SerializeField] private BlueprintForgeController forgeController;

        [Header("State")]
        [SerializeField] private bool isHubVisible = true;

        public event Action OnHubOpened;
        public event Action OnHubClosed;

        public ScreenNavigationController Navigation => navigation;
        public MetaProgressionManager MetaManager => metaManager;
        public BlueprintForgeController Forge => forgeController;
        public bool IsHubVisible => isHubVisible;
        public int DisplayedEmbers => metaManager != null ? metaManager.EmbersBalance : 0;
        public int UnlockedBlueprintCount => metaManager != null ? metaManager.UnlockedBlueprintCount : 0;
        public int TotalBlueprintCount => metaManager != null && metaManager.Database != null ? metaManager.Database.TotalBlueprintCount : 0;

        public void Initialize(MetaProgressionManager meta, BlueprintForgeController forge)
        {
            metaManager = meta;
            forgeController = forge;
            isHubVisible = true;

            if (forgeController != null && metaManager != null)
            {
                forgeController.Initialize(metaManager);
            }
        }

        public void Initialize(ScreenNavigationController nav, MetaProgressionManager meta, BlueprintForgeController forge)
        {
            navigation = nav;
            Initialize(meta, forge);
        }

        public void ShowHub()
        {
            isHubVisible = true;
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            OnHubOpened?.Invoke();
        }

        public void HideHub()
        {
            isHubVisible = false;
            if (forgeController != null && forgeController.IsOpen)
            {
                forgeController.CloseForge();
            }
            OnHubClosed?.Invoke();
        }

        public void OpenBlueprintForge()
        {
            if (forgeController != null)
            {
                forgeController.OpenForge();
            }
        }

        public void CloseBlueprintForge()
        {
            if (forgeController != null)
            {
                forgeController.CloseForge();
            }
        }

        private void OnGUI()
        {
            if (!isHubVisible || metaManager == null) return;
            if (navigation != null && navigation.CurrentScreen != ScreenState.CAMPFIRE_HUB) return;

            // If Forge is currently open, let Forge render its modal window
            if (forgeController != null && forgeController.IsOpen)
            {
                return;
            }

            DrawHubWindow();
        }

        private void DrawHubWindow()
        {
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 920f;
            float panelHeight = 1200f;
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
            titleStyle.fontSize = 36;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(1f, 0.55f, 0.1f); // Magma Amber

            GUILayout.Label("🔥 CAMPFIRE META-HUB 🔥", titleStyle);
            GUILayout.Space(14);

            GUIStyle statStyle = new GUIStyle(GUI.skin.label);
            statStyle.fontSize = 20;
            statStyle.normal.textColor = Color.white;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"🔥 Dungeon Embers: <b>{DisplayedEmbers}</b>", statStyle);
            GUILayout.Label($"📜 Blueprints Unlocked: <b>{UnlockedBlueprintCount} / {TotalBlueprintCount}</b>", statStyle);
            GUILayout.Label($"⚔️ Runs Attempted: <b>{metaManager.TotalRunsAttempted}</b> | Boss Clears: <b>{metaManager.TotalBossClears}</b>", statStyle);
            GUILayout.EndVertical();

            GUILayout.Space(24);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 22;
            btnStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("ENTER BLUEPRINT FORGE", btnStyle, GUILayout.Height(65)))
            {
                OpenBlueprintForge();
            }
            GUILayout.Space(14);

            if (GUILayout.Button("HERO ROSTER & LOADOUTS", btnStyle, GUILayout.Height(65)))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.HERO_SELECTION);
            }
            GUILayout.Space(14);

            if (GUILayout.Button("ARCANE CODEX & BESTIARY", btnStyle, GUILayout.Height(65)))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.CODEX);
            }
            GUILayout.Space(14);

            if (GUILayout.Button("RETURN TO MAIN MENU", btnStyle, GUILayout.Height(65)))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.MAIN_MENU);
                else HideHub();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
