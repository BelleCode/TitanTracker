using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Models.Enums;

namespace TitanTracker.Models
{
    public class Ticket
    {
        //primary key
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created")]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Updated")]
        public DateTimeOffset? Updated { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        [DisplayName("Project")]
        public int ProjectId { get; set; }

        [DisplayName("Ticket Type")]
        public BTTicketType TicketType { get; set; }

        [DisplayName("Ticket Priority")]
        public BTTicketPriority TicketPriority { get; set; }

        [DisplayName("Ticket Status")]
        //Enums are always stored as int in the database
        //That int has meaning only within the code itself
        public BTTicketStatus TicketStatus { get; set; }

        [DisplayName("Ticket Owner")]
        public string OwnerUserId { get; set; }

        [DisplayName("Ticket Developer")]
        public string DeveloperUserId { get; set; }

        //=== Navigation Properties ==//

        // One to One Relationship
        public virtual Project Project { get; set; }

        //public virtual TicketType TicketType { get; set; }
        //public virtual TicketPriority TicketPriority { get; set; }
        //public virtual TicketStatus TicketStatus { get; set; }
        public virtual BTUser OwnerUser { get; set; }

        public virtual BTUser DeveloperUser { get; set; }

        // One to Many Relationship
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}