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

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {// is there a PM to be removed?
                BTUser currentPM = await GetProjectManagerAsync(projectId);

                if (currentPM != null)
                {
                    try
                    {
                        await RemoveProjectManagerAsync(projectId);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                try
                {
                    await AddUserToProjectAsync(userId, projectId);
                    return true;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
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

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            try
            {
                List<BTUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
                List<BTUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
                List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());

                List<BTUser> members = developers.Concat(submitters).Concat(admins).ToList();

                return members;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = new();

            try // set up list of projects we want to return back
            {
                projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                                .Include(p => p.Members)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Comments)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Attachments)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.History)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Notifications)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                                .ToListAsync();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            try
            {
                List<Project> projects = await GetAllProjectsByCompany(companyId);
                int priorityId = await LookupProjectPriorityId(priorityName);

                return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = new();

            try // set up list of projects we want to return back
            {
                projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                                .Include(p => p.Members)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Comments)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Attachments)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.History)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Notifications)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                .Include(p => p.ProjectPriority)
                                                .ToListAsync();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
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
                                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                return project;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                    {
                        return member;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            try
            {
                Project project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

                List<BTUser> members = new();

                foreach (BTUser user in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(user, role))
                    {
                        members.Add(user);
                    }
                }

                return members;
            }
            catch (Exception)
            {
                throw;
            }
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
                                                                    .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();
                return userProjects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** ERROR *** - Error Getting user projects list. --> {ex.Message}");
                throw;
            }
        }

        public async Task<List<Project>> GetAdminProjectsAsync(string userId, int companyId)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);

            return projects.Where(p => p.AdminId == userId).ToList();
        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            List<Project> result = new();
            List<Project> projects = new();

            try
            {
                projects = await _context.Projects.Where(p => p.CompanyId == companyId).ToListAsync();

                foreach (Project proj in projects)
                {
                    if ((await GetProjectMembersByRoleAsync(proj.Id, Roles.ProjectManager.ToString())).Count == 0)
                    {
                        result.Add(proj);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return (projects);
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            try
            {
                List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();
                return users.Where(u => u.CompanyId == companyId).ToList(); ;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsUserOnProject(string userId, int projectId) // Test
        {
            // find the project in order to get the members
            try
            {
                Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
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

        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            try
            {
                int priorityId = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name == priorityName)).Id;

                return priorityId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                Project project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync();

                try
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString()))
                        {
                            await RemoveUsersFromProjectByRoleAsync(member.Id, projectId);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProject(userId, projectId)) // If user is on the project
                    {
                        project.Members.Remove(user);
                        await UpdateProjectAsync(project);
                    }
                    else
                    {
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** ERROR *** - Error Getting user projects list. --> {ex.Message}");
                throw;
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser bTUser in members)
                {
                    try
                    {
                        project.Members.Remove(bTUser);
                        await UpdateProjectAsync(project);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*** ERROR *** - Error Getting user projects list. --> {ex.Message}");
                throw;
            }
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