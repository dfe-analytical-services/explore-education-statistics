using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.UpdateSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanUpdateAllReleasesAuthorizationHandler_ReleaseUnpublished()
        {
            // Assert that any users with the "UpdateAllReleases" claim can update an arbitrary 
            // Release if it is not published (and no other claim allows this)
            GetEnumValues<ReleaseStatus>().ForEach(status =>
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Status = status
                };

                AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                    new CanUpdateAllReleasesAuthorizationHandler(),
                    release,
                    UpdateAllReleases
                );
            });
        }

        [Fact]
        public void CanUpdateAllReleasesAuthorizationHandler_ReleasePublished()
        {
            // Assert that no usres can update an arbitrary Release 
            // if it is published (and no other claim allows this)
            GetEnumValues<ReleaseStatus>().ForEach(status =>
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Status = status,
                    Published = DateTime.Now
                };

                AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificReleaseRequirement>(
                    new CanUpdateAllReleasesAuthorizationHandler(),
                    release
                );
            });
        }        
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler()
        {
            GetEnumValues<ReleaseStatus>().ForEach(status =>
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    Status = status
                };

                // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can update it
                // if it is not in Approved status (and no other role or Release status)
                if (status != ReleaseStatus.Approved)
                {
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseRequirement>(
                        contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                        release,
                        ReleaseRole.Contributor, 
                        ReleaseRole.Lead, 
                        ReleaseRole.Approver
                    );
                }
                else
                {
                    AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<UpdateSpecificReleaseRequirement>(
                        contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                        release
                    );
                }
            });
        }
    }
}