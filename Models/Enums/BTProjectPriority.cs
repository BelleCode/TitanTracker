using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum BTProjectPriority
    {
        Low = 4,        // Review before next iteration. Remove if not Pri 3+
        Medium = 3,     // Product can ship without this item, but should be addressed before next release
        Important = 2,  // Product should not ship without successful resolution to the work item, but it doesn not have to be addressed in this iteration
        Urgent = 1,     // Product should not ship without successful resolution
        Required = 0    // Must Have: Required to ensure sprint deliverable
    };
}