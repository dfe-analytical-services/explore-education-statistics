using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers
{
    public class CanViewSubjectDataForReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class CanViewSubjectDataForReleaseAuthorizationHandler : EntityAuthorizationHandler<
        CanViewSubjectDataForReleaseRequirement, Release>
    {
        public CanViewSubjectDataForReleaseAuthorizationHandler() : base(ctx => ctx.Entity.Live)
        {
        }
    }
}