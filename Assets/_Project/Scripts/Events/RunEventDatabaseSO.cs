using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Events
{
    /// <summary>
    /// Canonical database of procedural run events for Lattirune 1.1.
    /// </summary>
    [CreateAssetMenu(fileName = "RunEventDatabase", menuName = "Lattirune/Events/Run Event Database")]
    public class RunEventDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<RunEventDefinitionSO> events = new List<RunEventDefinitionSO>();

        public IReadOnlyList<RunEventDefinitionSO> AllEvents => events;
        public int Count => events != null ? events.Count : 0;

        public void Initialize(List<RunEventDefinitionSO> eventList)
        {
            events = eventList ?? new List<RunEventDefinitionSO>();
        }

        public RunEventDefinitionSO GetEvent(string id)
        {
            if (string.IsNullOrEmpty(id) || events == null) return null;
            return events.Find(e => e != null && e.EventId == id);
        }

        public List<RunEventDefinitionSO> GetEligibleEventsForFloor(int floorIndex)
        {
            List<RunEventDefinitionSO> eligible = new List<RunEventDefinitionSO>();
            if (events == null) return eligible;

            for (int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                if (ev != null && ev.Weight > 0 && ev.IsEligibleForFloor(floorIndex))
                {
                    eligible.Add(ev);
                }
            }
            return eligible;
        }

        public static RunEventDatabaseSO CreateCanonicalEventDatabase()
        {
            var db = CreateInstance<RunEventDatabaseSO>();
            var list = new List<RunEventDefinitionSO>();

            // 1. Ancient Shrine (Floor 1-10, Weight 15)
            var e1 = CreateInstance<RunEventDefinitionSO>();
            e1.Initialize(
                ""event_ancient_shrine"",
                ""Ancient Shrine"",
                ""An ancient basalt altar pulses with latent magical resonance. Carved runic inscriptions invite your touch."",
                RunEventType.ModifierReward,
                eventWeight: 15,
                minFloor: 1,
                maxFloor: 10,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice(
                        ""choice_shrine_touch"",
                        ""Touch the Rune"",
                        ""Attune your soul to the relic. Gain Sharpened Runes (+15% Damage)."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: ""mod_sharpened_runes"",
                        curseModId: null,
                        reqGold: 0,
                        oneTime: true
                    ),
                    new RunEventChoice(
                        ""choice_shrine_leave"",
                        ""Leave Safely"",
                        ""Bow respectfully and continue down the dungeon corridor without disturbing the ancient magic."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: false
                    )
                }
            );
            list.Add(e1);

            // 2. Blood Altar (Floor 2-10, Weight 12)
            var e2 = CreateInstance<RunEventDefinitionSO>();
            e2.Initialize(
                ""event_blood_altar"",
                ""Blood Altar"",
                ""A crimson monolith demands a blood tithe in exchange for devastating offensive might."",
                RunEventType.HealthTrade,
                eventWeight: 12,
                minFloor: 2,
                maxFloor: 10,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice(
                        ""choice_altar_sacrifice"",
                        ""Sacrifice Vitality"",
                        ""Bleed upon the stone. Suffer 20% Max HP sacrifice to forge Sharpened Runes (+15% Damage)."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0.20f,
                        restoreHpPct: 0f,
                        grantModId: ""mod_sharpened_runes"",
                        curseModId: null,
                        reqGold: 0,
                        oneTime: true
                    ),
                    new RunEventChoice(
                        ""choice_altar_refuse"",
                        ""Refuse Altar"",
                        ""Keep your lifeblood intact and walk past the sinister stones."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: false
                    )
                }
            );
            list.Add(e2);

            // 3. Cursed Treasury (Floor 2-9, Weight 10)
            var e3 = CreateInstance<RunEventDefinitionSO>();
            e3.Initialize(
                ""event_cursed_treasury"",
                ""Cursed Treasury"",
                ""A gilded chest overflowing with gleaming dungeon coins sits beneath a shadowy hex."",
                RunEventType.RiskReward,
                eventWeight: 10,
                minFloor: 2,
                maxFloor: 9,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice(
                        ""choice_treasury_pillage"",
                        ""Pillage Coffers"",
                        ""Claim 75 Gold, but contract the Curse of Vulnerability (-20% Defense)."",
                        costGold: 0,
                        rewardGold: 75,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: ""mod_curse_vulnerability"",
                        reqGold: 0,
                        oneTime: true
                    ),
                    new RunEventChoice(
                        ""choice_treasury_leave"",
                        ""Walk Away"",
                        ""Resist temptation and leave the cursed hoard undisturbed."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: false
                    )
                }
            );
            list.Add(e3);

            // 4. Elemental Forge (Floor 1-8, Weight 14)
            var e4 = CreateInstance<RunEventDefinitionSO>();
            e4.Initialize(
                ""event_elemental_forge"",
                ""Elemental Forge"",
                ""A dormant conduit smithy still glows with primordial elemental heat."",
                RunEventType.GoldReward,
                eventWeight: 14,
                minFloor: 1,
                maxFloor: 8,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice(
                        ""choice_forge_infuse"",
                        ""Infuse Runes (30 Gold)"",
                        ""Pay 30 Gold to ignite your conduit matrix with Elemental Surge (+25% Elemental Damage)."",
                        costGold: 30,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: ""mod_elemental_surge"",
                        curseModId: null,
                        reqGold: 30,
                        oneTime: true
                    ),
                    new RunEventChoice(
                        ""choice_forge_pass"",
                        ""Pass By"",
                        ""Conserve your gold and move forward."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: false
                    )
                }
            );
            list.Add(e4);

            // 5. Ember Well (Floor 1-10, Weight 18)
            var e5 = CreateInstance<RunEventDefinitionSO>();
            e5.Initialize(
                ""event_ember_well"",
                ""Ember Well"",
                ""A soothing spring radiating pure celestial warmth wells up through the sewer floor."",
                RunEventType.Healing,
                eventWeight: 18,
                minFloor: 1,
                maxFloor: 10,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice(
                        ""choice_well_drink"",
                        ""Drink Deeply"",
                        ""Sip the soothing waters to restore 35% of your Max HP."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0.35f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: true
                    ),
                    new RunEventChoice(
                        ""choice_well_leave"",
                        ""Leave"",
                        ""Save your thirst and march onwards."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: false
                    )
                }
            );
            list.Add(e5);

            // 6. Mysterious Chest (Floor 1-10, Weight 16)
            var e6 = CreateInstance<RunEventDefinitionSO>();
            e6.Initialize(
                ""event_mysterious_chest"",
                ""Mysterious Chest"",
                ""An ornate ironbound chest sits unlocked in an alcove."",
                RunEventType.Mystery,
                eventWeight: 16,
                minFloor: 1,
                maxFloor: 10,
                choiceList: new List<RunEventChoice>
                {
                    new RunEventChoice(
                        ""choice_chest_unlock"",
                        ""Open Chest"",
                        ""Loot the contents. Discover 40 Gold and a rejuvenating tonic restoring 15% HP."",
                        costGold: 0,
                        rewardGold: 40,
                        costHpPct: 0f,
                        restoreHpPct: 0.15f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: true
                    ),
                    new RunEventChoice(
                        ""choice_chest_leave"",
                        ""Leave Unopened"",
                        ""Avoid potential trap triggers and proceed cautiously."",
                        costGold: 0,
                        rewardGold: 0,
                        costHpPct: 0f,
                        restoreHpPct: 0f,
                        grantModId: null,
                        curseModId: null,
                        reqGold: 0,
                        oneTime: false
                    )
                }
            );
            list.Add(e6);

            db.Initialize(list);
            return db;
        }
    }
}
