using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for the 10-Floor Dungeon Map DAG and branch path selection.
    /// Strictly adheres to PLAN.md Section 2 (Step 1: Explore) and Section 11.
    /// </summary>
    public class DungeonMapScreenController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RunManager runManager;
        [SerializeField] private ScreenNavigationController navigation;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private DungeonMapGraph _mapGraph;
        private string _selectedNodeId = "node_f1_entry";
        private Vector2 _scrollPos = Vector2.zero;

        public bool IsVisible => isVisible;
        public DungeonMapGraph MapGraph => _mapGraph;

        public void Initialize(
            RunManager run,
            ScreenNavigationController nav = null,
            DungeonMapGraph graph = null)
        {
            runManager = run;
            navigation = nav;
            _mapGraph = graph ?? DungeonMapGraph.CreateCanonicalCursedSewersMap();
            _selectedNodeId = _mapGraph.CurrentNodeId;

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
            if (next == ScreenState.RUN_START)
            {
                Show();
            }
            else if (prev == ScreenState.RUN_START)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            var available = _mapGraph.GetAvailableNodes();
            if (available.Count > 0)
            {
                _selectedNodeId = available[0].NodeId;
            }
        }

        public void Hide()
        {
            isVisible = false;
        }

        private void OnGUI()
        {
            if (navigation != null && navigation.CurrentScreen != ScreenState.RUN_START) return;
            if (!isVisible || _mapGraph == null) return;

            // Responsive scale matrix
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

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

            GUILayout.Label("🗺 DUNGEON MAP: THE CURSED SEWERS 🗺", titleStyle);
            GUILayout.Space(8);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 18;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.gray;
            GUILayout.Label("Select an available room to advance your descent.", subStyle);
            GUILayout.Space(14);

            // Scrollable Map Nodes List (Floors 1 to 10)
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(850));

            for (int f = 1; f <= 10; f++)
            {
                var floorNodes = _mapGraph.GetNodesOnFloor(f);
                if (floorNodes.Count == 0) continue;

                GUILayout.BeginVertical(GUI.skin.box);

                GUIStyle floorHeader = new GUIStyle(GUI.skin.label);
                floorHeader.fontSize = 20;
                floorHeader.fontStyle = FontStyle.Bold;
                floorHeader.normal.textColor = Color.yellow;
                GUILayout.Label($"── FLOOR {f} ──", floorHeader);

                GUILayout.BeginHorizontal();
                foreach (var node in floorNodes)
                {
                    bool isSelected = (node.NodeId == _selectedNodeId);
                    bool isAvailable = node.IsAvailable && !node.IsCleared;
                    bool isCleared = node.IsCleared;

                    string badge = GetNodeBadge(node.NodeType);
                    string statusIcon = isCleared ? "✓ " : (isAvailable ? "► " : "🔒 ");
                    string btnText = $"{statusIcon}{badge}\n{node.Title}";

                    if (isCleared) GUI.color = new Color(0.4f, 0.8f, 0.4f);
                    else if (isAvailable) GUI.color = isSelected ? Color.yellow : Color.cyan;
                    else GUI.color = Color.gray;

                    GUI.enabled = isAvailable || isCleared;
                    if (GUILayout.Button(btnText, GUILayout.Height(65), GUILayout.MinWidth(220)))
                    {
                        _selectedNodeId = node.NodeId;
                    }
                    GUI.enabled = true;
                }
                GUI.color = oldColor;
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(8);
            }

            GUILayout.EndScrollView();

            GUILayout.Space(15);

            // Selected Node Detail Card
            var selectedNode = _mapGraph.GetNode(_selectedNodeId);
            if (selectedNode != null)
            {
                GUILayout.BeginVertical(GUI.skin.box);

                GUIStyle detailTitle = new GUIStyle(GUI.skin.label);
                detailTitle.fontSize = 22;
                detailTitle.fontStyle = FontStyle.Bold;
                detailTitle.normal.textColor = selectedNode.IsAvailable ? Color.white : Color.gray;

                GUILayout.Label($"{GetNodeBadge(selectedNode.NodeType)}: {selectedNode.Title}", detailTitle);

                GUIStyle detailDesc = new GUIStyle(GUI.skin.label);
                detailDesc.fontSize = 16;
                detailDesc.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
                GUILayout.Label(selectedNode.Description, detailDesc);

                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            // Enter Selected Room Button
            bool canEnter = selectedNode != null && selectedNode.IsAvailable && !selectedNode.IsCleared;
            GUI.enabled = canEnter;

            GUIStyle enterBtnStyle = new GUIStyle(GUI.skin.button);
            enterBtnStyle.fontSize = 24;
            enterBtnStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("ENTER ROOM & BEGIN", enterBtnStyle, GUILayout.Height(65)))
            {
                if (_mapGraph.SelectAndEnterNode(_selectedNodeId))
                {
                    Hide();
                    if (selectedNode.NodeType == DungeonMapNodeType.MerchantStall)
                    {
                        if (navigation != null) navigation.NavigateTo(ScreenState.MERCHANT);
                    }
                    else if (selectedNode.NodeType == DungeonMapNodeType.CampfireRest)
                    {
                        if (navigation != null) navigation.NavigateTo(ScreenState.CAMPFIRE_REST);
                    }
                    else
                    {
                        if (navigation != null) navigation.NavigateTo(ScreenState.GRID_BUILD);
                    }
                }
            }

            GUI.enabled = true;

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private string GetNodeBadge(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.NormalBattle: return "⚔ BATTLE";
                case DungeonMapNodeType.EliteBattle: return "💀 ELITE";
                case DungeonMapNodeType.MysteryShrine: return "✨ SHRINE";
                case DungeonMapNodeType.MerchantStall: return "🛒 MERCHANT";
                case DungeonMapNodeType.TreasureVault: return "💎 VAULT";
                case DungeonMapNodeType.CampfireRest: return "⛺ REST SITE";
                case DungeonMapNodeType.Boss: return "👑 BOSS SANCTUM";
                default: return "ROOM";
            }
        }
    }
}
