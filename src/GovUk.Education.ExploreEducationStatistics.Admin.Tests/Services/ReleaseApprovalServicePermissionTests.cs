#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseApprovalServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListReleaseStatuses()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id, CanViewReleaseStatusHistory)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext: contentDbContext, userService.Object);
                    return await service.ListReleaseStatuses(releaseVersion.Id);
                }
            });
    }

    [Fact]
    public async Task UpdateReleaseStatus_Draft()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id, CanMarkSpecificReleaseAsDraft)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext: contentDbContext, userService.Object);

                    return await service.CreateReleaseStatus(
                        releaseVersion.Id,
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
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id, CanSubmitSpecificReleaseToHigherReview)
            .AssertForbidden(async userService =>
                {
                    var contentDbContextId = Guid.NewGuid().ToString();
                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        contentDbContext.ReleaseVersions.Add(releaseVersion);
                        await contentDbContext.SaveChangesAsync();
                    }

                    await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                    {
                        var service = BuildService(contentDbContext: contentDbContext, userService.Object);

                        return await service.CreateReleaseStatus(
                            releaseVersion.Id,
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
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        await PolicyCheckBuilder<SecurityPolicies>()
            .SetupResourceCheckToFailWithMatcher<ReleaseVersion>(rv => rv.Id == releaseVersion.Id, CanApproveSpecificRelease)
            .AssertForbidden(async userService =>
            {
                var contentDbContextId = Guid.NewGuid().ToString();
                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    contentDbContext.ReleaseVersions.Add(releaseVersion);
                    await contentDbContext.SaveChangesAsync();
                }

                await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
                {
                    var service = BuildService(contentDbContext: contentDbContext, userService.Object);
                    return await service.CreateReleaseStatus(
                        releaseVersion.Id,
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
            new ReleaseApprovalOptions().ToOptionsWrapper(),
            Mock.Of<IUserReleaseRoleService>(),
            Mock.Of<IEmailTemplateService>(),
            Mock.Of<IUserRepository>()
        );
    }
}
