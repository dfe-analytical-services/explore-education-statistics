using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers
{
    public class ViewSubjectDataForReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSubjectDataForPublishedReleasesAuthorizationHandler : EntityAuthorizationHandler<
        ViewSubjectDataForReleaseRequirement, Release>
    {
        public ViewSubjectDataForPublishedReleasesAuthorizationHandler() : base(ctx => ctx.Entity.Live)
        {
        }
    }
}