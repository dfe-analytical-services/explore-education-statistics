using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class ReleaseNoteServicePermissionTests
    {
        private readonly Release _release = new Release
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void AddReleaseNoteAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.AddReleaseNoteAsync(
                        _release.Id,
                        new ReleaseNoteSaveRequest()),
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteReleaseNoteAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.DeleteReleaseNoteAsync(
                        _release.Id,
                        Guid.NewGuid()),
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateReleaseNoteAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.UpdateReleaseNoteAsync(
                        _release.Id,
                        Guid.NewGuid(),
                        new ReleaseNoteSaveRequest()),
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        private void AssertSecurityPoliciesChecked<T>(
            Func<ReleaseNoteService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (mapper, contentDbContext, releaseHelper, userService) = Mocks();

            var service = new ReleaseNoteService(mapper.Object, contentDbContext.Object, releaseHelper.Object, userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _release, userService, service, policies);
        }

        private (
            Mock<IMapper>,
            Mock<ContentDbContext>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IUserService>) Mocks()
        {
            return (
                new Mock<IMapper>(),
                new Mock<ContentDbContext>(),
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(_release.Id, _release),
                new Mock<IUserService>());
        }
    }
}
