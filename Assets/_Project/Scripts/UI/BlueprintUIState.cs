namespace Lattirune.UI
{
    /// <summary>
    /// Lifecycle state for blueprint entries rendered in the Blueprint Forge UI.
    /// </summary>
    public enum BlueprintUIState
    {
        Locked,              // Prerequisite blueprints not met
        Available,           // Requirements met and affordable
        InsufficientEmbers,  // Requirements met but player cannot afford Ember cost
        Unlocked             // Already permanently unlocked
    }
}
