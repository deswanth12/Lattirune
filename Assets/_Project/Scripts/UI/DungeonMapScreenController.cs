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
                if (navigation.CurrentScreen == ScreenState.DUNGEON_MAP || navigation.CurrentScreen == ScreenState.RUN_START)
                {
                    Show();
                }
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
            if (next == ScreenState.DUNGEON_MAP || next == ScreenState.RUN_START)
            {
                Show();
            }
            else if (prev == ScreenState.DUNGEON_MAP || prev == ScreenState.RUN_START)
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
            if (navigation == null || (navigation.CurrentScreen != ScreenState.DUNGEON_MAP && navigation.CurrentScreen != ScreenState.RUN_START)) return;
            if (!isVisible || _mapGraph == null) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "THE CURSED SEWERS — 10-FLOOR DESCENT");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("THE CURSED SEWERS", "Select an active room node to advance your descent.");
            GUILayout.Space(10);

            // Run Telemetry Bar
            int gold = runManager != null ? runManager.CurrentGold : 0;
            int floorNum = runManager != null ? runManager.CurrentFloorNumber : 1;
            LattiruneUITheme.DrawBadge($"Floor: {floorNum} / 10  |  Gold: {gold}g  |  Target: The Lich Lord", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(12);

            // Scrollable Map Nodes List (Floors 1 to 10)
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(800));

            for (int f = 1; f <= 10; f++)
            {
                var floorNodes = _mapGraph.GetNodesOnFloor(f);
                if (floorNodes.Count == 0) continue;

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUIStyle floorHeader = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                floorHeader.fontSize = 18;
                floorHeader.fontStyle = FontStyle.Bold;
                floorHeader.normal.textColor = (f == floorNum) ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorTextMuted;
                
                string floorTag = (f == 10) ? "FLOOR 10 — BOSS LAIR" : $"FLOOR {f}";
                GUILayout.Label(floorTag, floorHeader);
                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                foreach (var node in floorNodes)
                {
                    bool isSelected = (node.NodeId == _selectedNodeId);
                    bool isAvailable = node.IsAvailable && !node.IsCleared;
                    bool isCleared = node.IsCleared;

                    string badge = GetNodeBadge(node.NodeType);
                    string statusIcon = isCleared ? "[CLEARED] " : (isAvailable ? "[ACTIVE] " : "");
                    string btnText = $"{statusIcon}{badge}\n{node.Title}";

                    GUI.enabled = isAvailable || isCleared;
                    bool clicked = false;
                    if (isSelected || isAvailable)
                    {
                        clicked = LattiruneUITheme.DrawPrimaryButton(btnText, 65f);
                    }
                    else
                    {
                        clicked = LattiruneUITheme.DrawSecondaryButton(btnText, 65f);
                    }

                    if (clicked)
                    {
                        _selectedNodeId = node.NodeId;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(8);
            }

            GUILayout.EndScrollView();

            GUILayout.Space(12);

            // Selected Node Detail Card
            var selectedNode = _mapGraph.GetNode(_selectedNodeId);
            if (selectedNode != null)
            {
                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUIStyle detailTitle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                detailTitle.fontSize = 20;
                detailTitle.fontStyle = FontStyle.Bold;
                detailTitle.normal.textColor = selectedNode.IsAvailable ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorTextMuted;

                GUILayout.Label($"{GetNodeBadge(selectedNode.NodeType)}: {selectedNode.Title}", detailTitle);

                GUIStyle detailDesc = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                detailDesc.fontSize = 15;
                detailDesc.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(selectedNode.Description, detailDesc);

                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            // Enter Selected Room Button
            bool canEnter = selectedNode != null && selectedNode.IsAvailable && !selectedNode.IsCleared;
            GUI.enabled = canEnter;

            if (LattiruneUITheme.DrawPrimaryButton("ENTER ROOM & BEGIN", 75f))
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
                case DungeonMapNodeType.NormalBattle: return "BATTLE";
                case DungeonMapNodeType.EliteBattle: return "ELITE";
                case DungeonMapNodeType.Boss: return "BOSS";
                case DungeonMapNodeType.MysteryShrine: return "SHRINE";
                case DungeonMapNodeType.MerchantStall: return "MERCHANT";
                case DungeonMapNodeType.CampfireRest: return "REST";
                case DungeonMapNodeType.TreasureVault: return "VAULT";
                default: return "ROOM";
            }
        }
    }
}
