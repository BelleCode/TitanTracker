using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum Roles
    { // nto strings, just values
        [Display(Name = "Administrator")]
        Admin,

        [Display(Name = "Program Manager")]
        ProgramManager,

        [Display(Name = "Project Manager")]
        ProjectManager,

        [Display(Name = "Dev Manager")]
        DevManager,

        Developer,

        [Display(Name = "Test Manager")]
        TestManager,

        Tester,
        Submitter,

        [Display(Name = "Scrum Master")]
        ScrumMaster,

        DemoUser    // DemoUser
    }
}