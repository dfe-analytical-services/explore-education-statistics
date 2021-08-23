using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    // Create
    public class CreateLegacyReleaseRequirement : IAuthorizationRequirement
    {}

    public class CreateLegacyReleaseAuthorizationHandler 
        : AuthorizationHandler<CreateLegacyReleaseRequirement, Publication>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public CreateLegacyReleaseAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CreateLegacyReleaseRequirement requirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, CreateAnyRelease))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles = await _userPublicationRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), publication.Id);

            if (ContainPublicationOwnerRole(publicationRoles))
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
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        public ViewLegacyReleaseAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
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

            var publicationRoles = await _userPublicationRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), legacyRelease.PublicationId);

            if (ContainPublicationOwnerRole(publicationRoles))
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
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        public UpdateLegacyReleaseAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
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

            var publicationRoles = await _userPublicationRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), legacyRelease.PublicationId);

            if (ContainPublicationOwnerRole(publicationRoles))
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
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        public DeleteLegacyReleaseAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
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

            var publicationRoles = await _userPublicationRoleRepository
                .GetAllRolesByUser(context.User.GetUserId(), legacyRelease.PublicationId);

            if (ContainPublicationOwnerRole(publicationRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
