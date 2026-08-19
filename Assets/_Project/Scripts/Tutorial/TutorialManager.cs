using System;
using UnityEngine;

namespace Lattirune.Tutorial
{
    public enum TutorialStep
    {
        DragWeaponToGrid = 0,
        ConnectRuneLaser = 1,
        StartFirstBattle = 2,
        Completed = 3
    }

    /// <summary>
    /// Contextual onboarding coordinator guiding first-time players through grid arrangement,
    /// laser conduit connection, and combat initiation.
    /// Strictly adheres to PLAN.md Section 2 and Section 34 (< 45s comprehension target).
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private TutorialStep currentStep = TutorialStep.DragWeaponToGrid;
        [SerializeField] private bool isTutorialCompleted = false;

        public TutorialStep CurrentStep => currentStep;
        public bool IsTutorialCompleted => isTutorialCompleted;

        public event Action<TutorialStep> OnStepChanged;
        public event Action OnTutorialCompleted;

        public void Initialize(bool alreadyCompleted = false)
        {
            isTutorialCompleted = alreadyCompleted;
            currentStep = alreadyCompleted ? TutorialStep.Completed : TutorialStep.DragWeaponToGrid;
        }

        public void AdvanceStep(TutorialStep completedStep)
        {
            if (isTutorialCompleted) return;

            if (currentStep == completedStep)
            {
                if (currentStep == TutorialStep.StartFirstBattle)
                {
                    CompleteTutorial();
                }
                else
                {
                    currentStep = (TutorialStep)((int)currentStep + 1);
                    OnStepChanged?.Invoke(currentStep);
                }
            }
        }

        public void CompleteTutorial()
        {
            currentStep = TutorialStep.Completed;
            isTutorialCompleted = true;
            OnTutorialCompleted?.Invoke();
        }

        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        public string GetCurrentStepHint()
        {
            switch (currentStep)
            {
                case TutorialStep.DragWeaponToGrid:
                    return ""1. DRAG TO ARRANGE: Pick up your sword from the staging tray and place it into the 5x5 Lattice Grid."";
                case TutorialStep.ConnectRuneLaser:
                    return ""2. EMIT CONDUIT: Position a directional Rune pointing at your sword to cast a glowing elemental laser!"";
                case TutorialStep.StartFirstBattle:
                    return ""3. RESOLVE COMBAT: Tap START BATTLE to unleash automatic weapon attacks and elemental reactions!"";
                default:
                    return ""Tutorial Completed!"";
            }
        }
    }
}
