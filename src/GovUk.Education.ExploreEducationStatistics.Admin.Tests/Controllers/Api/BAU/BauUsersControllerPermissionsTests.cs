using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.BAU;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.BAU
{
    public class BauUsersControllerPermissionsTests
    {
        [Fact]
        public void PolicyAtControllerLevel()
        {
            AssertPolicyEnforcedAtClassLevel<BauUsersController>(SecurityPolicies.CanManageUsersOnSystem);
        }
    }
}