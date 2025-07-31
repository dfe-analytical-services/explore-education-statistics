#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseInviteRepository(ContentDbContext contentDbContext) : IUserReleaseInviteRepository
{
    public async Task Create(Guid releaseVersionId,
        string email,
        ReleaseRole releaseRole,
        bool emailSent,
        Guid createdById,
        DateTime? createdDate = null)
    {
        await contentDbContext.AddAsync(
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

        await contentDbContext.SaveChangesAsync();
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

        await contentDbContext.AddRangeAsync(invites);
        await contentDbContext.SaveChangesAsync();
    }

    public async Task<bool> UserHasInvite(Guid releaseVersionId,
        string email,
        ReleaseRole role)
    {
        return await contentDbContext
            .UserReleaseInvites
            .AsQueryable()
            .AnyAsync(i =>
                i.ReleaseVersionId == releaseVersionId
                && i.Email == email
                && i.Role == role);
    }

    public async Task<bool> UserHasInvites(List<Guid> releaseVersionIds,
        string email,
        ReleaseRole role)
    {
        var inviteReleaseVersionIds = await contentDbContext
            .UserReleaseInvites
            .AsQueryable()
            .Where(i =>
                releaseVersionIds.Contains(i.ReleaseVersionId)
                && i.Email == email
                && i.Role == role)
            .Select(i => i.ReleaseVersionId)
            .ToListAsync();

        return releaseVersionIds.All(inviteReleaseVersionIds.Contains);
    }

    public async Task<List<UserReleaseInvite>> GetInvitesByEmail(string email)
    {
        return await contentDbContext
            .UserReleaseInvites
            .Where(invite => invite.Email == email)
            .ToListAsync();
    }

    public async Task Remove(
        Guid releaseVersionId,
        string email,
        ReleaseRole role,
        CancellationToken cancellationToken = default)
    {
        var invites = await contentDbContext.UserReleaseInvites
            .AsQueryable()
            .Where(uri =>
                uri.ReleaseVersionId == releaseVersionId
                && uri.Role == role
                && uri.Email == email) // DB comparison is case insensitive
            .ToListAsync(cancellationToken);

        if (!invites.Any())
        {
            return;
        }

        await RemoveMany(invites, cancellationToken);
    }

    public async Task RemoveMany(
        IReadOnlyList<UserReleaseInvite> userReleaseInvites,
        CancellationToken cancellationToken = default)
    {
        if (!userReleaseInvites.Any())
        {
            return;
        }

        contentDbContext.UserReleaseInvites.RemoveRange(userReleaseInvites);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveByPublication(
        Guid publicationId,
        string? email = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude)
    {
        var releaseVersionIds = contentDbContext.ReleaseVersions
            .Where(rv => rv.Release.PublicationId == publicationId)
            .Select(rv => rv.Id)
            .ToHashSet();

        var invites = await contentDbContext.UserReleaseInvites
            .Where(i => releaseVersionIds.Contains(i.ReleaseVersionId))
            .If(!string.IsNullOrEmpty(email))
                .ThenWhere(i => i.Email == email)
            .If(rolesToInclude.Any())
                .ThenWhere(i => rolesToInclude.Contains(i.Role))
            .ToListAsync(cancellationToken);

        await RemoveMany(invites, cancellationToken);
    }

    public async Task RemoveByReleaseVersion(
        Guid releaseVersionId,
        string? email = null,
        CancellationToken cancellationToken = default,
        params ReleaseRole[] rolesToInclude)
    {
        var invites = await contentDbContext.UserReleaseInvites
            .Where(i => i.ReleaseVersionId == releaseVersionId)
            .If(!string.IsNullOrEmpty(email))
                .ThenWhere(i => i.Email == email)
            .If(rolesToInclude.Any())
                .ThenWhere(i => rolesToInclude.Contains(i.Role))
            .ToListAsync(cancellationToken);

        await RemoveMany(invites, cancellationToken);
    }

    public async Task RemoveByUserEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        var invites = await contentDbContext.UserReleaseInvites
            .Where(i => i.Email == email)
            .ToListAsync(cancellationToken);

        await RemoveMany(invites, cancellationToken);
    }
}
