using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Audio;

namespace Lattirune.UI
{
    /// <summary>
    /// Centralized navigation coordinator managing mobile screen transitions, navigation history stack,
    /// and safe Android Back button handling.
    /// Strictly adheres to PLAN.md Section 14 and Section 19.
    /// </summary>
    public class ScreenNavigationController : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private ScreenState currentScreen = ScreenState.MAIN_MENU;

        private readonly Stack<ScreenState> _history = new Stack<ScreenState>();

        public event Action<ScreenState, ScreenState> OnScreenChanged;
        public event Action<ScreenState> OnBackNavigationBlocked;

        public ScreenState CurrentScreen => currentScreen;
        public int HistoryCount => _history.Count;

        [Header("References")]
        [SerializeField] private GameObject worldGridObject;
        [SerializeField] private MonoBehaviour tutorialController;

        private readonly Dictionary<ScreenState, MonoBehaviour> _screenRegistry = new Dictionary<ScreenState, MonoBehaviour>();

        public void BindWorldGrid(GameObject gridObj)
        {
            worldGridObject = gridObj;
        }

        public void RegisterScreenController(ScreenState state, MonoBehaviour controller)
        {
            if (controller != null)
            {
                _screenRegistry[state] = controller;
                EnforceScreenVisibility(currentScreen);
            }
        }

        public void Initialize(ScreenState startingScreen = ScreenState.MAIN_MENU)
        {
            currentScreen = startingScreen;
            _history.Clear();
            EnforceScreenVisibility(startingScreen);
        }

        public void NavigateTo(ScreenState nextScreen, bool recordHistory = true)
        {
            if (currentScreen == nextScreen) return;

            ScreenState previous = currentScreen;

            if (recordHistory)
            {
                _history.Push(previous);
            }

            currentScreen = nextScreen;
            AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
            HapticFeedback.Trigger(HapticFeedbackType.Selection);

            EnforceScreenVisibility(currentScreen);
            OnScreenChanged?.Invoke(previous, currentScreen);
        }

        public void ShowScreen(ScreenState state)
        {
            NavigateTo(state, recordHistory: true);
        }

        private void EnforceScreenVisibility(ScreenState activeState)
        {
            // 1. Control 3D Grid & World space rendering visibility
            bool showWorldGrid = (activeState == ScreenState.GRID_BUILD || activeState == ScreenState.COMBAT || activeState == ScreenState.REWARD_SELECTION || activeState == ScreenState.INVENTORY);
            if (worldGridObject != null)
            {
                worldGridObject.SetActive(showWorldGrid);
            }

            // 2. Control registered mono screen controllers
            if (activeState != ScreenState.SETTINGS)
            {
                HashSet<MonoBehaviour> targetControllers = new HashSet<MonoBehaviour>();
                foreach (var kvp in _screenRegistry)
                {
                    if (kvp.Value != null && kvp.Key == activeState)
                    {
                        targetControllers.Add(kvp.Value);
                    }
                }

                HashSet<MonoBehaviour> visited = new HashSet<MonoBehaviour>();
                foreach (var kvp in _screenRegistry)
                {
                    if (kvp.Value != null && visited.Add(kvp.Value))
                    {
                        kvp.Value.enabled = targetControllers.Contains(kvp.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Handles Android hardware back button or in-app back navigation.
        /// Enforces screen safety rules to prevent run state corruption.
        /// </summary>
        public bool NavigateBack()
        {
            // Safety: Disallow accidental exit from live Combat without explicit forfeit
            if (currentScreen == ScreenState.COMBAT)
            {
                OnBackNavigationBlocked?.Invoke(currentScreen);
                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                HapticFeedback.Trigger(HapticFeedbackType.Failure);
                return false;
            }

            // Contextual back routing
            if (currentScreen == ScreenState.BLUEPRINT_FORGE || currentScreen == ScreenState.HERO_SELECTION || currentScreen == ScreenState.CODEX)
            {
                NavigateTo(ScreenState.CAMPFIRE_HUB, recordHistory: false);
                return true;
            }

            if (currentScreen == ScreenState.CAMPFIRE_HUB || currentScreen == ScreenState.SETTINGS)
            {
                NavigateTo(ScreenState.MAIN_MENU, recordHistory: false);
                return true;
            }

            if (currentScreen == ScreenState.MERCHANT || currentScreen == ScreenState.CAMPFIRE_REST || currentScreen == ScreenState.INVENTORY)
            {
                NavigateTo(ScreenState.GRID_BUILD, recordHistory: false);
                return true;
            }

            if (_history.Count > 0)
            {
                ScreenState previous = _history.Pop();
                ScreenState old = currentScreen;
                currentScreen = previous;

                AudioController.Instance?.PlaySfx(AudioCueType.ButtonClick);
                HapticFeedback.Trigger(HapticFeedbackType.Selection);
                OnScreenChanged?.Invoke(old, currentScreen);
                return true;
            }

            return false;
        }

        public void ClearHistory()
        {
            _history.Clear();
        }

        private void Update()
        {
            // Android Hardware Back button trigger (Escape key in Unity)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                NavigateBack();
            }
        }
    }
}

