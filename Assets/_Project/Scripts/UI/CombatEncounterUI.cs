using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Synergy;

namespace Lattirune.UI
{
    /// <summary>
    /// Master UI Component for the Combat Encounter Screen.
    /// Integrates the 2D Animated Battle Arena Stage, real character portraits,
    /// monster visuals, boss phase transitions, speed controls, emergency potions,
    /// and visual victory reward cards (0 emoji, 0 placeholders).
    /// </summary>
    public class CombatEncounterUI : MonoBehaviour
    {
        [Header("Combat System References")]
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private SynergySystem synergySystem;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private Lattirune.Dungeon.RunManager runManager;

        [Header("Reward Modal References")]
        [SerializeField] private List<ItemDataSO> _itemCatalogue;
        [SerializeField] private Transform rewardSpawnParent;
        [SerializeField] private Vector3 rewardSpawnPosition = Vector3.zero;

        [Header("State")]
        private List<RewardOption> _currentRewardOptions = new List<RewardOption>();
        private RewardOption _selectedRewardOption;
        private bool _isShowingRewards = false;
        private string _combatLog = "Prepare your grid alignment and initiate battle.";

        public bool IsShowingRewards => _isShowingRewards;
        public RewardOption SelectedRewardOption => _selectedRewardOption;
        public IReadOnlyList<RewardOption> CurrentRewardOptions => _currentRewardOptions;

        public void Initialize(
            CombatSystem combat, 
            SynergySystem synergy, 
            RewardService service, 
            List<ItemDataSO> catalogue,
            Transform spawnParent)
        {
            combatSystem = combat;
            synergySystem = synergy;
            rewardService = service;
            _itemCatalogue = catalogue ?? new List<ItemDataSO>();
            rewardSpawnParent = spawnParent;

            if (combatSystem != null)
            {
                combatSystem.OnAttackExecuted += HandleAttackExecuted;
                combatSystem.OnVictory += HandleVictory;
                combatSystem.OnDefeat += HandleDefeat;
            }
        }

        public void Initialize(
            CombatSystem combat, 
            SynergySystem synergy, 
            RewardService service, 
            List<ItemDataSO> catalogue,
            Transform spawnParent,
            ScreenNavigationController nav,
            Lattirune.Dungeon.RunManager run = null)
        {
            navigation = nav;
            runManager = run;
            Initialize(combat, synergy, service, catalogue, spawnParent);
        }

        [SerializeField] private DungeonMapScreenController mapController;
        [SerializeField] private RunCompleteController runCompleteController;

        public void BindControllers(DungeonMapScreenController map, RunCompleteController runComplete)
        {
            mapController = map;
            runCompleteController = runComplete;
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnAttackExecuted -= HandleAttackExecuted;
                combatSystem.OnVictory -= HandleVictory;
                combatSystem.OnDefeat -= HandleDefeat;
            }
        }

        private void HandleAttackExecuted(DamageResult damage)
        {
            string bonus = damage.HasSynergyBonus ? $" (+{damage.RuneBonus} Flame Synergy)" : "";
            _combatLog = $"{damage.SourceName} strikes {damage.TargetName} for {damage.FinalDamage} DMG{bonus}!";

            // Trigger 2D Visual Stage Animation & VFX
            if (CombatStageVisualController.Instance != null && combatSystem != null)
            {
                if (damage.SourceName == combatSystem.Player?.CombatantName)
                {
                    CombatStageVisualController.Instance.TriggerHeroAttack();
                    CombatStageVisualController.Instance.TriggerEnemyHit(damage.FinalDamage, damage.HasSynergyBonus);
                }
                else
                {
                    CombatStageVisualController.Instance.TriggerEnemyAttack();
                    CombatStageVisualController.Instance.TriggerHeroHit(damage.FinalDamage);
                }
            }
        }

        private void HandleVictory()
        {
            _combatLog = "VICTORY! Enemy vanquished. Choose your reward.";
            _isShowingRewards = true;
            _selectedRewardOption = null;

            if (rewardService != null)
            {
                rewardService.ResetSelectionLock();
            }

            _currentRewardOptions = RewardGenerator.GenerateRewardOptions(_itemCatalogue, count: 3);
            if (navigation != null)
            {
                navigation.NavigateTo(ScreenState.REWARD_SELECTION);
            }
        }

        private void HandleDefeat()
        {
            _combatLog = "DEFEAT! Player succumbed to the dungeon horrors.";
            _isShowingRewards = false;
            _selectedRewardOption = null;

            if (runManager != null && !runManager.CanRevivePlayer)
            {
                if (runCompleteController != null)
                {
                    int clearedFloors = Mathf.Max(0, runManager.CurrentFloorNumber - 1);
                    runCompleteController.SetupSummary(victory: false, floors: clearedFloors, gold: runManager.CurrentGold, embers: runManager.CurrentEmbers);
                }
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.DEATH);
                }
            }
        }

        public void SelectReward(RewardOption option)
        {
            if (_selectedRewardOption != null || option == null || rewardService == null)
            {
                return;
            }

            _selectedRewardOption = option;
            rewardService.ApplyReward(option, rewardSpawnPosition, rewardSpawnParent);
            _combatLog = $"Reward Applied: {option.DisplayName} added to staging inventory.";
        }

        public void CloseRewardScreenAndContinue()
        {
            _isShowingRewards = false;
            _selectedRewardOption = null;
            _currentRewardOptions.Clear();

            if (mapController != null && mapController.MapGraph != null)
            {
                mapController.MapGraph.CompleteCurrentNode();
            }

            if (runManager != null && runManager.CurrentState == Lattirune.Dungeon.RunState.RewardSelection)
            {
                runManager.ContinueAfterReward();
            }

            if (combatSystem != null)
            {
                combatSystem.ResetCombat();
            }

            if (navigation != null)
            {
                if (runManager != null && runManager.CurrentState == Lattirune.Dungeon.RunState.RunComplete)
                {
                    if (runCompleteController != null)
                    {
                        runCompleteController.SetupSummary(victory: true, floors: 10, gold: runManager.CurrentGold, embers: runManager.CurrentEmbers);
                    }
                    navigation.NavigateTo(ScreenState.VICTORY);
                }
                else
                {
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }
        }

        private void OnGUI()
        {
            if (combatSystem == null) return;
            if (navigation != null && navigation.CurrentScreen != ScreenState.COMBAT && navigation.CurrentScreen != ScreenState.GRID_BUILD && navigation.CurrentScreen != ScreenState.REWARD_SELECTION)
            {
                return;
            }

            if (_isShowingRewards || (navigation != null && navigation.CurrentScreen == ScreenState.REWARD_SELECTION))
            {
                DrawRewardSelectionModal();
            }
            else
            {
                DrawCombatTopHUD();
            }
        }

        private void DrawCombatTopHUD()
        {
            float hudWidth = 1000f;
            float hudHeight = 440f;
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float posX = (1080f - hudWidth) * 0.5f;
            float posY = 20f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, hudWidth, hudHeight), "BATTLE ARENA");

            GUILayout.BeginArea(new Rect(posX + 20, posY + 15, hudWidth - 40, hudHeight - 30));

            PlayerCombatant player = combatSystem.Player;
            EnemyCombatant enemy = combatSystem.Enemy;

            bool isBoss = (runManager != null && runManager.CurrentFloor != null && runManager.CurrentFloor.GetEncounter(0) != null && runManager.CurrentFloor.GetEncounter(0).IsBoss)
                          || (enemy != null && (enemy.CombatantName.ToLower().Contains("goliath") || enemy.CombatantName.ToLower().Contains("lich")));

            int bossPhase = 1;
            if (isBoss && enemy != null)
            {
                if (enemy.CombatantName.ToLower().Contains("goliath") && enemy.CurrentHp < enemy.MaxHp * 0.5f) bossPhase = 2;
                else if (enemy.CombatantName.ToLower().Contains("lich"))
                {
                    if (enemy.CurrentHp < enemy.MaxHp * 0.33f) bossPhase = 3;
                    else if (enemy.CurrentHp < enemy.MaxHp * 0.66f) bossPhase = 2;
                }
            }

            Texture2D heroTex = VisualAssetProvider.GetHeroTexture(player != null ? player.CombatantName : "hero_rune_knight");
            Texture2D enemyTex = VisualAssetProvider.GetEnemyTexture(enemy != null ? enemy.CombatantName : "enemy_sewer_rat", isBoss, bossPhase);

            // 1. Draw 2D Interactive Battle Arena Stage (Hero vs Villain Cards + Avatars + Health + Stats + VFX)
            Rect stageRect = GUILayoutUtility.GetRect(hudWidth - 40, 240f);
            if (CombatStageVisualController.Instance != null)
            {
                CombatStageVisualController.Instance.DrawBattleArenaStage(
                    stageRect,
                    heroTex,
                    player != null ? player.CombatantName : "Rune Knight",
                    player != null ? player.CurrentHp : 100,
                    player != null ? player.MaxHp : 100,
                    player != null ? player.Armor : 0,
                    player != null ? player.BaseAttackDamage + player.ActiveRuneBonus : 10,
                    enemyTex,
                    enemy != null ? enemy.CombatantName : "Sewer Rat",
                    enemy != null ? enemy.CurrentHp : 30,
                    enemy != null ? enemy.MaxHp : 30,
                    enemy != null ? enemy.Armor : 0,
                    enemy != null ? enemy.BaseAttackDamage : 5,
                    isBoss,
                    bossPhase
                );
            }
            else
            {
                DrawFallbackStage(stageRect, heroTex, enemyTex, player, enemy, isBoss, bossPhase);
            }

            GUILayout.Space(8);

            // 2. Action Log & Combos
            if (combatSystem.Combo != null && combatSystem.Combo.CurrentCombo > 0)
            {
                GUIStyle comboStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                comboStyle.fontSize = 16;
                comboStyle.fontStyle = FontStyle.Bold;
                comboStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
                GUILayout.Label($"COMBO: {combatSystem.Combo.CurrentCombo}x  |  MULTIPLIER: {combatSystem.Combo.ComboMultiplier:0.00}x DMG", comboStyle);
            }

            GUIStyle logStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            logStyle.fontSize = 14;
            logStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUILayout.Label($"<i>Log: {_combatLog}</i>", logStyle);
            GUILayout.Space(6);

            // 3. Combat Controls (Touch friendly >= 65dp)
            GUILayout.BeginHorizontal();

            if (combatSystem.CurrentState == CombatState.Preparing && !_isShowingRewards)
            {
                if (LattiruneUITheme.DrawPrimaryButton("START BATTLE", 65f))
                {
                    combatSystem.StartCombat();
                }
            }
            else if (combatSystem.CurrentState == CombatState.Fighting)
            {
                string speedLabel = combatSystem.SpeedMultiplier switch
                {
                    >= 3.0f => "SPEED: 3.0x",
                    >= 2.0f => "SPEED: 2.0x",
                    _ => "SPEED: 1.0x"
                };

                if (LattiruneUITheme.DrawSecondaryButton(speedLabel, 65f))
                {
                    float nextSpeed = combatSystem.SpeedMultiplier switch
                    {
                        >= 3.0f => 1.0f,
                        >= 2.0f => 3.0f,
                        _ => 2.0f
                    };
                    combatSystem.SetSpeedMultiplier(nextSpeed);
                }

                GUILayout.Space(12);

                if (LattiruneUITheme.DrawSecondaryButton("POTION (+25 HP)", 65f))
                {
                    combatSystem.UseEmergencyPotion(player, 25);
                }
            }
            else if (combatSystem.CurrentState == CombatState.Defeat)
            {
                if (runManager != null && runManager.CanRevivePlayer)
                {
                    if (LattiruneUITheme.DrawPrimaryButton("REVIVE (50% HP)", 65f))
                    {
                        runManager.RevivePlayer(0.5f);
                    }
                    GUILayout.Space(12);
                }

                if (LattiruneUITheme.DrawDangerButton("RETRY ENCOUNTER", 65f))
                {
                    combatSystem.ResetCombat();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }

        private void DrawFallbackStage(Rect stageRect, Texture2D heroTex, Texture2D enemyTex, PlayerCombatant player, EnemyCombatant enemy, bool isBoss, int bossPhase)
        {
            float cardW = (stageRect.width - 20f) * 0.5f;
            
            // Hero Card
            Rect heroCard = new Rect(stageRect.x, stageRect.y, cardW, stageRect.height);
            LattiruneUITheme.DrawCard(heroCard);
            if (heroTex != null) GUI.DrawTexture(new Rect(heroCard.x + 10, heroCard.y + 10, 100, 100), heroTex, ScaleMode.ScaleToFit);

            // Enemy Card
            Rect enemyCard = new Rect(stageRect.x + cardW + 20f, stageRect.y, cardW, stageRect.height);
            LattiruneUITheme.DrawCard(enemyCard);
            if (enemyTex != null) GUI.DrawTexture(new Rect(enemyCard.x + 10, enemyCard.y + 10, 100, 100), enemyTex, ScaleMode.ScaleToFit);
        }

        private void DrawRewardSelectionModal()
        {
            float modalWidth = 960f;
            float modalHeight = 1300f;
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float posX = (1080f - modalWidth) * 0.5f;
            float posY = 300f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, modalWidth, modalHeight), "VICTORY REWARDS");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, modalWidth - 80, modalHeight - 80));

            LattiruneUITheme.DrawHeader("VICTORY REWARDS", "Select ONE reward to reinforce your build:");
            GUILayout.Space(16);

            for (int i = 0; i < _currentRewardOptions.Count; i++)
            {
                var option = _currentRewardOptions[i];
                if (option == null) continue;

                bool isSelected = (_selectedRewardOption == option);
                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUILayout.BeginHorizontal();

                // Real Item Artwork Icon
                Texture2D itemIcon = VisualAssetProvider.GetItemTexture(option.ItemData != null ? option.ItemData.ItemId : "");
                if (itemIcon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(80f, 80f, GUILayout.Width(80f), GUILayout.Height(80f));
                    GUI.DrawTexture(iconRect, itemIcon, ScaleMode.ScaleToFit);
                    GUILayout.Space(12);
                }

                GUILayout.BeginVertical();
                GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                titleStyle.fontSize = 20;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = isSelected ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorTextPrimary;
                GUILayout.Label(option.DisplayName, titleStyle);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 15;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(option.Description, descStyle);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (isSelected)
                {
                    LattiruneUITheme.DrawBadge("CLAIMED & STAGED", LattiruneUITheme.ColorGoldPrimary);
                }
                else
                {
                    if (LattiruneUITheme.DrawPrimaryButton($"CLAIM {option.DisplayName.ToUpper()}", 55f))
                    {
                        SelectReward(option);
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(12);
            }

            GUILayout.FlexibleSpace();

            if (_selectedRewardOption != null)
            {
                if (LattiruneUITheme.DrawPrimaryButton("CONTINUE DESCENT", 75f))
                {
                    CloseRewardScreenAndContinue();
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
