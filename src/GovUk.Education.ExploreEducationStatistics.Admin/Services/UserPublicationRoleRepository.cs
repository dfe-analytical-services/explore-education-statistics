#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserPublicationRoleRepository(ContentDbContext contentDbContext) : IUserPublicationRoleRepository
{
    public async Task<UserPublicationRole> Create(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        Guid createdById,
        DateTime? createdDate = null,
        CancellationToken cancellationToken = default
    )
    {
        var newUserPublicationRole = new UserPublicationRole
        {
            UserId = userId,
            PublicationId = publicationId,
            Role = role,
            Created = createdDate?.ToUniversalTime() ?? DateTime.UtcNow,
            CreatedById = createdById,
        };

        contentDbContext.UserPublicationRoles.Add(newUserPublicationRole);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPublicationRole;
    }

    public async Task<List<UserPublicationRole>> CreateManyIfNotExists(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    )
    {
        var newUserPublicationRoles = await userPublicationRoles
            .ToAsyncEnumerable()
            .WhereAwait(async userPublicationRole =>
                !await UserHasRoleOnPublication(
                    userId: userPublicationRole.UserId,
                    publicationId: userPublicationRole.PublicationId,
                    role: userPublicationRole.Role,
                    resourceRoleFilter: ResourceRoleFilter.All,
                    cancellationToken: cancellationToken
                )
            )
            .ToListAsync(cancellationToken);

        contentDbContext.UserPublicationRoles.AddRange(newUserPublicationRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return newUserPublicationRoles;
    }

    public async Task<UserPublicationRole?> GetById(
        Guid userPublicationRoleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .SingleOrDefaultAsync(upr => upr.Id == userPublicationRoleId, cancellationToken);
    }

    public async Task<UserPublicationRole?> GetByCompositeKey(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(ResourceRoleFilter.All)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// <para>
    /// The optional <paramref name="includeNewPermissionsSystemRoles"/> parameter will be removed once we remove all OLD publication
    /// roles from the DB in STEP 11 (EES-6212) of the Permissions Rework. For now, in certain situations, we need to be able to grab
    /// ALL of the publication roles (NEW &amp; OLD permissions system roles).
    /// </para>
    /// </summary>
    /// <param name="resourceRoleFilter">Filter resource roles by their status (see <see cref="ResourceRoleFilter"/>).</param>
    /// <param name="includeNewPermissionsSystemRoles">
    /// <para>Temporary parameter that controls which roles are included.</para>
    /// <para>When <c>true</c>, includes NEW permissions system roles in addition to OLD roles.</para>
    /// <para>When <c>false</c>, includes only OLD roles.</para>
    /// </param>
    public IQueryable<UserPublicationRole> Query(
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        bool includeNewPermissionsSystemRoles = false
    )
    {
        var userPublicationRoles = contentDbContext.UserPublicationRoles.AsQueryable();

        if (includeNewPermissionsSystemRoles)
        {
            userPublicationRoles = userPublicationRoles.IgnoreQueryFilters();
        }

        return resourceRoleFilter switch
        {
            ResourceRoleFilter.ActiveOnly => userPublicationRoles.WhereUserIsActive(),
            ResourceRoleFilter.PendingOnly => userPublicationRoles.WhereUserHasPendingInvite(),
            ResourceRoleFilter.AllButExpired => userPublicationRoles.WhereUserIsActiveOrHasPendingInvite(),
            ResourceRoleFilter.All => userPublicationRoles,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceRoleFilter), resourceRoleFilter, null),
        };
    }

    public async Task Remove(UserPublicationRole userPublicationRole, CancellationToken cancellationToken = default)
    {
        contentDbContext.UserPublicationRoles.Remove(userPublicationRole);

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMany(
        IReadOnlyList<UserPublicationRole> userPublicationRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!userPublicationRoles.Any())
        {
            return;
        }

        contentDbContext.UserPublicationRoles.RemoveRange(userPublicationRoles);
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var userPublicationRoles = await Query(ResourceRoleFilter.All)
            .Where(upr => upr.UserId == userId)
            .ToListAsync(cancellationToken);

        await RemoveMany(userPublicationRoles, cancellationToken);
    }

    public async Task<bool> UserHasRoleOnPublication(
        Guid userId,
        Guid publicationId,
        PublicationRole role,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default
    )
    {
        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(role)
            .AnyAsync(cancellationToken);
    }

    public async Task<bool> UserHasAnyRoleOnPublication(
        Guid userId,
        Guid publicationId,
        ResourceRoleFilter resourceRoleFilter = ResourceRoleFilter.ActiveOnly,
        CancellationToken cancellationToken = default,
        params PublicationRole[] rolesToInclude
    )
    {
        var rolesToCheck = rolesToInclude.IsNullOrEmpty() ? EnumUtil.GetEnumsArray<PublicationRole>() : rolesToInclude;

        return await Query(resourceRoleFilter)
            .WhereForUser(userId)
            .WhereForPublication(publicationId)
            .WhereRolesIn(rolesToCheck)
            .AnyAsync(cancellationToken);
    }

    public async Task MarkEmailAsSent(
        Guid userPublicationRoleId,
        DateTimeOffset? dateSent = null,
        CancellationToken cancellationToken = default
    )
    {
        var userPublicationRole = await GetById(
            userPublicationRoleId: userPublicationRoleId,
            cancellationToken: cancellationToken
        );

        if (userPublicationRole is null)
        {
            throw new InvalidOperationException($"No User Publication Role found with ID {userPublicationRoleId}.");
        }

        userPublicationRole.EmailSent = dateSent ?? DateTimeOffset.UtcNow;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }
}
