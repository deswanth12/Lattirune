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

                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                JuiceController.Instance?.TriggerHaptic(HapticType.Success);
            }
            else
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
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

            float screenW = 1080f;
            float virtualH = Screen.height / scale;
            float padX = 35f;
            float contentW = screenW - (padX * 2f);

            // =================================================================
            // 1. TOP HEADER & HUD BAR
            // =================================================================
            float topY = 45f;
            float topH = 120f;
            Rect topBarRect = new Rect(padX, topY, contentW, topH);
            LattiruneUITheme.DrawCard(topBarRect);

            Texture2D merchantIcon = VisualAssetProvider.GetUIIcon("ui_icon_merchant");
            if (merchantIcon != null)
            {
                GUI.DrawTexture(new Rect(padX + 18f, topY + 18f, 84f, 84f), merchantIcon, ScaleMode.ScaleToFit);
            }

            GUIStyle titleStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = LattiruneUITheme.ColorGoldBright;
            GUI.Label(new Rect(padX + 116f, topY + 18f, 400f, 26f), "THE UNDERGROUND OUTPOST", titleStyle);

            GUIStyle dialogueStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            dialogueStyle.fontSize = 13;
            dialogueStyle.fontStyle = FontStyle.Italic;
            dialogueStyle.alignment = TextAnchor.MiddleLeft;
            dialogueStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUI.Label(new Rect(padX + 116f, topY + 48f, contentW - 320f, 38f), "â€œAh, an adventurer! Fresh wares from the upper catacombs... for a price.â€", dialogueStyle);

            int gold = runManager != null ? runManager.CurrentGold : 100;
            Texture2D iconGold = VisualAssetProvider.GetUIIcon("ui_icon_gold");
            float pillW = 160f;
            float pillX = padX + contentW - pillW - 12f;
            LattiruneUITheme.DrawIconValue(new Rect(pillX, topY + 30f, pillW, 30f), iconGold, $"{gold} Gold", LattiruneUITheme.ColorGoldPrimary, 16);

            // =================================================================
            // 2. MERCHANT ITEMS LIST
            // =================================================================
            float itemsY = topY + topH + 16f;
            float botBtnH = 85f;
            float botMargin = 25f;
            float actY = virtualH - botBtnH - botMargin;
            float listH = actY - itemsY - 16f;

            Rect listRect = new Rect(padX, itemsY, contentW, listH);
            LattiruneUITheme.DrawCard(listRect);

            float cardY = itemsY + 16f;
            float cardH = (listH - 48f) / 3f;

            for (int i = 0; i < _inventory.Count; i++)
            {
                var item = _inventory[i];
                Rect itemCardRect = new Rect(padX + 16f, cardY, contentW - 32f, cardH);
                
                Color cardBg = item.isPurchased ? new Color(0.06f, 0.08f, 0.10f, 0.70f) : new Color(0.12f, 0.16f, 0.24f, 0.90f);
                GUI.color = cardBg;
                LattiruneUITheme.DrawCard(itemCardRect);
                GUI.color = Color.white;

                if (!item.isPurchased)
                {
                    LattiruneUITheme.DrawBorder(itemCardRect, 1.5f, item.rarityColor);
                }

                // Item Artwork
                Texture2D icon = VisualAssetProvider.GetItemTexture(item.id);
                if (icon != null)
                {
                    Rect iconRect = new Rect(itemCardRect.x + 16f, itemCardRect.y + (cardH - 80f) * 0.5f, 80f, 80f);
                    Color oldC = GUI.color;
                    if (item.isPurchased) GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    GUI.color = oldC;
                }

                // Item Info
                float textX = itemCardRect.x + 110f;
                float textW = itemCardRect.width - 320f;

                GUIStyle itemNameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                itemNameStyle.fontSize = 18;
                itemNameStyle.fontStyle = FontStyle.Bold;
                itemNameStyle.normal.textColor = item.isPurchased ? LattiruneUITheme.ColorTextMuted : Color.white;
                GUI.Label(new Rect(textX, itemCardRect.y + 16f, textW, 24f), item.name, itemNameStyle);

                // Rarity pill
                Rect rarityRect = new Rect(textX, itemCardRect.y + 44f, 95f, 22f);
                GUI.DrawTexture(rarityRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                LattiruneUITheme.DrawBorder(rarityRect, 1f, item.rarityColor);
                GUIStyle rStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                rStyle.fontSize = 11;
                rStyle.fontStyle = FontStyle.Bold;
                rStyle.alignment = TextAnchor.MiddleCenter;
                rStyle.normal.textColor = item.rarityColor;
                GUI.Label(rarityRect, item.rarity, rStyle);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 13;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUI.Label(new Rect(textX, itemCardRect.y + 72f, textW, 36f), item.desc, descStyle);

                // Buy Action Button / Purchased State
                float btnW = 180f;
                float btnX = itemCardRect.x + itemCardRect.width - btnW - 16f;
                float btnY = itemCardRect.y + (cardH - 55f) * 0.5f;
                Rect btnRect = new Rect(btnX, btnY, btnW, 55f);

                if (item.isPurchased)
                {
                    GUI.DrawTexture(btnRect, LattiruneUITheme.StyleCard.normal.background ?? Texture2D.blackTexture);
                    LattiruneUITheme.DrawBorder(btnRect, 1f, LattiruneUITheme.ColorTextMuted);
                    GUIStyle pStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                    pStyle.alignment = TextAnchor.MiddleCenter;
                    pStyle.fontSize = 14;
                    pStyle.fontStyle = FontStyle.Bold;
                    pStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                    GUI.Label(btnRect, "PURCHASED", pStyle);
                }
                else
                {
                    if (GUI.Button(btnRect, $"BUY ({item.cost}g)", LattiruneUITheme.StylePrimaryBtn))
                    {
                        BuyItem(i);
                    }
                }

                cardY += cardH + 8f;
            }

            // =================================================================
            // 3. BOTTOM ACTION BUTTON
            // =================================================================
            Rect actRect = new Rect(padX, actY, contentW, botBtnH);
            if (GUI.Button(actRect, "LEAVE OUTPOST & RESUME DESCENT", LattiruneUITheme.StylePrimaryBtn))
            {
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                navigation?.NavigateTo(ScreenState.DUNGEON_MAP);
            }

            GUI.matrix = oldMatrix;
        }
    }
}

