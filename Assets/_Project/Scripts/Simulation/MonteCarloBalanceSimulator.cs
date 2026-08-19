using System;
using System.Collections.Generic;
using UnityEngine;
using Lattirune.Dungeon;
using Lattirune.Progression;

namespace Lattirune.Simulation
{
    [Serializable]
    public class SimulationMetrics
    {
        public int totalRuns;
        public int successfulClears;
        public float winRatePercent;
        public float averageFloorReached;
        public float averageDps;
        public Dictionary<int, int> deathsPerFloor = new Dictionary<int, int>();
        public Dictionary<string, int> classClears = new Dictionary<string, int>();
        public Dictionary<string, int> classRuns = new Dictionary<string, int>();

        public override string ToString()
        {
            return $""Monte Carlo Results: {successfulClears}/{totalRuns} Cleared ({winRatePercent:0.1}%) | Avg Floor: {averageFloorReached:0.2} | Avg DPS: {averageDps:0.1}"";
        }
    }

    /// <summary>
    /// Headless, high-speed Monte Carlo battle and run simulator balancing mathematical curves across 10 floors.
    /// Strictly adheres to PLAN.md Section 29.
    /// </summary>
    public static class MonteCarloBalanceSimulator
    {
        public static SimulationMetrics RunBatchSimulation(int runCount = 1000, bool includeMetaUpgrades = false)
        {
            var metrics = new SimulationMetrics
            {
                totalRuns = runCount,
                successfulClears = 0
            };

            for (int f = 1; f <= 10; f++)
            {
                metrics.deathsPerFloor[f] = 0;
            }

            string[] heroClasses = new[] { ""class_rune_knight"", ""class_elementalist"", ""class_shadow_rogue"", ""class_iron_juggernaut"" };
            foreach (var c in heroClasses)
            {
                metrics.classClears[c] = 0;
                metrics.classRuns[c] = 0;
            }

            float totalFloorsReached = 0;
            float totalDpsAccumulator = 0;

            for (int runIdx = 0; runIdx < runCount; runIdx++)
            {
                int seed = runIdx + 1;
                var rng = new System.Random(seed);

                string selectedClass = heroClasses[runIdx % heroClasses.Length];
                metrics.classRuns[selectedClass]++;

                // Base Class Config
                int playerMaxHp = 100;
                int playerArmor = 2;
                float playerAttack = 10;
                float attackInterval = 1.8f;

                switch (selectedClass)
                {
                    case ""class_elementalist"":
                        playerMaxHp = 85;
                        playerArmor = 0;
                        playerAttack = 12; // Wand + spark rune
                        attackInterval = 1.4f;
                        break;
                    case ""class_shadow_rogue"":
                        playerMaxHp = 90;
                        playerArmor = 1;
                        playerAttack = 14;
                        attackInterval = 1.0f;
                        break;
                    case ""class_iron_juggernaut"":
                        playerMaxHp = 140;
                        playerArmor = 6;
                        playerAttack = 14;
                        attackInterval = 2.4f;
                        break;
                }

                if (includeMetaUpgrades)
                {
                    playerMaxHp += 20; // Meta HP blueprint
                    playerAttack += 3;
                }

                int currentHp = playerMaxHp;
                int gold = 0;
                int floorReached = 1;
                bool runWon = true;

                // Simulate 10 Floors
                for (int floor = 1; floor <= 10; floor++)
                {
                    floorReached = floor;

                    // In-Run scaling: Player upgrades equipment every 2 floors (loot cache / merchant)
                    if (floor == 3 || floor == 5 || floor == 7 || floor == 9)
                    {
                        playerAttack += 3.5f;
                    }

                    // Floor 8: Campfire heal
                    if (floor == 8)
                    {
                        currentHp = Mathf.Min(playerMaxHp, currentHp + Mathf.RoundToInt(playerMaxHp * 0.4f));
                    }

                    // Enemy stats per floor
                    int enemyHp = 35 + (floor - 1) * 20;
                    int enemyArmor = floor >= 5 ? 4 + (floor - 5) * 2 : 0;
                    float enemyAttack = 4 + (floor - 1) * 2.2f;
                    float enemySpeed = 1.5f;

                    if (floor == 5) { enemyHp = 160; enemyArmor = 8; enemyAttack = 14; } // Mid-Boss
                    if (floor == 10) { enemyHp = 650; enemyArmor = 10; enemyAttack = 18; } // Boss: The Lich Lord

                    // Battle Simulation
                    float playerDps = (playerAttack * 1.15f) / attackInterval; // Includes combo & rune multiplier
                    totalDpsAccumulator += playerDps;

                    float effectiveEnemyArmor = Mathf.Max(0, enemyArmor - 2);
                    float playerDamagePerSec = Mathf.Max(2f, playerDps - (effectiveEnemyArmor * 0.5f));

                    float enemyDps = Mathf.Max(1f, enemyAttack - playerArmor);
                    float enemyDamagePerSec = enemyDps / enemySpeed;

                    float timeToKillEnemy = enemyHp / playerDamagePerSec;
                    float damageTakenInFight = timeToKillEnemy * enemyDamagePerSec;

                    // Random dodge/crit variance (+/- 15%)
                    float variance = 0.85f + (float)rng.NextDouble() * 0.30f;
                    damageTakenInFight *= variance;

                    currentHp -= Mathf.RoundToInt(damageTakenInFight);

                    if (currentHp <= 0)
                    {
                        // 1-Time Revive (Offline Monetization / Ad Revive at 50% HP)
                        currentHp = Mathf.RoundToInt(playerMaxHp * 0.5f);

                        // If player takes lethal damage again in the same floor -> Defeat!
                        if (damageTakenInFight > currentHp * 1.5f)
                        {
                            runWon = false;
                            metrics.deathsPerFloor[floor]++;
                            break;
                        }
                    }
                    else
                    {
                        gold += 15 + floor * 5;
                    }
                }

                if (runWon)
                {
                    metrics.successfulClears++;
                    metrics.classClears[selectedClass]++;
                    totalFloorsReached += 10;
                }
                else
                {
                    totalFloorsReached += floorReached;
                }
            }

            metrics.winRatePercent = (metrics.successfulClears / (float)runCount) * 100f;
            metrics.averageFloorReached = totalFloorsReached / runCount;
            metrics.averageDps = totalDpsAccumulator / (runCount * 10f);

            return metrics;
        }
    }
}
