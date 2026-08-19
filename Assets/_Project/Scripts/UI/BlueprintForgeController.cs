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
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (1920f - panelHeight) * 0.5f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.96f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 36;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(1f, 0.55f, 0.1f); // Magma Amber

            GUILayout.Label("⚒️ BLUEPRINT FORGE ⚒️", titleStyle);
            GUILayout.Space(8);

            GUIStyle emberStyle = new GUIStyle(GUI.skin.label);
            emberStyle.fontSize = 22;
            emberStyle.fontStyle = FontStyle.Bold;
            emberStyle.alignment = TextAnchor.MiddleCenter;
            emberStyle.normal.textColor = Color.yellow;
            GUILayout.Label($"Dungeon Embers: <b>{metaManager.EmbersBalance}</b> 🔥", emberStyle);
            GUILayout.Space(16);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(1150));

            var list = blueprintDatabase.AllBlueprints;
            if (list != null)
            {
                GUIStyle cardBoxStyle = new GUIStyle(GUI.skin.box);
                GUIStyle cardTitleStyle = new GUIStyle(GUI.skin.label);
                cardTitleStyle.fontSize = 22;
                cardTitleStyle.fontStyle = FontStyle.Bold;
                cardTitleStyle.normal.textColor = Color.white;

                GUIStyle descStyle = new GUIStyle(GUI.skin.label);
                descStyle.fontSize = 18;
                descStyle.wordWrap = true;
                descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

                GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                btnStyle.fontSize = 20;
                btnStyle.fontStyle = FontStyle.Bold;

                for (int i = 0; i < list.Count; i++)
                {
                    var bp = list[i];
                    if (bp == null) continue;

                    BlueprintUIState state = GetBlueprintState(bp);
                    bool isSelected = selectedBlueprint == bp;

                    GUILayout.BeginVertical(cardBoxStyle);

                    string stateText = state switch
                    {
                        BlueprintUIState.Unlocked => "<color=#4ade80>[UNLOCKED]</color>",
                        BlueprintUIState.Available => "<color=#facc15>[AVAILABLE]</color>",
                        BlueprintUIState.InsufficientEmbers => "<color=#fb923c>[NEED EMBERS]</color>",
                        BlueprintUIState.Locked => "<color=#94a3b8>[LOCKED]</color>",
                        _ => ""
                    };

                    GUILayout.Label($"<b>{bp.DisplayName}</b> — {bp.EmberCost} 🔥 {stateText}", cardTitleStyle);
                    GUILayout.Space(4);
                    GUILayout.Label(bp.Description, descStyle);
                    GUILayout.Space(8);

                    // Touch target compliant (>= 52 dp -> 65px)
                    if (isSelected)
                    {
                        GUI.enabled = state == BlueprintUIState.Available && !_isProcessingPurchase;
                        GUI.color = state == BlueprintUIState.Available ? Color.green : Color.white;
                        if (GUILayout.Button(state == BlueprintUIState.Unlocked ? "ALREADY UNLOCKED" : $"FORGE ({bp.EmberCost} EMBERS)", btnStyle, GUILayout.Height(65)))
                        {
                            TryPurchaseSelectedBlueprint();
                        }
                        GUI.color = oldColor;
                        GUI.enabled = true;
                    }
                    else
                    {
                        if (GUILayout.Button("SELECT & INSPECT", btnStyle, GUILayout.Height(65)))
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

            GUIStyle closeBtnStyle = new GUIStyle(GUI.skin.button);
            closeBtnStyle.fontSize = 22;
            closeBtnStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("CLOSE FORGE", closeBtnStyle, GUILayout.Height(65)))
            {
                CloseForge();
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
