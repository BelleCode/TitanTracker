using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Data;
using TitanTracker.Models;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Services
{
    public class BTHistoryService : IBTHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }
    }
}