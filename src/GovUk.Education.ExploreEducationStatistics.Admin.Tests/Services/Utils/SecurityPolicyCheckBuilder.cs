using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Utils
{
    public class SecurityPolicyCheckBuilder
    {
        private readonly Mock<IUserService> _userService = new Mock<IUserService>();

        public SecurityPolicyCheckBuilder ExpectCheck(SecurityPolicies policy, bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public SecurityPolicyCheckBuilder ExpectResourceCheck(
            object resource,
            SecurityPolicies policy,
            bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(resource, policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public Mock<IUserService> GetUserServiceMock()
        {
            return _userService;
        }

        public async void AssertForbidden<T>(Func<Mock<IUserService>, Task<Either<ActionResult, T>>> action)
        {
            var result = await action.Invoke(_userService);

            PermissionTestUtil.AssertForbidden(result);

            _userService.VerifyAll();
            _userService.VerifyNoOtherCalls();
        }

        public async void AssertSuccess<T>(Func<Mock<IUserService>, Task<Either<ActionResult, T>>> action)
        {
            var result = await action.Invoke(_userService);

            Assert.NotNull(result);
            Assert.True(result.IsRight);

            _userService.VerifyAll();
            _userService.VerifyNoOtherCalls();
        }
    }
}