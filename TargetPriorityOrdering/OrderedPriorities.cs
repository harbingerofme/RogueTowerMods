using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using GO = UnityEngine.GameObject;

namespace TargetPriorityOrdering
{
    [BepInPlugin("harbingerOfMe.TargetPriorityOrdering", "Ordered Priorities", "1.1.1")]
    public class OrderedPriorities : BaseUnityPlugin
    {
        private static ManualLogSource logger;
        private int basePriorityCount;
        private int lastCompiledPriorityCount = (int)Tower.Priority.Marked + 1;

        private static readonly Dictionary<Tower.Priority, PriorityHandler> prioritisers = new();
        private static int customPrioritisersCount = 0;

        public static void AddCustomPriority(PriorityHandler priorityHandler)
        {
            var index = (Tower.Priority)(Enum.GetValues(typeof(Tower.Priority)).Length + customPrioritisersCount);
            prioritisers.Add(index, priorityHandler);
            customPrioritisersCount++;
        }

        public static void OverwriteVanillaPriority(Tower.Priority priority, PriorityHandler newHandler, bool allowOverwritingPreviousOverwrites = false)
        {
            if (!Enum.IsDefined(typeof(Tower.Priority), priority))
            {
                throw new ArgumentException($"Failed to overwrite vanilla priority: Supplied priority ({priority}) is not a vanilla priority.", nameof(priority));
            }
            if (!allowOverwritingPreviousOverwrites && prioritisers.ContainsKey(priority))
            {
                logger.LogWarning($"Failed to overwrite vanilla priority ({Enum.GetName(typeof(Tower.Priority), priority)}): This priority " +
                    $"has already been overwritten once before (potentially by another mod). Instead, a new priority is created. " +
                    $"For Modders: If you intend to allow overwriting a previously overwritten vanilla priority, please set the argument " +
                    $"`allowOverwritingPreviousOverwrites` of `OverwriteVanillaPriority` to true.");
                AddCustomPriority(newHandler);
                return;
            }
            prioritisers[priority] = newHandler;
        }

        private void Awake()
        {
            logger = Logger;
            basePriorityCount = Enum.GetValues(typeof(Tower.Priority)).Length;

            if (basePriorityCount != lastCompiledPriorityCount)
            {
                Logger.LogWarning("Priorities in the game do not line up with priorities this mod was made for. Use at your own risk!");
                Logger.LogDebug($"Expected {lastCompiledPriorityCount} priorities, got {basePriorityCount} instead!");
            }

            On.Tower.SelectEnemy += orderedEnemySelection;

            On.TowerUI.TogglePriorityUp += fixTowerPriorityUp;
            On.TowerUI.TogglePriorityDown += fixTowerPriorityDown;
            On.TowerUI.SetStats += TowerUI_SetStats;

            On.Tower.TogglePriority += extendTowerPriority;

            On.LevelLoader.Start += RewordPriorityTip;

            AddCustomPriority(new ExampleRandomPriority());
        }

        private void RewordPriorityTip(On.LevelLoader.orig_Start orig, LevelLoader self)
        {
            orig(self);

            for (int i = 0; i < self.tips.Length; i++)
            {
                string s = self.tips[i];
                Regex prio = new("priorit", RegexOptions.IgnoreCase);
                Regex order = new("order", RegexOptions.IgnoreCase);
                if (prio.IsMatch(s) && order.IsMatch(s))
                {
                    self.tips[i] = "Priorities matter, and so does their order.";
                }
            }

        }

        private void TowerUI_SetStats(On.TowerUI.orig_SetStats orig, TowerUI self, Tower myTower)
        {
            orig(self, myTower);
            for (int i = 0; i < self.priorityTexts.Length; i++)
            {
                FixText(self, i);
            }
        }

        private void extendTowerPriority(On.Tower.orig_TogglePriority orig, Tower self, int index, int direction)
        {
            int max = basePriorityCount + customPrioritisersCount;
            self.priorities[index] = (Tower.Priority)(((int)self.priorities[index] + direction % max + max) % max);
        }

        private void fixTowerPriorityDown(On.TowerUI.orig_TogglePriorityDown orig, TowerUI self, int index)
        {
            orig(self, index);
            FixText(self, index);
        }

        private void fixTowerPriorityUp(On.TowerUI.orig_TogglePriorityUp orig, TowerUI self, int index)
        {
            orig(self, index);
            FixText(self, index);
        }

        private void FixText(TowerUI UI, int index)
        {

            var prio = UI.myTower.priorities[index];
            if (prioritisers.ContainsKey(prio))
            {
                UI.priorityTexts[index].text = prioritisers[prio].Name;
            }
        }

        private GO orderedEnemySelection(On.Tower.orig_SelectEnemy orig, Tower self, UnityEngine.Collider[] possibleTargets)
        {
            List<PrioritiserTarget> targets = new List<Collider>(possibleTargets)
                .ConvertAll(
                    (Collider c) =>
                    new PrioritiserTarget(self, c, c.GetComponent<Enemy>(), c.GetComponent<Pathfinder>())
                );

            const float maxinum = 1f;
            const float mininum = 0.001f;

            foreach (var priority in self.priorities)
            {
                var optimalList = new List<PrioritiserTarget>();
                var best = float.NegativeInfinity;
                foreach (PrioritiserTarget target in targets)
                {
                    float score = maxinum;

                    if (prioritisers.ContainsKey(priority))
                    {
                        score = prioritisers[priority].GetPriorityForTarget(target);
                    }
                    else
                    {
                        switch (priority)
                        {
                            case Tower.Priority.Progress:
                                score /= Mathf.Max(mininum, target.pathfinder.distanceFromEnd);
                                break;
                            //Combined HP, ok...
                            case Tower.Priority.NearDeath:
                                score /= Mathf.Max(mininum, target.enemy.CurrentHealth());
                                break;
                            case Tower.Priority.LeastHealth:
                                score /= Mathf.Max(maxinum, target.enemy.health);
                                break;
                            case Tower.Priority.MostHealth:
                                score *= Mathf.Max(maxinum, target.enemy.health);
                                break;
                            case Tower.Priority.LeastArmor:
                                score /= Mathf.Max(mininum, target.enemy.armor);
                                break;
                            case Tower.Priority.MostArmor:
                                score *= Mathf.Max(maxinum, target.enemy.armor);
                                break;
                            case Tower.Priority.LeastShield:
                                score /= Mathf.Max(mininum, target.enemy.shield);
                                break;
                            case Tower.Priority.MostShield:
                                score *= Mathf.Max(maxinum, target.enemy.shield);
                                break;
                            case Tower.Priority.Fastest:
                                score *= Mathf.Max(maxinum, target.pathfinder.speed);
                                break;
                            case Tower.Priority.Slowest:
                                score /= Mathf.Max(maxinum, target.pathfinder.speed);
                                break;
                            case Tower.Priority.Marked:
                                score *= target.enemy.mark != null ? 2f : 1f;
                                break;
                        }
                    }

                    score = Mathf.Clamp(score, float.MinValue, float.MaxValue); // Handle infinities

                    if (score > best)
                    {
                        best = score;
                        optimalList.Clear();
                    }

                    if (Mathf.Approximately(score, best))
                    {
                        optimalList.Add(target);
                    }

                }

                targets = optimalList;
            }

            return targets[0].enemy.gameObject;
        }


    }
}
