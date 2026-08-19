using System;
using UnityEngine;

namespace Lattirune.Items
{
    /// <summary>
    /// Factory for spawning runtime ItemInstance GameObjects from ItemDataSO definitions.
    /// </summary>
    public static class ItemFactory
    {
        private static int _instanceCounter = 0;

        public static ItemInstance CreateInstance(
            ItemDataSO data, 
            Vector3 spawnPosition, 
            Transform parent = null, 
            int initialRotation = 0)
        {
            if (data == null)
            {
                Debug.LogError("[Lattirune] Cannot create ItemInstance from null ItemDataSO.");
                return null;
            }

            _instanceCounter++;
            string instanceId = $"{data.ItemId}_inst_{_instanceCounter}";

            GameObject itemObj = new GameObject($"Item_{data.DisplayName}_{_instanceCounter}");
            if (parent != null)
            {
                itemObj.transform.SetParent(parent);
            }
            itemObj.transform.position = spawnPosition;

            ItemInstance instance = itemObj.AddComponent<ItemInstance>();
            instance.Initialize(data, instanceId, spawnPosition, initialRotation);

            return instance;
        }
    }
}
