using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSubjectDataForSpecificReleaseAuthorizationHandler : AuthorizationHandler<
        CanViewSubjectDataForReleaseRequirement, Release>
    {
        private readonly ContentDbContext _context;
        private readonly IPreReleaseService _preReleaseService;

        public ViewSubjectDataForSpecificReleaseAuthorizationHandler(ContentDbContext context,
            IPreReleaseService preReleaseService)
        {
            _context = context;
            _preReleaseService = preReleaseService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanViewSubjectDataForReleaseRequirement requirement,
            Release release)
        {
            await new CanViewSubjectDataForReleaseAuthorizationHandler().HandleAsync(context);

            if (!context.HasSucceeded)
            {
                await ViewSpecificReleaseAuthorizationHandler(context, requirement, release);
            }
        }

        private async Task ViewSpecificReleaseAuthorizationHandler(AuthorizationHandlerContext context,
            IAuthorizationRequirement requirement, Release release)
        {
            var contentRelease = _context.Releases.First(r => r.Id == release.Id);
            
            var viewSpecificReleaseAuthorizationContext = new AuthorizationHandlerContext(
                new[] {new ViewSpecificReleaseRequirement()}, context.User, contentRelease);
            
            await new ViewSpecificReleaseAuthorizationHandler(_context, _preReleaseService).HandleAsync(viewSpecificReleaseAuthorizationContext);
            if (viewSpecificReleaseAuthorizationContext.HasSucceeded)
            {
                context.Succeed(requirement);
            }
        }
    }
}