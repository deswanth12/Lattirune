using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Progression;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Commercial dark-fantasy mobile Dungeon Map screen controller.
    /// Visualizes the 10-floor branching DAG with room cards, risk levels, reward previews,
    /// player location beacons, and native sprite iconography (0 emoji, 0 placeholders).
    /// </summary>
    public class DungeonMapScreenController : MonoBehaviour
    {
        public void ResetMapForNewRun() { _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap(); }
        public DungeonMapGraph MapGraph => _mapGraph;

        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;
        [SerializeField] private CombatEncounterUI combatUI;
        [SerializeField] private MetaProgressionManager metaProgression;
        [SerializeField] private HeroClassManager classManager;

        private DungeonMapGraph _mapGraph;
        private Vector2 _scrollPos = Vector2.zero;
        private string _selectedNodeId = null;

        public void Initialize(RunManager run, ScreenNavigationController nav, DungeonMapGraph map)
        {
            navigation = nav;
            runManager = run;
            _mapGraph = map ?? DungeonMapGraph.CreateCanonicalCursedSewersMap();
            combatUI = FindFirstObjectByType<CombatEncounterUI>();
            metaProgression = FindFirstObjectByType<MetaProgressionManager>();
            classManager = FindFirstObjectByType<HeroClassManager>();
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
            metaProgression = FindFirstObjectByType<MetaProgressionManager>();
            classManager = FindFirstObjectByType<HeroClassManager>();
            if (_mapGraph == null) _mapGraph = DungeonMapGraph.CreateCanonicalCursedSewersMap();
        }

        public void SelectNode(string nodeId)
        {
            _selectedNodeId = nodeId;
            AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
            HapticFeedback.Trigger(HapticFeedbackType.Light);
        }

        public void EnterSelectedNode()
        {
            if (string.IsNullOrEmpty(_selectedNodeId) && _mapGraph != null)
            {
                var avail = _mapGraph.GetAvailableNodes();
                if (avail.Count > 0) _selectedNodeId = avail[0].NodeId;
            }

            if (string.IsNullOrEmpty(_selectedNodeId)) return;

            AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
            HapticFeedback.Trigger(HapticFeedbackType.Medium);

            var node = _mapGraph.GetNode(_selectedNodeId);
            if (node != null)
            {
                _mapGraph.SelectAndEnterNode(_selectedNodeId);
                _mapGraph.CompleteCurrentNode();

                if (node.NodeType == DungeonMapNodeType.MerchantStall)
                {
                    navigation?.NavigateTo(ScreenState.MERCHANT);
                }
                else if (node.NodeType == DungeonMapNodeType.CampfireRest)
                {
                    navigation?.NavigateTo(ScreenState.CAMPFIRE_REST);
                }
                else if (node.NodeType == DungeonMapNodeType.MysteryShrine)
                {
                    navigation?.NavigateTo(ScreenState.EVENT);
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

                    navigation?.NavigateTo(ScreenState.COMBAT);
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

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 35f;
            float contentW = screenW - (padX * 2f);

            // =================================================================
            // 1. TOP HEADER & HUD BAR (Anchored at Top)
            // =================================================================
            float topY = 45f;
            float topH = 110f;
            Rect topBarRect = new Rect(padX, topY, contentW, topH);
            LattiruneUITheme.DrawCard(topBarRect);

            // Hero Emblem & Floor Title
            string selectedHeroId = classManager != null ? classManager.SelectedClassId : "class_rune_knight";
            Texture2D heroEmblem = VisualAssetProvider.GetClassEmblem(selectedHeroId);
            if (heroEmblem != null)
            {
                GUI.DrawTexture(new Rect(padX + 18f, topY + 18f, 74f, 74f), heroEmblem, ScaleMode.ScaleToFit);
            }

            int curFloor = runManager != null ? runManager.CurrentFloorNumber : 1;
            GUIStyle floorTitleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            floorTitleStyle.fontSize = 18;
            floorTitleStyle.fontStyle = FontStyle.Bold;
            floorTitleStyle.alignment = TextAnchor.MiddleLeft;
            floorTitleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 104f, topY + 22f, 400f, 28f), $"FLOOR {curFloor:D2} / 10 — THE CURSED SEWERS", floorTitleStyle);

            GUIStyle bossTargetStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            bossTargetStyle.fontSize = 13;
            bossTargetStyle.fontStyle = FontStyle.Italic;
            bossTargetStyle.alignment = TextAnchor.MiddleLeft;
            bossTargetStyle.normal.textColor = new Color(1f, 0.45f, 0.45f);
            GUI.Label(new Rect(padX + 104f, topY + 56f, 400f, 22f), "Target: The Lich Lord (Floor 10 Crypt)", bossTargetStyle);

            // Currency & HP Pills
            int gold = runManager != null ? runManager.CurrentGold : 0;
            int embers = metaProgression != null ? metaProgression.CurrentEmbers : 0;
            Texture2D iconGold = VisualAssetProvider.GetUIIcon("ui_icon_gold");
            Texture2D iconEmbers = VisualAssetProvider.GetUIIcon("ui_icon_embers");
            Texture2D iconHp = VisualAssetProvider.GetUIIcon("ui_icon_hp");

            float pillW = 160f;
            float pillX = padX + contentW - pillW - 12f;
            LattiruneUITheme.DrawIconValue(new Rect(pillX, topY + 16f, pillW, 24f), iconHp, "HP: 100/100", new Color(0.3f, 0.9f, 0.4f), 14);
            LattiruneUITheme.DrawIconValue(new Rect(pillX, topY + 42f, pillW, 24f), iconGold, $"{gold} Gold", LattiruneUITheme.ColorGoldPrimary, 14);
            LattiruneUITheme.DrawIconValue(new Rect(pillX, topY + 68f, pillW, 24f), iconEmbers, $"{embers} Embers", new Color(1f, 0.6f, 0.2f), 14);

            // =================================================================
            // 2. SCROLLABLE 10-FLOOR DAG MAP VIEW (Fills Middle Area)
            // =================================================================
            float previewCardH = 220f;
            float actionBtnH = 85f;
            float botBarMargin = 25f;
            float actY = virtualH - actionBtnH - botBarMargin;
            float previewY = actY - previewCardH - 12f;

            float mapY = topY + topH + 12f;
            float mapH = previewY - mapY - 12f;

            Rect scrollAreaRect = new Rect(padX, mapY, contentW, mapH);
            LattiruneUITheme.DrawCard(scrollAreaRect);

            GUILayout.BeginArea(new Rect(padX + 16f, mapY + 12f, contentW - 32f, mapH - 24f));
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Width(contentW - 32f), GUILayout.Height(mapH - 24f));

            var availNodes = _mapGraph.GetAvailableNodes();
            if (string.IsNullOrEmpty(_selectedNodeId) && availNodes.Count > 0)
            {
                _selectedNodeId = availNodes[0].NodeId;
            }

            int totalFloors = 10;
            for (int f = 1; f <= totalFloors; f++)
            {
                int floorNum = f;
                var floorNodes = _mapGraph.GetNodesOnFloor(floorNum);
                if (floorNodes.Count == 0) continue;

                // Floor Banner
                string floorBanner = (floorNum == 5) 
                    ? "FLOOR 5 — MID-BOSS LAIR (GRAVE GOLIATH)" 
                    : ((floorNum == 10) ? "FLOOR 10 — THE LICH LORD'S CRYPT" : $"FLOOR {floorNum:D2}");

                GUIStyle bannerStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                bannerStyle.fontSize = (floorNum == 5 || floorNum == 10) ? 18 : 16;
                bannerStyle.fontStyle = FontStyle.Bold;
                bannerStyle.normal.textColor = (floorNum == 10) ? new Color(1f, 0.35f, 0.35f) : ((floorNum == 5) ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorCyanArcane);

                GUILayout.Space(6);
                GUILayout.Label(floorBanner, bannerStyle);
                GUILayout.Space(4);

                foreach (var node in floorNodes)
                {
                    bool isAvailable = node.IsAvailable;
                    bool isCleared = node.IsCleared;
                    bool isSelected = (_selectedNodeId == node.NodeId);

                    // Room Card
                    Rect cardRect = GUILayoutUtility.GetRect(contentW - 48f, 75f, GUILayout.Width(contentW - 48f), GUILayout.Height(75f));

                    Color cardBg = isSelected 
                        ? new Color(0.20f, 0.25f, 0.36f, 0.95f)
                        : (isCleared ? new Color(0.06f, 0.08f, 0.10f, 0.70f) : (isAvailable ? new Color(0.10f, 0.14f, 0.20f, 0.90f) : new Color(0.05f, 0.06f, 0.08f, 0.60f)));
                    
                    GUI.color = cardBg;
                    LattiruneUITheme.DrawCard(cardRect);
                    GUI.color = Color.white;

                    // Border
                    if (isSelected)
                    {
                        LattiruneUITheme.DrawBorder(cardRect, 2.5f, LattiruneUITheme.ColorGoldBright);
                    }
                    else if (isAvailable)
                    {
                        LattiruneUITheme.DrawBorder(cardRect, 1.5f, LattiruneUITheme.ColorCyanArcane);
                    }

                    // Room Icon
                    Texture2D roomIcon = GetRoomIcon(node.NodeType);
                    if (roomIcon != null)
                    {
                        Rect iconRect = new Rect(cardRect.x + 14f, cardRect.y + (cardRect.height - 48f) * 0.5f, 48f, 48f);
                        Color oldC = GUI.color;
                        if (!isAvailable && !isCleared) GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                        GUI.DrawTexture(iconRect, roomIcon, ScaleMode.ScaleToFit);
                        GUI.color = oldC;
                    }

                    // Node Title & Status
                    float titleX = cardRect.x + 72f;
                    float titleW = cardRect.width - 240f;

                    GUIStyle nodeTitleStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    nodeTitleStyle.fontSize = 15;
                    nodeTitleStyle.fontStyle = FontStyle.Bold;
                    nodeTitleStyle.alignment = TextAnchor.MiddleLeft;
                    nodeTitleStyle.normal.textColor = isSelected ? LattiruneUITheme.ColorGoldBright : (isCleared ? LattiruneUITheme.ColorTextMuted : (isAvailable ? Color.white : new Color(0.5f, 0.55f, 0.65f)));
                    GUI.Label(new Rect(titleX, cardRect.y + 14f, titleW, 22f), node.Title.ToUpper(), nodeTitleStyle);

                    GUIStyle subtitleStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    subtitleStyle.fontSize = 12;
                    subtitleStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                    string sub = GetRoomSubtext(node.NodeType);
                    GUI.Label(new Rect(titleX, cardRect.y + 38f, titleW, 20f), sub, subtitleStyle);

                    // Status Badge
                    Rect badgeRect = new Rect(cardRect.x + cardRect.width - 125f, cardRect.y + 22f, 110f, 30f);
                    if (isCleared)
                    {
                        GUI.DrawTexture(badgeRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                        LattiruneUITheme.DrawBorder(badgeRect, 1f, LattiruneUITheme.ColorTextMuted);
                        GUIStyle bStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                        bStyle.alignment = TextAnchor.MiddleCenter;
                        bStyle.fontSize = 12;
                        bStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                        GUI.Label(badgeRect, "CLEARED", bStyle);
                    }
                    else if (isAvailable)
                    {
                        Color bCol = isSelected ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorCyanArcane;
                        GUI.DrawTexture(badgeRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                        LattiruneUITheme.DrawBorder(badgeRect, 1.5f, bCol);
                        GUIStyle bStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                        bStyle.alignment = TextAnchor.MiddleCenter;
                        bStyle.fontSize = 12;
                        bStyle.fontStyle = FontStyle.Bold;
                        bStyle.normal.textColor = bCol;
                        GUI.Label(badgeRect, isSelected ? "SELECTED" : "AVAILABLE", bStyle);
                    }
                    else
                    {
                        Texture2D lockIcon = VisualAssetProvider.GetUIIcon("ui_icon_lock");
                        if (lockIcon != null)
                        {
                            GUI.DrawTexture(new Rect(cardRect.x + cardRect.width - 55f, cardRect.y + (cardRect.height - 30f) * 0.5f, 30f, 30f), lockIcon, ScaleMode.ScaleToFit);
                        }
                    }

                    // Click detection
                    if (isAvailable && GUI.Button(cardRect, GUIContent.none, GUIStyle.none))
                    {
                        SelectNode(node.NodeId);
                    }

                    GUILayout.Space(6);
                }

                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // =================================================================
            // 3. SELECTED NODE PREVIEW CARD
            // =================================================================
            Rect previewRect = new Rect(padX, previewY, contentW, previewCardH);
            LattiruneUITheme.DrawCard(previewRect);

            var activeNode = !string.IsNullOrEmpty(_selectedNodeId) ? _mapGraph.GetNode(_selectedNodeId) : (availNodes.Count > 0 ? availNodes[0] : null);
            if (activeNode != null)
            {
                Texture2D pIcon = GetRoomIcon(activeNode.NodeType);
                if (pIcon != null)
                {
                    Rect pIconRect = new Rect(padX + 20f, previewY + 20f, 64f, 64f);
                    GUI.DrawTexture(pIconRect, pIcon, ScaleMode.ScaleToFit);
                }

                float pTextX = padX + 96f;
                float pTextW = contentW - 116f;

                GUIStyle pTitleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                pTitleStyle.fontSize = 20;
                pTitleStyle.fontStyle = FontStyle.Bold;
                pTitleStyle.alignment = TextAnchor.MiddleLeft;
                pTitleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
                GUI.Label(new Rect(pTextX, previewY + 16f, pTextW, 26f), activeNode.Title.ToUpper(), pTitleStyle);

                GUIStyle pDescStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                pDescStyle.fontSize = 14;
                pDescStyle.normal.textColor = LattiruneUITheme.ColorTextPrimary;
                GUI.Label(new Rect(pTextX, previewY + 44f, pTextW, 40f), activeNode.Description, pDescStyle);

                // Risk & Expected Rewards
                float infoRowY = previewY + 95f;
                string riskText = GetRiskText(activeNode.NodeType);
                Color riskCol = GetRiskColor(activeNode.NodeType);
                string rewardText = GetRewardText(activeNode.NodeType);

                GUIStyle riskLabelStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                riskLabelStyle.fontSize = 14;
                riskLabelStyle.fontStyle = FontStyle.Bold;
                riskLabelStyle.normal.textColor = riskCol;
                GUI.Label(new Rect(padX + 20f, infoRowY, 240f, 24f), $"THREAT LEVEL: {riskText}", riskLabelStyle);

                GUIStyle rewardLabelStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                rewardLabelStyle.fontSize = 14;
                rewardLabelStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
                GUI.Label(new Rect(padX + 280f, infoRowY, contentW - 300f, 24f), $"REWARDS: {rewardText}", rewardLabelStyle);
            }

            // =================================================================
            // 4. BOTTOM ACTION BUTTON
            // =================================================================
            Rect actRect = new Rect(padX, actY, contentW, actionBtnH);

            if (availNodes.Count > 0)
            {
                string btnLabel = activeNode != null ? $"ENTER {activeNode.Title.ToUpper()} & BEGIN" : "ENTER NEXT ROOM";
                if (GUI.Button(actRect, btnLabel, LattiruneUITheme.StylePrimaryBtn))
                {
                    EnterSelectedNode();
                }
            }
            else
            {
                if (GUI.Button(actRect, "DESCENT COMPLETE — ASCEND TO SURFACE", LattiruneUITheme.StyleSecondaryBtn))
                {
                    navigation?.NavigateTo(ScreenState.VICTORY);
                }
            }

            GUI.matrix = oldMatrix;
        }

        private Texture2D GetRoomIcon(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.Boss:
                    return VisualAssetProvider.GetUIIcon("ui_icon_boss_skull");
                case DungeonMapNodeType.EliteBattle:
                    return VisualAssetProvider.GetUIIcon("ui_icon_elite");
                case DungeonMapNodeType.MerchantStall:
                    return VisualAssetProvider.GetUIIcon("ui_icon_merchant");
                case DungeonMapNodeType.CampfireRest:
                    return VisualAssetProvider.GetUIIcon("ui_icon_campfire");
                case DungeonMapNodeType.MysteryShrine:
                    return VisualAssetProvider.GetUIIcon("ui_icon_event");
                case DungeonMapNodeType.TreasureVault:
                    return VisualAssetProvider.GetUIIcon("ui_icon_treasure");
                default:
                    return VisualAssetProvider.GetUIIcon("ui_icon_battle");
            }
        }

        private string GetRoomSubtext(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.Boss: return "Lethal Encounter • Final Crypt Boss";
                case DungeonMapNodeType.EliteBattle: return "High Threat • Guaranteed Rare Loot";
                case DungeonMapNodeType.MerchantStall: return "Safe Haven • Weapons & Restoratives";
                case DungeonMapNodeType.CampfireRest: return "Safe Respite • Restore HP & Forge Runes";
                case DungeonMapNodeType.MysteryShrine: return "Ancient Magic • Mysterious Boons";
                case DungeonMapNodeType.TreasureVault: return "Guarded Relics • Subterranean Spoils";
                default: return "Normal Encounter • Gold & Common Runes";
            }
        }

        private string GetRiskText(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.Boss: return "LETHAL (Boss)";
                case DungeonMapNodeType.EliteBattle: return "HIGH (Elite)";
                case DungeonMapNodeType.MerchantStall: return "NONE (Safe)";
                case DungeonMapNodeType.CampfireRest: return "NONE (Safe)";
                case DungeonMapNodeType.MysteryShrine: return "UNKNOWN (Event)";
                case DungeonMapNodeType.TreasureVault: return "LOW (Loot)";
                default: return "MODERATE (Normal)";
            }
        }

        private Color GetRiskColor(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.Boss: return new Color(1f, 0.25f, 0.25f);
                case DungeonMapNodeType.EliteBattle: return new Color(1f, 0.5f, 0.2f);
                case DungeonMapNodeType.MerchantStall: return new Color(0.3f, 0.9f, 0.4f);
                case DungeonMapNodeType.CampfireRest: return new Color(0.3f, 0.9f, 0.4f);
                case DungeonMapNodeType.MysteryShrine: return LattiruneUITheme.ColorCyanArcane;
                case DungeonMapNodeType.TreasureVault: return LattiruneUITheme.ColorGoldPrimary;
                default: return new Color(0.9f, 0.75f, 0.3f);
            }
        }

        private string GetRewardText(DungeonMapNodeType type)
        {
            switch (type)
            {
                case DungeonMapNodeType.Boss: return "+100 Gold, Soul Embers, Victory Triumph";
                case DungeonMapNodeType.EliteBattle: return "+45 Gold, Rare Relics, High-Tier Runes";
                case DungeonMapNodeType.MerchantStall: return "Shop Catalog Access, Health Potions";
                case DungeonMapNodeType.CampfireRest: return "Recover 40% HP or Upgrade Runes";
                case DungeonMapNodeType.MysteryShrine: return "Runic Boons, Stat Multipliers, Curses";
                case DungeonMapNodeType.TreasureVault: return "Free Relic Selection, +30 Gold";
                default: return "+15-25 Gold, Common Runes, Consumables";
            }
        }
    }
}
