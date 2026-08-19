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
            return $"Monte Carlo Results: {successfulClears}/{totalRuns} Cleared ({winRatePercent:0.1}%) | Avg Floor: {averageFloorReached:0.2} | Avg DPS: {averageDps:0.1}";
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

            string[] heroClasses = new[] { "class_rune_knight", "class_elementalist", "class_shadow_rogue", "class_iron_juggernaut" };
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
                    case "class_elementalist":
                        playerMaxHp = 85;
                        playerArmor = 0;
                        playerAttack = 12; // Wand + spark rune
                        attackInterval = 1.4f;
                        break;
                    case "class_shadow_rogue":
                        playerMaxHp = 90;
                        playerArmor = 1;
                        playerAttack = 14;
                        attackInterval = 1.0f;
                        break;
                    case "class_iron_juggernaut":
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
                bool hasRevived = false;

                // Simulate 10 Floors
                for (int floor = 1; floor <= 10; floor++)
                {
                    floorReached = floor;

                    // In-Run scaling: Player acquires items, upgrades, and synergies across floors
                    playerAttack += 2.0f;
                    if (floor % 2 == 0) playerArmor += 1;

                    // Floor 8: Campfire heal
                    if (floor == 8)
                    {
                        currentHp = Mathf.Min(playerMaxHp, currentHp + Mathf.RoundToInt(playerMaxHp * 0.4f));
                    }

                    // Enemy stats per floor
                    int enemyHp = 40 + (floor - 1) * 25;
                    int enemyArmor = floor >= 5 ? 3 + (floor - 5) * 2 : 0;
                    float enemyAttack = 5 + (floor - 1) * 2.2f;
                    float enemySpeed = 1.5f;

                    if (floor == 5) { enemyHp = 150; enemyArmor = 6; enemyAttack = 12; } // Mid-Boss
                    if (floor == 10) { enemyHp = 380; enemyArmor = 9; enemyAttack = 17; } // Boss: The Lich Lord

                    // Battle Simulation with Synergies and Combos
                    float synergyMultiplier = 1.0f + 0.10f * floor;
                    float playerDps = (playerAttack * synergyMultiplier) / attackInterval;
                    totalDpsAccumulator += playerDps;

                    float effectiveEnemyArmor = Mathf.Max(0, enemyArmor - 2);
                    float playerDamagePerSec = Mathf.Max(3f, playerDps - (effectiveEnemyArmor * 0.4f));

                    float effectivePlayerArmor = playerArmor + (floor >= 3 ? 2 : 0);
                    float enemyDps = Mathf.Max(1.5f, enemyAttack - (effectivePlayerArmor * 0.5f));
                    float enemyDamagePerSec = enemyDps / enemySpeed;

                    float timeToKillEnemy = enemyHp / playerDamagePerSec;
                    float damageTakenInFight = timeToKillEnemy * enemyDamagePerSec;

                    // Random dodge/crit variance (+/- 15%)
                    float variance = 0.85f + (float)rng.NextDouble() * 0.30f;
                    damageTakenInFight *= variance;

                    currentHp -= Mathf.RoundToInt(damageTakenInFight);

                    if (currentHp <= 0)
                    {
                        if (!hasRevived)
                        {
                            hasRevived = true;
                            // 1-Time Revive (Offline Monetization / Ad Revive at 50% HP)
                            currentHp = Mathf.RoundToInt(playerMaxHp * 0.5f);

                            if (damageTakenInFight > playerMaxHp * 0.85f)
                            {
                                runWon = false;
                                metrics.deathsPerFloor[floor]++;
                                break;
                            }
                        }
                        else
                        {
                            runWon = false;
                            metrics.deathsPerFloor[floor]++;
                            break;
                        }
                    }
                    else
                    {
                        gold += 15 + floor * 5;
                        currentHp = Mathf.Min(playerMaxHp, currentHp + 20);
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
