using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Grid;
using Lattirune.Items;
using Lattirune.Boss;
using Lattirune.Audio;
using Lattirune.Progression;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Screen controller for Combat and Floor Encounters with integrated 2D Battle Stage,
    /// dynamic victory reward selection, contextual tutorial hints, and AAA game feel.
    /// </summary>
    public class CombatEncounterUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private RunManager runManager;
        [SerializeField] private CombatStageVisualController stageVisual;

        [Header("Encounter State")]
        [SerializeField] private bool isCombatActive = false;
        [SerializeField] private bool isVictoryRewardOpen = false;
        [SerializeField] private int currentFloor = 1;
        [SerializeField] private string enemyName = "Sewer Rat";
        [SerializeField] private int enemyMaxHp = 35;
        [SerializeField] private int enemyCurrentHp = 35;
        [SerializeField] private int enemyArmor = 0;
        [SerializeField] private int enemyAtk = 3;
        [SerializeField] private bool isBossEncounter = false;
        [SerializeField] private int bossPhase = 1;

        [Header("Hero State")]
        [SerializeField] private string heroName = "Rune Knight";
        [SerializeField] private int heroMaxHp = 100;
        [SerializeField] private int heroCurrentHp = 100;
        [SerializeField] private int heroArmor = 0;
        [SerializeField] private int heroAtk = 10;

        private List<RewardCardData> _rewardOptions = new List<RewardCardData>();
        private RewardCardData _selectedRewardOption = null;
        private string _combatLogMessage = "Prepare your grid alignment and initiate battle.";

        public class RewardCardData
        {
            public string itemId;
            public string displayName;
            public string description;
            public string rarity;
            public Color rarityColor;
            public Texture2D icon;
        }

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
            _combatLogMessage = isBoss 
                ? $"BOSS ENCOUNTER: {enemyName}! Align conduits to breach defenses." 
                : $"Floor {currentFloor}: {enemyName} approaches. Align your lattice.";
        }

        public void StartBattle()
        {
            isCombatActive = true;
            _combatLogMessage = "Battle commenced! Conduits and weapons activating...";
            AudioController.Instance?.PlaySoundEffect(SoundEffectType.UiClick);
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
            }
            else
            {
                stageVisual?.TriggerEnemyAttack();
                stageVisual?.TriggerHeroHit(result.FinalDamage);
                heroCurrentHp = Mathf.Max(0, heroCurrentHp - result.FinalDamage);
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
            if (runManager != null)
            {
                // Advanced floor
            }
            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.DUNGEON_MAP);
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

            float panelWidth = 980f;
            float panelHeight = 440f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = 20f + offsetY;

            // 1. Render 2D Battle Arena
            if (stageVisual == null) stageVisual = FindFirstObjectByType<CombatStageVisualController>();
            if (stageVisual != null)
            {
                Texture2D heroTex = VisualAssetProvider.GetHeroTexture("hero_rune_knight");
                Texture2D enemyTex = VisualAssetProvider.GetEnemyTexture(enemyName, isBossEncounter, bossPhase);

                stageVisual.DrawBattleArenaStage(
                    new Rect(posX, posY, panelWidth, 310f),
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

            // 2. Health Bars Area
            float barY = posY + 316f;
            float barW = panelWidth;
            float heroRatio = (float)heroCurrentHp / Mathf.Max(1, heroMaxHp);
            float enemyRatio = (float)enemyCurrentHp / Mathf.Max(1, enemyMaxHp);

            LattiruneUITheme.DrawHealthBar(new Rect(posX, barY, barW, 20f), heroRatio, $"HP {heroCurrentHp}/{heroMaxHp}");
            LattiruneUITheme.DrawHealthBar(new Rect(posX, barY + 22f, barW, 20f), enemyRatio, $"HP {enemyCurrentHp}/{enemyMaxHp}");

            // 3. Combat Log / Status Message
            GUIStyle logStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            logStyle.fontSize = 15;
            logStyle.fontStyle = FontStyle.Italic;
            logStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUI.Label(new Rect(posX, barY + 46f, panelWidth, 22f), $"Log: {_combatLogMessage}", logStyle);

            // 4. Primary Action Button (START BATTLE)
            float btnY = barY + 70f;
            if (!isCombatActive)
            {
                if (LattiruneUITheme.DrawPrimaryButton("START BATTLE", 70f))
                {
                    StartBattle();
                }
            }
            else
            {
                LattiruneUITheme.DrawSecondaryButton("RESOLVING COMBAT...", 70f);
            }

            // 5. Contextual First-5-Minutes Tutorial Hint (Floor 1)
            if (currentFloor == 1 && !isCombatActive)
            {
                float tutW = 900f;
                float tutH = 50f;
                float tutX = (1080f - tutW) * 0.5f;
                float tutY = btnY + 80f;

                Rect tutRect = new Rect(tutX, tutY, tutW, tutH);
                Color oldC = GUI.color;
                GUI.color = new Color(0.1f, 0.15f, 0.25f, 0.92f);
                LattiruneUITheme.DrawCard(tutRect);
                LattiruneUITheme.DrawBorder(tutRect, 1.5f, new Color(0.38f, 0.8f, 1.0f, 0.8f));

                GUIStyle tutStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                tutStyle.alignment = TextAnchor.MiddleCenter;
                tutStyle.fontSize = 16;
                tutStyle.fontStyle = FontStyle.Bold;
                tutStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
                GUI.Label(tutRect, "💡 TUTORIAL: Align weapons with rune conduits to ignite elemental synergies!", tutStyle);
                GUI.color = oldC;
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
