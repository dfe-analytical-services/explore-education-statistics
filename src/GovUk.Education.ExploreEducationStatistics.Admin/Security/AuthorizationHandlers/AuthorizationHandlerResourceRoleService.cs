using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class AuthorizationHandlerResourceRoleService
{
    private static readonly ReleaseRole[] ReleaseEditorRoles =
    {
        ReleaseRole.Contributor, 
        ReleaseRole.Lead
    };
    
    public static readonly ReleaseRole[] UnrestrictedReleaseViewerRoles =
    {
        ReleaseRole.Viewer, 
        ReleaseRole.Contributor,
        ReleaseRole.Approver, 
        ReleaseRole.Lead
    };

    public static readonly List<ReleaseRole> ReleaseEditorAndApproverRoles = 
        ReleaseEditorRoles
            .Append(ReleaseRole.Approver)
            .ToList();
    
    private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
    private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

    public AuthorizationHandlerResourceRoleService(
        IUserReleaseRoleRepository userReleaseRoleRepository, 
        IUserPublicationRoleRepository userPublicationRoleRepository)
    {
        _userReleaseRoleRepository = userReleaseRoleRepository;
        _userPublicationRoleRepository = userPublicationRoleRepository;
    }

    public Task<bool> HasRolesOnPublicationOrRelease(
        Guid userId,
        Guid publicationId,
        Guid releaseId,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        return HasRolesOnPublicationOrRelease(
            userId, 
            publicationId, 
            () => Task.FromResult((Guid?) releaseId), 
            publicationRoles, 
            releaseRoles);
    }
    
    public async Task<bool> HasRolesOnPublicationOrRelease(
        Guid userId,
        Guid publicationId,
        Func<Task<Guid?>> releaseIdSupplier,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);
            
        if (usersPublicationRoles.Any(publicationRoles.Contains))
        {
            return true;
        }

        var releaseId = await releaseIdSupplier.Invoke();

        if (releaseId == null)
        {
            return false;
        }
        
        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndRelease(userId, releaseId.Value);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }
    
    public async Task<bool> HasRolesOnPublicationOrAnyRelease(
        Guid userId,
        Guid publicationId,
        IEnumerable<PublicationRole> publicationRoles,
        IEnumerable<ReleaseRole> releaseRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);
            
        if (usersPublicationRoles.Any(publicationRoles.Contains))
        {
            return true;
        }

        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }
    
    public async Task<bool> HasRolesOnPublication(
        Guid userId,
        Guid publicationId,
        params PublicationRole[] publicationRoles)
    {
        var usersPublicationRoles = await _userPublicationRoleRepository
            .GetAllRolesByUserAndPublication(userId, publicationId);

        return usersPublicationRoles.Any(publicationRoles.Contains);
    }
    
    public async Task<bool> HasRolesOnRelease(
        Guid userId,
        Guid releaseId,
        params ReleaseRole[] releaseRoles)
    {
        var usersReleaseRoles = await _userReleaseRoleRepository
            .GetAllRolesByUserAndRelease(userId, releaseId);

        return usersReleaseRoles.Any(releaseRoles.Contains);
    }
}