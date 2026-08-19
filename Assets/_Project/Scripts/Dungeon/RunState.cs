namespace Lattirune.Dungeon
{
    /// <summary>
    /// Explicit state machine stages representing a multi-floor dungeon run lifecycle.
    /// </summary>
    public enum RunState
    {
        NotStarted,         // No active run
        Starting,           // Run is initializing
        FloorPreparing,     // Setting up grid/items for new floor
        EncounterActive,    // 1v1 Combat encounter in progress
        RewardSelection,    // Post-combat 3-card reward draft
        FloorTransition,    // Transitioning between cleared floor and next floor
        RunComplete,        // Final floor boss defeated, run successfully completed
        Defeated,           // Player defeated, run failed
        EventActive         // Procedural run event in progress (between encounters/floors)
    }
}
