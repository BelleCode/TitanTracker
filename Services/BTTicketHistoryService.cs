using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Data;
using TitanTracker.Models;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            try
            {
                // New Ticket has been added
                if (oldTicket == null && newTicket != null)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"newticket"
                    };

                    try
                    {
                        await _context.TicketHistories.AddAsync(history);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    // Check Ticket Title
                    if (oldTicket.Title != newTicket.Title)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            Property = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Ticket Title: {newTicket.Title}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Description
                    if (oldTicket.Description != newTicket.Description)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            Property = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Ticket Description: {newTicket.Description}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Status
                    if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            Property = "TicketStatus",
                            OldValue = oldTicket.TicketStatus.Name,
                            NewValue = newTicket.TicketStatus.Name,
                            Created = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Ticket Status: {newTicket.TicketStatus.Name}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Priority
                    if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            Property = "TicketPriority",
                            OldValue = oldTicket.TicketPriority.Name,
                            NewValue = newTicket.TicketPriority.Name,
                            Created = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Ticket Priority: {newTicket.TicketPriority.Name}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Type
                    if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            Property = "TicketType",
                            OldValue = oldTicket.TicketType.Name,
                            NewValue = newTicket.TicketType.Name,
                            Created = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Ticket Type: {newTicket.TicketType.Name}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    // Check Ticket Developer
                    if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            Property = "Developer",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket.DeveloperUser?.FullName,
                            Created = DateTimeOffset.Now,
                            UserId = userId,
                            Description = $"New Ticket Developer: {newTicket.DeveloperUser.FullName}"
                        };
                        await _context.TicketHistories.AddAsync(history);
                    }

                    try
                    {
                        // Save the TicketHistory DataBaseSet to the database
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                                                        .Include(c => c.Projects)
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(t => t.History)
                                                                    .ThenInclude(h => h.User)
                                                        .FirstOrDefaultAsync(c => c.Id == companyId)).Projects.ToList();
                // Projects are now filtered by company..
                List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();

                List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();

                return ticketHistories;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                Project project = (await _context.Projects
                                                  .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.History)
                                                            .ThenInclude(h => h.User)
                                                  .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId));

                List<TicketHistory> ticketHistories = project.Tickets.SelectMany(t => t.History).ToList();

                return ticketHistories;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}