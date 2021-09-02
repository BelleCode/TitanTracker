using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.ViewModels
{
    public class AddProjectWithPMViewModel
    {
        public Project Project { get; set; } = new Project();
        public SelectList PMList { get; set; }  //project manager list
        public string PmId { get; set; }        // b/c pm is a user, their info is an string
        public SelectList PriorityList { get; set; }    // List of priorities that are available
        public int ProjectPriority { get; set; }
    }
}