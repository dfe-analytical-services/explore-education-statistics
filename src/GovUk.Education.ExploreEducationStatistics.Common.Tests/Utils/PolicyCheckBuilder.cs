using System;
using System.Linq.Expressions;
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

        public PolicyCheckBuilder<TPolicy> SetupCheck(TPolicy policy, bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public PolicyCheckBuilder<TPolicy> ExpectCheckToFail(TPolicy policy)
        {
            return SetupCheck(policy, false);
        }

        public PolicyCheckBuilder<TPolicy> SetupResourceCheck(
            object resource,
            TPolicy policy,
            bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(resource, policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public PolicyCheckBuilder<TPolicy> SetupResourceCheckWithMatcher<T>(
            Expression<Func<T, bool>> matcher,
            TPolicy policy,
            bool checkResult = true)
        {
            _userService
                .Setup(s => s.MatchesPolicy(It.Is(matcher), policy))
                .ReturnsAsync(checkResult);

            return this;
        }

        public PolicyCheckBuilder<TPolicy> SetupResourceCheckToFail(object resource, TPolicy policy)
        {
            return SetupResourceCheck(resource, policy, false);
        }

        public PolicyCheckBuilder<TPolicy> SetupResourceCheckToFailWithMatcher<T>(Expression<Func<T, bool>> matcher, TPolicy policy)
        {
            return SetupResourceCheckWithMatcher(matcher, policy, false);
        }

        public Mock<IUserService> GetUserServiceMock()
        {
            return _userService;
        }

        public async Task AssertForbidden<T>(Func<Mock<IUserService>, Task<Either<ActionResult, T>>> action)
        {
            var result = await action.Invoke(_userService);

            MockUtils.VerifyAllMocks(_userService);

            PermissionTestUtils.AssertForbidden(result);
        }

        public async Task AssertSuccess<T>(Func<Mock<IUserService>, Task<Either<ActionResult, T>>> action)
        {
            var result = await action.Invoke(_userService);

            MockUtils.VerifyAllMocks(_userService);

            Assert.NotNull(result);
            Assert.True(result.IsRight);
        }

        public async Task AssertSuccess<T>(Func<Mock<IUserService>, Task<T>> action)
        {
            await action.Invoke(_userService);
            MockUtils.VerifyAllMocks(_userService);
        }
    }
}
