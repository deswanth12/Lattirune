using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Economy;
using Lattirune.Grid;
using Lattirune.Inventory;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for the In-Run Merchant Stall.
    /// Displays item artwork icons, price tags, and inventory restocking (0 emoji, 0 placeholders).
    /// </summary>
    public class MerchantStallUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MerchantSystem merchantSystem;
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private LatticeGrid latticeGrid;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunManager runManager;
        [SerializeField] private ScreenNavigationController navigation;
        [SerializeField] private DungeonMapScreenController mapController;

        private IEconomyService _economyService;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private string _feedbackMessage = "Welcome, traveler! What supplies do you seek?";

        public bool IsVisible => isVisible;

        public void Initialize(
            MerchantSystem merchant,
            IEconomyService economy,
            InventorySystem inventory,
            LatticeGrid grid,
            PlayerCombatant player,
            RunManager run = null,
            ScreenNavigationController nav = null)
        {
            merchantSystem = merchant;
            _economyService = economy ?? (run as IEconomyService);
            inventorySystem = inventory;
            latticeGrid = grid;
            playerCombatant = player;
            runManager = run;
            navigation = nav;
            _feedbackMessage = "Welcome, traveler! What supplies do you seek?";

            if (navigation != null)
            {
                navigation.OnScreenChanged += HandleScreenChanged;
            }
        }

        private void OnDestroy()
        {
            if (navigation != null)
            {
                navigation.OnScreenChanged -= HandleScreenChanged;
            }
        }

        private void HandleScreenChanged(ScreenState prev, ScreenState next)
        {
            if (next == ScreenState.MERCHANT)
            {
                Show();
            }
            else if (prev == ScreenState.MERCHANT)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            if (merchantSystem != null)
            {
                int floor = runManager != null ? runManager.CurrentFloorNumber : 1;
                merchantSystem.GenerateOffers(floor);
            }
            _feedbackMessage = "Welcome, traveler! What supplies do you seek?";
        }

        public void Hide()
        {
            isVisible = false;
        }

        public void BindMapController(DungeonMapScreenController map)
        {
            mapController = map;
        }

        private void OnGUI()
        {
            if (navigation == null || navigation.CurrentScreen != ScreenState.MERCHANT) return;
            if (!isVisible || merchantSystem == null) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "THE RAT-FOLK TRADER — OUTPOST");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("MERCHANT OUTPOST", "Exchange hard-earned gold for vital dungeon supplies.");
            GUILayout.Space(10);

            // Economy bar
            int gold = _economyService != null ? _economyService.GoldBalance : (runManager != null ? runManager.CurrentGold : 0);
            int floorNum = runManager != null ? runManager.CurrentFloorNumber : 1;
            LattiruneUITheme.DrawBadge($"HERO GOLD: {gold}g  |  FLOOR {floorNum} STOCK", LattiruneUITheme.ColorGoldPrimary);
            GUILayout.Space(12);

            // Dialogue / Feedback
            GUIStyle feedbackStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            feedbackStyle.fontSize = 17;
            feedbackStyle.fontStyle = FontStyle.Italic;
            feedbackStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(_feedbackMessage, feedbackStyle);
            GUILayout.Space(14);

            // Offer Cards
            var offers = merchantSystem.CurrentOffers;
            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                if (offer == null) continue;

                GUILayout.BeginVertical(LattiruneUITheme.StyleCard);

                GUILayout.BeginHorizontal();

                // Item Artwork Icon
                Texture2D itemIcon = VisualAssetProvider.GetItemTexture(offer.ItemData != null ? offer.ItemData.ItemId : "");
                if (itemIcon != null)
                {
                    Rect iconRect = GUILayoutUtility.GetRect(64f, 64f, GUILayout.Width(64f), GUILayout.Height(64f));
                    GUI.DrawTexture(iconRect, itemIcon, ScaleMode.ScaleToFit);
                    GUILayout.Space(12);
                }

                GUILayout.BeginVertical();
                GUIStyle nameStyle = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
                nameStyle.fontSize = 19;
                nameStyle.fontStyle = FontStyle.Bold;
                nameStyle.normal.textColor = offer.IsSold ? LattiruneUITheme.ColorTextMuted : LattiruneUITheme.ColorTextPrimary;
                GUILayout.Label(offer.Title, nameStyle);

                GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
                descStyle.fontSize = 14;
                descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
                GUILayout.Label(offer.Description, descStyle);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Price Tag & Purchase Button
                if (offer.IsSold)
                {
                    LattiruneUITheme.DrawBadge("PURCHASED", LattiruneUITheme.ColorTextMuted);
                }
                else
                {
                    bool canAfford = gold >= offer.CurrentPrice;
                    GUI.enabled = canAfford;

                    if (LattiruneUITheme.DrawPrimaryButton($"BUY ({offer.CurrentPrice}g)", 55f))
                    {
                        if (merchantSystem.BuyOffer(i, _economyService, inventorySystem, latticeGrid, playerCombatant))
                        {
                            _feedbackMessage = $"Acquired {offer.Title}!";
                        }
                    }

                    GUI.enabled = true;
                }

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(8);
            }

            GUILayout.FlexibleSpace();

            // Exit Button
            if (LattiruneUITheme.DrawSecondaryButton("LEAVE MERCHANT OUTPOST", 75f))
            {
                if (mapController != null && mapController.MapGraph != null)
                {
                    mapController.MapGraph.CompleteCurrentNode();
                }
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.DUNGEON_MAP);
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
