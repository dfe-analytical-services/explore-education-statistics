#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    // Create
    public class ManageLegacyReleasesRequirement : IAuthorizationRequirement
    {}

    public class ManageLegacyReleasesAuthorizationHandler 
        : AuthorizationHandler<ManageLegacyReleasesRequirement, Publication>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public ManageLegacyReleasesAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageLegacyReleasesRequirement requirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, CreateAnyRelease))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        publication.Id,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }

    // View
    public class ViewLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class ViewLegacyReleaseAuthorizationHandler 
        : AuthorizationHandler<ViewLegacyReleaseRequirement, LegacyRelease>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;
        
        public ViewLegacyReleaseAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ViewLegacyReleaseRequirement requirement,
            LegacyRelease legacyRelease)
        {
            if (SecurityUtils.HasClaim(context.User, AccessAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        legacyRelease.PublicationId,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }

    // Update
    public class UpdateLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class UpdateLegacyReleaseAuthorizationHandler 
        : AuthorizationHandler<UpdateLegacyReleaseRequirement, LegacyRelease>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;
        
        public UpdateLegacyReleaseAuthorizationHandler(AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateLegacyReleaseRequirement requirement,
            LegacyRelease legacyRelease)
        {
            if (SecurityUtils.HasClaim(context.User, UpdateAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        legacyRelease.PublicationId,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }

    // Delete
    public class DeleteLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class DeleteLegacyReleaseAuthorizationHandler 
        : AuthorizationHandler<DeleteLegacyReleaseRequirement, LegacyRelease>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;
        
        public DeleteLegacyReleaseAuthorizationHandler(AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DeleteLegacyReleaseRequirement requirement,
            LegacyRelease legacyRelease)
        {
            if (SecurityUtils.HasClaim(context.User, UpdateAllReleases))
            {
                context.Succeed(requirement);
                return;
            }

            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        legacyRelease.PublicationId,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
