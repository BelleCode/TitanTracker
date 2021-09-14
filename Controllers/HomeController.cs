using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Extensions;
using TitanTracker.Models;
using TitanTracker.Models.Enums;
using TitanTracker.Models.ViewModels;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _logger = logger;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Landing()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}