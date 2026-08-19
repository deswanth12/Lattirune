namespace Lattirune.Combat.Effects
{
    /// <summary>
    /// Categories of mechanical combat effects produced by elemental reactions and status modifications.
    /// </summary>
    public enum CombatEffectType
    {
        DirectDamage,    // Instantaneous damage burst (e.g. Toxic Flame detonation)
        DamageOverTime,  // Periodic ticking damage (e.g. Plasma beam)
        ArmorModifier,   // Temporary defense/resistance reduction (e.g. Superconductor)
        AttackModifier,  // Accuracy / attack strength reduction (e.g. Steam blind/miss)
        DamageModifier,  // Increased damage vulnerability (e.g. Frostbite +50% tick dmg)
        Status           // General combat status flags
    }
}
