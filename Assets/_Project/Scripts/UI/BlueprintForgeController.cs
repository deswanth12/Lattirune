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

        private void OnGUI()
        {
            if (!isOpen || metaManager == null || blueprintDatabase == null) return;

            DrawForgeWindow();
        }

        private void DrawForgeWindow()
        {
            float modalWidth = 360f;
            float modalHeight = 520f;
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

            GUILayout.Label("BLUEPRINT FORGE", titleStyle);
            GUILayout.Label($"Embers: <b>{metaManager.EmbersBalance}</b> 🔥", GUI.skin.label);
            GUILayout.Space(6);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(360));

            var list = blueprintDatabase.AllBlueprints;
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var bp = list[i];
                    if (bp == null) continue;

                    BlueprintUIState state = GetBlueprintState(bp);
                    bool isSelected = selectedBlueprint == bp;

                    GUILayout.BeginVertical(GUI.skin.box);

                    string stateText = state switch
                    {
                        BlueprintUIState.Unlocked => "<color=green>[UNLOCKED]</color>",
                        BlueprintUIState.Available => "<color=yellow>[AVAILABLE]</color>",
                        BlueprintUIState.InsufficientEmbers => "<color=orange>[NEED EMBERS]</color>",
                        BlueprintUIState.Locked => "<color=grey>[LOCKED]</color>",
                        _ => ""
                    };

                    GUILayout.Label($"<b>{bp.DisplayName}</b> - {bp.EmberCost} Embers {stateText}");
                    GUILayout.Label($"<size=11>{bp.Description}</size>");

                    // Touch target compliant (>= 52 dp)
                    if (isSelected)
                    {
                        GUI.enabled = state == BlueprintUIState.Available && !_isProcessingPurchase;
                        if (GUILayout.Button(state == BlueprintUIState.Unlocked ? "UNLOCKED" : "FORGE BLUEPRINT", GUILayout.Height(52)))
                        {
                            TryPurchaseSelectedBlueprint();
                        }
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (GUILayout.Button("INSPECT", GUILayout.Height(52)))
                        {
                            SelectBlueprint(bp);
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.Space(8);

            if (GUILayout.Button("CLOSE FORGE", GUILayout.Height(52)))
            {
                CloseForge();
            }

            GUILayout.EndArea();
        }
    }
}
