using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum BTTicketType
    {
        [Display(Name = "Feature Defect")]
        FeatureDefect,      // Something doesn't work as designed

        Performance,        // Feature works as designed, but is too slow or too demanding on other resources
        Polish,             // Feature works well, but is "rough around the edges", and has imperfections or cosmetic issues which impact perception
        Security,           // Feature works but is security is lack or data is vulnerable
        Usability,          // Feature works as designed, but is difficult for the user to use or undiscoverable
        Accessibility       // Feature is not ADA compliant
    };
}