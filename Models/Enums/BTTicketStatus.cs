using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum BTTicketStatus
    {
        New,
        Development,
        Testing,    // checking through test cases
        Resolved,
        Rejected,   // Is it a valid defect
        Deffered,   // Is it within scope
        Duplicate,  // Was this issue already raised
        Ready,      // Ready to be assigned
        Committed,  // Ready for work in this sprint
        Doing,      // Work in Progress
        Fixed,      // Code is Fixed but not tested
        Reopened,   // Failed Test
    }
}