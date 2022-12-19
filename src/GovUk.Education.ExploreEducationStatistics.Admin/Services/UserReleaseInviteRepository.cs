#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UserReleaseInviteRepository : IUserReleaseInviteRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public UserReleaseInviteRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task Create(Guid releaseId,
            string email,
            ReleaseRole releaseRole,
            bool emailSent,
            Guid createdById,
            DateTime? createdDate = null)
        {
            await _contentDbContext.AddAsync(
                new UserReleaseInvite
                {
                    Email = email.ToLower(),
                    ReleaseId = releaseId,
                    Role = releaseRole,
                    EmailSent = emailSent,
                    Created = createdDate ?? DateTime.UtcNow,
                    CreatedById = createdById,
                }
            );

            await _contentDbContext.SaveChangesAsync();
        }

        public async Task CreateManyIfNotExists(List<Guid> releaseIds,
            string email,
            ReleaseRole releaseRole,
            bool emailSent,
            Guid createdById,
            DateTime? createdDate = null)
        {
            var invites = await releaseIds
                .ToAsyncEnumerable()
                .WhereAwait(async releaseId => !await UserHasInvite(releaseId, email, releaseRole))
                .Select(releaseId =>
                    new UserReleaseInvite
                    {
                        Email = email.ToLower(),
                        ReleaseId = releaseId,
                        Role = releaseRole,
                        EmailSent = emailSent,
                        Created = createdDate ?? DateTime.UtcNow,
                        CreatedById = createdById,
                    }
                ).ToListAsync();

            await _contentDbContext.AddRangeAsync(invites);
            await _contentDbContext.SaveChangesAsync();
        }

        public async Task Remove(Guid releaseId, string email, ReleaseRole role)
        {
            var invites = await _contentDbContext.UserReleaseInvites
                .AsQueryable()
                .Where(uri =>
                    uri.ReleaseId == releaseId
                    && uri.Role == role
                    && uri.Email == email) // DB comparison is case insensitive
                .ToListAsync();
            _contentDbContext.UserReleaseInvites.RemoveRange(invites);
            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<bool> UserHasInvite(Guid releaseId, string email, ReleaseRole role)
        {
            return await _contentDbContext
                .UserReleaseInvites
                .AsQueryable()
                .AnyAsync(i =>
                    i.ReleaseId == releaseId
                    && i.Email.ToLower().Equals(email.ToLower())
                    && i.Role == role);
        }

        public async Task<bool> UserHasInvites(List<Guid> releaseIds, string email, ReleaseRole role)
        {
            var inviteReleaseIds = await _contentDbContext
                .UserReleaseInvites
                .AsQueryable()
                .Where(i =>
                    releaseIds.Contains(i.ReleaseId)
                    && i.Email.ToLower().Equals(email.ToLower())
                    && i.Role == role)
                .Select(i => i.ReleaseId)
                .ToListAsync();

            return releaseIds.All(releaseId => inviteReleaseIds.Contains(releaseId));
        }

        public async Task RemoveByPublication(Publication publication, string email, ReleaseRole role)
        {
            _contentDbContext.Update(publication);
            await _contentDbContext.Entry(publication)
                .Collection(p => p.Releases)
                .LoadAsync();

            var releaseIds = publication.Releases
                .Select(r => r.Id)
                .ToList();

            var invites = await _contentDbContext
                .UserReleaseInvites
                .AsQueryable()
                .Where(i =>
                    releaseIds.Contains(i.ReleaseId)
                    && i.Email.ToLower().Equals(email.ToLower())
                    && i.Role == role)
                .ToListAsync();

            _contentDbContext.RemoveRange(invites);
            await _contentDbContext.SaveChangesAsync();
        }
    }
}
