using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.ViewModels
{
    public class PMIndexViewModel
    {
        public List<Project> Projects { get; set; }
        public SelectList PMList { get; set; }  //project manager list
        public string PmId { get; set; }        // b/c pm is a user, their info is an string
    }
}