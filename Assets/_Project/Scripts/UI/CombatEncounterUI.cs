using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Items;
using Lattirune.Synergy;

namespace Lattirune.UI
{
    /// <summary>
    /// Unified prototype HUD for combat state, health bars, synergy indicators,
    /// and the post-victory 3-card reward selection flow.
    /// [DEVELOPMENT / PROTOTYPE UI]
    /// </summary>
    public class CombatEncounterUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private SynergySystem synergySystem;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private Lattirune.Dungeon.RunManager runManager;

        [Header("Reward Configuration")]
        [SerializeField] private Transform rewardSpawnParent;
        [SerializeField] private Vector3 rewardSpawnPosition = new Vector3(0f, -4f, 0f);

        private List<ItemDataSO> _itemCatalogue = new List<ItemDataSO>();
        private List<RewardOption> _currentRewardOptions = new List<RewardOption>();
        private RewardOption _selectedRewardOption = null;
        private bool _isShowingRewards = false;
        private string _combatLog = "Arrange items on the 5x5 grid, then tap 'START BATTLE'.";

        public ScreenNavigationController Navigation => navigation;
        public CombatSystem Combat => combatSystem;
        public RewardService Rewards => rewardService;
        public Lattirune.Dungeon.RunManager RunManager => runManager;
        public IReadOnlyList<RewardOption> CurrentRewardOptions => _currentRewardOptions;
        public RewardOption SelectedRewardOption => _selectedRewardOption;
        public bool IsShowingRewards => _isShowingRewards;

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
        }

        private void HandleVictory()
        {
            _combatLog = ">>> VICTORY! Enemy vanquished. Choose a reward. <<<";
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
            _combatLog = ">>> DEFEAT! Player succumbed. Tap 'RETRY' to challenge again. <<<";
            _isShowingRewards = false;
            _selectedRewardOption = null;
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
            if (navigation == null || (navigation.CurrentScreen != ScreenState.GRID_BUILD && navigation.CurrentScreen != ScreenState.COMBAT && navigation.CurrentScreen != ScreenState.REWARD_SELECTION))
            {
                return;
            }

            if (combatSystem == null || combatSystem.Player == null || combatSystem.Enemy == null) return;

            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            DrawCombatTopHUD();

            // Victory Reward Selection Overlay
            if (_isShowingRewards && _currentRewardOptions != null && _currentRewardOptions.Count > 0)
            {
                DrawRewardSelectionModal();
            }

            GUI.matrix = oldMatrix;
        }

        private void DrawCombatTopHUD()
        {
            float hudWidth = 1000f;
            float hudHeight = 360f;
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float posX = (1080f - hudWidth) * 0.5f;
            float posY = 20f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, hudWidth, hudHeight), "COMBAT HUD");

            GUILayout.BeginArea(new Rect(posX + 24, posY + 16, hudWidth - 48, hudHeight - 32));

            PlayerCombatant player = combatSystem.Player;
            EnemyCombatant enemy = combatSystem.Enemy;

            string floorTitle = runManager != null 
                ? (runManager.IsEndlessMode ? $"DUNGEON FLOOR {runManager.CurrentFloorNumber} [ENDLESS TIER {runManager.EndlessTier}]" : $"DUNGEON FLOOR {runManager.CurrentFloorNumber}")
                : "DUNGEON ENCOUNTER";

            string eliteAffixBadge = "";
            if (enemy != null && enemy.EliteAffix != EliteAffixType.None)
            {
                string affixDesc = enemy.EliteAffix switch
                {
                    EliteAffixType.Vampiric => "Leeches 25% DMG as HP",
                    EliteAffixType.Juggernaut => "+40% Max HP & +8 Base Armor",
                    EliteAffixType.Frenzied => "+35% Attack Speed",
                    EliteAffixType.MoltenAura => "+2 ATK & 25% Thorns Reflection",
                    EliteAffixType.ToxicThorns => "Inflicts Poison On Hit",
                    EliteAffixType.Frostbound => "+6 Armor & Chilling Aura",
                    EliteAffixType.Blighted => "+30% Max HP & Healing Suppression",
                    _ => ""
                };
                eliteAffixBadge = $" [ELITE: {enemy.EliteAffix.ToString().ToUpper()}: {affixDesc}]";
            }

            GUIStyle floorHeaderStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            floorHeaderStyle.fontSize = 18;
            floorHeaderStyle.fontStyle = FontStyle.Bold;
            floorHeaderStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;

            GUILayout.Label(floorTitle, floorHeaderStyle);
            GUILayout.Space(4);

            // Health bars
            LattiruneUITheme.DrawProgressBar(player.CurrentHp, player.MaxHp, $"HERO HP: {player.CurrentHp}/{player.MaxHp}  |  DEF: {player.Armor}  |  ATK: {player.BaseAttackDamage}+{player.ActiveRuneBonus}", LattiruneUITheme.ColorGreenHealth, 26f);
            GUILayout.Space(4);
            LattiruneUITheme.DrawProgressBar(enemy.CurrentHp, enemy.MaxHp, $"{enemy.CombatantName} HP: {enemy.CurrentHp}/{enemy.MaxHp}  |  DEF: {enemy.Armor}  |  ATK: {enemy.BaseAttackDamage}{eliteAffixBadge}", LattiruneUITheme.ColorRedDanger, 26f);
            GUILayout.Space(4);

            if (combatSystem.Combo != null && combatSystem.Combo.CurrentCombo > 0)
            {
                GUIStyle comboStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                comboStyle.fontSize = 17;
                comboStyle.fontStyle = FontStyle.Bold;
                comboStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
                GUILayout.Label($"COMBO: {combatSystem.Combo.CurrentCombo}x  |  MULTIPLIER: {combatSystem.Combo.ComboMultiplier:0.00}x DMG", comboStyle);
            }

            GUIStyle textStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            textStyle.fontSize = 15;
            textStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUILayout.Label($"<i>Log: {_combatLog}</i>", textStyle);
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();

            // Battle Start Button (in Preparing State)
            if (combatSystem.CurrentState == CombatState.Preparing && !_isShowingRewards)
            {
                if (LattiruneUITheme.DrawPrimaryButton("START BATTLE", 65f))
                {
                    combatSystem.StartCombat();
                }
            }
            // Active Fighting Controls: Speed Multiplier & Emergency Heal
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
            // Retry and Revive Controls (in Defeat State)
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
        }

        private void DrawRewardSelectionModal()
        {
            float modalWidth = 960f;
            float modalHeight = 1200f;
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float posX = (1080f - modalWidth) * 0.5f;
            float posY = 380f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, modalWidth, modalHeight), "VICTORY REWARDS");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, modalWidth - 80, modalHeight - 80));

            LattiruneUITheme.DrawHeader("VICTORY REWARDS", "Select ONE reward to reinforce your build:");
            GUILayout.Space(20);

            for (int i = 0; i < _currentRewardOptions.Count; i++)
            {
                RewardOption opt = _currentRewardOptions[i];
                if (opt == null) continue;

                bool isSelected = _selectedRewardOption == opt;
                bool isLocked = _selectedRewardOption != null;

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                string selectState = isSelected ? " [CHOSEN]" : "";
                GUIStyle cardTitleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                cardTitleStyle.fontSize = 22;
                cardTitleStyle.fontStyle = FontStyle.Bold;
                cardTitleStyle.normal.textColor = isSelected ? LattiruneUITheme.ColorGoldBright : LattiruneUITheme.ColorTextPrimary;

                GUILayout.Label($"{opt.DisplayName} ({opt.Footprint.x}x{opt.Footprint.y} {opt.Category}){selectState}", cardTitleStyle);
                GUILayout.Space(4);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 17;
                descStyle.wordWrap = true;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(opt.Description, descStyle);
                GUILayout.Space(8);

                GUI.enabled = !isLocked;
                if (isSelected)
                {
                    LattiruneUITheme.DrawPrimaryButton("REWARD CHOSEN", 60f);
                }
                else
                {
                    if (LattiruneUITheme.DrawPrimaryButton("CLAIM REWARD", 60f))
                    {
                        SelectReward(opt);
                    }
                }
                GUI.enabled = true;

                GUILayout.EndVertical();
                GUILayout.Space(12);
            }

            GUILayout.Space(20);

            // Continue Button (enabled after a reward is chosen)
            if (_selectedRewardOption != null)
            {
                if (LattiruneUITheme.DrawPrimaryButton("PROCEED TO NEXT ROOM", 75f))
                {
                    CloseRewardScreenAndContinue();
                }
            }

            GUILayout.EndArea();
        }
    }
}
