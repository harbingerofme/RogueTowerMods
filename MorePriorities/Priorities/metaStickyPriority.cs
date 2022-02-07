using TargetPriorityOrdering;

namespace MorePriorities.Priorities
{
    public class StickyPriority : PriorityHandler
    {
        public override string Name => "Sticky";

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            if (prioritiserTarget.enemy.gameObject == prioritiserTarget.tower.currentTarget)
            {
                return 200;
            }

            return 1;
        }
    }
}
