using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Services.Interfaces;
using TitanTracker.Models;
using TitanTracker.Data;
using Microsoft.EntityFrameworkCore;
using TitanTracker.Models.Enums;

namespace TitanTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService, IBTProjectService projectService)
        {
            _context = context;
            _rolesService = rolesService;
            _projectService = projectService;
        }

        // CRUD : Create
        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // CRUD : Delete
        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true; // switch the ticket from archived is == to true
                await UpdateTicketAsync(ticket);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket != null)
                {
                    try
                    {
                        ticket.DeveloperUserId = userId;
                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync(BTTicketStatus.Development.ToString())).Value;
                        await UpdateTicketAsync(ticket);
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

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                // Not quite as efficien
                //tickets = await _context.Tickets.Where(p => p.Project.CompanyId == companyId && p.Archived == false)
                //                                .Include(t => t.Comments)
                //                                .Include(t => t.Attachments)
                //                                .Include(t => t.History)
                //                                .Include(t => t.Notifications)
                //                                .Include(t => t.DeveloperUser)
                //                                .Include(t => t.OwnerUser)
                //                                .Include(t => t.TicketStatus)
                //                                .Include(t => t.TicketPriority)
                //                                .Include(t => t.TicketType)
                //                                .ToListAsync();
                //return tickets;

                // Possible to have tickets whos projects aren't archived
                tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                 .SelectMany(p => p.Tickets)
                                                 .Where(t => t.Archived == false)
                                                 .Include(t => t.Comments)
                                                 .Include(t => t.Attachments)
                                                 .Include(t => t.History)
                                                 .Include(t => t.Notifications)
                                                 .Include(t => t.DeveloperUser)
                                                 .Include(t => t.OwnerUser)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketType)
                                                 .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }

            // extremely inefficient: Due to requiring traslantion to SQL to understand items
            //    List<Project> projects = await projectService.GetAllProjectsByCompany(companyId);

            //    foreach (Project project in projects)
            //    {
            //        foreach (Ticket ticket in project.Tickets)
            //        {
            //            tickets.Add(ticket);
            //        }
            //    }
            //    return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            List<Ticket> tickets = new();

            try
            {
                int priorityId = (await LookupTicketPriorityIdAsync(priorityName)).Value;

                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.TicketPriorityId == priorityId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            List<Ticket> tickets = new();

            try
            {
                int statusId = (await LookupTicketStatusIdAsync(statusName)).Value;

                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.TicketStatusId == statusId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            List<Ticket> tickets = new();

            try
            {
                int typeId = (await LookupTicketTypeIdAsync(typeName)).Value;

                tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.TicketTypeId == typeId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                 .SelectMany(p => p.Tickets)
                                                 .Where(t => t.Archived == true)
                                                 .Include(t => t.Comments)
                                                 .Include(t => t.Attachments)
                                                 .Include(t => t.History)
                                                 .Include(t => t.Notifications)
                                                 .Include(t => t.DeveloperUser)
                                                 .Include(t => t.OwnerUser)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketType)
                                                 .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetTicketsByRoleAsync(role, userId, companyId)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByStatusAsync(companyId, statusName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByTypeAsync(companyId, typeName)).Where(t => t.ProjectId == projectId).ToList();
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNotTrackingAsync(int ticketId)
        {
            try
            {
                Ticket ticket = await _context.Tickets
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketType)
                                              .Include(t => t.Project)
                                              .Include(t => t.DeveloperUser)
                                              .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            Ticket ticket = await _context.Tickets.Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Comments)
                    .ThenInclude(t => t.User)
                .Include(t => t.Attachments)
                    .ThenInclude(t => t.User)
                .Include(t => t.History)
                    .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            return ticket;
        }

        public async Task<BTUser> GetTicketByIdAsync(int ticketId, int companyId)
        {
            BTUser developer = new();

            try
            {
                // Implicit declaration : used when you don't know what will be returned (i.e. API calls etc... ) may not have a model
                //var tiket = new Ticket();

                // Explict declaration
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);
                if (ticket.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }

                return ticket.DeveloperUser;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                // Implicit declaration : used when you don't know what will be returned (i.e. API calls etc... ) may not have a model
                //var tiket = new Ticket();

                // Explict declaration
                Ticket ticket = await _context.Tickets.Include(t => t.DeveloperUser)
                    .Include(t => t.OwnerUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .Include(t => t.Comments)
                        .ThenInclude(t => t.User)
                    .Include(t => t.Attachments)
                        .ThenInclude(t => t.User)
                    .Include(t => t.History)
                        .ThenInclude(t => t.User)
                    .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            BTUser developer = new();
            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);

                if (ticket.DeveloperUserId != null)
                {
                    developer = ticket.DeveloperUser;
                }
                return developer;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                if (role == Roles.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if (role == Roles.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList();
                }
                else if (role == Roles.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => t.OwnerUserId == userId).ToList();
                }
                else if (role == Roles.ProjectManager.ToString())
                {
                    tickets = await GetTicketsByUserIdAsync(userId, companyId);
                }
                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                BTUser btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (await _rolesService.IsUserInRoleAsync(btUser, Roles.Admin.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                    .SelectMany(p => p.Tickets)
                                                    .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser, Roles.Developer.ToString()))
                {
                    List<Ticket> devTickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                                     .SelectMany(p => p.Tickets)
                                                                     .Where(t => t.DeveloperUserId == userId)
                                                                     .ToList();
                    List<Ticket> subTickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                                    .SelectMany(p => p.Tickets)
                                                                    .Where(t => t.OwnerUserId == userId)
                                                                    .ToList();
                    tickets = devTickets.Concat(subTickets).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser, Roles.Submitter.ToString()))
                {
                    tickets = (await _projectService.GetAllProjectsByCompany(companyId))
                                                    .SelectMany(t => t.Tickets)
                                                    .Where(t => t.OwnerUserId == userId)
                                                    .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser, Roles.ProjectManager.ToString()))
                {
                    tickets = (await _projectService.GetUserProjectsAsync(userId))
                                                    .SelectMany(t => t.Tickets)
                                                    .ToList();
                    // TODO:
                    // Fixup concatenation of submitted tickets
                }

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(p => p.Name == priorityName);

                return priority.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == statusName);

                return status.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            TicketType ticketType = await _context.TicketTypes.FirstOrDefaultAsync(p => p.Name == typeName);

            return ticketType.Id;
        }

        //CRUD : Update
        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket); //Ask database to update items
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}