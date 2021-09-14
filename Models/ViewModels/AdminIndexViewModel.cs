using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.ViewModels
{
    // Displays all projects for company.
    public class AdminIndexViewModel
    {
        public int ProjectId { get; set; }
        public List<Project> Projects { get; set; }
        public SelectList PMList { get; set; }  //project manager list
        public MultiSelectList DevList { get; set; }  //project manager list
        public MultiSelectList SubList { get; set; }  //project manager list
        public string PmId { get; set; }        // b/c pm is a user, their info is an string
        public List<string> DevId { get; set; }
        public List<string> SubId { get; set; }
        public List<string> SelectedDevs { get; set; } // recieves selected users
        public List<string> SelectedSubs { get; set; } // recieves selected users
    }
}