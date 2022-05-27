using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSubjectDataAuthorizationHandler : CompoundAuthorizationHandler<
        ViewSubjectDataRequirement, ReleaseSubject>
    {
        public ViewSubjectDataAuthorizationHandler(
            ContentDbContext contentDbContext,
            IPreReleaseService preReleaseService,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService) : base(
            new ViewSubjectDataForPublishedReleasesAuthorizationHandler(contentDbContext),
            new SubjectBelongsToViewableReleaseAuthorizationHandler(
                contentDbContext,
                preReleaseService,
                authorizationHandlerResourceRoleService))
        {
        }

        public class SubjectBelongsToViewableReleaseAuthorizationHandler : AuthorizationHandler<
            ViewSubjectDataRequirement, ReleaseSubject>
        {
            private readonly ContentDbContext _contentDbContext;
            private readonly IPreReleaseService _preReleaseService;
            private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

            public SubjectBelongsToViewableReleaseAuthorizationHandler(
                ContentDbContext contentDbContext,
                IPreReleaseService preReleaseService,
                AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
            {
                _contentDbContext = contentDbContext;
                _preReleaseService = preReleaseService;
                _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewSubjectDataRequirement requirement,
                ReleaseSubject releaseSubject)
            {
                var viewSpecificReleaseHandler = new
                    ViewSpecificReleaseAuthorizationHandler(
                        _preReleaseService,
                        _authorizationHandlerResourceRoleService);

                var contentRelease = await _contentDbContext
                    .Releases
                    .FirstAsync(release => release.Id == releaseSubject.ReleaseId);

                var delegatedContext = new AuthorizationHandlerContext(
                    new[] {new ViewReleaseRequirement()}, context.User, contentRelease);

                await viewSpecificReleaseHandler.HandleAsync(delegatedContext);

                if (delegatedContext.HasSucceeded)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
