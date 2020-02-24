using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataBlockServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };
        
        private readonly DataBlock _dataBlock = new DataBlock()
        {
            Id = Guid.NewGuid()
        };
        
        [Fact]
        public void CreateAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                service.CreateAsync(_release.Id, new CreateDataBlockViewModel()), CanUpdateSpecificRelease);
        }
        
        [Fact]
        public void UpdateAsync()
        {
            AssertSecurityPoliciesChecked(service => 
                service.UpdateAsync(_dataBlock.Id, new UpdateDataBlockViewModel()), CanUpdateSpecificRelease);
        }
        
        [Fact]
        public void DeleteAsync()
        {
            AssertSecurityPoliciesChecked(service => service.DeleteAsync(_dataBlock.Id), CanUpdateSpecificRelease);
        }
        
        private void AssertSecurityPoliciesChecked<T>(
            Func<DataBlockService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (userService, persistenceHelper) = Mocks();

            using (var context = DbUtils.InMemoryApplicationDbContext())
            {
                context.Add(new ReleaseContentBlock
                    {
                        Release = _release,
                        ContentBlockId = _dataBlock.Id
                    });
                context.SaveChanges();
                
                var service = new DataBlockService(context, AdminMapper(), 
                    persistenceHelper.Object, userService.Object);

                PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _release, userService, service, policies);
            }
        }

        private (
            Mock<IUserService>, 
            Mock<IPersistenceHelper<ContentDbContext>>) Mocks()
        {
            var persistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(persistenceHelper, _release.Id, _release);
            MockUtils.SetupCall(persistenceHelper, _dataBlock.Id, _dataBlock);

            return (
                new Mock<IUserService>(), 
                persistenceHelper);
        }
    }
}