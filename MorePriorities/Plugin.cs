using BepInEx;
using System;
using System.Linq;
using System.Reflection;
using TargetPriorityOrdering;

namespace MorePriorities
{
    [BepInPlugin("harbingerOfMe.morePriorities", "More priorities", "1.0.0")]
    [BepInDependency("harbingerOfMe.TargetPriorityOrdering", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(TargetPriorityOrdering.PriorityHandler))))
            {
                OrderedPriorities.AddCustomPriority((PriorityHandler)Activator.CreateInstance(t));
            }
        }
    }
}
