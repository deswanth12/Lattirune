using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Dungeon
{
    public enum DungeonMapNodeType
    {
        NormalBattle,
        EliteBattle,
        MysteryShrine,
        MerchantStall,
        TreasureVault,
        CampfireRest,
        Boss
    }

    /// <summary>
    /// Represents an individual room or encounter node in the 10-Floor Dungeon Map DAG.
    /// Derived strictly from PLAN.md Section 2 and Section 11.
    /// </summary>
    [Serializable]
    public class DungeonMapNode
    {
        [SerializeField] private string nodeId;
        [SerializeField] private int floorNumber;
        [SerializeField] private DungeonMapNodeType nodeType;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private bool isCleared;
        [SerializeField] private bool isAvailable;
        [SerializeField] private List<string> nextNodeIds = new List<string>();

        public string NodeId => nodeId;
        public int FloorNumber => floorNumber;
        public DungeonMapNodeType NodeType => nodeType;
        public string Title => title;
        public string Description => description;
        public bool IsCleared => isCleared;
        public bool IsAvailable => isAvailable;
        public IReadOnlyList<string> NextNodeIds => nextNodeIds;

        public DungeonMapNode(
            string id,
            int floor,
            DungeonMapNodeType type,
            string title,
            string desc,
            List<string> nextIds = null)
        {
            this.nodeId = id;
            this.floorNumber = Mathf.Max(1, floor);
            this.nodeType = type;
            this.title = title;
            this.description = desc;
            this.isCleared = false;
            this.isAvailable = (floor == 1);
            this.nextNodeIds = nextIds ?? new List<string>();
        }

        public void SetAvailable(bool available)
        {
            isAvailable = available;
        }

        public void MarkCleared()
        {
            isCleared = true;
            isAvailable = false;
        }
    }
}
