using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Progression;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Dark Fantasy Dungeon Map DAG Screen Controller.
    /// Visualizes 10 dungeon floors, node branching, room clearance states,
    /// and pulsing golden beacons for active accessible rooms.
    /// </summary>
    public class DungeonMapScreenController : MonoBehaviour
    {
        public void ResetMapForNewRun() { _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap(); }
        public DungeonMapGraph MapGraph => _mapGraph;
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;
        [SerializeField] private CombatEncounterUI combatUI;

        private DungeonMapGraph _mapGraph;
        private Vector2 _scrollPos = Vector2.zero;
        private string _selectedNodeId = null;

        public void Initialize(RunManager run, ScreenNavigationController nav, DungeonMapGraph map)
        {
            navigation = nav;
            runManager = run;
            _mapGraph = map ?? DungeonMapGraph.CreateCanonicalCursedSewersMap();
            combatUI = FindFirstObjectByType<CombatEncounterUI>();
        }

        public void Initialize(RunManager run, ScreenNavigationController nav, object map)
        {
            if (map is DungeonMapGraph dmg)
            {
                Initialize(run, nav, dmg);
            }
            else
            {
                Initialize(nav, run);
            }
        }

        public void Initialize(RunManager run, ScreenNavigationController nav)
        {
            Initialize(nav, run);
        }

        public void Initialize(ScreenNavigationController nav, RunManager run, CombatEncounterUI combat = null)
        {
            navigation = nav;
            runManager = run;
            combatUI = combat ?? FindFirstObjectByType<CombatEncounterUI>();
            if (_mapGraph == null) _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
        }

        public void SelectNode(string nodeId)
        {
            _selectedNodeId = nodeId;
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);
        }

        public void EnterSelectedNode()
        {
            if (string.IsNullOrEmpty(_selectedNodeId) && _mapGraph != null)
            {
                var avail = _mapGraph.GetAvailableNodes();
                if (avail.Count > 0) _selectedNodeId = avail[0].NodeId;
            }

            if (string.IsNullOrEmpty(_selectedNodeId)) return;

            AudioController.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
            JuiceController.Instance?.TriggerHaptic(HapticType.Medium);

            var node = _mapGraph.GetNode(_selectedNodeId);
            if (node != null)
            {
                _mapGraph.SelectAndEnterNode(_selectedNodeId);
                _mapGraph.CompleteCurrentNode();

                if (node.NodeType == DungeonMapNodeType.MerchantStall)
                {
                    if (navigation != null) navigation.NavigateTo(ScreenState.MERCHANT);
                }
                else if (node.NodeType == DungeonMapNodeType.CampfireRest)
                {
                    if (navigation != null) navigation.NavigateTo(ScreenState.CAMPFIRE_REST);
                }
                else if (node.NodeType == DungeonMapNodeType.MysteryShrine)
                {
                    if (navigation != null) navigation.NavigateTo(ScreenState.EVENT);
                }
                else
                {
                    // Combat Encounter (Normal, Elite, Boss)
                    if (combatUI == null) combatUI = FindFirstObjectByType<CombatEncounterUI>();
                    if (combatUI != null)
                    {
                        bool isBoss = (node.NodeType == DungeonMapNodeType.Boss);
                        bool isElite = (node.NodeType == DungeonMapNodeType.EliteBattle);
                        int hp = isBoss ? 150 : (isElite ? 80 : 35);
                        int atk = isBoss ? 12 : (isElite ? 7 : 3);
                        int armor = isBoss ? 5 : (isElite ? 2 : 0);
                        string enemy = isBoss ? "The Lich Lord" : (isElite ? "Armored Skeleton" : "Sewer Rat");
                        
                        combatUI.SetupEncounter(node.FloorNumber, enemy, hp, atk, armor, isBoss, 1);
                    }

                    if (navigation != null) navigation.NavigateTo(ScreenState.COMBAT);
                }
            }
        }

        private void OnGUI()
        {
            if (navigation == null || (navigation.CurrentScreen != ScreenState.DUNGEON_MAP && navigation.CurrentScreen != ScreenState.RUN_START)) return;
            if (_mapGraph == null)
            {
                _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            }

            DrawDungeonMap();
        }

        private void DrawDungeonMap()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 980f;
            float panelHeight = 1750f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = 80f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "THE CURSED SEWERS");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("THE CURSED SEWERS", "Select an active room node to advance your descent.");
            GUILayout.Space(8);

            int curFloor = runManager != null ? runManager.CurrentFloorNumber : 1;
            int gold = runManager != null ? runManager.CurrentGold : 0;
            LattiruneUITheme.DrawBadge($"Floor: {curFloor} / 10  |  Gold: {gold}g  |  Target: The Lich Lord", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(16);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(panelHeight - 280));

            var allNodes = _mapGraph.AllNodes;
            int totalFloors = 10;

            for (int f = 1; f <= totalFloors; f++)
            {
                int floorNum = f;
                var floorNodes = _mapGraph.GetNodesOnFloor(floorNum);
                if (floorNodes.Count == 0) continue;

                string floorHeader = (floorNum == 5) ? "FLOOR 5 — MID-BOSS LAIR" : ((floorNum == 10) ? "FLOOR 10 — LICH LORD'S CRYPT" : $"FLOOR {floorNum}");
                LattiruneUITheme.DrawSectionTitle(floorHeader);

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                foreach (var node in floorNodes)
                {
                    bool isAvailable = node.IsAvailable;
                    bool isCleared = node.IsCleared;
                    bool isSelected = (_selectedNodeId == node.NodeId || (string.IsNullOrEmpty(_selectedNodeId) && isAvailable));

                    GUILayout.BeginHorizontal();

                    // Room Type Icon Badge
                    Texture2D roomIcon = GetRoomIcon(node.NodeType);
                    if (roomIcon != null)
                    {
                        Rect iconRect = GUILayoutUtility.GetRect(40f, 40f, GUILayout.Width(40f), GUILayout.Height(40f));
                        GUI.DrawTexture(iconRect, roomIcon, ScaleMode.ScaleToFit);
                        GUILayout.Space(10);
                    }

                    string statusText = isCleared ? $"[CLEARED] {node.Title.ToUpper()}" : (isAvailable ? $">> [AVAILABLE] {node.Title.ToUpper()} <<" : $"[LOCKED] {node.Title.ToUpper()}");

                    if (isCleared)
                    {
                        GUIStyle clearedStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                        clearedStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                        clearedStyle.fontSize = 17;
                        GUILayout.Label(statusText, clearedStyle, GUILayout.Height(45f));
                    }
                    else if (isAvailable)
                    {
                        if (LattiruneUITheme.DrawPrimaryButton(statusText, 55f))
                        {
                            SelectNode(node.NodeId);
                            EnterSelectedNode();
                        }
                    }
                    else
                    {
                        GUIStyle lockedStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                        lockedStyle.normal.textColor = new Color(0.4f, 0.45f, 0.55f);
                        lockedStyle.fontSize = 16;
                        GUILayout.Label(statusText, lockedStyle, GUILayout.Height(40f));
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(6);
                }

                GUILayout.EndVertical();
                GUILayout.Space(14);
            }

            GUILayout.EndScrollView();
            GUILayout.Space(16);

            // Action Button
            var availNodes = _mapGraph.GetAvailableNodes();
            if (availNodes.Count > 0)
            {
                var activeNode = !string.IsNullOrEmpty(_selectedNodeId) ? _mapGraph.GetNode(_selectedNodeId) : availNodes[0];
                string btnLabel = activeNode != null ? $"ENTER {activeNode.Title.ToUpper()} & BEGIN" : "ENTER NEXT ROOM";

                if (LattiruneUITheme.DrawPrimaryButton(btnLabel, 75f))
                {
                    EnterSelectedNode();
                }
            }
            else
            {
                if (LattiruneUITheme.DrawSecondaryButton("DESCENT COMPLETE — ASCEND TO SURFACE", 75f))
                {
                    if (navigation != null) navigation.NavigateTo(ScreenState.VICTORY);
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private Texture2D GetRoomIcon(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.Boss:
                    return VisualAssetProvider.GetUIIcon("ui_icon_boss");
                case DungeonMapNodeType.EliteBattle:
                    return VisualAssetProvider.GetUIIcon("ui_icon_elite");
                case DungeonMapNodeType.MerchantStall:
                    return VisualAssetProvider.GetUIIcon("ui_icon_merchant");
                case DungeonMapNodeType.CampfireRest:
                    return VisualAssetProvider.GetUIIcon("ui_icon_campfire");
                case DungeonMapNodeType.MysteryShrine:
                    return VisualAssetProvider.GetUIIcon("ui_icon_event");
                default:
                    return VisualAssetProvider.GetUIIcon("ui_icon_battle");
            }
        }
    }
}
