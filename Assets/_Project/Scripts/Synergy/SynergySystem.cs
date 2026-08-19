using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Synergy
{
    /// <summary>
    /// Evaluates conduit-to-item intersections on the LatticeGrid and activates elemental synergies.
    /// Supports the complete 5-element matrix (Fire, Ice, Lightning, Poison, Light) via data-driven SynergyDatabaseSO.
    /// </summary>
    public class SynergySystem : MonoBehaviour
    {
        [Header("Data-Driven Matrix Database")]
        [SerializeField] private SynergyDatabaseSO synergyDatabase;

        [Header("Legacy / Direct Registrations")]
        [SerializeField] private List<SynergyDefinition> registeredSynergies = new List<SynergyDefinition>();

        private readonly Dictionary<string, SynergyResult> _activeSynergiesByInstance = new Dictionary<string, SynergyResult>();

        public event Action<SynergyResult> OnSynergyActivated;
        public event Action<SynergyResult> OnSynergyDeactivated;

        public SynergyDatabaseSO Database => synergyDatabase;
        public IReadOnlyDictionary<string, SynergyResult> ActiveSynergies => _activeSynergiesByInstance;

        private void Awake()
        {
            EnsureDefaultDefinitions();
        }

        public void Initialize(SynergyDatabaseSO database)
        {
            synergyDatabase = database;
            EnsureDefaultDefinitions();
        }

        public void EnsureDefaultDefinitions()
        {
            if (synergyDatabase == null)
            {
                synergyDatabase = SynergyDatabaseSO.CreateDefaultDatabase();
            }

            if (registeredSynergies == null || registeredSynergies.Count == 0)
            {
                registeredSynergies = new List<SynergyDefinition>
                {
                    SynergyDefinition.CreateDefaultFireSword()
                };
            }
        }

        public void RegisterDefinition(SynergyDefinition definition)
        {
            if (definition != null && !registeredSynergies.Exists(d => d.SynergyId == definition.SynergyId))
            {
                registeredSynergies.Add(definition);
            }
        }

        public void RegisterDefinitionSO(SynergyDefinitionSO definitionSO)
        {
            if (definitionSO != null)
            {
                EnsureDefaultDefinitions();
                synergyDatabase.Register(definitionSO);
            }
        }

        /// <summary>
        /// Evaluates whether a single conduit raycast intersects with a target item to produce an active synergy.
        /// </summary>
        public SynergyResult EvaluateConnection(
            RuneData rune, 
            Vector2Int runePos, 
            RuneConduitResult conduit, 
            ItemInstance targetItem)
        {
            if (rune == null || !rune.IsActive || conduit == null || conduit.TraversalLength == 0 || targetItem == null || !targetItem.IsPlacedOnGrid || targetItem.Data == null)
            {
                return SynergyResult.CreateInactive(rune?.RuneId, targetItem?.Data?.ItemId, targetItem?.InstanceId);
            }

            EnsureDefaultDefinitions();

            // 1. Look up in data-driven database first
            SynergyDefinitionSO matchSO = synergyDatabase != null ? synergyDatabase.FindMatchingDefinition(rune, targetItem.Data) : null;
            if (matchSO != null)
            {
                for (int i = 0; i < conduit.TraversalLength; i++)
                {
                    Vector2Int coord = conduit.TraversedCells[i];
                    if (targetItem.ContainsGridCoordinate(coord))
                    {
                        return SynergyResult.CreateActive(
                            matchSO,
                            rune.RuneId,
                            targetItem.Data.ItemId,
                            targetItem.InstanceId,
                            runePos,
                            coord
                        );
                    }
                }
            }

            // 2. Legacy fallback to registered SynergyDefinition list
            SynergyDefinition matchDef = null;
            for (int i = 0; i < registeredSynergies.Count; i++)
            {
                if (registeredSynergies[i] != null && registeredSynergies[i].IsMatch(rune, targetItem.Data))
                {
                    matchDef = registeredSynergies[i];
                    break;
                }
            }

            if (matchDef != null)
            {
                for (int i = 0; i < conduit.TraversalLength; i++)
                {
                    Vector2Int coord = conduit.TraversedCells[i];
                    if (targetItem.ContainsGridCoordinate(coord))
                    {
                        return SynergyResult.CreateActive(
                            matchDef,
                            rune.RuneId,
                            targetItem.Data.ItemId,
                            targetItem.InstanceId,
                            runePos,
                            coord
                        );
                    }
                }
            }

            return SynergyResult.CreateInactive(rune.RuneId, targetItem.Data.ItemId, targetItem.InstanceId);
        }

        /// <summary>
        /// Batch evaluates all active conduits against all active items, dispatching activation/deactivation events.
        /// </summary>
        public void UpdateSynergies(
            IEnumerable<(RuneData rune, Vector2Int pos, RuneConduitResult conduit)> activeConduits, 
            IEnumerable<ItemInstance> activeItems)
        {
            EnsureDefaultDefinitions();

            Dictionary<string, SynergyResult> newlyActiveSynergies = new Dictionary<string, SynergyResult>();

            if (activeConduits != null && activeItems != null)
            {
                foreach (var (rune, runePos, conduit) in activeConduits)
                {
                    if (rune == null || conduit == null || conduit.TraversalLength == 0) continue;

                    foreach (var item in activeItems)
                    {
                        if (item == null || !item.IsPlacedOnGrid) continue;

                        SynergyResult result = EvaluateConnection(rune, runePos, conduit, item);
                        if (result.IsSynergyActive)
                        {
                            // If an item is already targeted by another synergy, keep the highest priority
                            if (!newlyActiveSynergies.ContainsKey(item.InstanceId) || 
                                result.RuneBonus > newlyActiveSynergies[item.InstanceId].RuneBonus)
                            {
                                newlyActiveSynergies[item.InstanceId] = result;
                            }
                        }
                    }
                }
            }

            // 1. Detect deactivations
            List<string> toDeactivate = new List<string>();
            foreach (var kvp in _activeSynergiesByInstance)
            {
                if (!newlyActiveSynergies.ContainsKey(kvp.Key))
                {
                    toDeactivate.Add(kvp.Key);
                }
            }

            foreach (string instanceId in toDeactivate)
            {
                SynergyResult prevResult = _activeSynergiesByInstance[instanceId];
                _activeSynergiesByInstance.Remove(instanceId);

                // Update visual on item instance if still in activeItems
                if (activeItems != null)
                {
                    foreach (var item in activeItems)
                    {
                        if (item != null && item.InstanceId == instanceId)
                        {
                            item.SetSynergyState(null);
                        }
                    }
                }

                OnSynergyDeactivated?.Invoke(prevResult);
            }

            // 2. Detect new activations and apply state
            foreach (var kvp in newlyActiveSynergies)
            {
                string instanceId = kvp.Key;
                SynergyResult newResult = kvp.Value;

                bool wasAlreadyActive = _activeSynergiesByInstance.ContainsKey(instanceId);
                _activeSynergiesByInstance[instanceId] = newResult;

                // Update visual on item instance
                if (activeItems != null)
                {
                    foreach (var item in activeItems)
                    {
                        if (item != null && item.InstanceId == instanceId)
                        {
                            item.SetSynergyState(newResult.SynergyId, newResult.SynergyColor);
                        }
                    }
                }

                if (!wasAlreadyActive)
                {
                    OnSynergyActivated?.Invoke(newResult);
                }
            }
        }
    }
}
