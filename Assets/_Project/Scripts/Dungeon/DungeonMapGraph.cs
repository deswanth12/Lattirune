using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lattirune.Dungeon
{
    /// <summary>
    /// Master Graph Data Structure managing the 10-Floor Dungeon Map DAG and branch path progression.
    /// Strictly adheres to PLAN.md Section 2 (Explore) and Section 11 (The Cursed Sewers).
    /// </summary>
    [Serializable]
    public class DungeonMapGraph
    {
        [SerializeField] private List<DungeonMapNode> nodes = new List<DungeonMapNode>();
        [SerializeField] private string currentNodeId = "node_f1_entry";

        private readonly Dictionary<string, DungeonMapNode> _lookup = new Dictionary<string, DungeonMapNode>();

        public IReadOnlyList<DungeonMapNode> AllNodes => nodes;
        public string CurrentNodeId => currentNodeId;

        public void Initialize(List<DungeonMapNode> nodeList, string startingNodeId = "node_f1_entry")
        {
            nodes = nodeList ?? new List<DungeonMapNode>();
            currentNodeId = startingNodeId;
            BuildLookup();
        }

        public void BuildLookup()
        {
            _lookup.Clear();
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.NodeId))
                {
                    if (!_lookup.ContainsKey(node.NodeId))
                    {
                        _lookup.Add(node.NodeId, node);
                    }
                }
            }
        }

        public DungeonMapNode GetNode(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_lookup.Count != (nodes != null ? nodes.Count : 0))
            {
                BuildLookup();
            }

            if (_lookup.TryGetValue(id, out var node)) return node;
            return nodes?.Find(x => x != null && x.NodeId == id);
        }

        public List<DungeonMapNode> GetNodesOnFloor(int floor)
        {
            List<DungeonMapNode> floorNodes = new List<DungeonMapNode>();
            if (nodes == null) return floorNodes;

            foreach (var n in nodes)
            {
                if (n != null && n.FloorNumber == floor)
                {
                    floorNodes.Add(n);
                }
            }
            return floorNodes;
        }

        public List<DungeonMapNode> GetAvailableNodes()
        {
            List<DungeonMapNode> available = new List<DungeonMapNode>();
            if (nodes == null) return available;

            foreach (var n in nodes)
            {
                if (n != null && n.IsAvailable && !n.IsCleared)
                {
                    available.Add(n);
                }
            }
            return available;
        }

        public bool SelectAndEnterNode(string nodeId)
        {
            var node = GetNode(nodeId);
            if (node == null || !node.IsAvailable || node.IsCleared)
            {
                return false;
            }

            currentNodeId = nodeId;
            return true;
        }

        public bool CompleteCurrentNode()
        {
            var current = GetNode(currentNodeId);
            if (current == null) return false;

            current.MarkCleared();

            // Unlock next connected nodes in DAG
            foreach (var nextId in current.NextNodeIds)
            {
                var next = GetNode(nextId);
                if (next != null)
                {
                    next.SetAvailable(true);
                }
            }

            return true;
        }

        /// <summary>
        /// Generates the canonical 10-Floor branching dungeon DAG as defined in PLAN.md Section 2 and 11.
        /// </summary>
        public static DungeonMapGraph CreateCanonicalCursedSewersMap()
        {
            var graph = new DungeonMapGraph();
            var list = new List<DungeonMapNode>();

            // Floor 1: Sewer Entry (Normal Fight) -> connects to Floor 2
            list.Add(new DungeonMapNode(
                id: "node_f1_entry",
                floor: 1,
                type: DungeonMapNodeType.NormalBattle,
                title: "Floor 1: Sewer Entry",
                desc: "A damp corridor infested with Sewer Rats.",
                nextIds: new List<string> { "node_f2_cache" }
            ));

            // Floor 2: Loot Cache -> Branches to Floor 3 Elite OR Floor 3 Shrine
            list.Add(new DungeonMapNode(
                id: "node_f2_cache",
                floor: 2,
                type: DungeonMapNodeType.TreasureVault,
                title: "Floor 2: Drain Basin",
                desc: "A submerged cistern guarded by Goblin Scouts.",
                nextIds: new List<string> { "node_f3_elite", "node_f3_shrine" }
            ));

            // Floor 3 Branch A: Elite Fight (Acid Slime) -> connects to Floor 4
            list.Add(new DungeonMapNode(
                id: "node_f3_elite",
                floor: 3,
                type: DungeonMapNodeType.EliteBattle,
                title: "Floor 3: Caustic Cavern (Elite)",
                desc: "A corrosive den housing the fearsome Acid Slime. High risk, guaranteed rare relic.",
                nextIds: new List<string> { "node_f4_merchant" }
            ));

            // Floor 3 Branch B: Mystery Shrine -> connects to Floor 4
            list.Add(new DungeonMapNode(
                id: "node_f3_shrine",
                floor: 3,
                type: DungeonMapNodeType.MysteryShrine,
                title: "Floor 3: Forgotten Shrine",
                desc: "An ancient subterranean altar radiating mysterious arcane resonance.",
                nextIds: new List<string> { "node_f4_merchant" }
            ));

            // Floor 4: Merchant Stall -> connects to Floor 5
            list.Add(new DungeonMapNode(
                id: "node_f4_merchant",
                floor: 4,
                type: DungeonMapNodeType.MerchantStall,
                title: "Floor 4: Merchant Outpost",
                desc: "A shadowy merchant offering rare weapons, runes, and grid slot expansions.",
                nextIds: new List<string> { "node_f5_midboss" }
            ));

            // Floor 5: Mid-Boss Challenge (Armored Skeleton) -> connects to Floor 6
            list.Add(new DungeonMapNode(
                id: "node_f5_midboss",
                floor: 5,
                type: DungeonMapNodeType.EliteBattle,
                title: "Floor 5: Armory Gate (Mid-Boss)",
                desc: "An impenetrable armored skeleton champion guarding the central vault.",
                nextIds: new List<string> { "node_f6_vault" }
            ));

            // Floor 6: Treasure Vault -> Branches to Floor 7 Necromancer OR Floor 7 Spider Nest
            list.Add(new DungeonMapNode(
                id: "node_f6_vault",
                floor: 6,
                type: DungeonMapNodeType.TreasureVault,
                title: "Floor 6: Royal Vault",
                desc: "An untouched treasure chamber overflowing with gold and ancient relics.",
                nextIds: new List<string> { "node_f7_elite", "node_f7_spider" }
            ));

            // Floor 7 Branch A: Necromancer (Elite) -> connects to Floor 8
            list.Add(new DungeonMapNode(
                id: "node_f7_elite",
                floor: 7,
                type: DungeonMapNodeType.EliteBattle,
                title: "Floor 7: Bone Crypt (Elite)",
                desc: "A dark necromancer summoning unending skeletal hordes.",
                nextIds: new List<string> { "node_f8_rest" }
            ));

            // Floor 7 Branch B: Spider Sentry -> connects to Floor 8
            list.Add(new DungeonMapNode(
                id: "node_f7_spider",
                floor: 7,
                type: DungeonMapNodeType.NormalBattle,
                title: "Floor 7: Webbed Depths",
                desc: "A venomous spider nest dripping with toxic venom.",
                nextIds: new List<string> { "node_f8_rest" }
            ));

            // Floor 8: Campfire Rest Site -> connects to Floor 9
            list.Add(new DungeonMapNode(
                id: "node_f8_rest",
                floor: 8,
                type: DungeonMapNodeType.CampfireRest,
                title: "Floor 8: Campfire Rest Site",
                desc: "A tranquil sanctuary. Rest to heal 40% HP or forge a permanent rune upgrade.",
                nextIds: new List<string> { "node_f9_merchant" }
            ));

            // Floor 9: Pre-Boss Armory & Merchant -> connects to Floor 10
            list.Add(new DungeonMapNode(
                id: "node_f9_merchant",
                floor: 9,
                type: DungeonMapNodeType.MerchantStall,
                title: "Floor 9: Pre-Boss Outpost",
                desc: "Final provisions and supplies before descending into the Lich Lord's chamber.",
                nextIds: new List<string> { "node_f10_boss" }
            ));

            // Floor 10: Boss Chamber (The Lich Lord)
            list.Add(new DungeonMapNode(
                id: "node_f10_boss",
                floor: 10,
                type: DungeonMapNodeType.Boss,
                title: "Floor 10: The Lich Sanctum (BOSS)",
                desc: "The master of the Cursed Sewers. Freezes grid rows and inverts laser conduits.",
                nextIds: new List<string>()
            ));

            graph.Initialize(list, "node_f1_entry");
            return graph;
        }
    }
}
