using UnityEngine;
using Lattirune.Modifiers;

namespace Lattirune.Choices
{
    /// <summary>
    /// ScriptableObject defining a dual-edged risk/reward offering during a procedural run.
    /// </summary>
    [CreateAssetMenu(fileName = "RunChoice", menuName = "Lattirune/Choices/Run Choice Definition")]
    public class RunChoiceDefinitionSO : ScriptableObject
    {
        [SerializeField] private string choiceId;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private string positiveEffectDescription;
        [SerializeField] private string negativeEffectDescription;
        [SerializeField] private int goldCost = 0;
        [SerializeField] private float healthCostPercentage = 0f; // 0.0 to 1.0 (e.g. 0.2 = 20% max HP)
        [SerializeField] private RunModifierDefinitionSO grantedModifier;
        [SerializeField] private RunModifierDefinitionSO curseModifier;
        [SerializeField] private bool isOneTimeUse = true;

        public string ChoiceId => choiceId;
        public string Title => title;
        public string Description => description;
        public string PositiveEffectDescription => positiveEffectDescription;
        public string NegativeEffectDescription => negativeEffectDescription;
        public int GoldCost => goldCost;
        public float HealthCostPercentage => healthCostPercentage;
        public RunModifierDefinitionSO GrantedModifier => grantedModifier;
        public RunModifierDefinitionSO CurseModifier => curseModifier;
        public bool IsOneTimeUse => isOneTimeUse;

        public void Initialize(
            string id,
            string choiceTitle,
            string desc,
            string posDesc,
            string negDesc,
            int costGold,
            float costHpPct,
            RunModifierDefinitionSO grantMod = null,
            RunModifierDefinitionSO curseMod = null,
            bool oneTime = true)
        {
            choiceId = id;
            title = choiceTitle;
            description = desc;
            positiveEffectDescription = posDesc;
            negativeEffectDescription = negDesc;
            goldCost = Mathf.Max(0, costGold);
            healthCostPercentage = Mathf.Clamp01(costHpPct);
            grantedModifier = grantMod;
            curseModifier = curseMod;
            isOneTimeUse = oneTime;
        }
    }
}
