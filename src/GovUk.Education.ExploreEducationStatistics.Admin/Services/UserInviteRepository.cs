#nullable enable
using System;
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

        public async Task<UserInvite> CreateIfNotExists(
            string email, 
            Role role, 
            Guid createdById,
            DateTime? createdDate = null)
        {
            return await CreateIfNotExists(email, role.GetEnumValue(), createdById, createdDate);
        }

        public async Task<UserInvite> CreateIfNotExists(
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
                .AsQueryable()
                .SingleOrDefaultAsync(i => i.Email.ToLower().Equals(email.ToLower()));

            if (existingInvite != null)
            {
                return existingInvite;
            }

            var newInvite = new UserInvite
            {
                Email = email.ToLower(),
                RoleId = roleId,
                Created = createdDate ?? DateTime.UtcNow,
                CreatedById = createdById.ToString(),
            };
            await _usersAndRolesDbContext.AddAsync(newInvite);
            await _usersAndRolesDbContext.SaveChangesAsync();
            return newInvite;
        }
    }
}
