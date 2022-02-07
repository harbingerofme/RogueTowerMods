using TargetPriorityOrdering;

namespace MorePriorities.Priorities
{
    public class dotLeastBleedingPriority : PriorityHandler
    {
        public override string Name => "Least Bleeding";

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            return prioritiserTarget.enemy.bleeding ? 1f / prioritiserTarget.enemy.bleed : 1f;
        }
    }
}
