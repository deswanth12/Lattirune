using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Grid;
using Lattirune.Inventory;
using Lattirune.Items;
using Lattirune.Runes;

namespace Lattirune.Progression
{
    /// <summary>
    /// Coordinates Hero Class unlocking via Meta Embers, active class selection,
    /// and starting loadout provisioning for new dungeon runs.
    /// Strictly adheres to PLAN.md Section 12 and Section 26.
    /// </summary>
    public class HeroClassManager : MonoBehaviour
    {
        [Header("Databases")]
        [SerializeField] private HeroClassDatabaseSO classDatabase;
        [SerializeField] private ItemDatabaseSO itemDatabase;
        [SerializeField] private RuneDatabaseSO runeDatabase;

        [Header("State")]
        [SerializeField] private string selectedClassId = "class_rune_knight";
        [SerializeField] private List<string> unlockedClassIds = new List<string> { "class_rune_knight" };

        private readonly HashSet<string> _unlockedSet = new HashSet<string>();

        public HeroClassDatabaseSO Database => classDatabase;
        public string SelectedClassId => selectedClassId;
        public IReadOnlyCollection<string> UnlockedClassIds => _unlockedSet;

        public event Action<string> OnClassSelected;
        public event Action<string> OnClassUnlocked;

        public void Initialize(
            HeroClassDatabaseSO heroDb = null,
            ItemDatabaseSO itemDb = null,
            RuneDatabaseSO runeDb = null)
        {
            classDatabase = heroDb != null ? heroDb : HeroClassDatabaseSO.CreateCanonicalDatabase();
            itemDatabase = itemDb != null ? itemDb : ItemDatabaseSO.CreateCanonicalDatabase();
            runeDatabase = runeDb != null ? runeDb : RuneDatabaseSO.CreateCanonicalDatabase();

            _unlockedSet.Clear();
            if (unlockedClassIds != null)
            {
                foreach (var id in unlockedClassIds)
                {
                    if (!string.IsNullOrEmpty(id)) _unlockedSet.Add(id);
                }
            }

            // Ensure default unlocked classes from DB are included
            if (classDatabase != null)
            {
                foreach (var def in classDatabase.AllClasses)
                {
                    if (def != null && def.DefaultUnlocked)
                    {
                        _unlockedSet.Add(def.ClassId);
                    }
                }
            }

            if (string.IsNullOrEmpty(selectedClassId) || !_unlockedSet.Contains(selectedClassId))
            {
                selectedClassId = "class_rune_knight";
            }
        }

        public bool IsClassUnlocked(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return false;
            return _unlockedSet.Contains(classId);
        }

        public bool SelectClass(string classId)
        {
            if (!IsClassUnlocked(classId)) return false;
            if (classDatabase == null || !classDatabase.HasClass(classId)) return false;

            selectedClassId = classId;
            OnClassSelected?.Invoke(selectedClassId);
            return true;
        }

        public bool UnlockClass(string classId, MetaProgressionManager meta)
        {
            if (string.IsNullOrEmpty(classId)) return false;
            if (IsClassUnlocked(classId)) return true;
            if (classDatabase == null) return false;

            var def = classDatabase.GetClass(classId);
            if (def == null) return false;

            if (meta == null || meta.CurrentEmbers < def.EmbersCost)
            {
                return false;
            }

            if (meta.SpendEmbers(def.EmbersCost))
            {
                _unlockedSet.Add(classId);
                if (!unlockedClassIds.Contains(classId))
                {
                    unlockedClassIds.Add(classId);
                }
                OnClassUnlocked?.Invoke(classId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Applies the selected Hero Class starting stats and provisions starting items into inventory.
        /// </summary>
        public void ApplyStartingLoadout(
            PlayerCombatant player,
            InventorySystem inventory,
            LatticeGrid grid)
        {
            if (classDatabase == null) return;
            var def = classDatabase.GetClass(selectedClassId);
            if (def == null) return;

            // Apply base stats to PlayerCombatant
            if (player != null)
            {
                player.SetupDefaultPlayer(def.BaseHp);
                player.SetExplicitStats(
                    baseDamage: def.BaseAttack,
                    runeBonus: 0,
                    armorValue: def.BaseArmor,
                    interval: def.AttackInterval
                );
            }

            // Provision starting items into inventory staging
            if (inventory != null && itemDatabase != null)
            {
                foreach (var itemId in def.StartingItemIds)
                {
                    var itemData = itemDatabase.GetItem(itemId);
                    if (itemData != null)
                    {
                        var instance = ItemFactory.CreateInstance(itemData, Vector3.zero);
                        inventory.AddItemToStaging(instance);
                    }
                }
            }
        }

        public void LoadFromSave(string savedSelectedClass, List<string> savedUnlockedClasses)
        {
            if (savedUnlockedClasses != null && savedUnlockedClasses.Count > 0)
            {
                _unlockedSet.Clear();
                unlockedClassIds.Clear();
                foreach (var id in savedUnlockedClasses)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _unlockedSet.Add(id);
                        unlockedClassIds.Add(id);
                    }
                }
            }

            // Always ensure default classes
            if (classDatabase != null)
            {
                foreach (var def in classDatabase.AllClasses)
                {
                    if (def != null && def.DefaultUnlocked)
                    {
                        _unlockedSet.Add(def.ClassId);
                    }
                }
            }

            if (!string.IsNullOrEmpty(savedSelectedClass) && _unlockedSet.Contains(savedSelectedClass))
            {
                selectedClassId = savedSelectedClass;
            }
            else
            {
                selectedClassId = "class_rune_knight";
            }
        }

        public List<string> ExportUnlockedClassIds()
        {
            return new List<string>(_unlockedSet);
        }
    }
}
