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
                combatSystem.OnStateChanged += HandleCombatStateChanged;
            }

            if (rewardService != null)
            {
                rewardService.OnRewardApplied += HandleRewardApplied;
            }
        }

        public void Initialize(
            CombatSystem combat, 
            SynergySystem synergy, 
            RewardService service, 
            List<ItemDataSO> catalogue,
            Transform spawnParent,
            ScreenNavigationController nav)
        {
            navigation = nav;
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

            if (combatSystem != null)
            {
                combatSystem.ResetCombat();
            }
        }

        private void OnGUI()
        {
            if (navigation != null && navigation.CurrentScreen != ScreenState.GRID_BUILD && navigation.CurrentScreen != ScreenState.COMBAT)
            {
                return;
            }

            if (combatSystem == null || combatSystem.Player == null || combatSystem.Enemy == null) return;

            GUIStyle panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.fontSize = 13;
            panelStyle.alignment = TextAnchor.UpperLeft;

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;

            // 1. Top Combat HUD Panel (360x220)
            GUILayout.BeginArea(new Rect(20, 20, 360, 220), panelStyle);

            GUILayout.Label($"[STATUS: {combatSystem.CurrentState.ToString().ToUpper()}]", headerStyle);
            GUILayout.Space(4);

            PlayerCombatant player = combatSystem.Player;
            string flameNote = player.HasActiveSynergy ? " [🔥 FLAMEBOUND EDGE ACTIVE]" : "";
            GUILayout.Label($"Hero HP: {player.CurrentHp}/{player.MaxHp} | DEF: {player.Armor} | ATK: {player.BaseAttackDamage}+{player.ActiveRuneBonus}{flameNote}");

            EnemyCombatant enemy = combatSystem.Enemy;
            GUILayout.Label($"{enemy.CombatantName} HP: {enemy.CurrentHp}/{enemy.MaxHp} | DEF: {enemy.Armor} | ATK: {enemy.BaseAttackDamage}");

            GUILayout.Space(6);
            GUILayout.Label($"Log: {_combatLog}");
            GUILayout.Space(6);

            // Battle Start Button (in Preparing State)
            if (combatSystem.CurrentState == CombatState.Preparing && !_isShowingRewards)
            {
                if (GUILayout.Button("START BATTLE", GUILayout.Height(40)))
                {
                    combatSystem.StartCombat();
                }
            }
            // Retry Button (in Defeat State)
            else if (combatSystem.CurrentState == CombatState.Defeat)
            {
                if (GUILayout.Button("RETRY ENCOUNTER", GUILayout.Height(40)))
                {
                    combatSystem.ResetCombat();
                }
            }

            GUILayout.EndArea();

            // 2. Victory Reward Selection Overlay (Portrait Center Modal)
            if (_isShowingRewards && _currentRewardOptions != null && _currentRewardOptions.Count > 0)
            {
                DrawRewardSelectionModal();
            }
        }

        private void DrawRewardSelectionModal()
        {
            float modalWidth = 360f;
            float modalHeight = 440f;
            float startX = 20f;
            float startY = 250f;

            GUIStyle modalStyle = new GUIStyle(GUI.skin.box);
            modalStyle.fontSize = 13;
            modalStyle.alignment = TextAnchor.UpperCenter;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(new Rect(startX, startY, modalWidth, modalHeight), modalStyle);

            GUILayout.Label("VICTORY REWARDS", titleStyle);
            GUILayout.Label("Select ONE reward to reinforce your build:", GUI.skin.label);
            GUILayout.Space(8);

            for (int i = 0; i < _currentRewardOptions.Count; i++)
            {
                RewardOption opt = _currentRewardOptions[i];
                if (opt == null) continue;

                bool isSelected = _selectedRewardOption == opt;
                bool isLocked = _selectedRewardOption != null;

                GUILayout.BeginVertical(GUI.skin.box);

                string selectState = isSelected ? " [SELECTED]" : "";
                GUILayout.Label($"<b>{opt.DisplayName}</b> ({opt.Footprint.x}x{opt.Footprint.y} {opt.Category}){selectState}");
                GUILayout.Label($"<size=11>{opt.Description}</size>");

                GUI.enabled = !isLocked;
                // Minimum touch target height 52dp compliant (52px in reference canvas GUI)
                if (GUILayout.Button(isSelected ? "SELECTED" : "CHOOSE REWARD", GUILayout.Height(48)))
                {
                    SelectReward(opt);
                }
                GUI.enabled = true;

                GUILayout.EndVertical();
                GUILayout.Space(4);
            }

            GUILayout.Space(8);

            // Continue Button (enabled after a reward is chosen)
            if (_selectedRewardOption != null)
            {
                if (GUILayout.Button("CONTINUE", GUILayout.Height(44)))
                {
                    CloseRewardScreenAndContinue();
                }
            }

            GUILayout.EndArea();
        }
    }
}
