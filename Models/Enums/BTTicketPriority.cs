using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum BTTicketPriority
    {
        Low = 4,        // Pri : 4 Review before next iteration. Remove if not Pri 3 +
        Medium = 3,     // Pri : 3 Product can ship without this item, but should be addressed before next release
        High = 2,       // Pri : 2 Product should not ship without successful resolution to the work item, but it doesn not have to be addressed in this iteration
        Urgent = 1,     // Pri : 1 Product should not ship without successful resolution
        Breaking = 0    // Pri : 0 Product is live and this is a breaking Bug
    };
}