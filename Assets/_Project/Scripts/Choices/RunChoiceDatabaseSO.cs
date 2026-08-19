using System.Collections.Generic;
using UnityEngine;
using Lattirune.Modifiers;

namespace Lattirune.Choices
{
    /// <summary>
    /// Canonical repository of standard risk/reward offerings for Lattirune 1.1.
    /// </summary>
    [CreateAssetMenu(fileName = "RunChoiceDatabase", menuName = "Lattirune/Choices/Run Choice Database")]
    public class RunChoiceDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<RunChoiceDefinitionSO> choices = new List<RunChoiceDefinitionSO>();

        public IReadOnlyList<RunChoiceDefinitionSO> AllChoices => choices;
        public int Count => choices != null ? choices.Count : 0;

        public void Initialize(List<RunChoiceDefinitionSO> list)
        {
            choices = list ?? new List<RunChoiceDefinitionSO>();
        }

        public RunChoiceDefinitionSO GetChoice(string id)
        {
            if (string.IsNullOrEmpty(id) || choices == null) return null;
            return choices.Find(c => c != null && c.ChoiceId == id);
        }

        public static RunChoiceDatabaseSO CreateCanonicalChoiceDatabase()
        {
            var db = CreateInstance<RunChoiceDatabaseSO>();
            var modDb = RunModifierDatabaseSO.CreateCanonicalDatabase();
            var list = new List<RunChoiceDefinitionSO>();

            // Choice 1: Blood Pact (Gain Sharpened Runes for 20% Current HP)
            var c1 = CreateInstance<RunChoiceDefinitionSO>();
            c1.Initialize(
                "choice_blood_pact",
                "Blood Pact",
                "Sacrifice vitality in exchange for permanent run damage.",
                "+15% Attack Damage (Sharpened Runes)",
                "Sacrifice 20% Hero Max HP",
                costGold: 0,
                costHpPct: 0.20f,
                grantMod: modDb.GetModifier("mod_sharpened_runes"),
                curseMod: null,
                oneTime: true
            );
            list.Add(c1);

            // Choice 2: Greed of the Lich (Gain 50% Gold drops, but suffer Curse of Vulnerability)
            var c2 = CreateInstance<RunChoiceDefinitionSO>();
            c2.Initialize(
                "choice_lich_greed",
                "Greed of the Lich",
                "Accept a cursed talisman overflowing with stolen dungeon gold.",
                "+50% Gold Multiplier (Midas Touch)",
                "Inflicted with Curse of Vulnerability (-20% Defense)",
                costGold: 0,
                costHpPct: 0f,
                grantMod: modDb.GetModifier("mod_midas_touch"),
                curseMod: modDb.GetModifier("mod_curse_vulnerability"),
                oneTime: true
            );
            list.Add(c2);

            // Choice 3: Alchemical Transmutation (Spend 30 Gold to gain Elemental Surge)
            var c3 = CreateInstance<RunChoiceDefinitionSO>();
            c3.Initialize(
                "choice_alchemical_transmutation",
                "Alchemical Transmutation",
                "Hire a traveling conduit artificer to refine your runes.",
                "+25% Elemental Reaction Damage (Elemental Surge)",
                "Cost: 30 Gold",
                costGold: 30,
                costHpPct: 0f,
                grantMod: modDb.GetModifier("mod_elemental_surge"),
                curseMod: null,
                oneTime: true
            );
            list.Add(c3);

            db.Initialize(list);
            return db;
        }
    }
}
