using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Modifiers
{
    /// <summary>
    /// Runtime component tracking active procedural run modifiers, evaluating aggregate multipliers,
    /// and managing modifier lifecycles for Lattirune 1.1.
    /// </summary>
    public class RunModifierManager : MonoBehaviour
    {
        [SerializeField] private RunModifierDatabaseSO database;
        private readonly List<RunModifierDefinitionSO> _activeModifiers = new List<RunModifierDefinitionSO>();
        private readonly HashSet<string> _activeModifierIds = new HashSet<string>();

        public event Action<RunModifierDefinitionSO> OnModifierAdded;
        public event Action<RunModifierDefinitionSO> OnModifierRemoved;
        public event Action OnModifiersChanged;

        public IReadOnlyList<RunModifierDefinitionSO> ActiveModifiers => _activeModifiers;
        public int ActiveCount => _activeModifiers.Count;
        public RunModifierDatabaseSO Database => database;

        public void Initialize(RunModifierDatabaseSO db = null)
        {
            database = db ?? RunModifierDatabaseSO.CreateCanonicalDatabase();
            _activeModifiers.Clear();
            _activeModifierIds.Clear();
        }

        public bool AddModifier(RunModifierDefinitionSO modifier)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.ModifierId)) return false;
            if (_activeModifierIds.Contains(modifier.ModifierId)) return false; // Prevent duplicate

            _activeModifiers.Add(modifier);
            _activeModifierIds.Add(modifier.ModifierId);

            OnModifierAdded?.Invoke(modifier);
            OnModifiersChanged?.Invoke();
            return true;
        }

        public bool AddModifierById(string modifierId)
        {
            if (database == null) database = RunModifierDatabaseSO.CreateCanonicalDatabase();
            RunModifierDefinitionSO def = database.GetModifier(modifierId);
            return AddModifier(def);
        }

        public bool RemoveModifier(string modifierId)
        {
            if (string.IsNullOrEmpty(modifierId) || !_activeModifierIds.Contains(modifierId)) return false;

            int index = _activeModifiers.FindIndex(m => m != null && m.ModifierId == modifierId);
            if (index >= 0)
            {
                RunModifierDefinitionSO removed = _activeModifiers[index];
                _activeModifiers.RemoveAt(index);
                _activeModifierIds.Remove(modifierId);

                OnModifierRemoved?.Invoke(removed);
                OnModifiersChanged?.Invoke();
                return true;
            }
            return false;
        }

        public bool HasModifier(string modifierId)
        {
            if (string.IsNullOrEmpty(modifierId)) return false;
            return _activeModifierIds.Contains(modifierId);
        }

        public float GetAggregateMultiplier(RunModifierType type, float baseMultiplier = 1.0f)
        {
            float mult = baseMultiplier;
            for (int i = 0; i < _activeModifiers.Count; i++)
            {
                var mod = _activeModifiers[i];
                if (mod != null && mod.ModifierType == type)
                {
                    mult += mod.EffectValue;
                }
            }
            return Mathf.Max(0f, mult);
        }

        public List<string> ExportActiveModifierIds()
        {
            return new List<string>(_activeModifierIds);
        }

        public void ImportActiveModifierIds(IEnumerable<string> ids)
        {
            _activeModifiers.Clear();
            _activeModifierIds.Clear();

            if (ids == null) return;
            if (database == null) database = RunModifierDatabaseSO.CreateCanonicalDatabase();

            foreach (var id in ids)
            {
                var def = database.GetModifier(id);
                if (def != null)
                {
                    _activeModifiers.Add(def);
                    _activeModifierIds.Add(def.ModifierId);
                }
            }
            OnModifiersChanged?.Invoke();
        }

        public void ClearModifiers()
        {
            _activeModifiers.Clear();
            _activeModifierIds.Clear();
            OnModifiersChanged?.Invoke();
        }
    }
}
