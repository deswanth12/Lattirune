using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Dungeon;

namespace Lattirune.Tests
{
    [TestFixture]
    public class DungeonMapSystemTests
    {
        [Test]
        public void CanonicalMap_GeneratesComplete10FloorTopology()
        {
            var map = DungeonMapGraph.CreateCanonicalCursedSewersMap();
            Assert.IsNotNull(map);
            Assert.AreEqual(12, map.AllNodes.Count); // 10 floors + 2 branch nodes (Floor 3 and Floor 7)

            // Verify Floor 1
            var f1 = map.GetNodesOnFloor(1);
            Assert.AreEqual(1, f1.Count);
            Assert.AreEqual("node_f1_entry", f1[0].NodeId);
            Assert.IsTrue(f1[0].IsAvailable);

            // Verify Floor 3 Branch (2 nodes: Elite and Shrine)
            var f3 = map.GetNodesOnFloor(3);
            Assert.AreEqual(2, f3.Count);
            Assert.IsTrue(f3.Exists(n => n.NodeType == DungeonMapNodeType.EliteBattle));
            Assert.IsTrue(f3.Exists(n => n.NodeType == DungeonMapNodeType.MysteryShrine));

            // Verify Floor 10 Boss
            var f10 = map.GetNodesOnFloor(10);
            Assert.AreEqual(1, f10.Count);
            Assert.AreEqual(DungeonMapNodeType.Boss, f10[0].NodeType);
        }

        [Test]
        public void MapProgression_BranchSelection_UnlocksCorrectDownstreamNodes()
        {
            var map = DungeonMapGraph.CreateCanonicalCursedSewersMap();

            // Floor 1 -> Clear
            Assert.IsTrue(map.SelectAndEnterNode("node_f1_entry"));
            Assert.IsTrue(map.CompleteCurrentNode());

            // Floor 2 should now be available
            var f2 = map.GetNode("node_f2_cache");
            Assert.IsNotNull(f2);
            Assert.IsTrue(f2.IsAvailable);

            // Complete Floor 2 -> Unlocks both Floor 3 branches
            Assert.IsTrue(map.SelectAndEnterNode("node_f2_cache"));
            Assert.IsTrue(map.CompleteCurrentNode());

            var f3Elite = map.GetNode("node_f3_elite");
            var f3Shrine = map.GetNode("node_f3_shrine");
            Assert.IsTrue(f3Elite.IsAvailable);
            Assert.IsTrue(f3Shrine.IsAvailable);

            // Player chooses Shrine branch
            Assert.IsTrue(map.SelectAndEnterNode("node_f3_shrine"));
            Assert.IsTrue(map.CompleteCurrentNode());

            // Floor 4 Merchant is now available
            var f4 = map.GetNode("node_f4_merchant");
            Assert.IsNotNull(f4);
            Assert.IsTrue(f4.IsAvailable);
            Assert.AreEqual(DungeonMapNodeType.MerchantStall, f4.NodeType);
        }

        [Test]
        public void Floor7_BranchingPath_ReconvergesAtFloor8Campfire()
        {
            var map = DungeonMapGraph.CreateCanonicalCursedSewersMap();

            // Advance through Floor 1-6
            map.SelectAndEnterNode("node_f1_entry");
            map.CompleteCurrentNode();
            map.SelectAndEnterNode("node_f2_cache");
            map.CompleteCurrentNode();
            map.SelectAndEnterNode("node_f3_elite");
            map.CompleteCurrentNode();
            map.SelectAndEnterNode("node_f4_merchant");
            map.CompleteCurrentNode();
            map.SelectAndEnterNode("node_f5_midboss");
            map.CompleteCurrentNode();
            map.SelectAndEnterNode("node_f6_vault");
            map.CompleteCurrentNode();

            // Floor 7 has Necromancer and Spider Sentry
            var f7Elite = map.GetNode("node_f7_elite");
            var f7Spider = map.GetNode("node_f7_spider");
            Assert.IsTrue(f7Elite.IsAvailable);
            Assert.IsTrue(f7Spider.IsAvailable);

            // Complete Necromancer
            map.SelectAndEnterNode("node_f7_elite");
            map.CompleteCurrentNode();

            // Floor 8 Campfire Rest is now available
            var f8 = map.GetNode("node_f8_rest");
            Assert.IsNotNull(f8);
            Assert.IsTrue(f8.IsAvailable);
            Assert.AreEqual(DungeonMapNodeType.CampfireRest, f8.NodeType);
        }
    }
}
