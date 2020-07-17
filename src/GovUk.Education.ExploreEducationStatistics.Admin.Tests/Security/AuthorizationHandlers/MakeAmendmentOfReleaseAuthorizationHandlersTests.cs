using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.MakeAmendmentOfSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanMakeAmendmentOfAllReleasesAuthorizationHandler()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                // Assert that no users can amend a non-Live Release
                AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    new CanMakeAmendmentOfAllReleasesAuthorizationHandler(context));
            }
        }

        [Fact]
        public void 
            MakeAmendmentOfSpecificReleaseCanMakeAmendmentOfAllReleasesAuthorizationHandler_LiveAndLatestVersion()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            
            var previousVersion = new Release
            {
                Id = new Guid("08f7c576-6e52-44ad-a98f-5215394d9abf"),
                PreviousVersionId = new Guid("08f7c576-6e52-44ad-a98f-5215394d9abf"),
                Publication = publication,
                Published = DateTime.UtcNow
            };
            
            var latestVersion = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = previousVersion.Id,
                Published = DateTime.UtcNow
            };

            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.AddRange(previousVersion, latestVersion);
                context.SaveChanges();
                
                // Assert that any users with the "MakeAmendmentOfAllReleases" claim can amend an arbitrary Live and latest Release
                // (and no other claim allows this)
                AssertReleaseHandlerSucceedsWithCorrectClaims<MakeAmendmentOfSpecificReleaseRequirement>(
                    new CanMakeAmendmentOfAllReleasesAuthorizationHandler(context),
                    latestVersion, MakeAmendmentsOfAllReleases);
            }
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler_ReleaseNotYetLive()
        {
            // Assert that no User Release roles will allow an Amendment to be made when the Release is not yet Live
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext));
        }

        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler_LiveAndLatestVersion()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };
            
            var previousVersion = new Release
            {
                Id = new Guid("08f7c576-6e52-44ad-a98f-5215394d9abf"),
                PreviousVersionId = new Guid("08f7c576-6e52-44ad-a98f-5215394d9abf"),
                Publication = publication,
                Published = DateTime.UtcNow
            };
            
            var latestVersion = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                PreviousVersionId = previousVersion.Id,
                Published = DateTime.UtcNow
            };

            // Assert that users with an editor role on the release can amend it if it is Live and the latest version
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<MakeAmendmentOfSpecificReleaseRequirement>(
                contentDbContext =>
                {
                    contentDbContext.Releases.AddRange(previousVersion, latestVersion);
                    contentDbContext.SaveChanges();
                    return new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext);
                },
                latestVersion,
                ReleaseRole.Contributor, ReleaseRole.Approver, ReleaseRole.Lead);
        }
    }
}