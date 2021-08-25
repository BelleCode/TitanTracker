using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TitanTracker.Models;

namespace TitanTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);

        public Task<List<IdentityRole>> GetRolesAsync();

        public Task<string> GetRoleNameByIdAsync(string roleId);

        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId); //get

        public Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId); //get

        public Task<IEnumerable<string>> GetUsersRoleAsync(BTUser user); //get

        public Task<bool> IsUserInRoleAsync(BTUser user, string roleName);

        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);

        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles);
    }
}