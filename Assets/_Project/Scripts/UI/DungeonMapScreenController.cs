using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Core;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for the 10-Floor Dungeon Map DAG and branch path selection.
    /// Displays crisp vector-quality node icons, room level tags, and authentic background (0 emoji, 0 placeholders).
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

        public void ResetMapForNewRun()
        {
            _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            _selectedNodeId = "node_f1_entry";
        }

        public void Show()
        {
            isVisible = true;
            if (_mapGraph == null)
            {
                _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            }
            var available = _mapGraph.GetAvailableNodes();
            if (available.Count > 0)
            {
                var curr = _mapGraph.GetNode(_selectedNodeId);
                if (curr == null || !curr.IsAvailable || curr.IsCleared)
                {
                    _selectedNodeId = available[0].NodeId;
                }
            }
        }

        public void Hide()
        {
            isVisible = false;
        }

                private void OnGUI()
        {
            if (navigation == null || (navigation.CurrentScreen != ScreenState.DUNGEON_MAP && navigation.CurrentScreen != ScreenState.RUN_START)) return;
            if (_mapGraph == null)
            {
                _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            }

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
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(850));

            for (int f = 1; f <= 10; f++)
            {
                var floorNodes = _mapGraph.GetNodesOnFloor(f);
                if (floorNodes.Count == 0) continue;

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUIStyle floorHeader = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                floorHeader.fontSize = 18;
                floorHeader.fontStyle = FontStyle.Bold;
                floorHeader.normal.textColor = (f == floorNum) ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorTextMuted;
                
                string floorTag = (f == 10) ? "FLOOR 10 — BOSS LAIR" : (f == 5 ? "FLOOR 5 — MID-BOSS LAIR" : $"FLOOR {f}");
                GUILayout.Label(floorTag, floorHeader);
                GUILayout.Space(6);

                foreach (var node in floorNodes)
                {
                    bool isSelected = (node.NodeId == _selectedNodeId);
                    bool isAvailable = node.IsAvailable && !node.IsCleared;

                    GUILayout.BeginHorizontal();

                    // Room Icon
                    Texture2D nodeIcon = GetNodeIcon(node.NodeType);
                    if (nodeIcon != null)
                    {
                        Rect iconRect = GUILayoutUtility.GetRect(48f, 48f, GUILayout.Width(48f), GUILayout.Height(48f));
                        GUI.DrawTexture(iconRect, nodeIcon, ScaleMode.ScaleToFit);
                        GUILayout.Space(10);
                    }

                    string statusTag = node.IsCleared ? "[CLEARED]" : (isAvailable ? "[AVAILABLE]" : "[LOCKED]");
                    string nodeTitle = $"{statusTag} {node.Title.ToUpper()}";

                    if (node.IsCleared)
                    {
                        GUI.enabled = false;
                        LattiruneUITheme.DrawSecondaryButton(nodeTitle, 52f);
                        GUI.enabled = true;
                    }
                    else if (isAvailable)
                    {
                        if (isSelected)
                        {
                            if (LattiruneUITheme.DrawPrimaryButton($">> {nodeTitle} <<", 55f))
                            {
                                _selectedNodeId = node.NodeId;
                            }
                        }
                        else
                        {
                            if (LattiruneUITheme.DrawSecondaryButton(nodeTitle, 52f))
                            {
                                _selectedNodeId = node.NodeId;
                            }
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        LattiruneUITheme.DrawSecondaryButton(nodeTitle, 52f);
                        GUI.enabled = true;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(6);
                }

                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(14);

            // Active Room Preview & Launch Button
            var selectedNode = _mapGraph.GetNode(_selectedNodeId);
            if (selectedNode != null && selectedNode.IsAvailable && !selectedNode.IsCleared)
            {
                if (LattiruneUITheme.DrawPrimaryButton($"ENTER {selectedNode.Title.ToUpper()} & BEGIN", 80f))
                {
                    EnterSelectedNode(selectedNode);
                }
            }
            else
            {
                GUI.enabled = false;
                LattiruneUITheme.DrawSecondaryButton("SELECT AN ACTIVE ROOM TO PROCEED", 80f);
                GUI.enabled = true;
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private Texture2D GetNodeIcon(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.NormalBattle: return VisualAssetProvider.GetUIIcon("ui_icon_battle");
                case DungeonMapNodeType.EliteBattle: return VisualAssetProvider.GetUIIcon("ui_icon_elite");
                case DungeonMapNodeType.MerchantStall: return VisualAssetProvider.GetUIIcon("ui_icon_merchant");
                case DungeonMapNodeType.CampfireRest: return VisualAssetProvider.GetUIIcon("ui_icon_campfire");
                case DungeonMapNodeType.MysteryShrine: return VisualAssetProvider.GetUIIcon("ui_icon_event");
                case DungeonMapNodeType.TreasureVault: return VisualAssetProvider.GetUIIcon("ui_icon_event");
                case DungeonMapNodeType.Boss: return VisualAssetProvider.GetUIIcon("ui_icon_boss");
                default: return VisualAssetProvider.GetUIIcon("ui_icon_battle");
            }
        }

        private void EnterSelectedNode(DungeonMapNode node)
        {
            if (node == null || runManager == null) return;

            _mapGraph.SelectAndEnterNode(node.NodeId);
            runManager.SetCurrentFloor(node.FloorNumber - 1);

            switch (node.NodeType)
            {
                case DungeonMapNodeType.NormalBattle:
                case DungeonMapNodeType.EliteBattle:
                case DungeonMapNodeType.Boss:
                    navigation?.NavigateTo(ScreenState.GRID_BUILD);
                    break;
                case DungeonMapNodeType.MerchantStall:
                    navigation?.NavigateTo(ScreenState.MERCHANT);
                    break;
                case DungeonMapNodeType.CampfireRest:
                    navigation?.NavigateTo(ScreenState.CAMPFIRE_REST);
                    break;
                case DungeonMapNodeType.MysteryShrine:
                case DungeonMapNodeType.TreasureVault:
                    navigation?.NavigateTo(ScreenState.EVENT);
                    break;
            }
        }
    }
}
