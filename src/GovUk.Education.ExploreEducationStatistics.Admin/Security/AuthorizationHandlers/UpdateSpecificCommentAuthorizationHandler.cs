using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificCommentRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificCommentAuthorizationHandler :
        EntityAuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
    {
        public UpdateSpecificCommentAuthorizationHandler() :
            base(ctx => ctx.User.GetUserId() == ctx.Entity.CreatedById)
        {
            // TODO EES-18 Needs to check user has permission to update the release including checking that the Release isn't already approved
        }
    }
}