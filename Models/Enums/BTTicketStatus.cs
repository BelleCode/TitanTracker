using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum BTTicketStatus
    {
        New,                    // 01 - unassigned just in and adding defect info

        [Display(Name = "Defect Review")]
        DefectReview,           // 02 - Assigned to someone for reviewDefect needs analysed to verify it is a bug

        [Display(Name = "Closed Duplicate")]
        ClosedDuplicate,        // 03 - Was this issue already raised

        [Display(Name = "Closed Rejected")]
        ClosedRejected,         // 04 - Is it a valid defect (obsolete, non-reproducible, incomplete data)

        Committed,              // 07 - Ready for work in this sprint/iteration
        Deffered,               // 05 - TODO: DEFFERED BUTTON!!!! -> Send this to the next iteration and set as Defect Review Is it within scope move backlog

        [Display(Name = "Ready for Development")]
        DevReady,               // 06 - Ticket approved and waiting for iteration/sprint

        [Display(Name = "In Development")]
        InDev,                   // 08 - Assigned and Dev Work in Progress status: doing

        [Display(Name = "In Test")]
        InTest,                 // 10 - Assigned and testing test cases

        [Display(Name = "Dev Complete - Test Ready")]
        DevCompleteTestReady,   // 09 - Code is Fixed but not tested

        [Display(Name = "Resolved Fixed")]
        ResolvedFixed,          // 11 - Code is fixed (archived)

        // ISSUE NOT RESOLVED BUTTON=> TODO: Ticket reopened, open new bug, attach this one.
    }
}