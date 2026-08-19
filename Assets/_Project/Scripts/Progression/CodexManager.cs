using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Save;

namespace Lattirune.Progression
{
    /// <summary>
    /// Master coordinator for persistent Bestiary, Synergy, and Elemental Reaction Codex tracking.
    /// Derived strictly from PLAN.md Section 10 and Section 12.
    /// </summary>
    public class CodexManager : MonoBehaviour
    {
        [Header("Databases")]
        [SerializeField] private BestiaryDatabaseSO bestiaryDatabase;

        [Header("Runtime Codex State")]
        [SerializeField] private List<string> discoveredEnemies = new List<string>();
        [SerializeField] private List<string> discoveredSynergies = new List<string>();
        [SerializeField] private List<string> discoveredReactions = new List<string>();

        private readonly HashSet<string> _discoveredEnemiesSet = new HashSet<string>();
        private readonly Dictionary<string, int> _enemyKillCounts = new Dictionary<string, int>();
        private readonly HashSet<string> _discoveredSynergiesSet = new HashSet<string>();
        private readonly HashSet<string> _discoveredReactionsSet = new HashSet<string>();

        public BestiaryDatabaseSO Bestiary => bestiaryDatabase;
        public IReadOnlyCollection<string> DiscoveredEnemies => _discoveredEnemiesSet;
        public IReadOnlyCollection<string> DiscoveredSynergies => _discoveredSynergiesSet;
        public IReadOnlyCollection<string> DiscoveredReactions => _discoveredReactionsSet;

        public event Action<string> OnEnemyDiscovered;
        public event Action<string, int> OnEnemyKilled;
        public event Action<string> OnSynergyDiscovered;
        public event Action<string> OnReactionDiscovered;

        public void Initialize(BestiaryDatabaseSO bestiary = null)
        {
            bestiaryDatabase = bestiary != null ? bestiary : BestiaryDatabaseSO.CreateCanonicalDatabase();
        }

        public bool IsEnemyDiscovered(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return false;
            return _discoveredEnemiesSet.Contains(enemyId);
        }

        public int GetEnemyKillCount(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return 0;
            return _enemyKillCounts.TryGetValue(enemyId, out int count) ? count : 0;
        }

        public void RecordEnemyEncounter(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            if (_discoveredEnemiesSet.Add(enemyId))
            {
                if (!discoveredEnemies.Contains(enemyId)) discoveredEnemies.Add(enemyId);
                OnEnemyDiscovered?.Invoke(enemyId);
            }
        }

        public void RecordEnemyDefeat(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return;
            RecordEnemyEncounter(enemyId);

            if (_enemyKillCounts.ContainsKey(enemyId))
            {
                _enemyKillCounts[enemyId]++;
            }
            else
            {
                _enemyKillCounts[enemyId] = 1;
            }

            OnEnemyKilled?.Invoke(enemyId, _enemyKillCounts[enemyId]);
        }

        public void RecordSynergyDiscovered(string synergyId)
        {
            if (string.IsNullOrEmpty(synergyId)) return;
            if (_discoveredSynergiesSet.Add(synergyId))
            {
                if (!discoveredSynergies.Contains(synergyId)) discoveredSynergies.Add(synergyId);
                OnSynergyDiscovered?.Invoke(synergyId);
            }
        }

        public void RecordReactionTriggered(string reactionId)
        {
            if (string.IsNullOrEmpty(reactionId)) return;
            if (_discoveredReactionsSet.Add(reactionId))
            {
                if (!discoveredReactions.Contains(reactionId)) discoveredReactions.Add(reactionId);
                OnReactionDiscovered?.Invoke(reactionId);
            }
        }

        public void LoadFromSave(
            List<string> savedEnemies, 
            List<string> savedEnemyKillKeys, 
            List<int> savedEnemyKillValues, 
            List<string> savedSynergies, 
            List<string> savedReactions)
        {
            _discoveredEnemiesSet.Clear();
            discoveredEnemies.Clear();
            if (savedEnemies != null)
            {
                foreach (var e in savedEnemies)
                {
                    if (!string.IsNullOrEmpty(e))
                    {
                        _discoveredEnemiesSet.Add(e);
                        discoveredEnemies.Add(e);
                    }
                }
            }

            _enemyKillCounts.Clear();
            if (savedEnemyKillKeys != null && savedEnemyKillValues != null)
            {
                int count = Mathf.Min(savedEnemyKillKeys.Count, savedEnemyKillValues.Count);
                for (int i = 0; i < count; i++)
                {
                    _enemyKillCounts[savedEnemyKillKeys[i]] = savedEnemyKillValues[i];
                }
            }

            _discoveredSynergiesSet.Clear();
            discoveredSynergies.Clear();
            if (savedSynergies != null)
            {
                foreach (var s in savedSynergies)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        _discoveredSynergiesSet.Add(s);
                        discoveredSynergies.Add(s);
                    }
                }
            }

            _discoveredReactionsSet.Clear();
            discoveredReactions.Clear();
            if (savedReactions != null)
            {
                foreach (var r in savedReactions)
                {
                    if (!string.IsNullOrEmpty(r))
                    {
                        _discoveredReactionsSet.Add(r);
                        discoveredReactions.Add(r);
                    }
                }
            }
        }

        public (List<string> keys, List<int> values) ExportKillCounts()
        {
            var keys = new List<string>(_enemyKillCounts.Keys);
            var values = new List<int>(_enemyKillCounts.Values);
            return (keys, values);
        }
    }
}
