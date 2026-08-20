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
    /// Master Combat Encounter HUD & Screen Controller.
    /// Manages the compact top status HUD (Hero vs Enemy HP/Stats/Floor),
    /// dedicated Upper-Middle Combat Stage, Bottom Action Bar (Start Battle / Speed / Potions),
    /// and Victory Reward drafting flow.
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
            _combatLogMessage = isBoss ? "A mighty dungeon boss looms! Prepare your lattice synergies." : $"{enemyName} approaches. Align conduits and begin!";
        }

        public void StartBattle()
        {
            if (isCombatActive) return;

            isCombatActive = true;
            _combatLogMessage = "Battle commenced! Conduits and weapons activating...";
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.CombatHit);
            JuiceController.Instance?.TriggerHaptic(HapticType.Light);

            if (combatSystem != null)
            {
                combatSystem.StartCombat();
            }
            else
            {
                Invoke(nameof(SimulateVictory), 0.5f);
            }
        }

        private void SimulateVictory()
        {
            HandleCombatResolved(true);
        }

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
                _combatLogMessage = $"Hero strikes {enemyName} for {result.FinalDamage} damage!";
            }
            else
            {
                stageVisual?.TriggerEnemyAttack();
                stageVisual?.TriggerHeroHit(result.FinalDamage);
                heroCurrentHp = Mathf.Max(0, heroCurrentHp - result.FinalDamage);
                _combatLogMessage = $"{enemyName} attacks Hero for {result.FinalDamage} damage!";
            }
        }

        private void HandleVictory()
        {
            HandleCombatResolved(true);
        }

        private void HandleDefeat()
        {
            HandleCombatResolved(false);
        }

        public void BindControllers(object a, object b) { }

        private void HandleCombatResolved(bool playerWon)
        {
            isCombatActive = false;
            if (playerWon)
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.Victory);
                JuiceController.Instance?.TriggerScreenShake(12f, 0.4f);
                JuiceController.Instance?.TriggerHaptic(HapticType.Success);
                GenerateRewardOptions();
                isVictoryRewardOpen = true;
                _combatLogMessage = "Victory! Select your reward.";
            }
            else
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.Defeat);
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
                rarityColor = new Color(0.22f, 0.74f, 0.97f), // Sky Blue
                icon = VisualAssetProvider.GetItemTexture("item_ruby_ring")
            });

            _rewardOptions.Add(new RewardCardData
            {
                itemId = "item_broadsword",
                displayName = "Iron Broadsword",
                description = "10 Base Dmg | Synergy: +4 Dmg for each adjacent weapon.",
                rarity = "UNCOMMON",
                rarityColor = new Color(0.2f, 0.85f, 0.4f), // Emerald Green
                icon = VisualAssetProvider.GetItemTexture("item_broadsword")
            });

            _rewardOptions.Add(new RewardCardData
            {
                itemId = "item_sapphire_ring",
                displayName = "Sapphire Ring",
                description = "Adjacent Ice Runes gain +25% slow potency and frost shield.",
                rarity = "EPIC",
                rarityColor = new Color(0.66f, 0.33f, 0.97f), // Void Purple
                icon = VisualAssetProvider.GetItemTexture("item_sapphire_ring")
            });

            _selectedRewardOption = null;
        }

        public void SelectReward(object rewardObj)
        {
            if (_rewardOptions.Count > 0)
            {
                SelectReward(_rewardOptions[0]);
            }
        }

        public void SelectReward(RewardCardData reward)
        {
            _selectedRewardOption = reward;
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.RewardClaimed);
            JuiceController.Instance?.TriggerHaptic(HapticType.Medium);
        }

        public void CloseRewardScreenAndContinue()
        {
            isVictoryRewardOpen = false;

            if (navigation == null) navigation = FindFirstObjectByType<ScreenNavigationController>();
            if (runManager == null) runManager = FindFirstObjectByType<RunManager>();

            var mapCtrl = FindFirstObjectByType<DungeonMapScreenController>();
            if (mapCtrl != null && mapCtrl.MapGraph != null)
            {
                mapCtrl.MapGraph.CompleteCurrentNode();
            }

            if (runManager != null)
            {
                runManager.ContinueAfterReward();
            }

            var runComp = FindFirstObjectByType<RunCompleteController>();
            if (runComp != null && (runManager == null || runManager.CurrentState == RunState.RunComplete))
            {
                runComp.SetupSummary(true, 10, 100, 50);
            }

            if (navigation != null)
            {
                if (runManager != null && runManager.CurrentState == RunState.RunComplete)
                {
                    navigation.NavigateTo(ScreenState.RUN_COMPLETE);
                }
                else
                {
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }
        }

        private void OnGUI()
        {
            if (navigation != null && 
                navigation.CurrentScreen != ScreenState.COMBAT && 
                navigation.CurrentScreen != ScreenState.RUN_START)
            {
                return;
            }

            if (isVictoryRewardOpen)
            {
                DrawRewardSelectionModal();
                return;
            }

            DrawCombatHUD();
        }

        private void DrawCombatHUD()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float screenW = 1080f;
            float padX = 35f;
            float topHUDY = 25f;

            // =================================================================
            // 1. TOP COMPACT COMBAT HUD (Floor / Currency / Compact Stats)
            // =================================================================
            float hudW = screenW - (padX * 2f);
            float hudH = 115f;
            Rect hudRect = new Rect(padX, topHUDY, hudW, hudH);
            LattiruneUITheme.DrawCard(hudRect);

            // 1a. Header Title Badge
            string encounterTitle = isBossEncounter ? $"FLOOR {currentFloor} — BOSS SANCTUM" : $"FLOOR {currentFloor} — NORMAL ENCOUNTER";
            int gold = runManager != null ? runManager.CurrentGold : 0;
            int embers = runManager != null ? runManager.CurrentEmbers : 0;

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 17;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 16f, topHUDY + 8f, 420f, 24f), encounterTitle, titleStyle);

            // 1b. Gold & Embers with Sprite Icons
            Texture2D iconGold = VisualAssetProvider.GetUIIcon("ui_icon_gold");
            Texture2D iconEmbers = VisualAssetProvider.GetUIIcon("ui_icon_embers");
            Texture2D iconAtk = VisualAssetProvider.GetUIIcon("ui_icon_attack");
            Texture2D iconArmor = VisualAssetProvider.GetUIIcon("ui_icon_armor");
            Texture2D iconBattle = VisualAssetProvider.GetUIIcon("ui_icon_battle");

            float currX = padX + hudW - 270f;
            LattiruneUITheme.DrawIconValue(new Rect(currX, topHUDY + 8f, 110f, 24f), iconGold, $"{gold}g", LattiruneUITheme.ColorGoldPrimary, 15);
            LattiruneUITheme.DrawIconValue(new Rect(currX + 120f, topHUDY + 8f, 140f, 24f), iconEmbers, $"{embers} Embers", new Color(1f, 0.55f, 0.2f), 15);

            // 1c. Hero Health & Stats (Left Sub-Pill)
            float pillW = (hudW - 40f) * 0.5f;
            float pillY = topHUDY + 38f;
            float heroRatio = (float)heroCurrentHp / Mathf.Max(1, heroMaxHp);
            float enemyRatio = (float)enemyCurrentHp / Mathf.Max(1, enemyMaxHp);

            // Hero HP Bar
            LattiruneUITheme.DrawHealthBar(new Rect(padX + 16f, pillY, pillW, 30f), heroRatio, $"HERO: {heroCurrentHp}/{heroMaxHp} HP");
            float statY = pillY + 36f;
            LattiruneUITheme.DrawIconValue(new Rect(padX + 16f, statY, 110f, 24f), iconAtk, $"ATK: {heroAtk}", LattiruneUITheme.ColorTextPrimary, 14);
            LattiruneUITheme.DrawIconValue(new Rect(padX + 130f, statY, 130f, 24f), iconArmor, $"ARMOR: {heroArmor}", LattiruneUITheme.ColorCyanArcane, 14);

            // Enemy HP Bar (Right Sub-Pill)
            Rect enemyBarRect = new Rect(padX + 24f + pillW, pillY, pillW, 30f);
            GUI.DrawTexture(enemyBarRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
            Rect enemyFillRect = new Rect(enemyBarRect.x + 2f, enemyBarRect.y + 2f, Mathf.Max(0f, (enemyBarRect.width - 4f) * Mathf.Clamp01(enemyRatio)), enemyBarRect.height - 4f);
            GUI.color = new Color(0.85f, 0.22f, 0.26f, 1f);
            GUI.DrawTexture(enemyFillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle enemyLabelStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            enemyLabelStyle.alignment = TextAnchor.MiddleCenter;
            enemyLabelStyle.fontSize = 17;
            enemyLabelStyle.fontStyle = FontStyle.Bold;
            enemyLabelStyle.normal.textColor = Color.white;
            GUI.Label(enemyBarRect, $"{enemyName.ToUpper()}: {enemyCurrentHp}/{enemyMaxHp} HP", enemyLabelStyle);

            float enemyStatX = padX + 24f + pillW;
            LattiruneUITheme.DrawIconValue(new Rect(enemyStatX, statY, 110f, 24f), iconAtk, $"ATK: {enemyAtk}", LattiruneUITheme.ColorTextPrimary, 14);
            LattiruneUITheme.DrawIconValue(new Rect(enemyStatX + 120f, statY, 130f, 24f), iconArmor, $"ARMOR: {enemyArmor}", LattiruneUITheme.ColorCyanArcane, 14);

            // =================================================================
            // 2. DEDICATED OPEN COMBAT STAGE (Hero vs Enemy Silhouette Arena)
            // =================================================================
            float stageY = topHUDY + hudH + 10f;
            float stageH = 580f;
            Rect stageRect = new Rect(padX, stageY, hudW, stageH);

            if (stageVisual == null) stageVisual = FindFirstObjectByType<CombatStageVisualController>();
            if (stageVisual != null)
            {
                Texture2D heroTex = VisualAssetProvider.GetHeroTexture("hero_rune_knight");
                Texture2D enemyTex = VisualAssetProvider.GetEnemyTexture(enemyName, isBossEncounter, bossPhase);

                stageVisual.DrawBattleArenaStage(
                    stageRect,
                    heroTex,
                    heroName,
                    heroCurrentHp,
                    heroMaxHp,
                    heroArmor,
                    heroAtk,
                    enemyTex,
                    enemyName,
                    enemyCurrentHp,
                    enemyMaxHp,
                    enemyArmor,
                    enemyAtk,
                    isBossEncounter,
                    bossPhase
                );
            }

            // =================================================================
            // 3. COMBAT LOG / STATUS MESSAGE (Below Combat Stage)
            // =================================================================
            float logY = stageY + stageH + 8f;
            GUIStyle logStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            logStyle.alignment = TextAnchor.MiddleCenter;
            logStyle.fontSize = 15;
            logStyle.fontStyle = FontStyle.Italic;
            logStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUI.Label(new Rect(padX, logY, hudW, 22f), _combatLogMessage, logStyle);

            // =================================================================
            // 4. CONTEXTUAL ONBOARDING PROMPT (Floor 1)
            // =================================================================
            if (currentFloor == 1 && !isCombatActive)
            {
                float tutY = logY + 26f;
                Rect tutRect = new Rect(padX + 20f, tutY, hudW - 40f, 38f);
                Color oldC = GUI.color;
                GUI.color = new Color(0.08f, 0.14f, 0.22f, 0.95f);
                LattiruneUITheme.DrawCard(tutRect);
                LattiruneUITheme.DrawBorder(tutRect, 1.5f, new Color(0.38f, 0.8f, 1.0f, 0.8f));

                GUIStyle tutStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                tutStyle.alignment = TextAnchor.MiddleCenter;
                tutStyle.fontSize = 14;
                tutStyle.fontStyle = FontStyle.Bold;
                tutStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
                GUI.Label(tutRect, "Align weapons with rune conduits to ignite elemental synergies!", tutStyle);
                GUI.color = oldC;
            }

            // =================================================================
            // 5. BOTTOM ACTION BAR (Large Mobile-First Touch Targets)
            // =================================================================
            float virtualHeight = Screen.height / scale;
            float botBarY = virtualHeight - 115f;
            float botBarW = hudW;
            float botBtnH = 85f;

            if (!isCombatActive)
            {
                Rect startBtnRect = new Rect(padX, botBarY, botBarW, botBtnH);
                if (GUI.Button(startBtnRect, "START BATTLE", LattiruneUITheme.StylePrimaryBtn))
                {
                    StartBattle();
                }
            }
            else
            {
                Rect activeBtnRect = new Rect(padX, botBarY, botBarW, botBtnH);
                GUI.Button(activeBtnRect, "RESOLVING COMBAT...", LattiruneUITheme.StyleSecondaryBtn);
            }

            GUI.matrix = oldMatrix;
        }

        private void DrawRewardSelectionModal()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float modalWidth = 960f;
            float modalHeight = 980f;
            float posX = (1080f - modalWidth) * 0.5f;
            float posY = 180f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, modalWidth, modalHeight), "VICTORY REWARDS");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, modalWidth - 80, modalHeight - 80));

            LattiruneUITheme.DrawHeader("VICTORY REWARDS", "Select ONE reward to reinforce your build:");
            GUILayout.Space(16);

            for (int i = 0; i < _rewardOptions.Count; i++)
            {
                var reward = _rewardOptions[i];
                bool isSelected = (_selectedRewardOption != null && _selectedRewardOption.itemId == reward.itemId);

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                // Card Rarity Glow Border if selected
                if (isSelected)
                {
                    LattiruneUITheme.DrawBorder(new Rect(posX + 40, posY + 120 + (i * 140), modalWidth - 80, 120), 3f, reward.rarityColor);
                }

                GUILayout.BeginHorizontal();

                // 1. Real Item Artwork Icon
                if (reward.icon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(80f, 80f, GUILayout.Width(80f), GUILayout.Height(80f));
                    GUI.DrawTexture(iconRect, reward.icon, ScaleMode.ScaleToFit);
                    GUILayout.Space(14);
                }

                // 2. Info & Rarity Tag
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                GUIStyle nameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                nameStyle.fontSize = 20;
                nameStyle.fontStyle = FontStyle.Bold;
                nameStyle.normal.textColor = isSelected ? LattiruneUITheme.ColorGoldBright : Color.white;
                GUILayout.Label(reward.displayName, nameStyle);

                GUILayout.FlexibleSpace();
                LattiruneUITheme.DrawBadge(reward.rarity, reward.rarityColor);
                GUILayout.EndHorizontal();

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 14;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(reward.description, descStyle);
                GUILayout.EndVertical();

                // 3. Selection Action Button
                GUILayout.FlexibleSpace();
                if (isSelected)
                {
                    LattiruneUITheme.DrawBadge("CLAIMED & STAGED", new Color(0.18f, 0.8f, 0.44f));
                }
                else
                {
                    if (LattiruneUITheme.DrawPrimaryButton($"CLAIM {reward.displayName.ToUpper()}", 55f))
                    {
                        SelectReward(reward);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(14);
            }

            GUILayout.Space(10);

            if (_selectedRewardOption != null)
            {
                if (LattiruneUITheme.DrawPrimaryButton("CONTINUE DESCENT", 75f))
                {
                    CloseRewardScreenAndContinue();
                }
            }
            else
            {
                LattiruneUITheme.DrawSecondaryButton("SELECT A REWARD ABOVE TO PROCEED", 70f);
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
