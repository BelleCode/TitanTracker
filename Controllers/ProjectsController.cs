using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TitanTracker.Data;
using TitanTracker.Extensions;
using TitanTracker.Models;
using TitanTracker.Models.Enums;
using TitanTracker.Models.ViewModels;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _companInfoService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;
        private readonly IBTFileService _fileService;
        private readonly UserManager<BTUser> _userManager;

        public ProjectsController(ApplicationDbContext context,
                                  IBTCompanyInfoService companInfoService,
                                  IBTRolesService rolesService,
                                  IBTProjectService projectService,
                                  IBTFileService fileService,
                                  UserManager<BTUser> userManager)
        {
            _context = context;
            _companInfoService = companInfoService;
            _rolesService = rolesService;
            _projectService = projectService;
            _fileService = fileService;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Company)
                                                        .Include(p => p.ProjectPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        public async Task<IActionResult> SelectPM(AdminIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.PmId != null)
                {
                    await _projectService.AddUserToProjectAsync(model.PmId, model.ProjectId);
                }
            }
            return RedirectToAction("AllCompanyProjects");
        }

        public async Task<IActionResult> SelectDevs(AdminIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedDevs != null)
                {
                    List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.ProjectId))
                                                                   .Select(m => m.Id).ToList();
                    foreach (string item in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(item, model.ProjectId);
                    }

                    foreach (string item in model.SelectedDevs)
                    {
                        await _projectService.AddUserToProjectAsync(item, model.ProjectId);
                    }

                    // go to project details
                    // return RedirectedAction("Details", "Projects", new { id = model.Project.Id });
                }
            }

            return RedirectToAction("AllCompanyProjects");
        }

        public async Task<IActionResult> SelectSubs(AdminIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedSubs != null)
                {
                    List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.ProjectId))
                                                                   .Select(m => m.Id).ToList();
                    foreach (string item in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(item, model.ProjectId);
                    }

                    foreach (string item in model.SelectedSubs)
                    {
                        await _projectService.AddUserToProjectAsync(item, model.ProjectId);
                    }

                    // go to project details
                    // return RedirectedAction("Details", "Projects", new { id = model.Project.Id });
                }
            }

            return RedirectToAction("AllCompanyProjects");
        }

        public async Task<IActionResult> AllCompanyProjects()
        {
            //string userId = _userManager.GetUserId(User);
            int companyId = User.Identity.GetCompanyId().Value;
            AdminIndexViewModel pMIndexViewModel = new AdminIndexViewModel();

            pMIndexViewModel.Projects = await _projectService.GetAllProjectsByCompany(companyId);

            List<BTUser> developers = (await _rolesService.GetUsersInRoleAsync(Roles.Developer.ToString(), companyId));
            List<BTUser> submitters = (await _rolesService.GetUsersInRoleAsync(Roles.Submitter.ToString(), companyId));

            List<string> devsIdList = new();
            List<string> subIdList = new();
            //foreach (BTUser member in developers)
            //{
            //    if (await _rolesService.IsUserInRoleAsync(member, Roles.Developer.ToString()))
            //    {
            //        devsIdList.Add(member.Id);
            //    }
            //}

            pMIndexViewModel.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "PreferredName");
            pMIndexViewModel.DevList = new MultiSelectList(developers, "Id", "PreferredName", devsIdList);
            pMIndexViewModel.SubList = new MultiSelectList(submitters, "Id", "PreferredName", subIdList);

            return View(pMIndexViewModel);
        }

        // Get projects that don't have a PM
        public async Task<IActionResult> AssignProjMIndex()
        {
            string userId = _userManager.GetUserId(User);
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }

        public async Task<IActionResult> AssignProjM(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            // Create new viewmodel for Project and list of Project Managers
            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id, companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            // Shows page that has a specific project,
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjM(AssignPMViewModel model)
        {
            // Set Parameter for AddProjectManagerViewModel

            if (!string.IsNullOrEmpty(model.PmId))
            {
                await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            // all company projects based on "id" parameter
            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            Project project = projects.FirstOrDefault(p => p.Id == id);

            model.Project = project;

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(Roles.Submitter.ToString(), companyId);

            List<BTUser> users = developers.Concat(submitters).ToList();

            List<string> members = project.Members.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(users, "Id", "FullName", members);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id))
                                                                   .Select(m => m.Id).ToList();
                    foreach (string item in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(item, model.Project.Id);
                    }

                    foreach (string item in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(item, model.Project.Id);
                    }

                    // go to project details
                    // return RedirectedAction("Details", "Projects", new { id = model.Project.Id });
                }
            }

            return RedirectToAction("AssignMembers", new { id = model.Project.Id });
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProgramManager, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            // Add ViewModel instance "AddProjectWithPMViewModel"
            AddProjectWithPMViewModel model = new();

            // Load SelectLists with data i.e. PMList & PriorityList
            model.PMList = new SelectList(await _rolesService
                               .GetUsersInRoleAsync(Roles.ProgramManager.ToString(), companyId), "id", "FullName");

            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");

            // Return View with viewModel instace as the model

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            // Change the above parameeter type to "AddProjectWithPMViewModel" as model

            // Test if model is null (aka if data has been capture from the form)
            if (model != null)
            {
                // Capture the image if one has been selected
                try
                {
                    if (model.Project.FormFile != null)
                    {
                        model.Project.FileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.FormFile);
                        model.Project.FileName = model.Project.FormFile.FileName;
                        model.Project.FileContentType = model.Project.FormFile.ContentType;
                    }

                    model.Project.CompanyId = companyId;

                    await _projectService.AddNewProjectAsync(model.Project);

                    // Is there a PM? If there is a Pm chosen, add them.
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddUserToProjectAsync(model.PmId, model.Project.Id);
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }

            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,FileData,FileContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}