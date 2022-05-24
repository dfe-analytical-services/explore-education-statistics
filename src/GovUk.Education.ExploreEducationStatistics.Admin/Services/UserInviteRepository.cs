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

        public async Task<UserInvite> Create(string email, Role role, Guid createdById)
        {
            return await Create(email, role.GetEnumValue(), createdById);
        }

        public async Task<UserInvite> Create(string email, string roleId, Guid createdById)
        {
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
                Created = DateTime.UtcNow,
                CreatedById = createdById.ToString(),
            };
            _usersAndRolesDbContext.Add(newInvite);
            await _usersAndRolesDbContext.SaveChangesAsync();
            return newInvite;
        }
    }
}
