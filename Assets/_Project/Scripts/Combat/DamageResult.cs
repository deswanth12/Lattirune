namespace Lattirune.Combat
{
    /// <summary>
    /// Encapsulates the complete mathematical breakdown of an attack execution.
    /// </summary>
    public class DamageResult
    {
        public string SourceName { get; private set; }
        public string TargetName { get; private set; }
        public int BaseDamage { get; private set; }
        public int RuneBonus { get; private set; }
        public float CritMultiplier { get; private set; }
        public float DamageModifiers { get; private set; }
        public int TargetArmor { get; private set; }
        public int FinalDamage { get; private set; }
        public bool IsCritical { get; private set; }
        public bool IsReflected { get; private set; }
        public bool HasSynergyBonus => RuneBonus > 0;

        public DamageResult(
            string source,
            string target,
            int baseDamage,
            int runeBonus,
            float critMultiplier,
            float damageModifiers,
            int targetArmor,
            int finalDamage,
            bool isCritical,
            bool isReflected = false)
        {
            SourceName = source;
            TargetName = target;
            BaseDamage = baseDamage;
            RuneBonus = runeBonus;
            CritMultiplier = critMultiplier;
            DamageModifiers = damageModifiers;
            TargetArmor = targetArmor;
            FinalDamage = finalDamage;
            IsCritical = isCritical;
            IsReflected = isReflected;
        }
    }
}
