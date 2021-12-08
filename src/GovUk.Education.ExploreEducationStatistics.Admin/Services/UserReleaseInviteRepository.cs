#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<Either<ActionResult, Unit>> Create(Guid releaseId, string email,
            ReleaseRole releaseRole, bool emailSent, Guid createdById, bool accepted = false)
        {
            await _contentDbContext.AddAsync(
                new UserReleaseInvite
                {
                    Email = email.ToLower(),
                    ReleaseId = releaseId,
                    Role = releaseRole,
                    Accepted = accepted,
                    EmailSent = emailSent,
                    Created = DateTime.UtcNow,
                    CreatedById = createdById,
                }
            );

            await _contentDbContext.SaveChangesAsync();

            return Unit.Instance;
        }

        public async Task<Either<ActionResult, Unit>> CreateManyIfNotExists(List<Guid> releaseIds, string email,
            ReleaseRole releaseRole, bool emailSent, Guid createdById, bool accepted = false)
        {
            foreach (var releaseId in releaseIds)
            {
                if (!await UserHasInvite(releaseId, email, releaseRole))
                {
                    await _contentDbContext.AddAsync(
                        new UserReleaseInvite
                        {
                            Email = email.ToLower(),
                            ReleaseId = releaseId,
                            Role = releaseRole,
                            EmailSent = emailSent,
                            Created = DateTime.UtcNow,
                            CreatedById = createdById,
                            Accepted = accepted,
                        }
                    );
                }
            }

            await _contentDbContext.SaveChangesAsync();
            return Unit.Instance;
        }

        public async Task MarkEmailAsSent(UserReleaseInvite invite)
        {
            invite.EmailSent = true;
            _contentDbContext.Update(invite);
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
                .AsAsyncEnumerable()
                .Where(i =>
                    releaseIds.Contains(i.ReleaseId)
                    && i.Email.ToLower().Equals(email.ToLower())
                    && i.Role == role)
                .Select(i => i.ReleaseId)
                .ToListAsync();

            return releaseIds.All(releaseId => inviteReleaseIds.Contains(releaseId));
        }

        public async Task RemoveMany(List<Guid> releaseIds, string email, ReleaseRole role)
        {
            var invites = await _contentDbContext
                .UserReleaseInvites
                .AsAsyncEnumerable()
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
