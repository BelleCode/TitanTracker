using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TitanTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Sent")]
        public DateTimeOffset InviteDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Join Date")]
        public DateTimeOffset JoinDate { get; set; }

        [DisplayName("Company Code")]
        public Guid CompanyToken { get; set; }

        [DisplayName("Company Id")]
        public int CompanyId { get; set; }

        [DisplayName("Project")]
        public string ProjectId { get; set; }

        [DisplayName("Invitee")]
        public string InviteeId { get; set; }

        [DisplayName("Invitor")]
        public string InvitorId { get; set; }

        [DisplayName("Invitee Email")]
        [DataType(DataType.EmailAddress)]
        public string InviteeEmail { get; set; }

        [DisplayName("Invitee First Name")]
        public string InviteeFirstName { get; set; }

        [DisplayName("Invitee Last Name")]
        public string InviteeLastName { get; set; }

        [DisplayName("Invite Message")]
        public string Message { get; set; }

        [DisplayName("Invite is Valid")]
        public bool IsValid { get; set; }

        //=== Navigation Properties ==//

        // One to One Relationship
        public virtual Company Company { get; set; }

        public virtual Project Project { get; set; }
        public virtual BTUser Invitee { get; set; }
        public virtual BTUser Invitor { get; set; }
    }
}