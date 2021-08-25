using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class TicketHistory
    {
        //primary key
        public int Id { get; set; }

        [DisplayName("Updated Item")]
        public string Property { get; set; }

        [DisplayName("Previous Item")]
        public string OldValue { get; set; }

        [DisplayName("Current Item")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        //=== Navigation Properties ==//
        //Ticket
        public virtual Ticket Ticket { get; set; }

        //user
        public virtual BTUser User { get; set; }
    }
}