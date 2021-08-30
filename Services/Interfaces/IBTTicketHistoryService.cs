using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Models;

namespace TitanTracker.Services.Interfaces
{
    public interface IBTTicketHistoryService
    {
        Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);

        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId);

        Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);
    }
}