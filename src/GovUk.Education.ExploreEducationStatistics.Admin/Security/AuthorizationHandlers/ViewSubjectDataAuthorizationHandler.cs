using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSubjectDataAuthorizationHandler : CompoundAuthorizationHandler<
        ViewSubjectDataRequirement, Subject>
    {
        public ViewSubjectDataAuthorizationHandler(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPreReleaseService preReleaseService,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository) : base(
            new ViewSubjectDataForPublishedReleasesAuthorizationHandler(statisticsDbContext),
            new SubjectBelongsToViewableReleaseAuthorizationHandler(contentDbContext,
                statisticsDbContext,
                preReleaseService,
                userPublicationRoleRepository,
                userReleaseRoleRepository))
        {
        }

        public class SubjectBelongsToViewableReleaseAuthorizationHandler : AuthorizationHandler<
            ViewSubjectDataRequirement, Subject>
        {
            private readonly ContentDbContext _contentDbContext;
            private readonly StatisticsDbContext _statisticsDbContext;
            private readonly IPreReleaseService _preReleaseService;
            private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
            private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

            public SubjectBelongsToViewableReleaseAuthorizationHandler(
                ContentDbContext contentDbContext,
                StatisticsDbContext statisticsDbContext,
                IPreReleaseService preReleaseService,
                IUserPublicationRoleRepository userPublicationRoleRepository,
                IUserReleaseRoleRepository userReleaseRoleRepository)
            {
                _contentDbContext = contentDbContext;
                _statisticsDbContext = statisticsDbContext;
                _preReleaseService = preReleaseService;
                _userPublicationRoleRepository = userPublicationRoleRepository;
                _userReleaseRoleRepository = userReleaseRoleRepository;
            }

            protected override async Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ViewSubjectDataRequirement requirement,
                Subject subject)
            {
                var linkedReleases = _statisticsDbContext
                    .ReleaseSubject
                    .Include(r => r.Release)
                    .Where(r => r.SubjectId == subject.Id)
                    .Select(r => r.Release);

                var viewSpecificReleaseHandler = new
                    ViewSpecificReleaseAuthorizationHandler(
                        _userPublicationRoleRepository,
                        _userReleaseRoleRepository,
                        _preReleaseService);

                foreach (var release in linkedReleases)
                {
                    var contentRelease = GetContentRelease(_contentDbContext, release);
                    var delegatedContext = new AuthorizationHandlerContext(
                        new[] {new ViewReleaseRequirement()}, context.User, contentRelease);

                    await viewSpecificReleaseHandler.HandleAsync(delegatedContext);

                    if (delegatedContext.HasSucceeded)
                    {
                        context.Succeed(requirement);
                        break;
                    }
                }
            }
        }

        private static Content.Model.Release GetContentRelease(ContentDbContext context, Release release)
        {
            return context.Releases.First(r => r.Id == release.Id);
        }
    }
}
