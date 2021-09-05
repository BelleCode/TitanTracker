using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public BTUser BTUser { get; set; }                  //BTUser Model (pre-existing)
        public MultiSelectList Roles { get; set; }          //Enum (pre-existing)
        public List<string> SelectedRoles { get; set; }     //List of roles being changed
    }
}