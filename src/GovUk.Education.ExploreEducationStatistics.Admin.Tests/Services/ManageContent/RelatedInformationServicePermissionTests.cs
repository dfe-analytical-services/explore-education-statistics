using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent
{
    public class RelatedInformationServicePermissionTests
    {
        private readonly ReleaseVersion _releaseVersion = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public void AddRelatedInformationAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.AddRelatedInformationAsync(
                        _releaseVersion.Id,
                        new CreateUpdateLinkRequest()),
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void DeleteRelatedInformationAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.DeleteRelatedInformationAsync(
                        releaseVersionId: _releaseVersion.Id,
                        relatedInformationId: Guid.NewGuid()),
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        [Fact]
        public void UpdateRelatedInformationAsync()
        {
            AssertSecurityPoliciesChecked(service =>
                    service.UpdateRelatedInformationAsync(
                        releaseVersionId: _releaseVersion.Id,
                        relatedInformationId: Guid.NewGuid(),
                        new CreateUpdateLinkRequest()),
                SecurityPolicies.CanUpdateSpecificRelease);
        }

        private void AssertSecurityPoliciesChecked<T>(
            Func<RelatedInformationService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (contentDbContext, releaseHelper, userService) = Mocks();

            var service = new RelatedInformationService(contentDbContext.Object, releaseHelper.Object, userService.Object);

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _releaseVersion, userService, service, policies);
        }

        private (
            Mock<ContentDbContext>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IUserService>) Mocks()
        {
            return (
                new Mock<ContentDbContext>(),
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id, _releaseVersion),
                new Mock<IUserService>());
        }
    }
}
