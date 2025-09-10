#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class PublishingServicePermissionTests
{
    private readonly DataFixture _dataFixture = new();

    public class RetryReleasePublishingTests : PublishingServicePermissionTests
    {
        [Fact]
        public async Task SecurityPolicyChecked()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithApprovalStatus(ReleaseApprovalStatus.Approved);

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(releaseVersion, SecurityPolicies.CanPublishSpecificRelease)
                .AssertForbidden(async userService =>
                {
                    await using var context = InMemoryApplicationDbContext();
                    context.ReleaseVersions.Add(releaseVersion);
                    await context.SaveChangesAsync();

                    var service = BuildService(context, userService: userService.Object);
                    return await service.RetryReleasePublishing(releaseVersion.Id);
                });
        }
    }

    private static PublishingService BuildService(
        ContentDbContext context,
        IPublisherClient? publisherClient = null,
        IUserService? userService = null)
    {
        return new PublishingService(
            context,
            publisherClient ?? Mock.Of<IPublisherClient>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            Mock.Of<ILogger<PublishingService>>());
    }
}
