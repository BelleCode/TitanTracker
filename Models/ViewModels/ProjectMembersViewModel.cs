using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project Project { get; set; }
        public MultiSelectList Users { get; set; }      //populates list box

        public List<string> SelectedUsers { get; set; } // recieves selected users
    }
}