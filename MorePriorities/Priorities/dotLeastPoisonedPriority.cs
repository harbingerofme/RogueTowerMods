using TargetPriorityOrdering;

namespace MorePriorities.Priorities
{
    public class dotLeastPoisonedPriority : PriorityHandler
    {
        public override string Name => "Least Poisoned";

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            return prioritiserTarget.enemy.poisoned ? 1f / prioritiserTarget.enemy.poison : 1f;
        }
    }
}
