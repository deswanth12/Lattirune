using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;
using Lattirune.Modifiers;
using Lattirune.Runes;

namespace Lattirune.UI
{
    /// <summary>
    /// Mobile portrait UI Controller for the Floor 8 Campfire Rest Site.
    /// Strictly adheres to PLAN.md Section 11 and Section 12 (Heal 40% HP OR Upgrade 1 Rune OR Cleanse Curse).
    /// </summary>
    public class CampfireRestUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RunManager runManager;
        [SerializeField] private PlayerCombatant playerCombatant;
        [SerializeField] private RunModifierManager modifierManager;
        [SerializeField] private ScreenNavigationController navigation;

        [Header("State")]
        [SerializeField] private bool isVisible = false;
        private string _feedbackMessage = "";
        private bool _hasChosenOption = false;

        public bool IsVisible => isVisible;

        public void Initialize(
            RunManager run,
            PlayerCombatant player,
            RunModifierManager modifiers = null,
            ScreenNavigationController nav = null)
        {
            runManager = run;
            playerCombatant = player;
            modifierManager = modifiers;
            navigation = nav;
            _hasChosenOption = false;
            _feedbackMessage = "The warmth of the campfire soothes your weary soul. Choose how to spend your rest.";

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
            if (next == ScreenState.CAMPFIRE_REST)
            {
                Show();
            }
            else if (prev == ScreenState.CAMPFIRE_REST)
            {
                Hide();
            }
        }

        public void Show()
        {
            isVisible = true;
            _hasChosenOption = false;
            _feedbackMessage = "The warmth of the campfire soothes your weary soul. Choose how to spend your rest.";
        }

        public void Hide()
        {
            isVisible = false;
        }

        public bool ChooseRestAndHeal()
        {
            if (_hasChosenOption || playerCombatant == null) return false;

            int healAmount = Mathf.Max(1, Mathf.RoundToInt(playerCombatant.MaxHp * 0.40f));
            playerCombatant.Heal(healAmount);
            _hasChosenOption = true;
            _feedbackMessage = $"You rested quietly by the flames, restoring {healAmount} Health points.";
            return true;
        }

        public bool ChooseUpgradeRune(string runeId = "fire_rune_01")
        {
            if (_hasChosenOption || runManager == null) return false;

            runManager.UpgradeRune(runeId, 3);
            _hasChosenOption = true;
            _feedbackMessage = $"You attuned your rune in the embers! +3 Elemental Power granted for the remainder of this run.";
            return true;
        }

        public bool ChooseCleanseCurse()
        {
            if (_hasChosenOption || modifierManager == null) return false;

            if (modifierManager.HasModifier("mod_curse_vulnerability"))
            {
                modifierManager.RemoveModifier("mod_curse_vulnerability");
                _hasChosenOption = true;
                _feedbackMessage = "The sacred flame dispelled the Curse of Vulnerability!";
                return true;
            }
            else
            {
                // If no curse, treat as blessing (+10 Max HP)
                if (playerCombatant != null)
                {
                    playerCombatant.Heal(15);
                }
                _hasChosenOption = true;
                _feedbackMessage = "With no active curses to cleanse, the sacred light granted you a minor blessing (+15 HP).";
                return true;
            }
        }

        private void OnGUI()
        {
            if (navigation != null && navigation.CurrentScreen != ScreenState.CAMPFIRE_REST) return;
            if (!isVisible) return;

            Matrix4x4 oldMatrix = LattiruneUITheme.PrepareGUIMatrix(out float scale, out float offsetY);

            float panelWidth = 960f;
            float panelHeight = 1500f;
            float posX = (1080f - panelWidth) * 0.5f;
            float posY = (Screen.height / scale - panelHeight) * 0.5f;

            LattiruneUITheme.DrawModalWindow(new Rect(posX, posY, panelWidth, panelHeight), "🔥 CAMPFIRE REST SANCTUARY 🔥");

            GUILayout.BeginArea(new Rect(posX + 40, posY + 40, panelWidth - 80, panelHeight - 80));

            LattiruneUITheme.DrawHeader("🔥 CAMPFIRE REST SANCTUARY 🔥", "A rare moment of warmth amidst the subterranean darkness.");
            GUILayout.Space(12);

            int currentHp = playerCombatant != null ? playerCombatant.CurrentHp : 100;
            int maxHp = playerCombatant != null ? playerCombatant.MaxHp : 100;
            LattiruneUITheme.DrawProgressBar(currentHp, maxHp, $"HERO HEALTH: {currentHp} / {maxHp} HP", LattiruneUITheme.ColorGreenHealth, 28f);
            GUILayout.Space(16);

            // Choices
            GUI.enabled = !_hasChosenOption;

            // Choice 1: Rest & Heal 40% HP
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUIStyle choiceHeader = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            choiceHeader.fontSize = 22;
            choiceHeader.fontStyle = FontStyle.Bold;
            choiceHeader.normal.textColor = LattiruneUITheme.ColorGreenHealth;

            GUILayout.Label("1. REST BY THE EMBERS", choiceHeader);
            GUIStyle descStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            descStyle.fontSize = 16;
            descStyle.normal.textColor = LattiruneUITheme.ColorTextMuted;
            GUILayout.Label("Heal 40% of Max HP immediately to recover from brutal combat.", descStyle);
            GUILayout.Space(6);
            if (LattiruneUITheme.DrawPrimaryButton("❤️ REST & HEAL (+40% HP)", 60f))
            {
                ChooseRestAndHeal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);

            // Choice 2: Attune Rune (+3 Power)
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUIStyle runeHeader = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            runeHeader.fontSize = 22;
            runeHeader.fontStyle = FontStyle.Bold;
            runeHeader.normal.textColor = LattiruneUITheme.ColorCyanArcane;

            GUILayout.Label("2. ATTUNE RUNE MATRIX", runeHeader);
            GUILayout.Label("Forge your active runes in sacred heat, granting +3 Flat Elemental Power.", descStyle);
            GUILayout.Space(6);
            if (LattiruneUITheme.DrawPrimaryButton("⚡ ATTUNE RUNES (+3 Power)", 60f))
            {
                ChooseUpgradeRune();
            }
            GUILayout.EndVertical();

            GUILayout.Space(12);

            // Choice 3: Cleanse Curse
            GUILayout.BeginVertical(LattiruneUITheme.StyleCard);
            GUIStyle curseHeader = new GUIStyle(LattiruneUITheme.StyleSectionTitle);
            curseHeader.fontSize = 22;
            curseHeader.fontStyle = FontStyle.Bold;
            curseHeader.normal.textColor = LattiruneUITheme.ColorPurpleRune;

            GUILayout.Label("3. SACRED CLEANSE", curseHeader);
            GUILayout.Label("Purge lingering dungeon curses and vulnerabilities from your spirit.", descStyle);
            GUILayout.Space(6);
            if (LattiruneUITheme.DrawPrimaryButton("✨ CLEANSE CURSES", 60f))
            {
                ChooseCleanseCurse();
            }
            GUILayout.EndVertical();

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            // Dialogue / Feedback
            GUIStyle feedbackStyle = new GUIStyle(LattiruneUITheme.StyleStatLabel);
            feedbackStyle.fontSize = 18;
            feedbackStyle.fontStyle = FontStyle.Italic;
            feedbackStyle.alignment = TextAnchor.MiddleCenter;
            feedbackStyle.normal.textColor = LattiruneUITheme.ColorGoldPrimary;
            GUILayout.Label($"\"{_feedbackMessage}\"", feedbackStyle);
            GUILayout.Space(12);

            // Leave / Continue Button
            GUI.enabled = _hasChosenOption;
            if (LattiruneUITheme.DrawPrimaryButton("⚔️ LEAVE REST SITE & DESCEND ➔", 75f))
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
            GUI.enabled = true;

            GUILayout.EndArea();
            GUI.matrix = oldMatrix;
        }
    }
}
