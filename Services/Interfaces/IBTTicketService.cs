using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Models;
using TitanTracker.Models.Enums;

namespace TitanTracker.Services.Interfaces
{
    public interface IBTTicketService
    {
        // CRUD Methods
        public Task AddNewTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketByIdAsync(int ticketId);

        public Task ArchiveTicketAsync(Ticket ticket);

        // Essential Services
        public Task AssignTicketAsync(int ticketId, string userId);

        public Task<List<Ticket>> GetArchivedTicketsAsync(int companyId);

        public Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId);

        public Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, BTTicketPriority priorityName);

        public Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, BTTicketStatus statusName);

        public Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, BTTicketType typeName);

        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId);

        // Get tickets by Role
        public Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId);

        public Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId);

        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);

        public Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId);

        // Status Priority and Type
        public Task<List<Ticket>> GetProjectTicketsByStatusAsync(BTTicketStatus statusName, int companyId, int projectId);

        public Task<List<Ticket>> GetProjectTicketsByPriorityAsync(BTTicketPriority priorityName, int companyId, int projectId);

        public Task<List<Ticket>> GetProjectTicketsByTypeAsync(BTTicketType typeName, int companyId, int projectId);

        //// Lookup Services (no need to query database)
        //public Task<int?> LookupTicketPriorityIdAsync(BTTicketPriority priorityName);

        //public Task<int?> LookupTicketStatusIdAsync(BTTicketStatus statusName);

        //public Task<int?> LookupTicketTypeIdAsync(BTTicketType typeName);
    }
}