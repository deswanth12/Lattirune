using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Progression
{
    /// <summary>
    /// Master coordinator for persistent meta-progression across dungeon runs.
    /// Manages the Ember Wallet, Campfire Hub, Blueprint Forge unlocks, and lifetime run statistics.
    /// Strictly adheres to PLAN.md Section 12, Section 13, and Section 22.
    /// </summary>
    public class MetaProgressionManager : MonoBehaviour
    {
        [Header("Databases")]
        [SerializeField] private BlueprintDatabaseSO blueprintDatabase;

        [Header("Persistent Meta State")]
        [SerializeField] private int embersBalance = 0;
        [SerializeField] private int totalRunsAttempted = 0;
        [SerializeField] private int totalBossClears = 0;

        private readonly HashSet<string> _unlockedBlueprintIds = new HashSet<string>();

        public event Action<int> OnEmbersChanged;
        public event Action<BlueprintDefinitionSO> OnBlueprintUnlocked;
        public event Action<string> OnBlueprintUnlockFailed;
        public event Action OnHubEntered;
        public event Action OnStatsUpdated;

        public BlueprintDatabaseSO Database => blueprintDatabase;
        public int EmbersBalance => embersBalance;
        public int TotalRunsAttempted => totalRunsAttempted;
        public int TotalBossClears => totalBossClears;
        public IReadOnlyCollection<string> UnlockedBlueprintIds => _unlockedBlueprintIds;
        public int UnlockedBlueprintCount => _unlockedBlueprintIds.Count;

        private void Awake()
        {
            EnsureDefaultDatabase();
        }

        public void EnsureDefaultDatabase()
        {
            if (blueprintDatabase == null)
            {
                blueprintDatabase = BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();
            }
        }

        public void Initialize(BlueprintDatabaseSO db = null)
        {
            blueprintDatabase = db ?? BlueprintDatabaseSO.CreateCanonicalBlueprintDatabase();
            embersBalance = 0;
            totalRunsAttempted = 0;
            totalBossClears = 0;
            _unlockedBlueprintIds.Clear();
        }

        // ==========================================
        // EMBER WALLET (PLAN.md Section 12 & 13)
        // ==========================================

        public void AddEmbers(int amount)
        {
            if (amount <= 0) return;
            embersBalance += amount;
            OnEmbersChanged?.Invoke(embersBalance);
        }

        public bool SpendEmbers(int amount)
        {
            if (amount <= 0 || embersBalance < amount) return false;
            embersBalance -= amount;
            OnEmbersChanged?.Invoke(embersBalance);
            return true;
        }

        public bool CanAfford(int cost)
        {
            return cost > 0 && embersBalance >= cost;
        }

        // ==========================================
        // BLUEPRINT FORGE (PLAN.md Section 12)
        // ==========================================

        public bool IsBlueprintUnlocked(string blueprintId)
        {
            if (string.IsNullOrEmpty(blueprintId)) return false;
            return _unlockedBlueprintIds.Contains(blueprintId);
        }

        public bool UnlockBlueprint(BlueprintDefinitionSO blueprint)
        {
            if (blueprint == null)
            {
                OnBlueprintUnlockFailed?.Invoke("Null blueprint definition");
                return false;
            }

            if (IsBlueprintUnlocked(blueprint.BlueprintId))
            {
                OnBlueprintUnlockFailed?.Invoke($"Blueprint '{blueprint.BlueprintId}' is already unlocked.");
                return false;
            }

            // Check prerequisite
            if (blueprint.HasPrerequisite && !IsBlueprintUnlocked(blueprint.PrerequisiteBlueprintId))
            {
                OnBlueprintUnlockFailed?.Invoke($"Prerequisite blueprint '{blueprint.PrerequisiteBlueprintId}' is not unlocked.");
                return false;
            }

            // Check Ember affordability
            if (!SpendEmbers(blueprint.EmberCost))
            {
                OnBlueprintUnlockFailed?.Invoke($"Insufficient Embers (Required: {blueprint.EmberCost}, Current: {embersBalance}).");
                return false;
            }

            _unlockedBlueprintIds.Add(blueprint.BlueprintId);
            OnBlueprintUnlocked?.Invoke(blueprint);
            return true;
        }

        public bool UnlockBlueprintById(string blueprintId)
        {
            EnsureDefaultDatabase();
            BlueprintDefinitionSO bp = blueprintDatabase != null ? blueprintDatabase.GetBlueprint(blueprintId) : null;
            return UnlockBlueprint(bp);
        }

        public List<BlueprintDefinitionSO> GetUnlockedBlueprints()
        {
            EnsureDefaultDatabase();
            List<BlueprintDefinitionSO> list = new List<BlueprintDefinitionSO>();
            if (blueprintDatabase == null) return list;

            foreach (var id in _unlockedBlueprintIds)
            {
                var bp = blueprintDatabase.GetBlueprint(id);
                if (bp != null)
                {
                    list.Add(bp);
                }
            }
            return list;
        }

        // ==========================================
        // AGGREGATED PERMANENT GAMEPLAY EFFECTS
        // ==========================================

        public int GetStartingGoldBonus()
        {
            return BlueprintEffectResolver.ComputeStartingGoldBonus(GetUnlockedBlueprints());
        }

        public int GetStartingHpBonus()
        {
            return BlueprintEffectResolver.ComputeStartingHpBonus(GetUnlockedBlueprints());
        }

        public HashSet<string> GetUnlockedItemIds()
        {
            return BlueprintEffectResolver.ComputeUnlockedItemIds(GetUnlockedBlueprints());
        }

        public HashSet<string> GetUnlockedRuneIds()
        {
            return BlueprintEffectResolver.ComputeUnlockedRuneIds(GetUnlockedBlueprints());
        }

        // ==========================================
        // CAMPFIRE META-HUB & LIFETIME STATS
        // ==========================================

        public void EnterCampfireHub()
        {
            OnHubEntered?.Invoke();
        }

        public void RecordRunAttempt()
        {
            totalRunsAttempted++;
            OnStatsUpdated?.Invoke();
        }

        public void RecordBossClear(int embersAwarded)
        {
            totalBossClears++;
            AddEmbers(embersAwarded);
            OnStatsUpdated?.Invoke();
        }

        // ==========================================
        // PERSISTENCE DTO IMPORT / EXPORT (SaveVersion 1)
        // ==========================================

        public SavedMetaData ExportMetaData()
        {
            return new SavedMetaData(embersBalance, _unlockedBlueprintIds, totalBossClears, totalRunsAttempted);
        }

        public void ImportMetaData(SavedMetaData meta)
        {
            if (meta == null) return;

            embersBalance = Mathf.Max(0, meta.embers);
            totalBossClears = Mathf.Max(0, meta.totalBossClears);
            totalRunsAttempted = Mathf.Max(0, meta.totalRunsAttempted);

            _unlockedBlueprintIds.Clear();
            if (meta.unlockedBlueprints != null)
            {
                foreach (var id in meta.unlockedBlueprints)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _unlockedBlueprintIds.Add(id);
                    }
                }
            }

            OnEmbersChanged?.Invoke(embersBalance);
            OnStatsUpdated?.Invoke();
        }
    }
}
