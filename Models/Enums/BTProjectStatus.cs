using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.Enums
{
    public enum BTProjectStatus
    {
        Ideation = 0,           // Client requests or PM ideation
        Requirements = 1,       // Requirements collection
        InDesign = 2,           // Designing from requirements
        InDevelopment = 3,      // Development
        InTest = 4,             // testing
        DeploymentReady = 5,    // Ready for deployment scheduled
        Live = 6,               // Livesite
        Deprecated = 8,         // Version is no longer supported, have all existing users move to newer version
        Sunset = 7              // Version or item no longer available but historical data is retained
    }
}