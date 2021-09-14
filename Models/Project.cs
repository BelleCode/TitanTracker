using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class Project
    {
        //primary key
        public int Id { get; set; }

        [DisplayName("Company Id")]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Start Date")]
        public DateTimeOffset StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Completed Date")]
        public DateTimeOffset EndDate { get; set; }

        [DisplayName("Project Priority Id")]
        public int? ProjectPriorityId { get; set; }

        [DisplayName("Admin")]
        public string AdminId { get; set; }

        [DisplayName("Project Manager")]
        public string ProjectManagerId { get; set; }

        [NotMapped]
        [DisplayName("Project FormFile")]
        [DataType(DataType.Upload)]
        public IFormFile FormFile { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }

        [DisplayName("File Data")]
        public byte[] FileData { get; set; }

        [DisplayName("File Extension")]
        public string FileContentType { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        // One to One Relationship
        public virtual Company Company { get; set; }

        public virtual ProjectPriority ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}