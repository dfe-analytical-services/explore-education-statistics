#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseInviteRepository : IUserReleaseInviteRepository
{
    private readonly ContentDbContext _contentDbContext;

    public UserReleaseInviteRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task Create(Guid releaseVersionId,
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
                ReleaseVersionId = releaseVersionId,
                Role = releaseRole,
                EmailSent = emailSent,
                Created = createdDate ?? DateTime.UtcNow,
                CreatedById = createdById,
            }
        );

        await _contentDbContext.SaveChangesAsync();
    }

    public async Task CreateManyIfNotExists(List<Guid> releaseVersionIds,
        string email,
        ReleaseRole releaseRole,
        bool emailSent,
        Guid createdById,
        DateTime? createdDate = null)
    {
        var invites = await releaseVersionIds
            .ToAsyncEnumerable()
            .WhereAwait(async releaseVersionId => !await UserHasInvite(releaseVersionId, email, releaseRole))
            .Select(releaseVersionId =>
                new UserReleaseInvite
                {
                    Email = email.ToLower(),
                    ReleaseVersionId = releaseVersionId,
                    Role = releaseRole,
                    EmailSent = emailSent,
                    Created = createdDate ?? DateTime.UtcNow,
                    CreatedById = createdById,
                }
            ).ToListAsync();

        await _contentDbContext.AddRangeAsync(invites);
        await _contentDbContext.SaveChangesAsync();
    }

    public async Task Remove(Guid releaseVersionId,
        string email,
        ReleaseRole role)
    {
        var invites = await _contentDbContext.UserReleaseInvites
            .AsQueryable()
            .Where(uri =>
                uri.ReleaseVersionId == releaseVersionId
                && uri.Role == role
                && uri.Email == email) // DB comparison is case insensitive
            .ToListAsync();
        _contentDbContext.UserReleaseInvites.RemoveRange(invites);
        await _contentDbContext.SaveChangesAsync();
    }

    public async Task<bool> UserHasInvite(Guid releaseVersionId,
        string email,
        ReleaseRole role)
    {
        return await _contentDbContext
            .UserReleaseInvites
            .AsQueryable()
            .AnyAsync(i =>
                i.ReleaseVersionId == releaseVersionId
                && i.Email.ToLower().Equals(email.ToLower())
                && i.Role == role);
    }

    public async Task<bool> UserHasInvites(List<Guid> releaseVersionIds,
        string email,
        ReleaseRole role)
    {
        var inviteReleaseVersionIds = await _contentDbContext
            .UserReleaseInvites
            .AsQueryable()
            .Where(i =>
                releaseVersionIds.Contains(i.ReleaseVersionId)
                && i.Email.ToLower().Equals(email.ToLower())
                && i.Role == role)
            .Select(i => i.ReleaseVersionId)
            .ToListAsync();

        return releaseVersionIds.All(releaseVersionId => inviteReleaseVersionIds.Contains(releaseVersionId));
    }

    public async Task RemoveByPublication(Publication publication, string email, ReleaseRole role)
    {
        _contentDbContext.Update(publication);
        await _contentDbContext.Entry(publication)
            .Collection(p => p.ReleaseVersions)
            .LoadAsync();

        var releaseVersionIds = publication.ReleaseVersions
            .Select(rv => rv.Id)
            .ToList();

        var invites = await _contentDbContext
            .UserReleaseInvites
            .AsQueryable()
            .Where(i =>
                releaseVersionIds.Contains(i.ReleaseVersionId)
                && i.Email.ToLower().Equals(email.ToLower())
                && i.Role == role)
            .ToListAsync();

        _contentDbContext.RemoveRange(invites);
        await _contentDbContext.SaveChangesAsync();
    }

    public Task<List<UserReleaseInvite>> ListByEmail(string email)
    {
        return _contentDbContext
            .UserReleaseInvites
            .Where(invite => invite.Email.ToLower().Equals(email.ToLower()))
            .ToListAsync();
    }
}
