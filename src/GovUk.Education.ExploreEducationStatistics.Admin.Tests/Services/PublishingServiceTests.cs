#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PublishingServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task RetryReleasePublishing()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var publisherClient = new Mock<IPublisherClient>(MockBehavior.Strict);

        publisherClient
            .Setup(s => s.RetryReleasePublishing(releaseVersion.Id, CancellationToken.None))
            .Returns(Task.CompletedTask);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publishingService = BuildPublishingService(
                contentDbContext: context,
                publisherClient: publisherClient.Object
            );

            var result = await publishingService.RetryReleasePublishing(releaseVersion.Id);

            publisherClient.Verify(
                s => s.RetryReleasePublishing(releaseVersion.Id, CancellationToken.None),
                Times.Once()
            );

            result.AssertRight();
        }

        VerifyAllMocks(publisherClient);
    }

    [Fact]
    public async Task RetryReleasePublishing_ReleaseNotFound()
    {
        await using var context = InMemoryApplicationDbContext();

        var publishingService = BuildPublishingService(contentDbContext: context);

        var result = await publishingService.RetryReleasePublishing(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task ReleaseChanged()
    {
        ReleaseStatus releaseStatus = _fixture.DefaultReleaseStatus();
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithReleaseStatuses(new[] { releaseStatus });

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var publisherClient = new Mock<IPublisherClient>(MockBehavior.Strict);

        var releasePublishingKey = new ReleasePublishingKey(releaseVersion.Id, releaseStatus.Id);

        publisherClient
            .Setup(s => s.HandleReleaseChanged(releasePublishingKey, true, CancellationToken.None))
            .Returns(Task.CompletedTask);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publishingService = BuildPublishingService(
                contentDbContext: context,
                publisherClient: publisherClient.Object
            );

            var result = await publishingService.ReleaseChanged(releasePublishingKey, immediate: true);

            publisherClient.Verify(
                s => s.HandleReleaseChanged(releasePublishingKey, true, CancellationToken.None),
                Times.Once()
            );

            result.AssertRight();
        }

        VerifyAllMocks(publisherClient);
    }

    [Fact]
    public async Task ReleaseChanged_ReleaseNotFound()
    {
        await using var context = InMemoryApplicationDbContext();

        var publishingService = BuildPublishingService(contentDbContext: context);

        var result = await publishingService.ReleaseChanged(
            new ReleasePublishingKey(Guid.NewGuid(), Guid.NewGuid()),
            immediate: true
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task PublishMethodologyFiles()
    {
        var methodologyVersion = new MethodologyVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var publisherClient = new Mock<IPublisherClient>(MockBehavior.Strict);

        publisherClient
            .Setup(s => s.PublishMethodologyFiles(methodologyVersion.Id, CancellationToken.None))
            .Returns(Task.CompletedTask);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publishingService = BuildPublishingService(
                contentDbContext: context,
                publisherClient: publisherClient.Object
            );

            var result = await publishingService.PublishMethodologyFiles(methodologyVersion.Id);

            publisherClient.Verify(
                s => s.PublishMethodologyFiles(methodologyVersion.Id, CancellationToken.None),
                Times.Once()
            );

            result.AssertRight();
        }

        VerifyAllMocks(publisherClient);
    }

    [Fact]
    public async Task PublishMethodologyFiles_MethodologyNotFound()
    {
        await using var context = InMemoryApplicationDbContext();

        var publishingService = BuildPublishingService(contentDbContext: context);

        var result = await publishingService.PublishMethodologyFiles(Guid.NewGuid());

        result.AssertNotFound();
    }

    private static PublishingService BuildPublishingService(
        ContentDbContext contentDbContext,
        IPublisherClient? publisherClient = null,
        IUserService? userService = null
    )
    {
        return new PublishingService(
            contentDbContext,
            publisherClient ?? Mock.Of<IPublisherClient>(MockBehavior.Strict),
            userService ?? AlwaysTrueUserService().Object,
            new Mock<ILogger<PublishingService>>().Object
        );
    }
}
