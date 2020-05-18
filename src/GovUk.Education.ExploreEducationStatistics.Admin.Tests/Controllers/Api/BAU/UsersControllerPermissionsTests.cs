using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.UserManagement;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.BAU
{
    public class UsersControllerPermissionsTests
    {
        [Fact]
        public void PolicyAtControllerLevel()
        {
            AssertPolicyEnforcedAtClassLevel<UsersController>(SecurityPolicies.CanManageUsersOnSystem);
        }
    }
}