namespace Lattirune.Audio
{
    /// <summary>
    /// Identifier catalog for all 15 prototype sound effect cues.
    /// </summary>
    public enum AudioCueType
    {
        ItemDragStart,
        ItemValidPlacement,
        ItemInvalidPlacement,
        RuneConduit,
        RuneConduitIgnite,
        SynergyActivated,
        SynergyDeactivated,
        Attack,
        Damage,
        CombatBladeSlash,
        CombatBurnTick,
        CombatFreezeShatter,
        CombatBossRoar,
        BgmDungeonLoop,
        Victory,
        Defeat,
        RewardSelected,
        RewardApplied,
        ButtonClick,
        Retry,
        Continue
    }

    /// <summary>
    /// UI/Gameplay sound effect alias enum matching audio cue mappings.
    /// </summary>
    public enum SoundEffectType
    {
        ButtonClick,
        UiClick,
        UIClick,
        ItemPlaced,
        ItemPickup,
        InvalidPlacement,
        RewardClaimed,
        Victory,
        Defeat,
        CombatHit
    }
}
