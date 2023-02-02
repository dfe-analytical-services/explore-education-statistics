#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseApprovalServicePermissionTests
    {
        private static readonly Publication Publication = new()
        {
            Id = Guid.NewGuid()
        };

        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            PublicationId = Publication.Id,
            Published = DateTime.Now,
            TimePeriodCoverage = TimeIdentifier.April
        };

        [Fact]
        public async Task GetReleaseStatuses()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanViewReleaseStatusHistory)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildService(userService.Object);
                        return service.GetReleaseStatuses(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task UpdateReleaseStatus_Draft()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanMarkSpecificReleaseAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildService(userService.Object);
                        return service.CreateReleaseStatus(
                            _release.Id,
                            new ReleaseStatusCreateRequest
                            {
                                ApprovalStatus = ReleaseApprovalStatus.Draft
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateReleaseStatus_HigherLevelReview()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanSubmitSpecificReleaseToHigherReview)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildService(userService.Object);
                        return service.CreateReleaseStatus(
                            _release.Id,
                            new ReleaseStatusCreateRequest
                            {
                                ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateReleaseStatus_Approve()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanApproveSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildService(userService.Object);
                        return service.CreateReleaseStatus(
                            _release.Id,
                            new ReleaseStatusCreateRequest
                            {
                                ApprovalStatus = ReleaseApprovalStatus.Approved
                            }
                        );
                    }
                );
        }

        private ReleaseApprovalService BuildService(IUserService userService)
        {
            return new ReleaseApprovalService(
                Mock.Of<ContentDbContext>(),
                DefaultPersistenceHelperMock().Object,
                new DateTimeProvider(),
                userService,
                Mock.Of<IPublishingService>(),
                Mock.Of<IReleaseChecklistService>(),
                Mock.Of<IContentService>(),
                Mock.Of<IPreReleaseUserService>(),
                Mock.Of<IReleaseFileRepository>(),
                Mock.Of<IReleaseFileService>(),
                Mock.Of<IReleaseRepository>(),
                Options.Create(new ReleaseApprovalOptions()), 
                Mock.Of<IUserReleaseRoleService>(),
                Mock.Of<IEmailTemplateService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockPersistenceHelper<ContentDbContext, Release>();
            SetupCall(mock, _release.Id, _release);
            SetupCall(mock, Publication.Id, Publication);

            return mock;
        }
    }
}
