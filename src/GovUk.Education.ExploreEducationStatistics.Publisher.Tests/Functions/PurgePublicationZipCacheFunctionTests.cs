using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Functions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Functions;

public class PurgePublicationZipCacheFunctionTests
{
    [Fact]
    public async Task PublicationChanged_PurgesLatestPublishedReleaseVersions()
    {
        var publicationId = Guid.NewGuid();
        var releaseVersionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var repository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
        repository
            .Setup(r => r.ListLatestReleaseVersionIds(publicationId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseVersionIds);

        var cache = new Mock<IFrontDoorCacheService>(MockBehavior.Strict);
        cache
            .Setup(s =>
                s.PurgeAllFilesZipCache(
                    It.Is<IReadOnlySet<Guid>>(ids => ids.SetEquals(releaseVersionIds)),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var function = new PurgePublicationZipCacheFunction(repository.Object, cache.Object);
        var publicationChangedEvent = new EventGridEvent(
            publicationId.ToString(),
            "publication-changed",
            "1.0",
            new { Title = "Updated publication title" }
        );

        await function.PurgePublicationZipCache(publicationChangedEvent, CancellationToken.None);

        repository.VerifyAll();
        cache.VerifyAll();
    }
}
