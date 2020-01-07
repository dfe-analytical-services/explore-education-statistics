using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftAuthorizationHandlersTests
    {
        [Fact]
        public void MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler()
        {
            AssertHandlerSucceedsWithCorrectClaims(
                new MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler(), MarkAllReleasesAsDraft);
        }
        
        [Fact]
        public void MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler()
        {
            AssertHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                Enum.GetValues(typeof(ReleaseRole)).Cast<ReleaseRole>().ToArray());
        }
    }
}