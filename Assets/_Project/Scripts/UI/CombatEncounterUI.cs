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
                    navigation.NavigateTo(ScreenState.MAIN_MENU);
                }
                else
                {
                    navigation.NavigateTo(ScreenState.GRID_BUILD);
                }
            }
        }

        private void OnGUI()
        {
            if (navigation != null && navigation.CurrentScreen != ScreenState.GRID_BUILD && navigation.CurrentScreen != ScreenState.COMBAT)
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
            float posX = (1080f - hudWidth) * 0.5f;
            float posY = 20f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.94f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, hudWidth, hudHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 24, posY + 16, hudWidth - 48, hudHeight - 32));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 22;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 18;
            textStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 20;
            btnStyle.fontStyle = FontStyle.Bold;

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
                eliteAffixBadge = $" <color=#ef4444>[💀 {enemy.EliteAffix.ToString().ToUpper()}: {affixDesc}]</color>";
            }

            GUIStyle floorHeaderStyle = new GUIStyle(GUI.skin.label);
            floorHeaderStyle.fontSize = 18;
            floorHeaderStyle.fontStyle = FontStyle.Bold;
            floorHeaderStyle.normal.textColor = new Color(0.77f, 0.61f, 0.15f);

            GUILayout.Label($"── {floorTitle} ──", floorHeaderStyle);

            string flameNote = player.HasActiveSynergy ? " <color=#f97316>[🔥 FLAME SYNERGY]</color>" : "";
            GUILayout.Label($"<b>HERO HP:</b> {player.CurrentHp}/{player.MaxHp} | <b>DEF:</b> {player.Armor} | <b>ATK:</b> {player.BaseAttackDamage}+{player.ActiveRuneBonus}{flameNote}", textStyle);
            GUILayout.Label($"<b>{enemy.CombatantName} HP:</b> {enemy.CurrentHp}/{enemy.MaxHp} | <b>DEF:</b> {enemy.Armor} | <b>ATK:</b> {enemy.BaseAttackDamage}{eliteAffixBadge}", textStyle);

            if (combatSystem.Combo != null && combatSystem.Combo.CurrentCombo > 0)
            {
                GUIStyle comboStyle = new GUIStyle(GUI.skin.label);
                comboStyle.fontSize = 18;
                comboStyle.fontStyle = FontStyle.Bold;
                comboStyle.normal.textColor = Color.yellow;
                GUILayout.Label($"⚡ COMBO: {combatSystem.Combo.CurrentCombo}x  |  MULT: {combatSystem.Combo.ComboMultiplier:0.00}x", comboStyle);
            }

            GUILayout.Label($"<size=15><i>Log: {_combatLog}</i></size>", textStyle);
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();

            // Battle Start Button (in Preparing State)
            if (combatSystem.CurrentState == CombatState.Preparing && !_isShowingRewards)
            {
                GUI.color = Color.green;
                if (GUILayout.Button("⚔️ START BATTLE", btnStyle, GUILayout.Height(65), GUILayout.Width(320)))
                {
                    combatSystem.StartCombat();
                }
                GUI.color = oldColor;
            }
            // Active Fighting Controls: Speed Multiplier & Emergency Heal
            else if (combatSystem.CurrentState == CombatState.Fighting)
            {
                string speedLabel = combatSystem.SpeedMultiplier switch
                {
                    >= 3.0f => "⏩ SPEED: 3.0x",
                    >= 2.0f => "⏩ SPEED: 2.0x",
                    _ => "▶️ SPEED: 1.0x"
                };

                if (GUILayout.Button(speedLabel, btnStyle, GUILayout.Height(65), GUILayout.Width(240)))
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

                if (GUILayout.Button("🧪 POTION (+25 HP)", btnStyle, GUILayout.Height(65), GUILayout.Width(260)))
                {
                    combatSystem.UseEmergencyPotion(player, 25);
                }
            }
            // Retry and Revive Controls (in Defeat State)
            else if (combatSystem.CurrentState == CombatState.Defeat)
            {
                if (runManager != null && runManager.CanRevivePlayer)
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("❤️ REVIVE (50% HP)", btnStyle, GUILayout.Height(65), GUILayout.Width(300)))
                    {
                        runManager.RevivePlayer(0.5f);
                    }
                    GUI.color = oldColor;
                    GUILayout.Space(12);
                }

                GUI.color = Color.red;
                if (GUILayout.Button("🔄 RETRY ENCOUNTER", btnStyle, GUILayout.Height(65), GUILayout.Width(300)))
                {
                    combatSystem.ResetCombat();
                }
                GUI.color = oldColor;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawRewardSelectionModal()
        {
            float modalWidth = 960f;
            float modalHeight = 1200f;
            float posX = (1080f - modalWidth) * 0.5f;
            float posY = 380f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.98f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, modalWidth, modalHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, modalWidth - 80, modalHeight - 80));

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 36;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.95f, 0.8f, 0.2f); // Gold

            GUILayout.Label("🏆 VICTORY REWARDS 🏆", titleStyle);
            GUILayout.Space(8);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 20;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.white;
            GUILayout.Label("Select ONE reward to reinforce your build:", subStyle);
            GUILayout.Space(24);

            GUIStyle cardBoxStyle = new GUIStyle(GUI.skin.box);
            GUIStyle cardTitleStyle = new GUIStyle(GUI.skin.label);
            cardTitleStyle.fontSize = 22;
            cardTitleStyle.fontStyle = FontStyle.Bold;
            cardTitleStyle.normal.textColor = Color.white;

            GUIStyle descStyle = new GUIStyle(GUI.skin.label);
            descStyle.fontSize = 18;
            descStyle.wordWrap = true;
            descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 20;
            btnStyle.fontStyle = FontStyle.Bold;

            for (int i = 0; i < _currentRewardOptions.Count; i++)
            {
                RewardOption opt = _currentRewardOptions[i];
                if (opt == null) continue;

                bool isSelected = _selectedRewardOption == opt;
                bool isLocked = _selectedRewardOption != null;

                GUILayout.BeginVertical(cardBoxStyle);

                string selectState = isSelected ? " <color=#4ade80>[SELECTED]</color>" : "";
                GUILayout.Label($"<b>{opt.DisplayName}</b> ({opt.Footprint.x}x{opt.Footprint.y} {opt.Category}){selectState}", cardTitleStyle);
                GUILayout.Space(4);
                GUILayout.Label(opt.Description, descStyle);
                GUILayout.Space(8);

                GUI.enabled = !isLocked;
                if (isSelected) GUI.color = Color.green;
                if (GUILayout.Button(isSelected ? "REWARD CHOSEN" : "CLAIM REWARD", btnStyle, GUILayout.Height(65)))
                {
                    SelectReward(opt);
                }
                GUI.color = oldColor;
                GUI.enabled = true;

                GUILayout.EndVertical();
                GUILayout.Space(12);
            }

            GUILayout.Space(20);

            // Continue Button (enabled after a reward is chosen)
            if (_selectedRewardOption != null)
            {
                GUI.color = Color.cyan;
                if (GUILayout.Button("PROCEED TO NEXT FLOOR ➔", btnStyle, GUILayout.Height(65)))
                {
                    CloseRewardScreenAndContinue();
                }
                GUI.color = oldColor;
            }

            GUILayout.EndArea();
        }
    }
}
