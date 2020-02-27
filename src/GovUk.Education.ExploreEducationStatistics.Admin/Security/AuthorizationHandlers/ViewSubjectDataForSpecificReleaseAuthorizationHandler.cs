using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSubjectDataForSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<
        ViewSubjectDataForReleaseRequirement, Release>
    {
        public ViewSubjectDataForSpecificReleaseAuthorizationHandler(ContentDbContext context,
            IPreReleaseService preReleaseService) : base(
            new ViewSubjectDataForPublishedReleasesAuthorizationHandler(),
            new DelegatingAuthorizationHandler<ViewSubjectDataForReleaseRequirement, Release, ViewSpecificReleaseRequirement, Content.Model.Release>(
                new ViewSpecificReleaseAuthorizationHandler(context, preReleaseService), release => GetContentRelease(context, release)))
        { }

        private static Content.Model.Release GetContentRelease(ContentDbContext context, Release release)
        {
            return context.Releases.First(r => r.Id == release.Id);
        }
    }
}