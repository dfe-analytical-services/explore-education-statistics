#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPrereleaseRoleRepository(ContentDbContext contentDbContext) : IUserPrereleaseRoleRepository
{
    public async Task<UserReleaseRole> Create(
        Guid userId,
        Guid releaseVersionId,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        createdDate ??= createdDate?.ToUniversalTime() ?? DateTime.UtcNow;

        var newUserPrereleaseRole = new UserReleaseRole
        {
            UserId = userId,
            ReleaseVersionId = releaseVersionId,
            Role = ReleaseRole.PrereleaseViewer,
            Created = createdDate!.Value,
            CreatedById = createdById,
        };

        contentDbContext.UserReleaseRoles.Add(newUserPrereleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPrereleaseRole;
    }

    public async Task<List<UserReleaseRole>> CreateManyIfNotExists(
        IEnumerable<UserPrereleaseRoleCreateDto> userPrereleaseRolesToCreate,
        CancellationToken cancellationToken = default
    )
    {
        return await userPrereleaseRolesToCreate
            .ToAsyncEnumerable()
            .Where(
                async (urr, ct) =>
                    !await UserHasPrereleaseRoleOnReleaseVersion(
                        userId: urr.UserId,
                        releaseVersionId: urr.ReleaseVersionId,
                        resourceRoleFilter: ResourceRoleFilter.All,
                        cancellationToken: ct
                    )
            )
            .Select(
                async (urr, ct) =>
                    await Create(
                        userId: urr.UserId,
                        releaseVersionId: urr.ReleaseVersionId,
                        createdById: urr.CreatedById,
                        createdDate: urr.CreatedDate,
                        cancellationToken: ct
                    )
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<UserReleaseRole?> GetById(
        Guid userPrereleaseRoleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .SingleOrDefaultAsync(urr => urr.Id == userPrereleaseRoleId, cancellationToken);
    }

    public async Task<UserReleaseRole?> GetByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public IQueryable<UserReleaseRole> Query(ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly)
    {
        var userPrereleaseRoles = contentDbContext.UserReleaseRoles.AsQueryable();

        return resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => userPrereleaseRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => userPrereleaseRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => userPrereleaseRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => userPrereleaseRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
        };
    }

    public async Task<bool> RemoveById(Guid userPrereleaseRoleId, CancellationToken cancellationToken = default)
    {
        var userPrereleaseRole = await GetById(userPrereleaseRoleId, cancellationToken);

        if (userPrereleaseRole is null)
        {
            return false;
        }

        await Remove(userPrereleaseRole, cancellationToken);

        return true;
    }

    public async Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        var userPrereleaseRole = await GetByCompositeKey(
            userId: userId,
            releaseVersionId: releaseVersionId,
            cancellationToken: cancellationToken
        );

        if (userPrereleaseRole is null)
        {
            return false;
        }

        await Remove(userPrereleaseRole, cancellationToken);

        return true;
    }

    public async Task RemoveMany(
        IEnumerable<UserReleaseRole> userPrereleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!userPrereleaseRoles.Any())
        {
            return;
        }

        contentDbContext.UserReleaseRoles.RemoveRange(userPrereleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPrereleaseRoles = await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .ToListAsync(cancellationToken);

        contentDbContext.UserReleaseRoles.RemoveRange(userPrereleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UserHasPrereleaseRoleOnReleaseVersion(
        Guid userId,
        Guid releaseVersionId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForReleaseVersion(releaseVersionId)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasPrereleaseRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .AnyAsync(cancellationToken);
    }

    public async Task MarkEmailAsSent(
        Guid userPrereleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userPrereleaseRole = await GetById(
            userPrereleaseRoleId: userPrereleaseRoleId,
            cancellationToken: cancellationToken
        );

        if (userPrereleaseRole is null)
        {
            throw new InvalidOperationException($"No User Release Role found with ID {userPrereleaseRoleId}.");
        }

        userPrereleaseRole.EmailSent = dateSent ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Remove(UserReleaseRole userPrereleaseRole, CancellationToken cancellationToken = default)
    {
        contentDbContext.UserReleaseRoles.Remove(userPrereleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public record UserPrereleaseRoleCreateDto(
        Guid UserId,
        Guid ReleaseVersionId,
        Guid CreatedById,
        DateTime? CreatedDate = null
    );
}
