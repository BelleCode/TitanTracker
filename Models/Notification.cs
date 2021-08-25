using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created")]
        public DateTimeOffset created { get; set; }

        [DisplayName("Has been viewed")]
        public bool Viewed { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Recipient")]
        public string RecipientId { get; set; }

        [DisplayName("Sender")]
        public string SenderId { get; set; }

        //=== Navigation Properties ==//
        public virtual Ticket Ticket { get; set; }

        public virtual BTUser Recipient { get; set; }
        public virtual BTUser Sender { get; set; }
    }
}