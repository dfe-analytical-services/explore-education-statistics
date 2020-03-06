using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseStatusServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public void GetReleaseStatusesAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                    service.GetReleaseStatusesAsync(_release.Id),  
                _release,
                CanViewSpecificRelease);
        }
        
        private void AssertSecurityPoliciesChecked<T, TEntity>(
            Func<ReleaseStatusService, Task<Either<ActionResult, T>>> protectedAction, TEntity protectedEntity, params SecurityPolicies[] policies)
            where TEntity : class
        {
            var (mapper, userService, persistenceHelper, tableStorageService) = Mocks();

            var service = new ReleaseStatusService(mapper.Object, userService.Object, 
                persistenceHelper.Object, tableStorageService.Object);
            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, protectedEntity, userService, service, policies);
        }
        
        private (
            Mock<IMapper>,
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<ITableStorageService>) Mocks()
        {
            return (
                new Mock<IMapper>(), 
                new Mock<IUserService>(), 
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release),
                new Mock<ITableStorageService>());
        }
    }
}