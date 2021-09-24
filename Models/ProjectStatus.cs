using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class ProjectStatus
    {
        public int Id { get; set; }

        [DisplayName("Project Status")]
        public string Name { get; set; }
    }
}