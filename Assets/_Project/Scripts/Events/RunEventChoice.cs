using System;
using UnityEngine;

namespace Lattirune.Events
{
    /// <summary>
    /// Serializable data structure representing a single selectable choice outcome within a RunEvent.
    /// </summary>
    [Serializable]
    public class RunEventChoice
    {
        [SerializeField] private string choiceId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private int goldCost = 0;
        [SerializeField] private int goldReward = 0;
        [SerializeField] private float healthCostPercentage = 0f;      // 0.0 to 1.0 (e.g. 0.20 = 20% max HP)
        [SerializeField] private float healthRestorePercentage = 0f;   // 0.0 to 1.0 (e.g. 0.30 = 30% max HP)
        [SerializeField] private string grantedModifierId;
        [SerializeField] private string curseModifierId;
        [SerializeField] private int requiredGold = 0;
        [SerializeField] private bool oneTimeUse = true;

        public string ChoiceId => choiceId;
        public string DisplayName => displayName;
        public string Description => description;
        public int GoldCost => goldCost;
        public int GoldReward => goldReward;
        public float HealthCostPercentage => healthCostPercentage;
        public float HealthRestorePercentage => healthRestorePercentage;
        public string GrantedModifierId => grantedModifierId;
        public string CurseModifierId => curseModifierId;
        public int RequiredGold => requiredGold;
        public bool OneTimeUse => oneTimeUse;

        public RunEventChoice() { }

        public RunEventChoice(
            string id,
            string name,
            string desc,
            int costGold = 0,
            int rewardGold = 0,
            float costHpPct = 0f,
            float restoreHpPct = 0f,
            string grantModId = null,
            string curseModId = null,
            int reqGold = 0,
            bool oneTime = true)
        {
            choiceId = id;
            displayName = name;
            description = desc;
            goldCost = Mathf.Max(0, costGold);
            goldReward = Mathf.Max(0, rewardGold);
            healthCostPercentage = Mathf.Clamp01(costHpPct);
            healthRestorePercentage = Mathf.Clamp01(restoreHpPct);
            grantedModifierId = grantModId;
            curseModifierId = curseModId;
            requiredGold = Mathf.Max(0, reqGold);
            oneTimeUse = oneTime;
        }
    }
}
