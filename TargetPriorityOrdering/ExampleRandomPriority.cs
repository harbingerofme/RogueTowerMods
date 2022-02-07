namespace TargetPriorityOrdering
{
    internal class ExampleRandomPriority : PriorityHandler
    {
        public ExampleRandomPriority()
        {
            this.CustomPriority = new CustomPriority("Random");
        }
        
        public override CustomPriority CustomPriority { get; set; }

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            return UnityEngine.Random.Range(0.001f, 100f);
        }
    }


}
