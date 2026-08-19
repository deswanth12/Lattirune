using System;
using UnityEngine;
using Lattirune.Combat;
using Lattirune.Core;
using Lattirune.Dungeon;

namespace Lattirune.Events
{
    /// <summary>
    /// Evaluates whether a procedural event should trigger between dungeon encounters or during floor transitions.
    /// Strictly guarantees events never fire during active combat simulation.
    /// </summary>
    public class RunEventTrigger : MonoBehaviour
    {
        [Header("Configuration")]
        [Range(0f, 1f)]
        [SerializeField] private float eventChancePerFloor = 0.60f; // 60% chance between encounters
        [SerializeField] private bool guaranteedOnFloorCadence = true; // Guaranteed on floors 2, 5, 7

        public float EventChancePerFloor => eventChancePerFloor;
        public bool GuaranteedOnFloorCadence => guaranteedOnFloorCadence;

        public void Configure(float chance, bool cadence = true)
        {
            eventChancePerFloor = Mathf.Clamp01(chance);
            guaranteedOnFloorCadence = cadence;
        }

        public bool ShouldTriggerEvent(
            int floorIndex, 
            int encounterIndex, 
            CombatSystem combatSystem, 
            IRandomSource randomSource)
        {
            // 1. Safeguard: NEVER trigger during active combat
            if (combatSystem != null && combatSystem.CurrentState == CombatState.Fighting)
            {
                return false;
            }

            int floorNumber = floorIndex + 1;

            // 2. Exclude merchant and campfire resting floors (Floors 4, 8, 9) from disruptive random events
            if (floorNumber == 4 || floorNumber == 8 || floorNumber == 9)
            {
                return false;
            }

            // 3. Exclude final floor boss arena (Floor 10)
            if (floorNumber >= 10)
            {
                return false;
            }

            // 4. Guaranteed cadence check
            if (guaranteedOnFloorCadence && (floorNumber == 2 || floorNumber == 5 || floorNumber == 7))
            {
                return true;
            }

            // 5. Seeded probabilistic check
            if (randomSource == null)
            {
                randomSource = new SystemRandomSource();
            }

            double roll = randomSource.NextDouble();
            return roll < eventChancePerFloor;
        }
    }
}
