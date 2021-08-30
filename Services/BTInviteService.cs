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
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId)
        {
            try
            {
                Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite != null)
                {
                    try
                    {
                        invite.IsValid = false;     // person needs an Id once they have registered in the system
                        invite.InviteeId = userId;  // to get invite or access (must be in company)

                        await _context.SaveChangesAsync();// Tracked entities are saved here

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
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            throw new NotImplementedException();
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                bool result = await _context.Invites.Where(i => i.CompanyId == companyId)
                                                    .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                              .Include(i => i.Company)
                                              .Include(i => i.Project)
                                              .Include(i => i.Invitor)
                                              .FirstOrDefaultAsync(i => i.Id == inviteId);
                return invite;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites.Where(i => i.CompanyId == companyId)
                                              .Include(i => i.Company)
                                              .Include(i => i.Project)
                                              .Include(i => i.Invitor)
                                              .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
                return invite;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if (token == null)
                {
                    return false;
                }

                bool result = false;

                Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite != null)
                {
                    //  Determine Invite date
                    DateTime inviteDate = invite.InviteDate.DateTime;

                    // Custom validation of invite based on date it was issued
                    // We will allow an invite to be valid for 7 days.
                    bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                    if (validDate)
                    {
                        result = invite.IsValid;
                    }
                }
                return result;
            }
            catch (Exception) { throw; }
        }
    }
}