#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserInviteRepository : IUserInviteRepository
    {
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;

        public UserInviteRepository(UsersAndRolesDbContext usersAndRolesDbContext)
        {
            _usersAndRolesDbContext = usersAndRolesDbContext;
        }

        public async Task<UserInvite> CreateOrUpdate(
            string email, 
            Role role, 
            Guid createdById,
            DateTime? createdDate = null)
        {
            return await CreateOrUpdate(email, role.GetEnumValue(), createdById, createdDate);
        }

        public async Task<UserInvite> CreateOrUpdate(
            string email, 
            string roleId, 
            Guid createdById,
            DateTime? createdDate = null)
        {
            if (createdDate != null && createdDate > DateTime.UtcNow)
            {
                throw new ArgumentException($"{nameof(UserInvite)} created date cannot be a future date");
            }
            
            var existingInvite = await _usersAndRolesDbContext
                .UserInvites
                .IgnoreQueryFilters()
                .AsQueryable()
                .SingleOrDefaultAsync(i => i.Email.ToLower().Equals(email.ToLower()));

            var inviteToPopulate = existingInvite ?? new UserInvite();
            inviteToPopulate.Email = email.ToLower();
            inviteToPopulate.RoleId = roleId;
            inviteToPopulate.Created = createdDate ?? DateTime.UtcNow;
            inviteToPopulate.CreatedById = createdById.ToString();

            if (existingInvite != null)
            {
                _usersAndRolesDbContext.UserInvites.Update(inviteToPopulate);
            }
            else
            {
                await _usersAndRolesDbContext.UserInvites.AddAsync(inviteToPopulate);
            }

            await _usersAndRolesDbContext.SaveChangesAsync();
            return inviteToPopulate;
        }
    }
}
