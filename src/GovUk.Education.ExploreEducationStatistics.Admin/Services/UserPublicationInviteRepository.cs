#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Guid = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;


public class UserPublicationInviteRepository(ContentDbContext contentDbContext) : IUserPublicationInviteRepository
{
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

        await contentDbContext.UserPublicationInvites.AddRangeAsync(invites);
        await contentDbContext.SaveChangesAsync();
    }

    public Task<List<UserPublicationInvite>> GetInvitesByEmail(string email)
    {
        return contentDbContext
            .UserPublicationInvites
            .Where(invite => invite.Email.ToLower().Equals(email.ToLower()))
            .ToListAsync();
    }

    public async Task Remove(
        Guid publicationId,
        string email,
        PublicationRole role,
        CancellationToken cancellationToken = default)
    {
        var invite = await contentDbContext.UserPublicationInvites
            .AsQueryable()
            .Where(uri =>
                uri.PublicationId == publicationId
                && uri.Role == role
                && uri.Email.ToLower().Equals(email!.ToLower())) // DB comparison is case insensitive
            .SingleOrDefaultAsync(cancellationToken);

        if (invite is null)
        {
            return;
        }

        contentDbContext.UserPublicationInvites.Remove(invite);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMany(
        IReadOnlyList<UserPublicationInvite> userPublicationInvites,
        CancellationToken cancellationToken = default)
    {
        if (!userPublicationInvites.Any())
        {
            return;
        }

        contentDbContext.UserPublicationInvites.RemoveRange(userPublicationInvites);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveByUserEmail(
        string email,
        CancellationToken cancellationToken = default)
    {
        var invites = await contentDbContext.UserPublicationInvites
            .Where(i => i.Email.ToLower().Equals(email.ToLower()))
            .ToListAsync(cancellationToken);

        await RemoveMany(invites, cancellationToken);
    }

    private async Task<bool> UserHasInvite(
        Guid publicationId,
        PublicationRole role,
        string email)
    {
        return await contentDbContext
            .UserPublicationInvites
            .AnyAsync(i =>
                i.PublicationId == publicationId
                && i.Role == role
                && i.Email.ToLower().Equals(email.ToLower()));
    }
}
