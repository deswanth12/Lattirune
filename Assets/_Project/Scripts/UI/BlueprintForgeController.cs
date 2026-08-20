using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for the Blueprint Forge interface.
    /// Dynamically lists canonical blueprints from BlueprintDatabaseSO, computes state,
    /// and executes validated meta-progression purchases via MetaProgressionManager.
    /// </summary>
    public class BlueprintForgeController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MetaProgressionManager metaManager;
        [SerializeField] private BlueprintDatabaseSO blueprintDatabase;

        [Header("State")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private BlueprintDefinitionSO selectedBlueprint = null;

        private Vector2 _scrollPosition = Vector2.zero;
        private bool _isProcessingPurchase = false;

        public event Action OnForgeOpened;
        public event Action OnForgeClosed;
        public event Action<BlueprintDefinitionSO> OnBlueprintSelected;
        public event Action<BlueprintDefinitionSO> OnBlueprintPurchased;

        public MetaProgressionManager MetaManager => metaManager;
        public BlueprintDatabaseSO Database => blueprintDatabase;
        public bool IsOpen => isOpen;
        public BlueprintDefinitionSO SelectedBlueprint => selectedBlueprint;

        public void Initialize(MetaProgressionManager meta, BlueprintDatabaseSO db = null)
        {
            metaManager = meta;
            blueprintDatabase = db ?? (meta != null ? meta.Database : BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase());
            isOpen = false;
            selectedBlueprint = null;
            _isProcessingPurchase = false;

            if (metaManager != null)
            {
                metaManager.OnBlueprintUnlocked += HandleBlueprintUnlocked;
            }
        }

        private void OnDestroy()
        {
            if (metaManager != null)
            {
                metaManager.OnBlueprintUnlocked -= HandleBlueprintUnlocked;
            }
        }

        public void OpenForge()
        {
            isOpen = true;
            _isProcessingPurchase = false;
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            HapticFeedback.Trigger(HapticFeedbackType.Selection);
            OnForgeOpened?.Invoke();
        }

        public void CloseForge()
        {
            isOpen = false;
            selectedBlueprint = null;
            _isProcessingPurchase = false;
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            OnForgeClosed?.Invoke();
        }

        public void SelectBlueprint(BlueprintDefinitionSO bp)
        {
            selectedBlueprint = bp;
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.ItemPickup);
            HapticFeedback.Trigger(HapticFeedbackType.Selection);
            OnBlueprintSelected?.Invoke(selectedBlueprint);
        }

        public BlueprintUIState GetBlueprintState(BlueprintDefinitionSO bp)
        {
            if (bp == null || metaManager == null) return BlueprintUIState.Locked;

            if (metaManager.IsBlueprintUnlocked(bp.BlueprintId))
            {
                return BlueprintUIState.Unlocked;
            }

            if (bp.HasPrerequisite && !metaManager.IsBlueprintUnlocked(bp.PrerequisiteBlueprintId))
            {
                return BlueprintUIState.Locked;
            }

            if (!metaManager.CanAfford(bp.EmberCost))
            {
                return BlueprintUIState.InsufficientEmbers;
            }

            return BlueprintUIState.Available;
        }

        public bool TryPurchaseSelectedBlueprint()
        {
            if (_isProcessingPurchase || selectedBlueprint == null || metaManager == null)
            {
                return false;
            }

            BlueprintUIState state = GetBlueprintState(selectedBlueprint);
            if (state != BlueprintUIState.Available)
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.InvalidPlacement);
                HapticFeedback.Trigger(HapticFeedbackType.Failure);
                return false;
            }

            _isProcessingPurchase = true;
            bool success = metaManager.UnlockBlueprint(selectedBlueprint);
            _isProcessingPurchase = false;

            if (success)
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.ItemPlaced);
                HapticFeedback.Trigger(HapticFeedbackType.Success);
                OnBlueprintPurchased?.Invoke(selectedBlueprint);
            }
            else
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.InvalidPlacement);
                HapticFeedback.Trigger(HapticFeedbackType.Failure);
            }

            return success;
        }

        private void HandleBlueprintUnlocked(BlueprintDefinitionSO bp)
        {
            _isProcessingPurchase = false;
        }

        [SerializeField] private ScreenNavigationController navigation;

        public void BindNavigation(ScreenNavigationController nav)
        {
            navigation = nav;
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.BLUEPRINT_FORGE) return;
            if (!isOpen || metaManager == null || blueprintDatabase == null) return;

            DrawForgeWindow();
        }

        private void DrawForgeWindow()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "BLUEPRINT FORGE");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("BLUEPRINT FORGE", "Unlock permanent runes and weapons using harvested Embers.");
            GUILayout.Space(10);

            LattiruneUITheme.DrawBadge($"Persistent Embers Available: {metaManager.EmbersBalance}", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(14);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(1150));

            var list = blueprintDatabase.AllBlueprints;
            if (list != null)
            {
                GUIStyle cardTitleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                cardTitleStyle.fontSize = 22;
                cardTitleStyle.fontStyle = FontStyle.Bold;
                cardTitleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 17;
                descStyle.wordWrap = true;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;

                for (int i = 0; i < list.Count; i++)
                {
                    var bp = list[i];
                    if (bp == null) continue;

                    BlueprintUIState state = GetBlueprintState(bp);
                    bool isSelected = selectedBlueprint == bp;

                    GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                    string stateText = state switch
                    {
                        BlueprintUIState.Unlocked => "<color=#4ade80>[UNLOCKED]</color>",
                        BlueprintUIState.Available => "<color=#facc15>[AVAILABLE]</color>",
                        BlueprintUIState.InsufficientEmbers => "<color=#fb923c>[NEED EMBERS]</color>",
                        BlueprintUIState.Locked => "<color=#94a3b8>[LOCKED]</color>",
                        _ => ""
                    };

                    GUILayout.Label($"<b>{bp.DisplayName}</b> — Cost: {bp.EmberCost} Embers {stateText}", cardTitleStyle);
                    GUILayout.Space(4);
                    GUILayout.Label(bp.Description, descStyle);
                    GUILayout.Space(8);

                    if (isSelected)
                    {
                        GUI.enabled = state == BlueprintUIState.Available && !_isProcessingPurchase;
                        if (LattiruneUITheme.DrawPrimaryButton(state == BlueprintUIState.Unlocked ? "ALREADY UNLOCKED" : $"FORGE BLUEPRINT ({bp.EmberCost} EMBERS)", 65f))
                        {
                            TryPurchaseSelectedBlueprint();
                        }
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (LattiruneUITheme.DrawSecondaryButton("SELECT & INSPECT", 65f))
                        {
                            SelectBlueprint(bp);
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(8);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.Space(14);

            if (LattiruneUITheme.DrawSecondaryButton("CLOSE FORGE", 65f))
            {
                CloseForge();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
