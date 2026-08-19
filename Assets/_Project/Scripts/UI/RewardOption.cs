using UnityEngine;
using Lattirune.Core;
using Lattirune.Items;

namespace Lattirune.UI
{
    /// <summary>
    /// Represents a single reward choice offered to the player after combat victory.
    /// </summary>
    public class RewardOption
    {
        public string RewardId { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public ItemDataSO ItemData { get; private set; }

        public ItemCategory Category => ItemData != null ? ItemData.Category : ItemCategory.Weapon;
        public Vector2Int Footprint => ItemData != null ? ItemData.BaseDimensions : Vector2Int.one;
        public Color PlaceholderColor => ItemData != null ? ItemData.PlaceholderColor : Color.white;

        public RewardOption(string rewardId, string displayName, string description, ItemDataSO itemData)
        {
            RewardId = rewardId;
            DisplayName = displayName;
            Description = description;
            ItemData = itemData;
        }

        public static RewardOption FromItemData(ItemDataSO item)
        {
            if (item == null) return null;

            return new RewardOption(
                rewardId: $"reward_{item.ItemId}",
                displayName: item.DisplayName,
                description: item.Description,
                itemData: item
            );
        }
    }
}
