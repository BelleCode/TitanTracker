using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Extensions;

namespace TitanTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [DisplayName("Preferred Name")]
        [StringLength(40, ErrorMessage = "The {0} must be at least {1} and at max {1} characters long.", MinimumLength = 2)]
        public string PreferredName { get; set; }

        [NotMapped]
        [DisplayName("FullName")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        [DisplayName("Select Image")]
        [DataType(DataType.Upload)]
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile AvatarFormFile { get; set; }

        [DisplayName("Avatar")]
        public string AvatarFileName { get; set; }

        public byte[] AvatarFileData { get; set; }

        [DisplayName("File Extension")]
        public string AvatarContentType { get; set; }

        // Additonal Properties
        public int? CompanyId { get; set; }

        //=== Navigation Properties ==//

        public virtual Company Company { get; set; }
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}