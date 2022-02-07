using TargetPriorityOrdering;

namespace MorePriorities.Priorities
{
    public class dotLeastBurningPriority : PriorityHandler
    {
        public override string Name => "Least Burning";

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            return prioritiserTarget.enemy.burning ? 1f / prioritiserTarget.enemy.burn : 1f;
        }
    }
}
