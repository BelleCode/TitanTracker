using Microsoft.AspNetCore.Http;
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

        [DisplayName("File Name")]
        public string FileName { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx ", ".pdf" })]
        public IFormFile FormFile { get; set; }

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