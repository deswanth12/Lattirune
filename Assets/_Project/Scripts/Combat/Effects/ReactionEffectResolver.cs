using UnityEngine;
using Lattirune.Reactions;

namespace Lattirune.Combat.Effects
{
    /// <summary>
    /// Translates an active ElementalReactionResult into concrete CombatEffectInstances.
    /// Bridges the spatial 2-beam reaction engine to the combat execution layer.
    /// </summary>
    public static class ReactionEffectResolver
    {
        public static CombatEffectInstance ResolveEffect(
            ElementalReactionResult reaction, 
            CombatEffectDatabaseSO effectDatabase,
            Combatant target)
        {
            if (reaction == null || !reaction.IsActive || effectDatabase == null || target == null)
            {
                return null;
            }

            CombatEffectDefinitionSO def = effectDatabase.GetByReactionId(reaction.ReactionId);
            if (def == null)
            {
                return null;
            }

            return new CombatEffectInstance(def, reaction.RuneAId, reaction.RuneBId, target);
        }
    }
}
