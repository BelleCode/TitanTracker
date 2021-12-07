using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class TicketComment
    {
        // Primary Key
        public int Id { get; set; }

        [DisplayName("Member Comment")]
        public string Comment { get; set; }

        [DisplayName("Comment Date")]
        public DateTimeOffset Created { get; set; }

        // Foreign Key
        [DisplayName("Ticket")]
        public string TicketId { get; set; }

        // Foreign Key
        [DisplayName("Team Member")]
        public string UserId { get; set; }

        //=== Navigation Properties ==//
        //Ticket
        public virtual Ticket Ticket { get; set; }

        //user
        public virtual BTUser User { get; set; }
    }
}