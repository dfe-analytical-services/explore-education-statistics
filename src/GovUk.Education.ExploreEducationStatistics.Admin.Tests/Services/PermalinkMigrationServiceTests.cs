#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationServiceTests
{
    [Fact]
    public async Task MigrateAll_FailsWithNullMessageCount()
    {
        var storageQueueService = MockStorageQueueService(messageCount: null);

        var service = SetupService(storageQueueService: storageQueueService.Object);

        var result = await service.MigrateAll();

        VerifyAllMocks(storageQueueService);

        result.AssertBadRequest(NullMessageCountForPermalinksMigrationQueue);
    }

    [Fact]
    public async Task MigrateAll_FailsWithNonZeroMessageCount()
    {
        var storageQueueService = MockStorageQueueService(messageCount: 1);

        var service = SetupService(storageQueueService: storageQueueService.Object);

        var result = await service.MigrateAll();

        VerifyAllMocks(storageQueueService);

        result.AssertBadRequest(NonEmptyPermalinksMigrationQueue);
    }

    [Fact]
    public async Task MigrateAll()
    {
        var storageQueueService = MockStorageQueueService(messageCount: 0);

        storageQueueService.Setup(mock =>
                mock.AddMessageAsync(PermalinksMigrationQueue, new PermalinksMigrationMessage()))
            .Returns(Task.CompletedTask);

        var service = SetupService(storageQueueService: storageQueueService.Object);

        var result = await service.MigrateAll();

        VerifyAllMocks(storageQueueService);

        result.AssertRight();
    }

    private static Mock<IStorageQueueService> MockStorageQueueService(int? messageCount)
    {
        var service = new Mock<IStorageQueueService>(MockBehavior.Strict);
        service.Setup(s => s.GetApproximateMessageCount(PermalinksMigrationQueue))
            .ReturnsAsync(messageCount);
        return service;
    }

    private static PermalinkMigrationService SetupService(
        IStorageQueueService? storageQueueService = null,
        IUserService? userService = null)
    {
        return new PermalinkMigrationService(
            storageQueueService ?? Mock.Of<IStorageQueueService>(MockBehavior.Strict),
            userService ?? AlwaysTrueUserService().Object
        );
    }
}
