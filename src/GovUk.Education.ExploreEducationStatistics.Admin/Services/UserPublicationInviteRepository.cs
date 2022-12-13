#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserPublicationInviteRepository : IUserPublicationInviteRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserPublicationInviteRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task CreateManyIfNotExists(
            List<UserPublicationRoleCreateRequest> userPublicationRoles,
            string email,
            Guid createdById,
            DateTime? createdDate = null)
        {
            var invites = await userPublicationRoles
                .ToAsyncEnumerable()
                .WhereAwait(async userPublicationRole =>
                    !await UserHasInvite(
                        userPublicationRole.PublicationId,
                        userPublicationRole.PublicationRole,
                        email))
                .Select(userPublicationRole =>
                    new UserPublicationInvite
                    {
                        Email = email.ToLower(),
                        PublicationId = userPublicationRole.PublicationId,
                        Role = userPublicationRole.PublicationRole,
                        Created = createdDate ?? DateTime.UtcNow,
                        CreatedById = createdById,
                    }
                ).ToListAsync();

            await _contentDbContext.UserPublicationInvites.AddRangeAsync(invites);
            await _contentDbContext.SaveChangesAsync();
        }

        private async Task<bool> UserHasInvite(
            Guid publicationId,
            PublicationRole role,
            string email)
        {
            return await _contentDbContext
                .UserPublicationInvites
                .AnyAsync(i =>
                    i.PublicationId == publicationId
                    && i.Role == role
                    && i.Email.ToLower().Equals(email.ToLower()));
        }
    }
}
