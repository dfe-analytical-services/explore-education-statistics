using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public static class PermissionTestUtils
    {
        public static PolicyCheckBuilder<T> PolicyCheckBuilder<T>(Mock<IUserService> userService = null)
            where T : Enum
        {
            return new PolicyCheckBuilder<T>(userService);
        }

        public static void AssertForbidden<T>(Either<ActionResult,T> result)
        {
            Assert.NotNull(result);
            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<ForbidResult>(result.Left);
        }
    }
}
