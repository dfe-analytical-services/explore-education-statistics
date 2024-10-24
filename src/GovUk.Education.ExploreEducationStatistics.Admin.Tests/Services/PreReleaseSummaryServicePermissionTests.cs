#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseSummaryServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void GetPreReleaseSummaryViewModelAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.GetPreReleaseSummaryViewModelAsync(_releaseVersion.Id),
                _releaseVersion,
                CanViewSpecificPreReleaseSummary);
        }

        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<PreReleaseSummaryService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity,
            params SecurityPolicies[] policies)
            where TEntity : class
        {
            var (persistenceHelper, userService) = Mocks();

            var service = new PreReleaseSummaryService(persistenceHelper.Object, userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, userService, service,
                policies);
        }

        private (
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IUserService>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(persistenceHelper, _releaseVersion.Id, _releaseVersion);

            return (persistenceHelper,
                MockUtils.AlwaysTrueUserService());
        }
    }
}
