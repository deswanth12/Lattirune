using System;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for selecting and unlocking Hero Classes.
    /// Strictly adheres to PLAN.md Section 12, 15, and 16.
    /// </summary>
    public class HeroClassSelectionUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HeroClassManager classManager;
        [SerializeField] private MetaProgressionManager metaProgression;
        [SerializeField] private ScreenNavigationController navigation;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private int _selectedPreviewIndex = 0;
        private string _feedbackMessage = "";

        public bool IsVisible => isVisible;

        public void Initialize(
            HeroClassManager heroManager,
            MetaProgressionManager meta,
            ScreenNavigationController nav = null)
        {
            classManager = heroManager;
            metaProgression = meta;
            navigation = nav;
            _selectedPreviewIndex = 0;
            _feedbackMessage = "Choose your Champion for the descent.";

            if (navigation != null)
            {
                navigation.OnScreenChanged += HandleScreenChanged;
            }
        }

        private void OnDestroy()
        {
            if (navigation != null)
            {
                navigation.OnScreenChanged -= HandleScreenChanged;
            }
        }

        private void HandleScreenChanged(ScreenState prev, ScreenState next)
        {
            if (next == ScreenState.HERO_SELECTION)
            {
                Show();
            }
            else if (prev == ScreenState.HERO_SELECTION)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            _feedbackMessage = "Choose your Champion for the descent.";
        }

        public void Hide()
        {
            isVisible = false;
        }

        private void OnGUI()
        {
            if (navigation != null && navigation.CurrentScreen != ScreenState.HERO_SELECTION) return;
            if (!isVisible || classManager == null || classManager.Database == null) return;

            var classes = classManager.Database.AllClasses;
            if (classes == null || classes.Count == 0) return;

            // Responsive scale matrix
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

            // Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 32;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.77f, 0.61f, 0.15f); // Burnished Brass

            GUILayout.Label("⚔ HERO CLASS SELECTION ⚔", titleStyle);
            GUILayout.Space(10);

            // Embers Display
            GUIStyle embersStyle = new GUIStyle(GUI.skin.label);
            embersStyle.fontSize = 22;
            embersStyle.alignment = TextAnchor.MiddleCenter;
            embersStyle.normal.textColor = new Color(1f, 0.45f, 0.15f); // Magma Ember

            int embers = metaProgression != null ? metaProgression.CurrentEmbers : 0;
            GUILayout.Label($"Persistent Embers: {embers} 🔥", embersStyle);
            GUILayout.Space(15);

            // Class Tabs / Selector Row
            GUILayout.BeginHorizontal();
            for (int i = 0; i < classes.Count; i++)
            {
                var def = classes[i];
                if (def == null) continue;

                bool isSelectedPreview = (i == _selectedPreviewIndex);
                bool isUnlocked = classManager.IsClassUnlocked(def.ClassId);
                bool isActiveHero = (def.ClassId == classManager.SelectedClassId);

                string tabText = def.ClassName;
                if (isActiveHero) tabText = $"★ {def.ClassName}";
                else if (!isUnlocked) tabText = $"🔒 {def.ClassName}";

                GUI.color = isSelectedPreview ? Color.yellow : (isUnlocked ? Color.white : Color.gray);
                if (GUILayout.Button(tabText, GUILayout.Height(50)))
                {
                    _selectedPreviewIndex = i;
                }
            }
            GUI.color = oldColor;
            GUILayout.EndHorizontal();

            GUILayout.Space(25);

            // Active Preview Card
            if (_selectedPreviewIndex >= 0 && _selectedPreviewIndex < classes.Count)
            {
                var currentDef = classes[_selectedPreviewIndex];
                bool isUnlocked = classManager.IsClassUnlocked(currentDef.ClassId);
                bool isActive = (currentDef.ClassId == classManager.SelectedClassId);

                GUILayout.BeginVertical(GUI.skin.box);

                GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontSize = 26;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = isActive ? Color.green : (isUnlocked ? Color.white : Color.gray);

                string badge = isActive ? "[CURRENTLY SELECTED]" : (isUnlocked ? "[UNLOCKED]" : $"[LOCKED - {currentDef.EmbersCost} Embers]");
                GUILayout.Label($"{currentDef.ClassName}  {badge}", headerStyle);
                GUILayout.Space(8);

                GUIStyle descStyle = new GUIStyle(GUI.skin.label);
                descStyle.fontSize = 18;
                descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
                GUILayout.Label(currentDef.Description, descStyle);
                GUILayout.Space(14);

                // Stats Matrix
                GUIStyle statStyle = new GUIStyle(GUI.skin.label);
                statStyle.fontSize = 18;
                statStyle.fontStyle = FontStyle.Bold;
                statStyle.normal.textColor = Color.cyan;

                GUILayout.Label($"Base Stats: ❤️ HP: {currentDef.BaseHp}  |  🛡 Armor: {currentDef.BaseArmor}  |  ⚔ Attack: {currentDef.BaseAttack}  |  ⏱ Speed: {currentDef.AttackInterval:0.0}s", statStyle);
                GUILayout.Space(14);

                // Starting Loadout
                GUIStyle loadoutStyle = new GUIStyle(GUI.skin.label);
                loadoutStyle.fontSize = 16;
                loadoutStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

                string itemsList = string.Join(", ", currentDef.StartingItemIds);
                string runesList = string.Join(", ", currentDef.StartingRuneIds);
                GUILayout.Label($"Starting Gear: {itemsList}", loadoutStyle);
                GUILayout.Label($"Starting Runes: {runesList}", loadoutStyle);

                GUILayout.Space(20);

                // Action Buttons
                if (!isUnlocked)
                {
                    bool canAfford = metaProgression != null && metaProgression.CurrentEmbers >= currentDef.EmbersCost;
                    GUI.enabled = canAfford;

                    if (GUILayout.Button($"🔥 UNLOCK CLASS ({currentDef.EmbersCost} Embers)", GUILayout.Height(60)))
                    {
                        if (classManager.UnlockClass(currentDef.ClassId, metaProgression))
                        {
                            classManager.SelectClass(currentDef.ClassId);
                            _feedbackMessage = $"Unlocked and selected {currentDef.ClassName}!";
                        }
                    }

                    GUI.enabled = true;
                }
                else if (!isActive)
                {
                    if (GUILayout.Button("SELECT THIS HERO", GUILayout.Height(60)))
                    {
                        classManager.SelectClass(currentDef.ClassId);
                        _feedbackMessage = $"Selected {currentDef.ClassName} as active champion!";
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("★ ACTIVE CHAMPION SELECTED", GUILayout.Height(60));
                    GUI.enabled = true;
                }

                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            // Feedback Dialogue
            GUIStyle feedbackStyle = new GUIStyle(GUI.skin.label);
            feedbackStyle.fontSize = 18;
            feedbackStyle.fontStyle = FontStyle.Italic;
            feedbackStyle.alignment = TextAnchor.MiddleCenter;
            feedbackStyle.normal.textColor = Color.yellow;
            GUILayout.Label(_feedbackMessage, feedbackStyle);
            GUILayout.Space(15);

            // Action / Navigation Buttons
            GUILayout.BeginHorizontal();

            GUI.color = Color.green;
            if (GUILayout.Button("DESCEND INTO DUNGEON ➔", GUILayout.Height(65)))
            {
                Hide();
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.RUN_START);
                }
            }
            GUI.color = oldColor;

            GUILayout.Space(12);

            if (GUILayout.Button("RETURN", GUILayout.Height(65), GUILayout.Width(240)))
            {
                Hide();
                if (navigation != null)
                {
                    navigation.NavigateBack();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
