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
    /// Strictly adheres to PLAN.md Section 11, 13.1, and 15 (>= 52 dp touch targets).
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

        private IEconomyService _economyService;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private string _feedbackMessage = "";

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

        private void OnGUI()
        {
            if (navigation != null && navigation.CurrentScreen != ScreenState.MERCHANT) return;
            if (!isVisible || merchantSystem == null) return;

            // Fullscreen dark neo-arcane backdrop (1080x1920 scaled)
            float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
            if (scale <= 0.01f) scale = 1.0f;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (1920f - panelHeight) * 0.5f;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.whiteTexture;

            Color oldColor = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.96f); // Slate Obsidian
            GUI.Box(new Rect(posX, posY, panelWidth, panelHeight), GUIContent.none, boxStyle);
            GUI.color = oldColor;

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            // Header
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 32;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.77f, 0.61f, 0.15f); // Burnished Brass

            GUILayout.Label("⚜ WANDERING MERCHANT ⚜", titleStyle);
            GUILayout.Space(10);

            // Subheader: Gold & Floor
            GUIStyle goldStyle = new GUIStyle(GUI.skin.label);
            goldStyle.fontSize = 24;
            goldStyle.alignment = TextAnchor.MiddleCenter;
            goldStyle.normal.textColor = Color.yellow;

            int currentGold = _economyService != null ? _economyService.CurrentGold : (runManager != null ? runManager.CurrentGold : 0);
            int floorNum = runManager != null ? runManager.CurrentFloorNumber : 1;
            GUILayout.Label($"Floor {floorNum}  |  Your Gold: {currentGold} 🪙", goldStyle);
            GUILayout.Space(10);

            // Feedback / Dialogue
            GUIStyle dialogueStyle = new GUIStyle(GUI.skin.label);
            dialogueStyle.fontSize = 18;
            dialogueStyle.fontStyle = FontStyle.Italic;
            dialogueStyle.alignment = TextAnchor.MiddleCenter;
            dialogueStyle.normal.textColor = Color.white;
            GUILayout.Label($"\"{_feedbackMessage}\"", dialogueStyle);
            GUILayout.Space(20);

            // Offers List
            var offers = merchantSystem.CurrentOffers;
            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                if (offer == null) continue;

                GUILayout.BeginVertical(GUI.skin.box);

                GUIStyle offerHeader = new GUIStyle(GUI.skin.label);
                offerHeader.fontSize = 22;
                offerHeader.fontStyle = FontStyle.Bold;
                offerHeader.normal.textColor = offer.IsSold ? Color.gray : Color.white;

                string statusText = offer.IsSold ? "[SOLD OUT]" : $"{offer.CurrentPrice} Gold";
                GUILayout.Label($"{offer.Title}  -  {statusText}", offerHeader);

                GUIStyle descStyle = new GUIStyle(GUI.skin.label);
                descStyle.fontSize = 16;
                descStyle.normal.textColor = offer.IsSold ? Color.gray : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.Label(offer.Description, descStyle);

                GUILayout.Space(6);

                if (!offer.IsSold)
                {
                    bool canAfford = _economyService != null && _economyService.CanAfford(offer.CurrentPrice);
                    GUI.enabled = canAfford;

                    if (GUILayout.Button($"PURCHASE ({offer.CurrentPrice}g)", GUILayout.Height(55)))
                    {
                        if (merchantSystem.BuyOffer(i, _economyService, inventorySystem, latticeGrid, playerCombatant))
                        {
                            _feedbackMessage = $"Pleasure doing business! You acquired {offer.Title}.";
                        }
                    }

                    GUI.enabled = true;
                }

                GUILayout.EndVertical();
                GUILayout.Space(12);
            }

            GUILayout.FlexibleSpace();

            // Reroll Button (10 Gold)
            bool canReroll = _economyService != null && _economyService.CanAfford(10);
            GUI.enabled = canReroll;
            if (GUILayout.Button("🔄 REROLL STOCK (10 Gold)", GUILayout.Height(60)))
            {
                if (merchantSystem.RerollOffers(_economyService, 10, floorNum))
                {
                    _feedbackMessage = "The merchant reveals a fresh crate of wares!";
                }
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            // Leave / Continue Button
            GUIStyle leaveBtnStyle = new GUIStyle(GUI.skin.button);
            leaveBtnStyle.fontSize = 22;
            leaveBtnStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("LEAVE MERCHANT & CONTINUE", leaveBtnStyle, GUILayout.Height(65)))
            {
                Hide();
                if (runManager != null)
                {
                    runManager.ContinueAfterReward();
                }
                if (navigation != null)
                {
                    navigation.NavigateTo(ScreenState.GRID_BUILD);
                }
            }

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
