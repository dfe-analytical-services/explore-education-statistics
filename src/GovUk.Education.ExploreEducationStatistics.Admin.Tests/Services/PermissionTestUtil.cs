using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.Core.Internal;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class PermissionTestUtil
    {
        public static async void AssertSecurityPoliciesChecked<TProtectedResource, TReturn, TService>(
            Func<TService, Task<Either<ActionResult, TReturn>>> protectedAction, 
            TProtectedResource resource, 
            Mock<IUserService> userService,
            TService service,
            params SecurityPolicies[] policies)
        {
            policies.ToList().ForEach(policy => 
                userService
                    .Setup(s => s.MatchesPolicy(resource, policy))
                    .ReturnsAsync(policy != policies.Last()));
            
            var result = await protectedAction.Invoke(service);

            AssertForbidden(result);
            
            policies.ToList().ForEach(policy =>
                userService.Verify(s => s.MatchesPolicy(resource, policy)));
            
            userService.VerifyNoOtherCalls();
        }

        public static void AssertPolicyEnforcedAtClassLevel<TClass>(SecurityPolicies policy)
        {
            var policyAttribute = typeof(TClass).GetAttribute<AuthorizeAttribute>();
            Assert.NotNull(policyAttribute);
            Assert.Equal(policy.ToString(), policyAttribute.Policy);

            var publicMethods = typeof(TClass).GetMethods(BindingFlags.Public);
            publicMethods.ToList().ForEach(method => Assert.Null(method.GetAttribute<AuthorizeAttribute>()));
        }

        private static void AssertForbidden<T>(Either<ActionResult,T> result)
        {
            Assert.NotNull(result);
            Assert.True(result.IsLeft);
            Assert.IsAssignableFrom<ForbidResult>(result.Left);
        }
    }
}