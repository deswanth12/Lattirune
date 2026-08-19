namespace Lattirune.Combat
{
    /// <summary>
    /// Lifecycle states for an auto-battle combat encounter.
    /// </summary>
    public enum CombatState
    {
        Preparing,  // Player organizing grid inventory prior to initiating battle
        Fighting,   // Auto-battle loop actively executing attacks and cooldowns
        Victory,    // Enemy HP reached 0; encounter won
        Defeat      // Player HP reached 0; encounter lost
    }
}
