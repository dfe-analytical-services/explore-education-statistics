#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers
{
    public class ViewSubjectDataForPublishedReleasesAuthorizationHandler : AuthorizationHandler<
        ViewSubjectDataRequirement, ReleaseSubject>
    {
        private readonly IReleaseRepository _releaseRepository;

        public ViewSubjectDataForPublishedReleasesAuthorizationHandler(IReleaseRepository releaseRepository)
        {
            _releaseRepository = releaseRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            ViewSubjectDataRequirement requirement,
            ReleaseSubject releaseSubject)
        {
            if (await _releaseRepository.IsLatestPublishedReleaseVersion(releaseSubject.ReleaseId))
            {
                authContext.Succeed(requirement);
            }
        }
    }
}
