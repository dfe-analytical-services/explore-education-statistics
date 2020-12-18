using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public class PolicyCheckBuilder<TPolicy> where TPolicy : Enum
    {
        private readonly Mock<IUserService> _userService;

        public PolicyCheckBuilder(Mock<IUserService> userService = null)
        {
            _userService = userService ?? new Mock<IUserService>();
        }

        public PolicyCheckBuilder<TPolicy> ExpectCheck(TPolicy policy, bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public PolicyCheckBuilder<TPolicy> ExpectCheckToFail(TPolicy policy)
        {
            return ExpectCheck(policy, false);
        }

        public PolicyCheckBuilder<TPolicy> ExpectResourceCheck(
            object resource,
            TPolicy policy,
            bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(resource, policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public PolicyCheckBuilder<TPolicy> ExpectResourceCheckToFail(object resource, TPolicy policy)
        {
            return ExpectResourceCheck(resource, policy, false);
        }

        public Mock<IUserService> GetUserServiceMock()
        {
            return _userService;
        }

        public async void AssertForbidden<T>(Func<Mock<IUserService>, Task<Either<ActionResult, T>>> action)
        {
            var result = await action.Invoke(_userService);

            PermissionTestUtils.AssertForbidden(result);

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