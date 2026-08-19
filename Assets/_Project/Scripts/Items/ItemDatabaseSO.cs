using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Items
{
    /// <summary>
    /// ScriptableObject catalog storing and validating all registered ItemDataSO assets.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Lattirune/Data/Item Database")]
    public class ItemDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<ItemDataSO> items = new List<ItemDataSO>();

        public IReadOnlyList<ItemDataSO> Items => items;

        public void Initialize(List<ItemDataSO> itemList)
        {
            items = itemList ?? new List<ItemDataSO>();
        }

        public ItemDataSO GetItemById(string id)
        {
            if (string.IsNullOrEmpty(id) || items == null) return null;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].ItemId == id)
                {
                    return items[i];
                }
            }

            return null;
        }

        public bool ValidateDatabase(out List<string> errors)
        {
            errors = new List<string>();
            HashSet<string> seenIds = new HashSet<string>();

            if (items == null || items.Count == 0)
            {
                errors.Add("Item database is empty.");
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemDataSO item = items[i];
                if (item == null)
                {
                    errors.Add($"Item entry at index {i} is null.");
                    continue;
                }

                if (!item.IsValid(out string itemError))
                {
                    errors.Add($"Item '{item.name}' validation failed: {itemError}");
                }

                if (seenIds.Contains(item.ItemId))
                {
                    errors.Add($"Duplicate Item ID '{item.ItemId}' detected on item '{item.name}'.");
                }
                else
                {
                    seenIds.Add(item.ItemId);
                }
            }

            return errors.Count == 0;
        }
    }
}
