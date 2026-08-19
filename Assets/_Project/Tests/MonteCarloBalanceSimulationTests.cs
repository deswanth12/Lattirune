using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lattirune.Simulation;

namespace Lattirune.Tests
{
    [TestFixture]
    public class MonteCarloBalanceSimulationTests
    {
        [Test]
        public void MonteCarlo_1000Runs_SimulatesWithZeroCrashesAndBalancedWinRates()
        {
            var rawMetrics = MonteCarloBalanceSimulator.RunBatchSimulation(runCount: 1000, includeMetaUpgrades: false);

            Assert.AreEqual(1000, rawMetrics.totalRuns);
            Assert.GreaterOrEqual(rawMetrics.averageFloorReached, 4.0f);
            Assert.Greater(rawMetrics.averageDps, 5.0f);

            // Win rate without meta upgrades should be a challenging roguelike rate (>10% and <70%)
            Assert.Greater(rawMetrics.winRatePercent, 10f);
            Assert.Less(rawMetrics.winRatePercent, 70f);

            // With meta upgrades, win rate should increase
            var upgradedMetrics = MonteCarloBalanceSimulator.RunBatchSimulation(runCount: 1000, includeMetaUpgrades: true);
            Assert.Greater(upgradedMetrics.winRatePercent, rawMetrics.winRatePercent);
            Assert.Greater(upgradedMetrics.averageFloorReached, rawMetrics.averageFloorReached);

            Debug.Log($""[MONTE CARLO STATS] Base: {rawMetrics}"");
            Debug.Log($""[MONTE CARLO STATS] Upgraded: {upgradedMetrics}"");
        }
    }
}
