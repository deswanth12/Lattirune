using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Progression;

namespace Lattirune.UI
{
    public enum CodexTab
    {
        Bestiary,
        SynergiesAndReactions
    }

    /// <summary>
    /// Mobile portrait UI Controller for the Bestiary and Master Synergy / Reaction Codex.
    /// Strictly adheres to PLAN.md Section 7, 10, 12, 15, and 16.
    /// </summary>
    public class CodexUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CodexManager codexManager;
        [SerializeField] private ScreenNavigationController navigation;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private CodexTab _currentTab = CodexTab.Bestiary;
        private Vector2 _scrollPos = Vector2.zero;

        public bool IsVisible => isVisible;

        public void Initialize(
            CodexManager codex,
            ScreenNavigationController nav = null)
        {
            codexManager = codex;
            navigation = nav;

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
            if (next == ScreenState.CODEX)
            {
                Show();
            }
            else if (prev == ScreenState.CODEX)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            _scrollPos = Vector2.zero;
        }

        public void Hide()
        {
            isVisible = false;
        }

        private void OnGUI()
        {
            if (!isVisible || codexManager == null) return;

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

            GUILayout.Label(""📜 ARCANE CODEX & BESTIARY 📜"", titleStyle);
            GUILayout.Space(12);

            // Tab Selector Row
            GUILayout.BeginHorizontal();
            
            int totalEnemies = codexManager.Bestiary != null ? codexManager.Bestiary.TotalCount : 7;
            int discoveredCount = codexManager.DiscoveredEnemies.Count;

            GUI.color = (_currentTab == CodexTab.Bestiary) ? Color.yellow : Color.white;
            if (GUILayout.Button($""💀 BESTIARY ({discoveredCount}/{totalEnemies})"", GUILayout.Height(55)))
            {
                _currentTab = CodexTab.Bestiary;
                _scrollPos = Vector2.zero;
            }

            GUI.color = (_currentTab == CodexTab.SynergiesAndReactions) ? Color.yellow : Color.white;
            if (GUILayout.Button(""⚡ SYNERGIES & REACTIONS"", GUILayout.Height(55)))
            {
                _currentTab = CodexTab.SynergiesAndReactions;
                _scrollPos = Vector2.zero;
            }

            GUI.color = oldColor;
            GUILayout.EndHorizontal();

            GUILayout.Space(18);

            // Scrollable Content
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(1050));

            if (_currentTab == CodexTab.Bestiary)
            {
                RenderBestiaryTab();
            }
            else
            {
                RenderSynergiesTab();
            }

            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            // Return / Close Button
            if (GUILayout.Button(""RETURN TO CAMPFIRE HUB"", GUILayout.Height(65)))
            {
                Hide();
                if (navigation != null)
                {
                    navigation.NavigateBack();
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private void RenderBestiaryTab()
        {
            var db = codexManager.Bestiary;
            if (db == null) return;

            foreach (var enemy in db.AllEntries)
            {
                if (enemy == null) continue;

                bool isDiscovered = codexManager.IsEnemyDiscovered(enemy.EnemyId);
                int killCount = codexManager.GetEnemyKillCount(enemy.EnemyId);

                GUILayout.BeginVertical(GUI.skin.box);

                GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.fontSize = 22;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = isDiscovered ? (enemy.Tier == EnemyTier.Boss ? Color.red : (enemy.Tier == EnemyTier.Elite ? Color.yellow : Color.white)) : Color.gray;

                string headerText = isDiscovered ? $"{enemy.EnemyName} [{enemy.Tier}]  (Slain: {killCount}x)" : "??? Undiscovered Creature";
                GUILayout.Label(headerText, headerStyle);

                if (isDiscovered)
                {
                    GUIStyle descStyle = new GUIStyle(GUI.skin.label);
                    descStyle.fontSize = 16;
                    descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
                    GUILayout.Label(enemy.Description, descStyle);

                    GUIStyle statStyle = new GUIStyle(GUI.skin.label);
                    statStyle.fontSize = 16;
                    statStyle.fontStyle = FontStyle.Bold;
                    statStyle.normal.textColor = Color.cyan;
                    GUILayout.Label($"Base Stats: ❤️ HP: {enemy.BaseHp}  |  🛡 DEF: {enemy.BaseArmor}  |  ⚔ ATK: {enemy.BaseAttack}  |  ⏱ Speed: {enemy.AttackSpeed:0.0}s", statStyle);

                    GUIStyle stratStyle = new GUIStyle(GUI.skin.label);
                    stratStyle.fontSize = 15;
                    stratStyle.normal.textColor = new Color(1f, 0.7f, 0.3f);
                    GUILayout.Label($"Mechanic: {enemy.UniqueMechanic}", stratStyle);
                    GUILayout.Label($"Counter: {enemy.CounterStrategy}", stratStyle);
                }
                else
                {
                    GUIStyle unknownStyle = new GUIStyle(GUI.skin.label);
                    unknownStyle.fontSize = 16;
                    unknownStyle.fontStyle = FontStyle.Italic;
                    unknownStyle.normal.textColor = Color.gray;
                    GUILayout.Label("Encounter this creature in the depths of the Cursed Sewers to unlock its combat telemetry and strategic weaknesses.", unknownStyle);
                }

                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
        }

        private void RenderSynergiesTab()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUIStyle sectionTitle = new GUIStyle(GUI.skin.label);
            sectionTitle.fontSize = 22;
            sectionTitle.fontStyle = FontStyle.Bold;
            sectionTitle.normal.textColor = Color.yellow;
            GUILayout.Label("── 5 MASTER ELEMENTAL REACTIONS (PLAN.md Section 7) ──", sectionTitle);
            GUILayout.Space(6);

            RenderReactionEntry("Steam", "Fire Beam + Ice Beam", "Blinds enemy, giving 25% miss chance.");
            RenderReactionEntry("Plasma", "Fire Beam + Lightning Beam", "Unleashes 18 DMG/s continuous burning laser ray.");
            RenderReactionEntry("Toxic Flame", "Fire Beam + Poison Beam", "Detonates all active poison stacks for 2x burst damage.");
            RenderReactionEntry("Superconductor", "Lightning Beam + Ice Beam", "Shreds -40% enemy armor and magic resistance.");
            RenderReactionEntry("Frostbite", "Ice Beam + Poison Beam", "Increases poison damage per tick by +50%.");

            GUILayout.EndVertical();

            GUILayout.Space(14);

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("── 5 MASTER ITEM SYNERGIES ──", sectionTitle);
            GUILayout.Space(6);

            RenderReactionEntry("Flaming Blade", "Ember Rune (→) + Iron Broadsword", "Adds +6 Fire Damage and inflicts Burn (3 dmg/s for 4s).");
            RenderReactionEntry("Venom Shiv", "Venom Rune (←) + Rusty Dagger", "Inflicts 2 Poison stacks on every swift 0.8s strike.");
            RenderReactionEntry("Thunder Bow", "Spark Rune (↑) + Shortbow", "Arrows chain 8 Lightning damage to additional enemy adds.");
            RenderReactionEntry("Molten Wall", "Ember Rune (↓) + Tower Shield", "Attackers take 8 Burn damage upon striking the shield.");
            RenderReactionEntry("Shatterstrike", "Frost Rune (↓) + Battleaxe", "Deals 2x damage against chilled or frozen enemies.");

            GUILayout.EndVertical();
        }

        private void RenderReactionEntry(string title, string formula, string effect)
        {
            GUIStyle tStyle = new GUIStyle(GUI.skin.label);
            tStyle.fontSize = 18;
            tStyle.fontStyle = FontStyle.Bold;
            tStyle.normal.textColor = Color.white;

            GUIStyle fStyle = new GUIStyle(GUI.skin.label);
            fStyle.fontSize = 16;
            fStyle.normal.textColor = Color.cyan;

            GUIStyle eStyle = new GUIStyle(GUI.skin.label);
            eStyle.fontSize = 15;
            eStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            GUILayout.Label($"✨ {title}: {formula}", tStyle);
            GUILayout.Label($"   Effect: {effect}", eStyle);
            GUILayout.Space(6);
        }
    }
}
