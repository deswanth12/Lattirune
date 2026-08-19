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
            float modalWidth = 360f;
            float modalHeight = 440f;
            float startX = 20f;
            float startY = 100f;

            GUIStyle modalStyle = new GUIStyle(GUI.skin.box);
            modalStyle.fontSize = 13;
            modalStyle.alignment = TextAnchor.UpperCenter;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(new Rect(startX, startY, modalWidth, modalHeight), modalStyle);

            GUILayout.Label("CAMPFIRE META-HUB", titleStyle);
            GUILayout.Space(6);

            GUILayout.Label($"🔥 Dungeon Embers: <b>{DisplayedEmbers}</b>");
            GUILayout.Label($"📜 Blueprints Unlocked: <b>{UnlockedBlueprintCount} / {TotalBlueprintCount}</b>");
            GUILayout.Label($"⚔️ Runs Attempted: {metaManager.TotalRunsAttempted} | Boss Clears: {metaManager.TotalBossClears}");

            GUILayout.Space(12);

            // Minimum touch target height 52dp compliant (52px in reference canvas GUI)
            if (GUILayout.Button("ENTER BLUEPRINT FORGE", GUILayout.Height(52)))
            {
                OpenBlueprintForge();
            }
            GUILayout.Space(6);

            if (GUILayout.Button("HERO ROSTER & LOADOUTS", GUILayout.Height(52)))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.HERO_SELECTION);
            }
            GUILayout.Space(6);

            if (GUILayout.Button("ARCANE CODEX & BESTIARY", GUILayout.Height(52)))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.CODEX);
            }
            GUILayout.Space(6);

            if (GUILayout.Button("BACK TO MAIN MENU", GUILayout.Height(52)))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.MAIN_MENU);
                else HideHub();
            }

            GUILayout.EndArea();
        }
    }
}
