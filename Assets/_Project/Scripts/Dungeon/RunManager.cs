using System;
using UnityEngine;
using Lattirune.Boss;
using Lattirune.Combat;
using Lattirune.Items;
using Lattirune.UI;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Master state machine coordinator for multi-floor dungeon run progression.
    /// Manages floor transitions, encounter sequencing, boss encounters, victory/defeat lifecycle, and run completion.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DungeonDefinitionSO dungeonDefinition;

        [Header("Systems")]
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private BossSystem bossSystem;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private EnemyCombatant enemyCombatant;

        [Header("Runtime State")]
        [SerializeField] private RunState currentState = RunState.NotStarted;
        [SerializeField] private int currentFloorIndex = 0;
        [SerializeField] private int currentEncounterIndex = 0;

        public event Action<RunState> OnStateChanged;
        public event Action<int, DungeonFloorDefinitionSO> OnFloorStarted;
        public event Action<EncounterDefinitionSO> OnEncounterStarted;
        public event Action OnEncounterVictory;
        public event Action OnRewardPhaseStarted;
        public event Action<int> OnFloorCompleted;
        public event Action OnRunCompleted;
        public event Action OnRunDefeated;

        public DungeonDefinitionSO Dungeon => dungeonDefinition;
        public RunState CurrentState => currentState;
        public int CurrentFloorIndex => currentFloorIndex;
        public int CurrentFloorNumber => currentFloorIndex + 1;
        public int CurrentEncounterIndex => currentEncounterIndex;
        public int TotalFloors => dungeonDefinition != null ? dungeonDefinition.TotalFloorCount : 0;
        public DungeonFloorDefinitionSO CurrentFloor => dungeonDefinition != null ? dungeonDefinition.GetFloor(currentFloorIndex) : null;
        public EncounterDefinitionSO CurrentEncounter => CurrentFloor != null ? CurrentFloor.GetEncounter(currentEncounterIndex) : null;
        public bool IsFinalFloor => dungeonDefinition != null && currentFloorIndex >= dungeonDefinition.TotalFloorCount - 1;
        public bool IsFinalEncounterOnFloor => CurrentFloor != null && currentEncounterIndex >= CurrentFloor.EncounterCount - 1;
        public bool IsRunFinished => currentState == RunState.RunComplete || currentState == RunState.Defeated;
        public BossSystem Boss => bossSystem;

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
            BossSystem boss = null)
        {
            dungeonDefinition = dungeon;
            EnsureDefaultDungeon();

            combatSystem = combat;
            bossSystem = boss;
            rewardService = rewards;
            playerCombatant = player;
            enemyCombatant = enemy;

            currentState = RunState.NotStarted;
            currentFloorIndex = 0;
            currentEncounterIndex = 0;

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
            EnsureDefaultDungeon();

            currentFloorIndex = 0;
            currentEncounterIndex = 0;

            SetState(RunState.Starting);
            SetState(RunState.FloorPreparing);

            OnFloorStarted?.Invoke(CurrentFloorNumber, CurrentFloor);
            PrepareCurrentEncounter();
            return true;
        }

        public void PrepareCurrentEncounter()
        {
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
                    enemyCombatant.SetupCustom(
                        name: CurrentEncounter.EnemyName,
                        hp: CurrentEncounter.EnemyHp,
                        baseArmor: CurrentEncounter.EnemyArmor,
                        attack: CurrentEncounter.EnemyAttack,
                        interval: CurrentEncounter.AttackInterval,
                        traits: CurrentEncounter.EnemyTraits
                    );
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

        public void ResetRun()
        {
            currentFloorIndex = 0;
            currentEncounterIndex = 0;
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

        public void RestoreRunState(int floorIdx, int encIdx, RunState state)
        {
            EnsureDefaultDungeon();
            currentFloorIndex = Mathf.Clamp(floorIdx, 0, Mathf.Max(0, TotalFloors - 1));
            currentEncounterIndex = encIdx;
            currentState = state;

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
