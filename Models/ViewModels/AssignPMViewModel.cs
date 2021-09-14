using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.ViewModels
{
    public class AssignPMViewModel //Assign PM to single project
    {
        public Project Project { get; set; }
        public SelectList PMList { get; set; }
        public string PmId { get; set; }
    }
}