#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public abstract class UserResourceRoleRepositoryBase<TParent, TResourceRole, TResource, TRoleEnum>(
    ContentDbContext contentDbContext)
    where TResourceRole : ResourceRole<TRoleEnum, TResource>
    where TResource : class
    where TRoleEnum : Enum
{
    protected readonly ContentDbContext ContentDbContext = contentDbContext;

    protected async Task<TResourceRole> Create(Guid userId, Guid resourceId, TRoleEnum role, Guid createdById)
    {
        var newResourceRole = NewResourceRole(userId, resourceId, role, createdById);

        await ContentDbContext.Set<TResourceRole>().AddAsync(newResourceRole);
        await ContentDbContext.SaveChangesAsync();

        return newResourceRole;
    }

    public async Task<TResourceRole> CreateIfNotExists(Guid userId, Guid resourceId, TRoleEnum role,
        Guid createdById)
    {
        var resourceRole = await GetResourceRole(userId, resourceId, role);
        if (resourceRole == null)
        {
            return await Create(userId, resourceId, role, createdById);
        }

        return resourceRole;
    }

    public async Task CreateManyIfNotExists(
        List<Guid> userIds,
        Guid resourceId,
        TRoleEnum role,
        Guid createdById)
    {
        var userIdsAlreadyHaveRole = await
            GetResourceRolesQueryByResourceId(resourceId)
            .Where(urr => urr.Role.Equals(role) && userIds.Contains(urr.UserId))
            .Select(urr => urr.UserId)
            .ToListAsync();

        var newResourceRoles = userIds
            .Except(userIdsAlreadyHaveRole)
            .Select(userId => NewResourceRole(userId, resourceId, role, createdById))
            .ToList();

        await ContentDbContext.Set<TResourceRole>().AddRangeAsync(newResourceRoles);
        await ContentDbContext.SaveChangesAsync();
    }

    public async Task CreateManyIfNotExists(
        Guid userId,
        List<Guid> resourceIds,
        TRoleEnum role,
        Guid createdById)
    {
        var alreadyExistingReleaseIds = await
            GetResourceRolesQueryByResourceIds(resourceIds)
            .Where(urr =>
                urr.UserId == userId
                && urr.Role.Equals(role))
            .Select(urr => urr.ResourceId)
            .ToListAsync();

        var newTResourceRoles = resourceIds
            .Except(alreadyExistingReleaseIds)
            .Select(resourceId => NewResourceRole(userId, resourceId, role, createdById))
            .ToList();

        await ContentDbContext.Set<TResourceRole>().AddRangeAsync(newTResourceRoles);
        await ContentDbContext.SaveChangesAsync();
    }

    protected async Task Remove(TResourceRole resourceRole, CancellationToken cancellationToken = default)
    {
        ContentDbContext.Remove(resourceRole);

        await ContentDbContext.SaveChangesAsync(cancellationToken);
    }

    protected async Task RemoveMany(
        IReadOnlyList<TResourceRole> resourceRoles,
        CancellationToken cancellationToken = default)
    {
        if (!resourceRoles.Any())
        {
            return;
        }

        ContentDbContext.RemoveRange(resourceRoles);

        await ContentDbContext.SaveChangesAsync(cancellationToken);
    }

    protected async Task<List<TRoleEnum>> GetDistinctResourceRolesByUser(Guid userId)
    {
        return await
            ContentDbContext
            .Set<TResourceRole>()
            .AsQueryable()
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .Distinct()
            .ToListAsync();
    }

    // The optional parameter 'includeNewPermissionsSystemRoles' is purely here to assist with the
    // code that SYNCS the creation and removal of the NEW permissions system publication role with the
    // OLD roles. It is not ideal to have it here, as this class is abstract and is not specific to
    // publication roles. However, it was put here as a temporary parameter that will be removed
    // in EES-6196, when we no longer have to cater for the old roles. Due to it being a short-lived temporary
    // parameter, it was not worth refactoring this class and the repositories inheriting from it.
    protected async Task<List<TRoleEnum>> GetAllResourceRolesByUserAndResource(
        Guid userId, 
        Guid resourceId, 
        bool includeNewPermissionsSystemRoles = false)
    {
        return await 
            GetResourceRolesQueryByResourceId(resourceId, includeNewPermissionsSystemRoles)
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .Distinct()
            .ToListAsync();
    }

    // The optional parameter 'includeNewPermissionsSystemRoles' is purely here to assist with the
    // code that SYNCS the creation and removal of the NEW permissions system publication role with the
    // OLD roles. It is not ideal to have it here, as this class is abstract and is not specific to
    // publication roles. However, it was put here as a temporary parameter that will be removed
    // in EES-6196, when we no longer have to cater for the old roles. Due to it being a short-lived temporary
    // parameter, it was not worth refactoring this class and the repositories inheriting from it.
    protected async Task<TResourceRole?> GetResourceRole(
        Guid userId, 
        Guid resourceId, 
        TRoleEnum role,
        bool includeNewPermissionsSystemRoles = false)
    {
        return await 
            GetResourceRolesQueryByResourceId(resourceId, includeNewPermissionsSystemRoles)
            .SingleOrDefaultAsync(r =>
                r.UserId == userId &&
                r.Role.Equals(role));
    }

    protected async Task<List<TResourceRole>> ListResourceRoles(
        Guid resourceId,
        TRoleEnum[]? rolesToInclude)
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumsArray<TRoleEnum>();

        return await
            GetResourceRolesQueryByResourceId(resourceId)
            .Include(urr => urr.User)
            .Where(urr => rolesToCheck.Contains(urr.Role))
            .ToListAsync();
    }

    protected async Task<bool> UserHasRoleOnResource(Guid userId, Guid resourceId, TRoleEnum role)
    {
        return await
            GetResourceRolesQueryByResourceId(resourceId)
            .AnyAsync(r =>
                r.UserId == userId &&
                r.Role.Equals(role));
    }

    protected async Task<bool> UserHasRoleOnResource(string email, Guid resourceId, TRoleEnum role)
    {
        return await
            GetResourceRolesQueryByResourceId(resourceId)
            .AnyAsync(r =>
                r.User.Email.ToLower().Equals(email.ToLower()) &&
                r.Role.Equals(role));
    }

    protected static TResourceRole NewResourceRole(Guid userId, Guid resourceId, TRoleEnum role, Guid createdById)
    {
        var resourceRole = Activator.CreateInstance<TResourceRole>();
        resourceRole.UserId = userId;
        resourceRole.ResourceId = resourceId;
        resourceRole.Role = role;
        resourceRole.Created = DateTime.UtcNow;
        resourceRole.CreatedById = createdById;
        return resourceRole;
    }

    // The optional parameter 'includeNewPermissionsSystemRoles' is purely here to assist with the
    // code that SYNCS the creation and removal of the NEW permissions system publication role with the
    // OLD roles. It is not ideal to have it here, as this class is abstract and is not specific to
    // publication roles. However, it was put here as a temporary parameter that will be removed
    // in EES-6196, when we no longer have to cater for the old roles. Due to it being a short-lived temporary
    // parameter, it was not worth refactoring this class and the repositories inheriting from it.
    protected abstract IQueryable<TResourceRole> GetResourceRolesQueryByResourceId(Guid resourceId, bool includeNewPermissionsSystemRoles = false);

    protected abstract IQueryable<TResourceRole> GetResourceRolesQueryByResourceIds(List<Guid> resourceIds);
}
