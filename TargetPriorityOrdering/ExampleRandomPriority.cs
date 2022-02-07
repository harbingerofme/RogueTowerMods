namespace TargetPriorityOrdering
{
    internal class ExampleRandomPriority : PriorityHandler
    {

        public override string Name => "Random";

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            return UnityEngine.Random.Range(0.001f, 100f);
        }
    }


}
