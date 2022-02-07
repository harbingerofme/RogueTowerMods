namespace TargetPriorityOrdering
{
    public abstract class PriorityHandler
    {
        public abstract float GetPriorityForTarget(PrioritiserTarget prioritiserTarget);

        public abstract CustomPriority CustomPriority { get; set; }
    }
}
