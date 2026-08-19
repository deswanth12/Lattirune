using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Combo;
using Lattirune.Economy;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Modifiers;
using Lattirune.Progression;
using Lattirune.Runes;
using Lattirune.UI;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Master state machine coordinator for multi-floor dungeon run progression.
    /// Manages floor transitions, encounter sequencing, in-run economy (Gold/Embers),
    /// Merchant Stall transactions, Campfire Rest Site decisions, boss encounters, run modifiers, combo rewards, and run lifecycle.
    /// Strictly adheres to PLAN.md Sections 9.1, 11, and 13.1.
    /// </summary>
    public class RunManager : MonoBehaviour, IEconomyService
    {
        [Header("Configuration")]
        [SerializeField] private DungeonDefinitionSO dungeonDefinition;

        [Header("Systems")]
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private BossSystem bossSystem;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private EnemyCombatant enemyCombatant;
        [SerializeField] private MetaProgressionManager metaProgression;
        [SerializeField] private RunModifierManager modifierManager;
        [SerializeField] private ComboTracker comboTracker;

        [Header("Runtime State")]
        [SerializeField] private RunState currentState = RunState.NotStarted;
        [SerializeField] private int currentFloorIndex = 0;
        [SerializeField] private int currentEncounterIndex = 0;

        [Header("In-Run Economy (PLAN.md Section 13.1)")]
        [SerializeField] private int currentGold = 0;
        [SerializeField] private int currentEmbers = 0;

        [Header("Campfire State (PLAN.md Section 11)")]
        [SerializeField] private bool campfireChoiceResolved = false;
        [SerializeField] private bool hasUsedReviveThisRun = false;
        [SerializeField] private bool isEndlessMode = false;
        private readonly Dictionary<string, int> _runtimeRuneUpgrades = new Dictionary<string, int>();
        private bool _victoryRewardsGranted = false;

        public event Action<RunState> OnStateChanged;
        public event Action<int, DungeonFloorDefinitionSO> OnFloorStarted;
        public event Action<EncounterDefinitionSO> OnEncounterStarted;
        public event Action OnEncounterVictory;
        public event Action OnRewardPhaseStarted;
        public event Action<int> OnFloorCompleted;
        public event Action OnRunCompleted;
        public event Action OnRunDefeated;

        public event Action<int> OnGoldChanged;
        public event Action<int> OnEmbersChanged;
        public event Action<ItemDataSO> OnItemPurchased;
        public event Action<RuneData> OnRunePurchased;
        public event Action OnBagExpansionPurchased;
        public event Action<int> OnCampfireHealed;
        public event Action<string> OnCampfireRuneUpgraded;

        public DungeonDefinitionSO Dungeon => dungeonDefinition;
        public RunState CurrentState => currentState;
        public int CurrentFloorIndex => currentFloorIndex;
        public int CurrentFloorNumber => currentFloorIndex + 1;
        public int CurrentEncounterIndex => currentEncounterIndex;
        public int TotalFloors => dungeonDefinition != null ? dungeonDefinition.TotalFloorCount : 0;
        public DungeonFloorDefinitionSO CurrentFloor => GetCurrentOrEndlessFloor();
        public EncounterDefinitionSO CurrentEncounter => CurrentFloor != null ? CurrentFloor.GetEncounter(currentEncounterIndex) : null;
        public bool IsFinalFloor => !isEndlessMode && dungeonDefinition != null && currentFloorIndex >= dungeonDefinition.TotalFloorCount - 1;
        public bool IsFinalEncounterOnFloor => CurrentFloor != null && currentEncounterIndex >= CurrentFloor.EncounterCount - 1;
        public bool IsRunFinished => currentState == RunState.RunComplete || currentState == RunState.Defeated;
        public BossSystem Boss => bossSystem;

        public int CurrentGold => currentGold;
        public int CurrentEmbers => currentEmbers;
        public bool IsCampfireResolved => campfireChoiceResolved;
        public bool IsMerchantFloor => CurrentFloorNumber == 4 || CurrentFloorNumber == 9;
        public bool IsCampfireFloor => CurrentFloorNumber == 8;
        public bool HasUsedReviveThisRun => hasUsedReviveThisRun;
        public bool CanRevivePlayer => currentState == RunState.Defeated && !hasUsedReviveThisRun;
        public bool IsEndlessMode => isEndlessMode;
        public int EndlessTier => isEndlessMode ? Mathf.Max(1, currentFloorIndex - 9) : 0;

        public void EnableEndlessMode()
        {
            isEndlessMode = true;
        }

        private DungeonFloorDefinitionSO GetCurrentOrEndlessFloor()
        {
            if (dungeonDefinition == null) return null;

            if (currentFloorIndex < dungeonDefinition.TotalFloorCount)
            {
                return dungeonDefinition.GetFloor(currentFloorIndex);
            }

            // Procedurally generate endless floor
            int endlessTier = currentFloorIndex - dungeonDefinition.TotalFloorCount + 1;
            int scaledHp = Mathf.RoundToInt(120 * (1.0f + 0.15f * endlessTier));
            int scaledAtk = Mathf.RoundToInt(15 * (1.0f + 0.10f * endlessTier));

            var floor = ScriptableObject.CreateInstance<DungeonFloorDefinitionSO>();
            var enc = ScriptableObject.CreateInstance<EncounterDefinitionSO>();
            enc.Initialize(
                id: $"endless_enc_{currentFloorIndex}",
                name: $"Abyssal Horror Mk.{endlessTier}",
                eName: $"Abyssal Horror Mk.{endlessTier}",
                hp: scaledHp,
                armor: 4 + endlessTier,
                attack: scaledAtk,
                interval: 2.0f,
                boss: (CurrentFloorNumber % 5 == 0)
            );

            floor.Initialize(CurrentFloorNumber, $"floor_endless_{CurrentFloorNumber}", $"Abyssal Depths - Level {endlessTier}", new List<EncounterDefinitionSO> { enc });
            return floor;
        }

        private void Awake()
        {
            EnsureDefaultDungeon();
        }

        public void EnsureDefaultDungeon()
        {
            if (dungeonDefinition == null)
            {
                dungeonDefinition = DungeonDefinitionSO.Create10FloorCursedSewersDungeon();
            }
        }

        public void Initialize(
            DungeonDefinitionSO dungeon,
            CombatSystem combat,
            RewardService rewards,
            PlayerCombatant player,
            EnemyCombatant enemy,
            BossSystem boss = null,
            MetaProgressionManager meta = null,
            RunModifierManager modifiers = null,
            ComboTracker tracker = null)
        {
            dungeonDefinition = dungeon;
            EnsureDefaultDungeon();

            combatSystem = combat;
            bossSystem = boss;
            rewardService = rewards;
            playerCombatant = player;
            enemyCombatant = enemy;
            metaProgression = meta;
            modifierManager = modifiers;
            comboTracker = tracker;

            currentState = RunState.NotStarted;
            currentFloorIndex = 0;
            currentEncounterIndex = 0;
            currentGold = 0;
            currentEmbers = 0;
            campfireChoiceResolved = false;
            _victoryRewardsGranted = false;
            _runtimeRuneUpgrades.Clear();

            if (combatSystem != null)
            {
                combatSystem.OnVictory += HandleCombatVictory;
                combatSystem.OnDefeat += HandleCombatDefeat;
            }
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnVictory -= HandleCombatVictory;
                combatSystem.OnDefeat -= HandleCombatDefeat;
            }
        }

        public bool StartRun()
        {
            return StartRun(metaProgression);
        }

        public bool StartRun(MetaProgressionManager meta)
        {
            EnsureDefaultDungeon();

            metaProgression = meta ?? metaProgression;

            currentFloorIndex = 0;
            currentEncounterIndex = 0;
            currentGold = metaProgression != null ? metaProgression.GetStartingGoldBonus() : 0;
            currentEmbers = 0;
            campfireChoiceResolved = false;
            hasUsedReviveThisRun = false;
            _victoryRewardsGranted = false;
            _runtimeRuneUpgrades.Clear();

            if (playerCombatant != null)
            {
                int bonusHp = metaProgression != null ? metaProgression.GetStartingHpBonus() : 0;
                playerCombatant.SetStats(100 + bonusHp, playerCombatant.Armor, playerCombatant.AttackInterval);
                playerCombatant.ResetHpToFull();
            }

            SetState(RunState.Starting);
            SetState(RunState.FloorPreparing);

            OnFloorStarted?.Invoke(CurrentFloorNumber, CurrentFloor);
            PrepareCurrentEncounter();
            return true;
        }

        public void PrepareCurrentEncounter()
        {
            _victoryRewardsGranted = false;
            if (CurrentFloorNumber != 8)
            {
                campfireChoiceResolved = false;
            }

            if (CurrentEncounter == null) return;

            if (CurrentEncounter.IsBoss && bossSystem != null)
            {
                bossSystem.StartBossFight();
            }
            else
            {
                if (bossSystem != null)
                {
                    bossSystem.StopBossFight();
                }

                if (enemyCombatant != null)
                {
                    int effectiveHp = CurrentEncounter.EnemyHp;
                    int effectiveAttack = CurrentEncounter.EnemyAttack;
                    int effectiveArmor = CurrentEncounter.EnemyArmor;
                    float effectiveInterval = CurrentEncounter.AttackInterval;

                    // Endless Mode Post-Floor 10 Exponential Stat Scaling (TASK B)
                    if (isEndlessMode && currentFloorIndex >= 10)
                    {
                        int endlessTier = currentFloorIndex - 9;
                        float hpScale = Mathf.Pow(1.18f, endlessTier);
                        float atkScale = Mathf.Pow(1.12f, endlessTier);
                        effectiveHp = Mathf.RoundToInt(effectiveHp * hpScale);
                        effectiveAttack = Mathf.RoundToInt(effectiveAttack * atkScale);
                        effectiveArmor += endlessTier * 3;
                    }

                    if (modifierManager != null)
                    {
                        float hpMultiplier = modifierManager.GetAggregateMultiplier(RunModifierType.EnemyHealthMultiplier, 1.0f);
                        effectiveHp = Mathf.RoundToInt(effectiveHp * hpMultiplier);
                    }

                    enemyCombatant.SetupCustom(
                        name: CurrentEncounter.EnemyName,
                        hp: effectiveHp,
                        baseArmor: effectiveArmor,
                        attack: effectiveAttack,
                        interval: effectiveInterval,
                        traits: CurrentEncounter.EnemyTraits
                    );

                    // Apply Randomized Elite Affix to Elites & Endless Champions (TASK A)
                    bool isEliteEncounter = CurrentFloorNumber == 5 || CurrentFloorNumber == 7 || 
                                           (CurrentEncounter.DisplayName != null && CurrentEncounter.DisplayName.Contains("Elite")) ||
                                           (isEndlessMode && currentFloorIndex >= 10 && currentEncounterIndex > 0);

                    if (isEliteEncounter && !CurrentEncounter.IsBoss)
                    {
                        EliteAffixType[] affixPool = new[]
                        {
                            EliteAffixType.Vampiric,
                            EliteAffixType.Juggernaut,
                            EliteAffixType.Frenzied,
                            EliteAffixType.MoltenAura,
                            EliteAffixType.ToxicThorns
                        };
                        int affixIndex = (currentFloorIndex + currentEncounterIndex) % affixPool.Length;
                        enemyCombatant.ApplyEliteAffix(affixPool[affixIndex]);
                    }
                }
            }

            if (playerCombatant != null)
            {
                playerCombatant.ResetHpToFull();
            }

            if (combatSystem != null)
            {
                combatSystem.ResetCombat();
            }

            OnEncounterStarted?.Invoke(CurrentEncounter);
        }

        public void StartEncounterCombat()
        {
            if (currentState != RunState.FloorPreparing && currentState != RunState.FloorTransition)
            {
                return;
            }

            SetState(RunState.EncounterActive);
            if (combatSystem != null)
            {
                combatSystem.StartCombat();
            }
        }

        private void HandleCombatVictory()
        {
            if (currentState != RunState.EncounterActive) return;

            // Grant in-run economy rewards according to PLAN.md Section 13.1
            if (!_victoryRewardsGranted)
            {
                _victoryRewardsGranted = true;
                if (CurrentEncounter != null && CurrentEncounter.IsBoss)
                {
                    AddEmbers(EconomyManager.GetBossEmbersDrop());
                }
                else
                {
                    bool isElite = CurrentFloorNumber == 3 || CurrentFloorNumber == 5 || CurrentFloorNumber == 7 || 
                                   (CurrentEncounter != null && (CurrentEncounter.EnemyHp >= 100 || CurrentEncounter.DisplayName.Contains("Elite")));
                    int goldDrop = EconomyManager.GetGoldDrop(isElite);

                    // Endless scaling bonus drops
                    if (isEndlessMode && currentFloorIndex >= 10)
                    {
                        int endlessTier = currentFloorIndex - 9;
                        goldDrop = Mathf.RoundToInt(goldDrop * Mathf.Pow(1.15f, endlessTier));
                        AddEmbers(Mathf.RoundToInt(2 * endlessTier));
                    }

                    if (modifierManager != null)
                    {
                        float goldMult = modifierManager.GetAggregateMultiplier(RunModifierType.GoldMultiplier, 1.0f);
                        goldDrop = Mathf.RoundToInt(goldDrop * goldMult);
                    }
                    AddGold(goldDrop);
                }

                // Grant Combo and Elemental Chain Reaction Bonus Rewards (TASK-050 & TASK-054)
                if (comboTracker != null)
                {
                    var chainOutcome = ChainReactionRewardCalculator.CalculateReward(comboTracker.CurrentCombo, comboTracker.ConsecutiveReactions);
                    if (chainOutcome.BonusGold > 0)
                    {
                        AddGold(chainOutcome.BonusGold);
                    }
                    if (chainOutcome.BonusEmbers > 0)
                    {
                        AddEmbers(chainOutcome.BonusEmbers);
                    }
                }
            }

            SetState(RunState.RewardSelection);
            OnEncounterVictory?.Invoke();
            OnRewardPhaseStarted?.Invoke();
        }

        private void HandleCombatDefeat()
        {
            if (currentState != RunState.EncounterActive) return;

            SetState(RunState.Defeated);
            OnRunDefeated?.Invoke();
        }

        /// <summary>
        /// Revives the player during a Defeated state (e.g. via rewarded ad or No-Ads revive perk).
        /// Restores a percentage of Max HP and resumes the encounter. Strictly limited to once per run.
        /// Adheres strictly to PLAN.md Section 27.
        /// </summary>
        public bool RevivePlayer(float hpFraction = 0.5f)
        {
            if (currentState != RunState.Defeated || hasUsedReviveThisRun)
            {
                return false;
            }

            hasUsedReviveThisRun = true;
            if (playerCombatant != null)
            {
                int restoreHp = Mathf.Max(1, Mathf.RoundToInt(playerCombatant.MaxHp * hpFraction));
                playerCombatant.Revive(restoreHp);
            }

            SetState(RunState.EncounterActive);
            if (combatSystem != null)
            {
                combatSystem.StartCombat();
            }

            return true;
        }

        public void ContinueAfterReward()
        {
            if (currentState != RunState.RewardSelection) return;

            if (IsFinalEncounterOnFloor)
            {
                OnFloorCompleted?.Invoke(CurrentFloorNumber);

                if (IsFinalFloor)
                {
                    SetState(RunState.RunComplete);
                    OnRunCompleted?.Invoke();
                }
                else
                {
                    SetState(RunState.FloorTransition);
                    currentFloorIndex++;
                    currentEncounterIndex = 0;

                    SetState(RunState.FloorPreparing);
                    OnFloorStarted?.Invoke(CurrentFloorNumber, CurrentFloor);
                    PrepareCurrentEncounter();
                }
            }
            else
            {
                currentEncounterIndex++;
                SetState(RunState.FloorPreparing);
                PrepareCurrentEncounter();
            }
        }

        public void PauseForEvent()
        {
            if (currentState != RunState.EncounterActive && !IsRunFinished)
            {
                SetState(RunState.EventActive);
            }
        }

        public void ResumeFromEvent()
        {
            if (currentState == RunState.EventActive)
            {
                SetState(RunState.FloorPreparing);
                PrepareCurrentEncounter();
            }
        }

        // ==========================================
        // IN-RUN ECONOMY OPERATIONS (Section 13.1)
        // ==========================================

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || currentGold < amount) return false;
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }

        public void AddEmbers(int amount)
        {
            if (amount <= 0) return;
            currentEmbers += amount;
            OnEmbersChanged?.Invoke(currentEmbers);
        }

        public bool CanAfford(int amount)
        {
            return amount > 0 && currentGold >= amount;
        }

        // ==========================================
        // MERCHANT STALL PURCHASES (Floor 4 & 9)
        // ==========================================

        public bool PurchaseCommonItem(ItemDataSO item)
        {
            int price = EconomyManager.GetCommonItemPrice();
            if (!SpendGold(price)) return false;

            OnItemPurchased?.Invoke(item);
            return true;
        }

        public bool PurchaseRareItem(ItemDataSO item)
        {
            int price = EconomyManager.GetRareItemPrice();
            if (!SpendGold(price)) return false;

            OnItemPurchased?.Invoke(item);
            return true;
        }

        public bool PurchaseRune(RuneData rune)
        {
            int price = EconomyManager.GetRunePrice();
            if (!SpendGold(price)) return false;

            OnRunePurchased?.Invoke(rune);
            return true;
        }

        public bool PurchaseBagExpansion(InventorySystem invSystem = null)
        {
            int price = EconomyManager.GetBagExpansionPrice();
            if (!SpendGold(price)) return false;

            if (invSystem != null)
            {
                invSystem.ExpandStorage();
            }
            OnBagExpansionPurchased?.Invoke();
            return true;
        }

        // ==========================================
        // CAMPFIRE REST SITE CHOICES (Floor 8)
        // ==========================================

        public bool ResolveCampfireHeal(PlayerCombatant player)
        {
            if (campfireChoiceResolved || player == null || !player.IsAlive) return false;

            int healAmount = Mathf.RoundToInt(player.MaxHp * 0.40f);
            player.Heal(healAmount);

            campfireChoiceResolved = true;
            OnCampfireHealed?.Invoke(healAmount);
            return true;
        }

        public void UpgradeRune(string runeId, int powerBonus = 2)
        {
            if (string.IsNullOrEmpty(runeId)) return;

            if (!_runtimeRuneUpgrades.ContainsKey(runeId))
            {
                _runtimeRuneUpgrades[runeId] = 0;
            }
            _runtimeRuneUpgrades[runeId] += powerBonus;
            OnCampfireRuneUpgraded?.Invoke(runeId);
        }

        public bool ResolveCampfireRuneUpgrade(string runeId)
        {
            if (campfireChoiceResolved || string.IsNullOrEmpty(runeId)) return false;

            UpgradeRune(runeId, 2);
            campfireChoiceResolved = true;
            return true;
        }

        public int GetRuntimeRuneUpgrade(string runeId)
        {
            if (!string.IsNullOrEmpty(runeId) && _runtimeRuneUpgrades.TryGetValue(runeId, out int bonus))
            {
                return bonus;
            }
            return 0;
        }

        public void ResetRun()
        {
            currentFloorIndex = 0;
            currentEncounterIndex = 0;
            currentGold = 0;
            currentEmbers = 0;
            campfireChoiceResolved = false;
            _victoryRewardsGranted = false;
            _runtimeRuneUpgrades.Clear();

            SetState(RunState.NotStarted);

            if (bossSystem != null)
            {
                bossSystem.ResetBoss();
            }

            if (combatSystem != null)
            {
                combatSystem.ResetCombat();
            }
        }

        public void RestoreRunState(int floorIdx, int encIdx, RunState state, int gold = 0, int embers = 0)
        {
            EnsureDefaultDungeon();
            currentFloorIndex = isEndlessMode ? Mathf.Max(0, floorIdx) : Mathf.Clamp(floorIdx, 0, Mathf.Max(0, TotalFloors - 1));
            currentEncounterIndex = encIdx;
            currentState = state;
            currentGold = Mathf.Max(0, gold);
            currentEmbers = Mathf.Max(0, embers);

            PrepareCurrentEncounter();
            OnStateChanged?.Invoke(currentState);
        }

        private void SetState(RunState nextState)
        {
            currentState = nextState;
            OnStateChanged?.Invoke(currentState);
        }
    }
}
