using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TitanTracker.Data;
using TitanTracker.Extensions;
using TitanTracker.Models;
using TitanTracker.Services.Interfaces;

namespace TitanTracker.Controllers
{
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTInviteService _inviteService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _dataProtector;

        public InvitesController(ApplicationDbContext context,
                                 IBTProjectService projectService,
                                 IBTInviteService inviteService,
                                 IEmailSender emailSender,
                                 UserManager<BTUser> userManager,
                                 IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _projectService = projectService;
            _inviteService = inviteService;
            _emailSender = emailSender;
            _userManager = userManager;
            _dataProtector = dataProtectionProvider.CreateProtector("CR.TitanTracker.21");
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id");
            //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");

            //TODO : Sweet Alerts if Model is invalid
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName, Message")] Invite invite)
        {
            if (ModelState.IsValid)
            {
                int companyId = User.Identity.GetCompanyId().Value;

                Guid guid = Guid.NewGuid();

                var token = _dataProtector.Protect(guid.ToString());
                var email = _dataProtector.Protect(invite.InviteeEmail);

                var callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email }, protocol: Request.Scheme);

                var body = invite.Message + Environment.NewLine + "Please join my Company."
                                          + Environment.NewLine + "Please click the following link to join <a href=\""
                                          + callbackUrl + "\">COLLABORATE</a>";
                var destination = invite.InviteeEmail;
                var subject = "TitanTracker - Company Invite";

                await _emailSender.SendEmailAsync(destination, subject, body);

                // Create a record in the Invite Table
                invite.CompanyToken = guid;
                invite.CompanyId = companyId;
                invite.InviteDate = DateTimeOffset.Now;
                invite.InvitorId = _userManager.GetUserId(User);
                invite.IsValid = true;

                await _inviteService.AddNewInviteAsync(invite);

                return RedirectToAction("Dashboard", "Home");
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            return View(invite);
        }

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InviteeId,InvitorId,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invite = await _context.Invites.FindAsync(id);
            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
            return _context.Invites.Any(e => e.Id == id);
        }
    }
}