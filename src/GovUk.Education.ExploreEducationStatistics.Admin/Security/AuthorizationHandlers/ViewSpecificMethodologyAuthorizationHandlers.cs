#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSpecificMethodologyAuthorizationHandler :
        AuthorizationHandler<ViewSpecificMethodologyRequirement, MethodologyVersion>
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;
        private readonly IPreReleaseService _preReleaseService;

        public ViewSpecificMethodologyAuthorizationHandler(
            ContentDbContext contentDbContext,
            IMethodologyRepository methodologyRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository,
            IPreReleaseService preReleaseService)
        {
            _contentDbContext = contentDbContext;
            _methodologyRepository = methodologyRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
            _preReleaseService = preReleaseService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ViewSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            // If the user has a global Claim that allows them to access any Methodology, allow it.
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

            // If the user is a Publication Owner of the Publication that owns this Methodology, they can view it.
            if (await _userPublicationRoleRepository.IsUserPublicationOwner(context.User.GetUserId(),
                owningPublication.Id))
            {
                context.Succeed(requirement);
                return;
            }

            // If the user is an Editor (Contributor, Lead) or an Approver of the latest (Live or non-Live) Release
            // of the owning Publication of this Methodology, they can view it.
            if (await _userReleaseRoleRepository.IsUserEditorOrApproverOnLatestRelease(
                context.User.GetUserId(),
                owningPublication.Id))
            {
                context.Succeed(requirement);
            }

            // If the user is an PrereleaseViewer of the latest non-Live, Approved Release of the owning Publication
            // of this Methodology, and the methodology is approved, and the latest release under that publication
            // is within the prerelease time window, they can view it
            if (methodologyVersion.Approved)
            {
                var publicationIds = await _methodologyRepository
                    .GetAllPublicationIds(methodologyVersion.MethodologyId);
                foreach (var publicationId in publicationIds)
                {
                    if (await _userReleaseRoleRepository.IsUserPrereleaseViewerOnLatestPreReleaseRelease(
                            context.User.GetUserId(),
                            publicationId))
                    {
                        var publication = await _contentDbContext.Publications
                            .Include(p => p.Releases)
                            .SingleAsync(p => p.Id == publicationId);
                        var latestRelease = publication.LatestRelease();
                        if (latestRelease != null
                            && _preReleaseService
                                .GetPreReleaseWindowStatus(latestRelease, DateTime.UtcNow)
                                .Access == PreReleaseAccess.Within)
                        {
                            context.Succeed(requirement);
                            break;
                        }
                    }
                }
            }
        }
    }
}
