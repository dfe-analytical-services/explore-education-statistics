#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyApprovalServicePermissionTests
    {
        private readonly MethodologyVersion _methodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            Status = Draft
        };

        private readonly MethodologyVersion _approvedMethodologyVersion = new()
        {
            Id = Guid.NewGuid(),
            Status = Approved
        };

        [Fact]
        public async Task UpdateApprovalStatus_Approve()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_methodologyVersion, CanApproveSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupService(userService: userService.Object);
                        return service.UpdateApprovalStatus(_methodologyVersion.Id, new MethodologyApprovalUpdateRequest
                        {
                            Status = Approved
                        });
                    }
                );
        }

        [Fact]
        public async Task UpdateApprovalStatus_MarkAsDraft()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_approvedMethodologyVersion, CanMarkSpecificMethodologyAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupService(userService: userService.Object);
                        return service.UpdateApprovalStatus(_approvedMethodologyVersion.Id, new MethodologyApprovalUpdateRequest
                        {
                            Status = Draft
                        });
                    }
                );
        }
        
        private MethodologyApprovalService SetupService(
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IMethodologyContentService? methodologyContentService = null,
            IMethodologyFileRepository? methodologyFileRepository = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IMethodologyImageService? methodologyImageService = null,
            IPublishingService? publishingService = null,
            IUserService? userService = null,
            IMethodologyCacheService? methodologyCacheService = null)
        {
            return new(
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                methodologyContentService ?? Mock.Of<IMethodologyContentService>(Strict),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
                methodologyImageService ?? Mock.Of<IMethodologyImageService>(Strict),
                publishingService ?? Mock.Of<IPublishingService>(Strict),
                userService ?? Mock.Of<IUserService>(),
                methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict));
        }
        
        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockPersistenceHelper<ContentDbContext, MethodologyVersion>(_methodologyVersion.Id, _methodologyVersion);
            SetupCall(mock, _approvedMethodologyVersion.Id, _approvedMethodologyVersion);
            return mock;
        }
    }
}
