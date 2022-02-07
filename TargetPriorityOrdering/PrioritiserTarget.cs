using UnityEngine;

namespace TargetPriorityOrdering
{

    public class PrioritiserTarget
    {
        public Collider collider;
        public Pathfinder pathfinder;
        public Enemy enemy;

        public Tower tower;

        public PrioritiserTarget(Tower tower, Collider collider, Enemy enemy, Pathfinder pathfinder)
        {
            this.tower = tower;
            this.collider = collider;
            this.enemy = enemy;
            this.pathfinder = pathfinder;
        }
    }
}
