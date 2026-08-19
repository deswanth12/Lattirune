using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Events
{
    /// <summary>
    /// ScriptableObject defining an immutable procedural run event definition for Lattirune 1.1.
    /// </summary>
    [CreateAssetMenu(fileName = "RunEvent", menuName = "Lattirune/Events/Run Event Definition")]
    public class RunEventDefinitionSO : ScriptableObject
    {
        [SerializeField] private string eventId;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private RunEventType eventType = RunEventType.Mystery;
        [SerializeField] private int weight = 10;
        [SerializeField] private int minimumFloor = 1;
        [SerializeField] private int maximumFloor = 10;
        [SerializeField] private List<RunEventChoice> choices = new List<RunEventChoice>();

        public string EventId => eventId;
        public string Title => title;
        public string Description => description;
        public RunEventType EventType => eventType;
        public int Weight => weight;
        public int MinimumFloor => minimumFloor;
        public int MaximumFloor => maximumFloor;
        public IReadOnlyList<RunEventChoice> Choices => choices;
        public int ChoiceCount => choices != null ? choices.Count : 0;

        public void Initialize(
            string id,
            string eventTitle,
            string desc,
            RunEventType type,
            int eventWeight,
            int minFloor,
            int maxFloor,
            List<RunEventChoice> choiceList)
        {
            eventId = id;
            title = eventTitle;
            description = desc;
            eventType = type;
            weight = Mathf.Max(0, eventWeight);
            minimumFloor = Mathf.Max(1, minFloor);
            maximumFloor = Mathf.Max(minimumFloor, maxFloor);
            choices = choiceList ?? new List<RunEventChoice>();
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                error = ""Event ID cannot be empty."";
                return false;
            }

            if (string.IsNullOrEmpty(title))
            {
                error = ""Event title cannot be empty."";
                return false;
            }

            if (weight < 0)
            {
                error = ""Event weight cannot be negative."";
                return false;
            }

            if (minimumFloor > maximumFloor)
            {
                error = $""Minimum floor ({minimumFloor}) cannot exceed maximum floor ({maximumFloor})."";
                return false;
            }

            if (choices == null || choices.Count == 0)
            {
                error = ""Event must contain at least one choice."";
                return false;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i] == null || string.IsNullOrEmpty(choices[i].ChoiceId))
                {
                    error = $""Choice at index {i} is null or has an empty ChoiceId."";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        public RunEventChoice GetChoice(string choiceId)
        {
            if (string.IsNullOrEmpty(choiceId) || choices == null) return null;
            return choices.Find(c => c != null && c.ChoiceId == choiceId);
        }

        public bool IsEligibleForFloor(int floorIndex)
        {
            int floorNumber = floorIndex + 1;
            return floorNumber >= minimumFloor && floorNumber <= maximumFloor;
        }
    }
}
