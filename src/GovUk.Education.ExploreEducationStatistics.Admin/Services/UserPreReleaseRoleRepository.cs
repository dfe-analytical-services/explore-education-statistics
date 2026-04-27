#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPreReleaseRoleRepository(ContentDbContext contentDbContext) : IUserPreReleaseRoleRepository
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

        var newUserPreReleaseRole = new UserReleaseRole
        {
            UserId = userId,
            ReleaseVersionId = releaseVersionId,
            Role = ReleaseRole.PrereleaseViewer,
            Created = createdDate!.Value,
            CreatedById = createdById,
        };

        contentDbContext.UserReleaseRoles.Add(newUserPreReleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPreReleaseRole;
    }

    public async Task<List<UserReleaseRole>> CreateManyIfNotExists(
        IEnumerable<UserPreReleaseRoleCreateDto> userPreReleaseRolesToCreate,
        CancellationToken cancellationToken = default
    )
    {
        return await userPreReleaseRolesToCreate
            .ToAsyncEnumerable()
            .Where(
                async (urr, ct) =>
                    !await UserHasPreReleaseRoleOnReleaseVersion(
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
        Guid userPreReleaseRoleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .SingleOrDefaultAsync(urr => urr.Id == userPreReleaseRoleId, cancellationToken);
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
        var userPreReleaseRoles = contentDbContext.UserReleaseRoles.AsQueryable();

        return resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => userPreReleaseRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => userPreReleaseRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => userPreReleaseRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => userPreReleaseRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
        };
    }

    public async Task<bool> RemoveById(Guid userPreReleaseRoleId, CancellationToken cancellationToken = default)
    {
        var userPreReleaseRole = await GetById(userPreReleaseRoleId, cancellationToken);

        if (userPreReleaseRole is null)
        {
            return false;
        }

        await Remove(userPreReleaseRole, cancellationToken);

        return true;
    }

    public async Task<bool> RemoveByCompositeKey(
        Guid userId,
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        var userPreReleaseRole = await GetByCompositeKey(
            userId: userId,
            releaseVersionId: releaseVersionId,
            cancellationToken: cancellationToken
        );

        if (userPreReleaseRole is null)
        {
            return false;
        }

        await Remove(userPreReleaseRole, cancellationToken);

        return true;
    }

    public async Task RemoveMany(
        IEnumerable<UserReleaseRole> userPreReleaseRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!userPreReleaseRoles.Any())
        {
            return;
        }

        contentDbContext.UserReleaseRoles.RemoveRange(userPreReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPreReleaseRoles = await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .ToListAsync(cancellationToken);

        contentDbContext.UserReleaseRoles.RemoveRange(userPreReleaseRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UserHasPreReleaseRoleOnReleaseVersion(
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

    public async Task<bool> UserHasPreReleaseRoleOnPublication(
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
        Guid userPreReleaseRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userPreReleaseRole = await GetById(
            userPreReleaseRoleId: userPreReleaseRoleId,
            cancellationToken: cancellationToken
        );

        if (userPreReleaseRole is null)
        {
            throw new InvalidOperationException($"No User Release Role found with ID {userPreReleaseRoleId}.");
        }

        userPreReleaseRole.EmailSent = dateSent ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Remove(UserReleaseRole userPreReleaseRole, CancellationToken cancellationToken = default)
    {
        contentDbContext.UserReleaseRoles.Remove(userPreReleaseRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public record UserPreReleaseRoleCreateDto(
        Guid UserId,
        Guid ReleaseVersionId,
        Guid CreatedById,
        DateTime? CreatedDate = null
    );
}
