using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;
using GO = UnityEngine.GameObject;

namespace TargetPriorityOrdering
{
    [BepInPlugin("harbingerOfMe.TargetPriorityOrdering", "Ordered Priorities", "1.1.0")]
    public class OrderedPriorities : BaseUnityPlugin
    {

        private int basePriorityCount;
        private int lastCompiledPriorityCount = (int)Tower.Priority.Marked + 1;

        private static readonly Dictionary<Tower.Priority, PriorityHandler> prioritisers = new Dictionary<Tower.Priority, PriorityHandler>();

        public static void AddCustomPriority(PriorityHandler priorityHandler)
        {
            var index = (Tower.Priority)(10 + prioritisers.Count);
            prioritisers.Add(index, priorityHandler);
        }


        private void Awake()
        {
            basePriorityCount = Enum.GetValues(typeof(Tower.Priority)).Length;

            if(basePriorityCount != lastCompiledPriorityCount)
            {
                Logger.LogWarning("Priorities in the game do not line up with priorities this mod was made for. Use at your own risk!");
                Logger.LogDebug($"Expected {lastCompiledPriorityCount} priorities, got {basePriorityCount} instead!");
            }

            On.Tower.SelectEnemy += orderedEnemySelection;

            On.TowerUI.TogglePriorityUp += fixTowerPriorityUp;
            On.TowerUI.TogglePriorityDown += fixTowerPriorityDown;
            On.TowerUI.SetStats += TowerUI_SetStats;

            On.Tower.TogglePriority += extendTowerPriority;

            AddCustomPriority(new ExampleRandomPriority());
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
            int max = basePriorityCount + prioritisers.Count;
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
                var best = -1f;
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
