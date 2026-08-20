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
            if (navigation == null || navigation.CurrentScreen != ScreenState.CAMPFIRE_HUB) return;
            if (!isHubVisible || metaManager == null) return;

            // If Forge is currently open, let Forge render its modal window
            if (forgeController != null && forgeController.IsOpen)
            {
                return;
            }

            DrawHubWindow();
        }

        private void DrawHubWindow()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 920f;
            float panelHeight = 1200f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "CAMPFIRE META-HUB");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 50, panelWidth - 80, panelHeight - 100));

            LattiruneUITheme.DrawHeader("CAMPFIRE META-HUB", "Forge blueprints, review stats, and manage permanent progression.");
            GUILayout.Space(16);

            GUIStyle statStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            statStyle.fontSize = 18;
            statStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;

            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUILayout.Label($"Persistent Embers: <b>{DisplayedEmbers}</b>", statStyle);
            GUILayout.Label($"Blueprints Unlocked: <b>{UnlockedBlueprintCount} / {TotalBlueprintCount}</b>", statStyle);
            GUILayout.Label($"Runs Attempted: <b>{metaManager.TotalRunsAttempted}</b> | Boss Clears: <b>{metaManager.TotalBossClears}</b>", statStyle);
            GUILayout.EndVertical();

            GUILayout.Space(24);

            if (LattiruneUITheme.DrawPrimaryButton("ENTER BLUEPRINT FORGE", 75f))
            {
                OpenBlueprintForge();
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton("HERO ROSTER & LOADOUTS", 65f))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.HERO_SELECTION);
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton("ARCANE CODEX & BESTIARY", 65f))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.CODEX);
            }
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton("RETURN TO MAIN MENU", 65f))
            {
                if (navigation != null) navigation.NavigateTo(ScreenState.MAIN_MENU);
                else HideHub();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
