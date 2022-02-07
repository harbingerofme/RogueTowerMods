using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TargetPriorityOrdering;

namespace MorePriorities.Priorities
{
    public class towerMarkedPriority : TargetPriorityOrdering.PriorityHandler
    {
        public override string Name => "Marked";

        public override float GetPriorityForTarget(PrioritiserTarget prioritiserTarget)
        {
            return prioritiserTarget.enemy.mark != null ? 2f : 1f;
        }
    }
}
