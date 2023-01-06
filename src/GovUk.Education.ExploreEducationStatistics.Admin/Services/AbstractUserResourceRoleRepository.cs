#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public abstract class AbstractUserResourceRoleRepository<TResourceRole, TResource, TRoleEnum> 
    where TResourceRole : ResourceRole<TRoleEnum, TResource>
    where TResource : class
    where TRoleEnum : Enum
{
    protected readonly ContentDbContext ContentDbContext;

    protected AbstractUserResourceRoleRepository(ContentDbContext contentDbContext)
    {
        ContentDbContext = contentDbContext;
    }

    public async Task<TResourceRole> Create(Guid userId, Guid resourceId, TRoleEnum role, Guid createdById)
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

    public async Task Remove(TResourceRole resourceRole, Guid deletedById)
    {
        resourceRole.Deleted = DateTime.UtcNow;
        resourceRole.DeletedById = deletedById;
        ContentDbContext.Update(resourceRole);
        await ContentDbContext.SaveChangesAsync();
    }

    public async Task RemoveMany(List<TResourceRole> resourceRoles, Guid deletedById)
    {
        resourceRoles.ForEach(resourceRole =>
        {
            resourceRole.Deleted = DateTime.UtcNow;
            resourceRole.DeletedById = deletedById;
        });
        ContentDbContext.UpdateRange(resourceRoles);

        await ContentDbContext.SaveChangesAsync();
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

    protected async Task<List<TRoleEnum>> GetAllResourceRolesByUserAndResource(Guid userId, Guid resourceId)
    {
        return await 
            GetResourceRolesQueryByResourceId(resourceId)
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .Distinct()
            .ToListAsync();
    }

    protected async Task<TResourceRole?> GetResourceRole(Guid userId, Guid resourceId, TRoleEnum role)
    {
        return await 
            GetResourceRolesQueryByResourceId(resourceId)
            .SingleOrDefaultAsync(r =>
                r.UserId == userId &&
                r.Role.Equals(role));
    }

    protected async Task<List<TResourceRole>> ListResourceRoles(
        Guid resourceId, 
        TRoleEnum[]? rolesToInclude)
    {
        var rolesToCheck = rolesToInclude ?? EnumUtil.GetEnumValuesAsArray<TRoleEnum>();
        
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

    private static TResourceRole NewResourceRole(Guid userId, Guid resourceId, TRoleEnum role, Guid createdById)
    {
        var resourceRole = Activator.CreateInstance<TResourceRole>();
        resourceRole.UserId = userId;
        resourceRole.ResourceId = resourceId;
        resourceRole.Role = role;
        resourceRole.Created = DateTime.UtcNow;
        resourceRole.CreatedById = createdById;
        return resourceRole;
    }

    protected abstract IQueryable<TResourceRole> GetResourceRolesQueryByResourceId(Guid resourceId);
    
    protected abstract IQueryable<TResourceRole> GetResourceRolesQueryByResourceIds(List<Guid> resourceIds);
}