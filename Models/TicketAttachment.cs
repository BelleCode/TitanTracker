using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class TicketAttachment
    {
        //primary key
        public int Id { get; set; }

        [DisplayName("File Date")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("File Description")]
        public string Description { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [DisplayName("File Name")]
        public byte[] FileName { get; set; }

        public string FormFile { get; set; }
        public byte[] FileData { get; set; }

        [DisplayName("File Extension")]
        public string FileContentType { get; set; }

        //=== Navigation Properties ==//
        //Ticket
        public virtual Ticket Ticket { get; set; }

        //user
        public virtual BTUser User { get; set; }
    }
}