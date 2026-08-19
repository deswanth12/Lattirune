namespace Lattirune.Runes
{
    /// <summary>
    /// Describes why a directional conduit raycast stopped traversing the grid.
    /// </summary>
    public enum ConduitTerminationReason
    {
        None,
        RangeReached,       // Conduit reached its maximum range parameter
        GridBoundary,       // Conduit reached the edge of the 5x5 LatticeGrid
        LockedCell,         // Conduit encountered a locked grid tile
        BlockedByOccupant,  // Conduit encountered an insulating/blocking occupied cell
        TargetFound,        // Conduit reached a terminating target cell
        InvalidOrigin,      // Conduit origin coordinate was out of bounds
        UnsupportedDirection // Direction was None or an unsupported composite direction
    }
}
