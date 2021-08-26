using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Services.Interfaces;
using TitanTracker.Models;
using TitanTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace TitanTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        // CRUD : Create
        public async Task AddNewProjectAsync(Project project)
        {
            try
            {
                await _context.AddAsync(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try
            {
                // find the project
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                // Add a user to a project
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)// is user already on project
                {
                    if (!await IsUserOnProject(userId, projectId)) // if user is NOT on project, let's add
                    {
                        try
                        {
                            project.Members.Add(user);
                            await UpdateProjectAsync(project);
                            return true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //CRUD: Delete
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        // CRUD: Read
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project project = await _context.Projects
                                                .Include(p => p.Tickets)
                                                .Include(p => p.Members)
                                                .Include(p => p.ProjectPriority)
                                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                return project;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            throw new NotImplementedException();
        }

        public Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Company)
                                                            .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Members)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(t => t.Tickets)
                                                                    .ThenInclude(t => t.DeveloperUser)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(t => t.Tickets)
                                                                    .ThenInclude(t => t.OwnerUser)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(t => t.Tickets)
                                                                    .ThenInclude(t => t.TicketPriority)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(t => t.Tickets)
                                                                    .ThenInclude(t => t.TicketStatus)
                                                           .Include(u => u.Projects)
                                                                .ThenInclude(t => t.Tickets)
                                                                    .ThenInclude(t => t.TicketType)
                                                                    .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
                return userProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** ERROR *** - Error Getting user projects list. --> {ex.Message}");
                throw;
            }
        }

        public Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsUserOnProject(string userId, int projectId) // Test
        {
            // find the project in order to get the members
            try
            {
                Project project = await _context.Projects.Include(p => p).FirstOrDefaultAsync(p => p.Id == projectId);
                bool result = false;

                if (project != null)
                {
                    result = project.Members.Any(m => m.Id == userId);
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> LookupProjectPriorityId(string priorityName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProjectManagerAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            throw new NotImplementedException();
        }

        // CRUD : Update
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}