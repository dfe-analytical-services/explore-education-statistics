#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserReleaseRoleRepository :
    AbstractUserResourceRoleRepository<UserReleaseRole, ReleaseVersion, ReleaseRole>, IUserReleaseRoleRepository
{
    public UserReleaseRoleRepository(ContentDbContext contentDbContext) : base(contentDbContext)
    {
    }

    protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceId(Guid releaseVersionId)
    {
        return ContentDbContext
            .UserReleaseRoles
            .Where(role => role.ReleaseVersionId == releaseVersionId);
    }

    protected override IQueryable<UserReleaseRole> GetResourceRolesQueryByResourceIds(List<Guid> releaseVersionIds)
    {
        return ContentDbContext
            .UserReleaseRoles
            .Where(role => releaseVersionIds.Contains(role.ReleaseVersionId));
    }

    public async Task RemoveAllForPublication(Guid userId, Publication publication, ReleaseRole role,
        Guid deletedById)
    {
        ContentDbContext.Update(publication);
        await ContentDbContext
            .Entry(publication)
            .Collection(p => p.ReleaseVersions)
            .LoadAsync();
        var allReleaseVersionIds = publication
            .ReleaseVersions // Remove on previous release versions as well
            .Select(rv => rv.Id)
            .ToList();
        var userReleaseRoles = await ContentDbContext.UserReleaseRoles
            .AsQueryable()
            .Where(urr =>
                urr.UserId == userId
                && allReleaseVersionIds.Contains(urr.ReleaseVersionId)
                && urr.Role == role)
            .ToListAsync();
        await RemoveMany(userReleaseRoles, deletedById);
    }

    public Task<List<ReleaseRole>> GetDistinctRolesByUser(Guid userId)
    {
        return GetDistinctResourceRolesByUser(userId);
    }

    public Task<List<ReleaseRole>> GetAllRolesByUserAndRelease(Guid userId, Guid releaseVersionId)
    {
        return GetAllResourceRolesByUserAndResource(userId, releaseVersionId);
    }

    public Task<List<ReleaseRole>> GetAllRolesByUserAndPublication(Guid userId, Guid publicationId)
    {
        return ContentDbContext
            .UserReleaseRoles
            .Where(role => role.UserId == userId && role.ReleaseVersion.PublicationId == publicationId)
            .Select(role => role.Role)
            .Distinct()
            .ToListAsync();
    }

    public async Task<UserReleaseRole?> GetUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role)
    {
        return await GetResourceRole(userId, releaseVersionId, role);
    }

    public Task<bool> HasUserReleaseRole(Guid userId, Guid releaseVersionId, ReleaseRole role)
    {
        return UserHasRoleOnResource(userId, releaseVersionId, role);
    }

    public Task<bool> HasUserReleaseRole(string email, Guid releaseVersionId, ReleaseRole role)
    {
        return UserHasRoleOnResource(email, releaseVersionId, role);
    }

    public Task<List<UserReleaseRole>> ListUserReleaseRoles(Guid releaseVersionId, ReleaseRole[]? rolesToInclude)
    {
        return ListResourceRoles(releaseVersionId, rolesToInclude);
    }
}
