namespace Lattirune.Core
{
    /// <summary>
    /// Represents the current physical state of a cell in the Lattice Grid.
    /// </summary>
    public enum TileState
    {
        Locked,     // Tile is unavailable for placement until unlocked via progression
        Active,     // Tile is unlocked and available for item placement
        Occupied    // Tile is currently occupied by an item footprint
    }

    /// <summary>
    /// Cardinal and special raycasting directions for directional runes.
    /// </summary>
    public enum ConduitDirection
    {
        None,
        North,      // ( 0,  1)
        South,      // ( 0, -1)
        East,       // ( 1,  0)
        West,       // (-1,  0)
        Cross,      // All 4 cardinal directions
        Split,      // Refracts incoming beam into diagonals
        Omni        // Radiates power to all adjacent neighbors
    }

    /// <summary>
    /// Item classification categories.
    /// </summary>
    public enum ItemCategory
    {
        Weapon,
        Shield,
        Armor,
        Relic,
        Consumable,
        Rune
    }

    /// <summary>
    /// Elemental affinities for items, runes, and damage calculations.
    /// </summary>
    public enum ElementType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Light,
        Shadow,
        Wind,
        Force,
        Earth
    }
}
