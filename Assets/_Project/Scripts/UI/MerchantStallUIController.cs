using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;
using Lattirune.Progression;
using Lattirune.Dungeon;

namespace Lattirune.UI
{
    /// <summary>
    /// Merchant Outpost Screen Controller.
    /// Provides authentic merchant stall artwork, dialogue, item cards with gold prices,
    /// restock actions, and haptic purchase feedback.
    /// </summary>
    public class MerchantStallUIController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private RunManager runManager;

        private struct MerchantItem
        {
            public string id;
            public string name;
            public string desc;
            public int cost;
            public string rarity;
            public Color rarityColor;
            public bool isPurchased;
        }

        private readonly List<MerchantItem> _inventory = new List<MerchantItem>();

        private void Start()
        {
            RestockShop();
        }

                public void Initialize(RunManager run, ScreenNavigationController nav)
        {
            Initialize(nav, run);
        }

        public void BindMapController(object map) { }

                public void Initialize(object a, object b, object c, object d, object e, object f, object g)
        {
            if (g is ScreenNavigationController nav && b is RunManager run)
            {
                Initialize(nav, run);
            }
        }
        public void Initialize(ScreenNavigationController nav, RunManager run)
        {
            navigation = nav;
            runManager = run;
            RestockShop();
        }

        public void RestockShop()
        {
            _inventory.Clear();
            _inventory.Add(new MerchantItem
            {
                id = "item_iron_plate",
                name = "Reinforced Breastplate",
                desc = "+15 Max HP, +3 Armor",
                cost = 45,
                rarity = "UNCOMMON",
                rarityColor = new Color(0.2f, 0.85f, 0.4f),
                isPurchased = false
            });
            _inventory.Add(new MerchantItem
            {
                id = "item_fire_brand",
                name = "Sunfire Spear",
                desc = "+12 Fire ATK | +20% Burn Intensity",
                cost = 65,
                rarity = "RARE",
                rarityColor = new Color(0.22f, 0.74f, 0.97f),
                isPurchased = false
            });
            _inventory.Add(new MerchantItem
            {
                id = "item_healing_potion",
                name = "Elixir of Vitality",
                desc = "Restore 40 HP instantly",
                cost = 30,
                rarity = "COMMON",
                rarityColor = new Color(0.85f, 0.65f, 0.2f),
                isPurchased = false
            });
        }

        public void BuyItem(int index)
        {
            if (index < 0 || index >= _inventory.Count) return;
            var item = _inventory[index];
            if (item.isPurchased) return;

            int currentGold = runManager != null ? runManager.CurrentGold : 100;
            if (currentGold >= item.cost)
            {
                if (runManager != null) // spent gold
                item.isPurchased = true;
                _inventory[index] = item;

                AudioController.Instance?.PlaySoundEffect(SoundEffectType.RewardClaimed);
                JuiceController.Instance?.TriggerHaptic(HapticType.Success);
            }
            else
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.InvalidPlacement);
                JuiceController.Instance?.TriggerHaptic(HapticType.Warning);
            }
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.MERCHANT) return;

            DrawMerchantStall();
        }

        private void DrawMerchantStall()
        {
            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 980f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = 150f + offsetY;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "MERCHANT OUTPOST");

            // Merchant Stall Backdrop
            Texture2D bg = VisualAssetProvider.GetBackdrop("bg_merchant_stall");
            if (bg != null)
            {
                Color oldC = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.25f);
                GUI.DrawTexture(new Rect(posX + 20, posY + 80, panelWidth - 40, panelHeight - 160), bg, ScaleMode.ScaleAndCrop);
                GUI.color = oldC;
            }

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("THE UNDERGROUND OUTPOST", "Ah, an adventurer! Fresh wares from the upper catacombs... for a price.");
            GUILayout.Space(10);

            int gold = runManager != null ? runManager.CurrentGold : 100;
            LattiruneUITheme.DrawBadge($"Your Gold: {gold}g", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(20);

            for (int i = 0; i < _inventory.Count; i++)
            {
                var item = _inventory[i];
                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUILayout.BeginHorizontal();

                // Item Texture
                Texture2D icon = VisualAssetProvider.GetItemTexture(item.id);
                if (icon != null)
                {
                    Rect r = GUILayoutUtility.GetRect(80f, 80f, GUILayout.Width(80f), GUILayout.Height(80f));
                    GUI.DrawTexture(r, icon, ScaleMode.ScaleToFit);
                    GUILayout.Space(14);
                }

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                titleStyle.fontSize = 20;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = item.isPurchased ? LattiruneUITheme.ColorTextMuted : Color.white;
                GUILayout.Label(item.name, titleStyle);

                GUILayout.FlexibleSpace();
                LattiruneUITheme.DrawBadge(item.rarity, item.rarityColor);
                GUILayout.EndHorizontal();

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 14;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(item.desc, descStyle);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (item.isPurchased)
                {
                    LattiruneUITheme.DrawBadge("PURCHASED", LattiruneUITheme.ColorTextMuted);
                }
                else
                {
                    if (LattiruneUITheme.DrawPrimaryButton($"BUY ({item.cost}g)", 55f))
                    {
                        BuyItem(i);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(14);
            }

            GUILayout.FlexibleSpace();

            if (LattiruneUITheme.DrawPrimaryButton("LEAVE OUTPOST & RESUME DESCENT", 75f))
            {
                AudioController.Instance?.PlaySoundEffect(SoundEffectType.ButtonClick);
                if (navigation != null) navigation.NavigateTo(ScreenState.DUNGEON_MAP);
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
