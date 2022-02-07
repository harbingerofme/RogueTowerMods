using UnityEngine;

namespace TargetPriorityOrdering
{

    public class PrioritiserTarget
    {
        public Collider collider;
        public Pathfinder pathfinder;
        public Enemy enemy;

        public PrioritiserTarget(Collider collider, Enemy enemy, Pathfinder pathfinder)
        {
            this.collider = collider;
            this.enemy = enemy;
            this.pathfinder = pathfinder;
        }
    }
}
