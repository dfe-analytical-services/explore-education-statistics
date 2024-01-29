#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class MakeAmendmentOfSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
    {
        private readonly AuthorizationHandlerService _authorizationHandlerService;
        private readonly IReleaseRepository _releaseRepository;

        public MakeAmendmentOfSpecificReleaseAuthorizationHandler(
            AuthorizationHandlerService authorizationHandlerService,
            IReleaseRepository releaseRepository)
        {
            _authorizationHandlerService = authorizationHandlerService;
            _releaseRepository = releaseRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MakeAmendmentOfSpecificReleaseRequirement requirement,
            Release release)
        {
            if (!release.Live)
            {
                return;
            }

            if (!await _releaseRepository.IsLatestReleaseVersion(release.Id))
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, MakeAmendmentsOfAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        release.PublicationId,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
