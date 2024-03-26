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
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseApprovalServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new()
    {
        Id = Guid.NewGuid(),
        Publication = new Publication(),
        Published = DateTime.Now,
        TimePeriodCoverage = TimeIdentifier.April
    };

    [Fact]
    public async Task ListReleaseStatuses()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == _releaseVersion.Id, CanViewReleaseStatusHistory)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.AddRange(_releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext: contentDbContext, userService.Object);
                    return await service.ListReleaseStatuses(_releaseVersion.Id);
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseStatus_Draft()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == _releaseVersion.Id, CanMarkSpecificReleaseAsDraft)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.AddRange(_releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext: contentDbContext, userService.Object);

                    return await service.CreateReleaseStatus(
                        _releaseVersion.Id,
                        new ReleaseStatusCreateRequest
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Draft
                        }
                    );
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseStatus_HigherLevelReview()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == _releaseVersion.Id, CanSubmitSpecificReleaseToHigherReview)
            .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        contentDbContext.ReleaseVersions.AddRange(_releaseVersion);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = BuildService(contentDbContext: contentDbContext, userService.Object);

                        return await service.CreateReleaseStatus(
                            _releaseVersion.Id,
                            new ReleaseStatusCreateRequest
                            {
                                ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview
                            }
                        );
                    }
                }
            );
    }

    [Fact]
    public async Task UpdateReleaseStatus_Approve()
    {
        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == _releaseVersion.Id, CanApproveSpecificRelease)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.AddRange(_releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext: contentDbContext, userService.Object);
                    return await service.CreateReleaseStatus(
                        _releaseVersion.Id,
                        new ReleaseStatusCreateRequest
                        {
                            ApprovalStatus = ReleaseApprovalStatus.Approved
                        }
                    );
                }
            });
    }

    private ReleaseApprovalService BuildService(ContentDbContext contentDbContext, IUserService userService)
    {
        return new ReleaseApprovalService(
            contentDbContext,
            new DateTimeProvider(),
            userService,
            Mock.Of<IPublishingService>(),
            Mock.Of<IReleaseChecklistService>(),
            Mock.Of<IContentService>(),
            Mock.Of<IPreReleaseUserService>(),
            Mock.Of<IReleaseFileRepository>(),
            Mock.Of<IReleaseFileService>(),
            Options.Create(new ReleaseApprovalOptions()),
            Mock.Of<IUserReleaseRoleService>(),
            Mock.Of<IEmailTemplateService>(),
            Mock.Of<IUserRepository>()
        );
    }
}
