using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Combat;
using Lattirune.Dungeon;
using Lattirune.Progression;

namespace Lattirune.UI
{
    /// <summary>
    /// Production-quality Combat Encounter Screen.
    /// Full dark-fantasy mobile battle scene:
    ///   TOP     : Enemy HP + stats nameplate
    ///   CENTER  : Hero vs Enemy combat stage (large sprites, auras, VFX)
    ///   BOTTOM  : Hero HP/Armor HUD, combat log, gold/embers, START BATTLE button
    /// Reward drafting flow handled by DrawRewardSelectionModal().
    /// </summary>
    public class CombatEncounterUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private RunManager runManager;
        [SerializeField] private CombatStageVisualController stageVisual;

        [Header("Encounter State")]
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private string enemyName = "Sewer Rat";
        [SerializeField] private int enemyMaxHp = 35;
        [SerializeField] private int enemyCurrentHp = 35;
        [SerializeField] private int enemyAtk = 3;
        [SerializeField] private int enemyArmor = 0;
        [SerializeField] private bool isBossEncounter = false;
        [SerializeField] private int bossPhase = 1;

        [Header("Hero State (Cached)")]
        [SerializeField] private string heroName = "Rune Knight";
        [SerializeField] private int heroMaxHp = 100;
        [SerializeField] private int heroCurrentHp = 100;
        [SerializeField] private int heroAtk = 10;
        [SerializeField] private int heroArmor = 0;
        [SerializeField] private string heroClassId = "hero_rune_knight";

        [Header("Runtime State")]
        [SerializeField] private bool isCombatActive = false;
        [SerializeField] private bool isVictoryRewardOpen = false;
        private string _combatLogMessage = "Align your weapons with rune conduits, then tap Start Battle.";

        [Serializable]
        public class RewardCardData
        {
            public string itemId;
            public string displayName;
            public string description;
            public string rarity;
            public Color rarityColor;
            public Texture2D icon;
        }

        private readonly List<RewardCardData> _rewardOptions = new List<RewardCardData>();
        private RewardCardData _selectedRewardOption = null;

        // Procedural textures
        private Texture2D _texStartBtnGlow;
        private bool _texturesBuilt = false;

        public void Initialize(ScreenNavigationController nav, CombatSystem combat, RunManager run, CombatStageVisualController stage = null)
        {
            navigation = nav;
            combatSystem = combat;
            runManager = run;
            stageVisual = stage ?? FindFirstObjectByType<CombatStageVisualController>();

            if (combatSystem != null)
            {
                combatSystem.OnVictory += HandleVictory;
                combatSystem.OnDefeat += HandleDefeat;
                combatSystem.OnAttackExecuted += HandleAttackExecuted;
            }
        }

        public void Initialize(CombatSystem combat, object synergy, object reward, object catalogue, Transform staging, ScreenNavigationController nav, RunManager run)
        {
            Initialize(nav, combat, run);
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnVictory -= HandleVictory;
                combatSystem.OnDefeat -= HandleDefeat;
                combatSystem.OnAttackExecuted -= HandleAttackExecuted;
            }
        }

        public void SetupEncounter(int floor, string enemy, int hp, int atk, int armor, bool isBoss = false, int phase = 1)
        {
            currentFloor = floor;
            enemyName = enemy;
            enemyMaxHp = hp;
            enemyCurrentHp = hp;
            enemyAtk = atk;
            enemyArmor = armor;
            isBossEncounter = isBoss;
            bossPhase = phase;
            isCombatActive = false;
            isVictoryRewardOpen = false;
            _selectedRewardOption = null;
            _combatLogMessage = isBoss ? $"A mighty dungeon boss looms! Prepare your lattice synergies." : $"{enemyName} approaches! Align conduits and begin.";

            if (stageVisual == null) stageVisual = FindFirstObjectByType<CombatStageVisualController>();
            stageVisual?.ResetCombo();
        }

        public void StartBattle()
        {
            if (isCombatActive) return;
            isCombatActive = true;
            _combatLogMessage = "Battle commenced! Conduits activating...";
            AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);

            if (combatSystem != null)
                combatSystem.StartCombat();
            else
                Invoke(nameof(SimulateVictory), 0.5f);
        }

        private void SimulateVictory() => HandleCombatResolved(true);

        private void HandleAttackExecuted(DamageResult result)
        {
            if (stageVisual == null) stageVisual = FindFirstObjectByType<CombatStageVisualController>();
            if (result == null) return;

            bool isHero = (result.SourceName == "Hero" || result.SourceName == heroName || result.TargetName != heroName);
            if (isHero)
            {
                stageVisual?.TriggerHeroAttack();
                stageVisual?.TriggerEnemyHit(result.FinalDamage, result.IsCritical);
                enemyCurrentHp = Mathf.Max(0, enemyCurrentHp - result.FinalDamage);
                _combatLogMessage = result.IsCritical
                    ? $"CRITICAL STRIKE! Hero hits {enemyName} for {result.FinalDamage} damage!"
                    : $"Hero strikes {enemyName} for {result.FinalDamage} damage!";
            }
            else
            {
                stageVisual?.TriggerEnemyAttack();
                stageVisual?.TriggerHeroHit(result.FinalDamage);
                heroCurrentHp = Mathf.Max(0, heroCurrentHp - result.FinalDamage);
                _combatLogMessage = $"{enemyName} attacks Hero for {result.FinalDamage} damage!";
            }
        }

        private void HandleVictory() => HandleCombatResolved(true);
        private void HandleDefeat() => HandleCombatResolved(false);
        public void BindControllers(object a, object b) { }

        private void HandleCombatResolved(bool playerWon)
        {
            isCombatActive = false;
            if (playerWon)
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                JuiceController.Instance?.TriggerScreenShake(12f, 0.4f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Medium);
                GenerateRewardOptions();
                isVictoryRewardOpen = true;
                _combatLogMessage = "Victory! Select your reward.";
            }
            else
            {
                JuiceController.Instance?.TriggerScreenShake(18f, 0.5f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Heavy);
                if (navigation != null) navigation.NavigateTo(ScreenState.DEATH);
            }
        }

        private void GenerateRewardOptions()
        {
            _rewardOptions.Clear();
            _rewardOptions.Add(new RewardCardData
            {
                itemId = "item_ruby_ring",
                displayName = "Ruby Ring",
                description = "Adjacent Fire Runes gain +25% burn duration and +3 ATK.",
                rarity = "RARE",
                rarityColor = new Color(0.22f, 0.74f, 0.97f),
                icon = VisualAssetProvider.GetItemTexture("item_ruby_ring")
            });
            _rewardOptions.Add(new RewardCardData
            {
                itemId = "item_broadsword",
                displayName = "Iron Broadsword",
                description = "10 Base Dmg | Synergy: +4 Dmg for each adjacent weapon.",
                rarity = "UNCOMMON",
                rarityColor = new Color(0.2f, 0.85f, 0.4f),
                icon = VisualAssetProvider.GetItemTexture("item_broadsword")
            });
            _rewardOptions.Add(new RewardCardData
            {
                itemId = "item_sapphire_ring",
                displayName = "Sapphire Ring",
                description = "Adjacent Ice Runes gain +25% slow potency and frost shield.",
                rarity = "EPIC",
                rarityColor = new Color(0.66f, 0.33f, 0.97f),
                icon = VisualAssetProvider.GetItemTexture("item_sapphire_ring")
            });
            _selectedRewardOption = null;
        }

        public void SelectReward(object rewardObj)
        {
            if (_rewardOptions.Count > 0) SelectReward(_rewardOptions[0]);
        }

        public void SelectReward(RewardCardData reward)
        {
            _selectedRewardOption = reward;
            AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
            JuiceController.Instance?.TriggerHaptic(HapticType.Medium);
        }

        public void CloseRewardScreenAndContinue()
        {
            isVictoryRewardOpen = false;
            if (navigation == null) navigation = FindFirstObjectByType<ScreenNavigationController>();
            if (runManager == null) runManager = FindFirstObjectByType<RunManager>();

            var mapCtrl = FindFirstObjectByType<DungeonMapScreenController>();
            if (mapCtrl != null && mapCtrl.MapGraph != null)
                mapCtrl.MapGraph.CompleteCurrentNode();

            if (runManager != null) runManager.ContinueAfterReward();

            var runComp = FindFirstObjectByType<RunCompleteController>();
            if (runComp != null && (runManager == null || runManager.CurrentState == RunState.RunComplete))
                runComp.SetupSummary(true, 10, 100, 50);

            if (navigation != null)
            {
                if (runManager != null && runManager.CurrentState == RunState.RunComplete)
                    navigation.NavigateTo(ScreenState.RUN_COMPLETE);
                else
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
            }
        }

        private void BuildTextures()
        {
            if (_texturesBuilt) return;
            _texturesBuilt = true;

            _texStartBtnGlow = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int x = 0; x < 64; x++) for (int y = 0; y < 64; y++)
            {
                float nx = (x - 32f) / 30f; float ny = (y - 32f) / 30f;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01(1f - d) * 0.5f;
                _texStartBtnGlow.SetPixel(x, y, new Color(0.95f, 0.75f, 0.2f, a));
            }
            _texStartBtnGlow.Apply();
        }

        private void OnGUI()
        {
            if (navigation != null &&
                navigation.CurrentScreen != ScreenState.COMBAT &&
                navigation.CurrentScreen != ScreenState.RUN_START)
                return;

            if (isVictoryRewardOpen)
            {
                DrawRewardSelectionModal();
                return;
            }

            DrawCombatScreen();
        }

        private void DrawCombatScreen()
        {
            BuildTextures();
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 30f;
            float contentW = screenW - padX * 2f;
            float topY = 45f;
            float t = Time.time;

            // ==================================================================
            // SECTION 1: TOP HUD — Floor title, gold, embers
            // ==================================================================
            float topHudH = 55f;
            Rect topHudRect = new Rect(padX, topY, contentW, topHudH);
            LattiruneUITheme.DrawCard(topHudRect);
            LattiruneUITheme.DrawBorder(topHudRect, 1.5f, isBossEncounter ? new Color(1f, 0.2f, 0.2f, 0.7f) : LattiruneUITheme.ColorBorderGold);

            // Floor title
            string floorTitle = isBossEncounter
                ? $"FLOOR {currentFloor}  --  BOSS SANCTUM"
                : $"FLOOR {currentFloor}  --  NORMAL ENCOUNTER";
            GUIStyle floorStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            floorStyle.fontSize = 16;
            floorStyle.fontStyle = FontStyle.Bold;
            floorStyle.alignment = TextAnchor.MiddleLeft;
            floorStyle.normal.textColor = isBossEncounter ? new Color(1f, 0.35f, 0.3f) : LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 16f, topY + 8f, 480f, 30f), floorTitle, floorStyle);

            // Currency pills (right side of top HUD)
            int gold = runManager != null ? runManager.CurrentGold : 0;
            int embers = runManager != null ? runManager.CurrentEmbers : 0;
            Texture2D iconGold = VisualAssetProvider.GetUIIcon("ui_icon_gold");
            Texture2D iconEmbers = VisualAssetProvider.GetUIIcon("ui_icon_embers");
            float pillRightX = padX + contentW - 270f;
            LattiruneUITheme.DrawIconValue(new Rect(pillRightX, topY + 12f, 115f, 26f), iconGold, $"{gold}g", LattiruneUITheme.ColorGoldPrimary, 15);
            LattiruneUITheme.DrawIconValue(new Rect(pillRightX + 125f, topY + 12f, 145f, 26f), iconEmbers, $"{embers} Embers", new Color(1f, 0.55f, 0.2f), 15);

            // ==================================================================
            // SECTION 2: ENEMY STATS NAMEPLATE (below top HUD)
            // ==================================================================
            float enemyHudY = topY + topHudH + 8f;
            float enemyHudH = 68f;
            Rect enemyHudRect = new Rect(padX, enemyHudY, contentW, enemyHudH);
            LattiruneUITheme.DrawCard(enemyHudRect);
            LattiruneUITheme.DrawBorder(enemyHudRect, 1.5f, isBossEncounter ? new Color(1f, 0.25f, 0.25f, 0.7f) : new Color(0.65f, 0.2f, 0.2f, 0.6f));

            // Enemy name
            GUIStyle enemyNameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            enemyNameStyle.fontSize = isBossEncounter ? 18 : 16;
            enemyNameStyle.fontStyle = FontStyle.Bold;
            enemyNameStyle.alignment = TextAnchor.MiddleLeft;
            enemyNameStyle.normal.textColor = isBossEncounter ? new Color(1f, 0.35f, 0.28f) : new Color(0.96f, 0.48f, 0.44f);
            string enemyDisplayTitle = isBossEncounter ? $"[BOSS P{bossPhase}]  {enemyName.ToUpper()}" : enemyName.ToUpper();
            GUI.Label(new Rect(padX + 14f, enemyHudY + 6f, 500f, 24f), enemyDisplayTitle, enemyNameStyle);

            // Enemy HP bar
            float enemyHpRatio = (float)enemyCurrentHp / Mathf.Max(1, enemyMaxHp);
            Rect enemyHpBgRect = new Rect(padX + 14f, enemyHudY + 34f, contentW - 28f, 22f);
            Color oldC = GUI.color;
            GUI.color = new Color(0.07f, 0.09f, 0.14f, 0.95f);
            GUI.DrawTexture(enemyHpBgRect, Texture2D.whiteTexture);
            GUI.color = oldC;
            LattiruneUITheme.DrawBorder(enemyHpBgRect, 1f, new Color(0.5f, 0.12f, 0.12f, 0.8f));
            float enemyFillW = Mathf.Max(0f, (enemyHpBgRect.width - 4f) * Mathf.Clamp01(enemyHpRatio));
            if (enemyFillW > 0f)
            {
                oldC = GUI.color;
                GUI.color = isBossEncounter ? new Color(1f, 0.18f, 0.18f, 1f) : new Color(0.88f, 0.22f, 0.26f, 1f);
                GUI.DrawTexture(new Rect(enemyHpBgRect.x + 2f, enemyHpBgRect.y + 2f, enemyFillW, enemyHpBgRect.height - 4f), Texture2D.whiteTexture);
                GUI.color = oldC;
            }
            GUIStyle enemyHpLbl = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            enemyHpLbl.alignment = TextAnchor.MiddleCenter;
            enemyHpLbl.fontSize = 13;
            enemyHpLbl.fontStyle = FontStyle.Bold;
            enemyHpLbl.normal.textColor = Color.white;
            GUI.Label(enemyHpBgRect, $"HP  {enemyCurrentHp} / {enemyMaxHp}", enemyHpLbl);

            // Enemy stat badges (ATK/ARMOR)
            Texture2D iconAtk = VisualAssetProvider.GetUIIcon("ui_icon_attack");
            Texture2D iconArmor = VisualAssetProvider.GetUIIcon("ui_icon_armor");
            float statX = padX + contentW - 290f;
            LattiruneUITheme.DrawIconValue(new Rect(statX, enemyHudY + 8f, 110f, 22f), iconAtk, $"ATK  {enemyAtk}", LattiruneUITheme.ColorTextPrimary, 13);
            if (enemyArmor > 0)
                LattiruneUITheme.DrawIconValue(new Rect(statX + 120f, enemyHudY + 8f, 130f, 22f), iconArmor, $"ARMOR  {enemyArmor}", LattiruneUITheme.ColorCyanArcane, 13);

            // ==================================================================
            // SECTION 3: COMBAT STAGE (hero vs enemy large sprites)
            // ==================================================================
            float stageY = enemyHudY + enemyHudH + 8f;
            float botSectionH = 200f;  // hero hud + log + button below stage
            float stageH = virtualH - stageY - botSectionH;
            stageH = Mathf.Max(stageH, 380f);
            Rect stageRect = new Rect(padX, stageY, contentW, stageH);

            if (stageVisual == null) stageVisual = FindFirstObjectByType<CombatStageVisualController>();
            if (stageVisual != null)
            {
                Texture2D heroTex = VisualAssetProvider.GetHeroTexture(heroClassId);
                Texture2D enemyTex = VisualAssetProvider.GetEnemyTexture(enemyName, isBossEncounter, bossPhase);
                stageVisual.DrawBattleArenaStage(
                    stageRect, heroTex, heroName,
                    heroCurrentHp, heroMaxHp, heroArmor, heroAtk,
                    enemyTex, enemyName,
                    enemyCurrentHp, enemyMaxHp, enemyArmor, enemyAtk,
                    isBossEncounter, bossPhase);
            }
            else
            {
                // Fallback minimal stage
                LattiruneUITheme.DrawCard(stageRect);
                GUIStyle fallbackStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                fallbackStyle.alignment = TextAnchor.MiddleCenter;
                fallbackStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUI.Label(stageRect, "COMBAT STAGE", fallbackStyle);
            }

            // ==================================================================
            // SECTION 4: HERO STATS HUD (below stage)
            // ==================================================================
            float heroHudY = stageY + stageH + 8f;
            float heroHudH = 58f;
            Rect heroHudRect = new Rect(padX, heroHudY, contentW, heroHudH);
            LattiruneUITheme.DrawCard(heroHudRect);
            LattiruneUITheme.DrawBorder(heroHudRect, 1.5f, new Color(0.72f, 0.58f, 0.12f, 0.65f));

            // Hero name badge
            Texture2D heroEmblem = VisualAssetProvider.GetClassEmblem(heroClassId);
            if (heroEmblem != null)
            {
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                GUI.DrawTexture(new Rect(padX + 10f, heroHudY + 8f, 42f, 42f), heroEmblem, ScaleMode.ScaleToFit);
                GUI.color = oldC;
            }

            GUIStyle heroNameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            heroNameStyle.fontSize = 15;
            heroNameStyle.fontStyle = FontStyle.Bold;
            heroNameStyle.alignment = TextAnchor.MiddleLeft;
            heroNameStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 60f, heroHudY + 6f, 220f, 22f), heroName.ToUpper(), heroNameStyle);

            // Hero HP bar
            float heroHpRatio = (float)heroCurrentHp / Mathf.Max(1, heroMaxHp);
            Rect heroHpBgRect = new Rect(padX + 60f, heroHudY + 30f, 260f, 20f);
            oldC = GUI.color;
            GUI.color = new Color(0.07f, 0.09f, 0.14f, 0.95f);
            GUI.DrawTexture(heroHpBgRect, Texture2D.whiteTexture);
            GUI.color = oldC;
            LattiruneUITheme.DrawBorder(heroHpBgRect, 1f, new Color(0.2f, 0.4f, 0.15f, 0.8f));
            float heroFillW = Mathf.Max(0f, (heroHpBgRect.width - 4f) * Mathf.Clamp01(heroHpRatio));
            if (heroFillW > 0f)
            {
                oldC = GUI.color;
                GUI.color = new Color(0.22f, 0.82f, 0.38f, 1f);
                GUI.DrawTexture(new Rect(heroHpBgRect.x + 2f, heroHpBgRect.y + 2f, heroFillW, heroHpBgRect.height - 4f), Texture2D.whiteTexture);
                GUI.color = oldC;
            }
            GUIStyle heroHpLbl = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            heroHpLbl.alignment = TextAnchor.MiddleCenter;
            heroHpLbl.fontSize = 12;
            heroHpLbl.fontStyle = FontStyle.Bold;
            heroHpLbl.normal.textColor = Color.white;
            GUI.Label(heroHpBgRect, $"HP  {heroCurrentHp}/{heroMaxHp}", heroHpLbl);

            // Hero stat badges (ATK / ARMOR / HP Icon)
            Texture2D iconHp = VisualAssetProvider.GetUIIcon("ui_icon_hp");
            LattiruneUITheme.DrawIconValue(new Rect(padX + 60f, heroHudY + 7f, 0f, 0f), iconHp, "", LattiruneUITheme.ColorGoldBright, 0);
            float heroStatX = padX + 340f;
            LattiruneUITheme.DrawIconValue(new Rect(heroStatX, heroHudY + 8f, 110f, 22f), iconAtk, $"ATK  {heroAtk}", LattiruneUITheme.ColorTextPrimary, 13);
            if (heroArmor > 0)
                LattiruneUITheme.DrawIconValue(new Rect(heroStatX + 120f, heroHudY + 8f, 130f, 22f), iconArmor, $"ARMOR  {heroArmor}", LattiruneUITheme.ColorCyanArcane, 13);

            // Combat log message
            float logY = heroHudY + heroHudH + 6f;
            GUIStyle logStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            logStyle.alignment = TextAnchor.MiddleCenter;
            logStyle.fontSize = 14;
            logStyle.fontStyle = FontStyle.Italic;
            logStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUI.Label(new Rect(padX, logY, contentW, 22f), _combatLogMessage, logStyle);

            // ==================================================================
            // SECTION 5: START BATTLE BUTTON (bottom, full-width, gold-glowing)
            // ==================================================================
            float botBtnH = 82f;
            float botBtnY = virtualH - botBtnH - 22f;
            Rect botBtnRect = new Rect(padX, botBtnY, contentW, botBtnH);

            if (!isCombatActive)
            {
                // Gold glow behind button
                if (_texStartBtnGlow != null)
                {
                    float glowPulse = 0.6f + Mathf.Sin(t * 2.8f) * 0.2f;
                    oldC = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, glowPulse);
                    GUI.DrawTexture(new Rect(botBtnRect.x - 20f, botBtnRect.y - 20f, botBtnRect.width + 40f, botBtnRect.height + 40f), _texStartBtnGlow, ScaleMode.StretchToFill);
                    GUI.color = oldC;
                }

                if (GUI.Button(botBtnRect, "", LattiruneUITheme.StylePrimaryBtn))
                {
                    StartBattle();
                }

                // Button label with rune decoration
                GUIStyle startBtnStyle = new GUIStyle(LattiruneUITheme.StylePrimaryBtn);
                startBtnStyle.fontSize = 24;
                startBtnStyle.fontStyle = FontStyle.Bold;
                startBtnStyle.alignment = TextAnchor.MiddleCenter;
                startBtnStyle.normal.textColor = new Color(0.06f, 0.04f, 0.02f, 1f);
                GUI.Label(botBtnRect, "START BATTLE", startBtnStyle);
            }
            else
            {
                // Resolving — pulsing active state
                float activePulse = 0.55f + Mathf.Sin(t * 4f) * 0.25f;
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, activePulse);
                GUI.Button(botBtnRect, "RESOLVING COMBAT...", LattiruneUITheme.StyleSecondaryBtn);
                GUI.color = oldC;
            }

            GUI.matrix = oldMatrix;
        }

        private void DrawRewardSelectionModal()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 30f;
            float contentW = screenW - padX * 2f;
            float topY = 45f;

            // Full-screen dark overlay
            Color oldC = GUI.color;
            GUI.color = new Color(0f, 0f, 0.02f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, screenW, virtualH), Texture2D.whiteTexture);
            GUI.color = oldC;

            // Header
            float headerH = 90f;
            Rect headerRect = new Rect(padX, topY, contentW, headerH);
            LattiruneUITheme.DrawCard(headerRect);
            LattiruneUITheme.DrawBorder(headerRect, 2f, LattiruneUITheme.ColorBorderGold);

            GUIStyle headerStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            headerStyle.fontSize = 26;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX, topY, contentW, 50f), "VICTORY REWARDS", headerStyle);

            GUIStyle subStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            subStyle.fontSize = 14;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(padX, topY + 50f, contentW, 30f), "Select ONE reward to reinforce your build:", subStyle);

            // Reward cards
            float cardStartY = topY + headerH + 16f;
            float cardH = 145f;
            float cardGap = 14f;

            for (int i = 0; i < _rewardOptions.Count; i++)
            {
                var reward = _rewardOptions[i];
                bool isSelected = (_selectedRewardOption != null && _selectedRewardOption.itemId == reward.itemId);
                float cardY = cardStartY + i * (cardH + cardGap);
                Rect cardRect = new Rect(padX, cardY, contentW, cardH);

                // Card background
                LattiruneUITheme.DrawCard(cardRect);
                LattiruneUITheme.DrawBorder(cardRect, isSelected ? 3f : 1.5f, isSelected ? reward.rarityColor : new Color(0.35f, 0.28f, 0.08f, 0.7f));

                // Item icon
                float iconSize = 100f;
                if (reward.icon != null)
                {
                    oldC = GUI.color;
                    if (isSelected)
                    {
                        float glowPulse = 0.6f + Mathf.Sin(Time.time * 3f) * 0.25f;
                        GUI.color = new Color(reward.rarityColor.r, reward.rarityColor.g, reward.rarityColor.b, glowPulse);
                        GUI.DrawTexture(new Rect(cardRect.x + 10f - 6f, cardY + 12f - 6f, iconSize + 12f, iconSize + 12f), Texture2D.whiteTexture);
                    }
                    GUI.color = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.88f);
                    GUI.DrawTexture(new Rect(cardRect.x + 10f, cardY + 14f, iconSize, iconSize), reward.icon, ScaleMode.ScaleToFit);
                    GUI.color = oldC;
                }

                // Rarity badge
                Rect rarityBadgeRect = new Rect(cardRect.x + 120f, cardY + 12f, 100f, 24f);
                oldC = GUI.color;
                GUI.color = new Color(reward.rarityColor.r * 0.25f, reward.rarityColor.g * 0.25f, reward.rarityColor.b * 0.25f, 0.9f);
                GUI.DrawTexture(rarityBadgeRect, Texture2D.whiteTexture);
                GUI.color = oldC;
                LattiruneUITheme.DrawBorder(rarityBadgeRect, 1.5f, reward.rarityColor);
                GUIStyle rarityStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                rarityStyle.fontSize = 12;
                rarityStyle.fontStyle = FontStyle.Bold;
                rarityStyle.alignment = TextAnchor.MiddleCenter;
                rarityStyle.normal.textColor = reward.rarityColor;
                GUI.Label(rarityBadgeRect, reward.rarity, rarityStyle);

                // Item name
                GUIStyle itemNameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                itemNameStyle.fontSize = 20;
                itemNameStyle.fontStyle = FontStyle.Bold;
                itemNameStyle.alignment = TextAnchor.MiddleLeft;
                itemNameStyle.normal.textColor = isSelected ? LattiruneUITheme.ColorGoldBright : Color.white;
                GUI.Label(new Rect(cardRect.x + 230f, cardY + 10f, contentW - 260f, 28f), reward.displayName, itemNameStyle);

                // Description
                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 14;
                descStyle.wordWrap = true;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUI.Label(new Rect(cardRect.x + 120f, cardY + 46f, contentW - 140f, 52f), reward.description, descStyle);

                // Action button
                float btnW = 240f;
                float btnH = 44f;
                Rect btnRect = new Rect(cardRect.x + contentW - btnW - 10f, cardY + cardH - btnH - 10f, btnW, btnH);

                if (isSelected)
                {
                    // Claimed state
                    oldC = GUI.color;
                    GUI.color = new Color(0.08f, 0.32f, 0.14f, 0.9f);
                    GUI.DrawTexture(btnRect, Texture2D.whiteTexture);
                    GUI.color = oldC;
                    LattiruneUITheme.DrawBorder(btnRect, 1.5f, new Color(0.18f, 0.8f, 0.44f, 0.9f));
                    GUIStyle claimedStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    claimedStyle.fontSize = 14;
                    claimedStyle.fontStyle = FontStyle.Bold;
                    claimedStyle.alignment = TextAnchor.MiddleCenter;
                    claimedStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
                    GUI.Label(btnRect, "CLAIMED", claimedStyle);
                }
                else
                {
                    if (GUI.Button(btnRect, $"CLAIM", LattiruneUITheme.StylePrimaryBtn))
                        SelectReward(reward);
                }
            }

            // Continue button
            float continueBtnY = virtualH - 105f;
            Rect continueBtnRect = new Rect(padX, continueBtnY, contentW, 80f);
            if (_selectedRewardOption != null)
            {
                if (GUI.Button(continueBtnRect, "CONTINUE DESCENT", LattiruneUITheme.StylePrimaryBtn))
                    CloseRewardScreenAndContinue();
            }
            else
            {
                oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUI.Button(continueBtnRect, "SELECT A REWARD ABOVE TO PROCEED", LattiruneUITheme.StyleSecondaryBtn);
                GUI.color = oldC;
            }

            GUI.matrix = oldMatrix;
        }
    }
}
