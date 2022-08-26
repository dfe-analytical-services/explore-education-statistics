#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

[Collection(CacheServiceTests)]
public class PublicationCacheServiceTests : CacheServiceTestFixture
{
    private const string PublicationSlug = "publication-slug";

    private readonly PublicationViewModel _publicationViewModel = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public async Task GetPublication_NoCachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        BlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
            .ReturnsAsync(null);

        BlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _publicationViewModel))
            .Returns(Task.CompletedTask);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.Get(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        var service = BuildService(publicationService: publicationService.Object);

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(publicationService, BlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public async Task GetPublication_CachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        BlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
            .ReturnsAsync(_publicationViewModel);

        var service = BuildService();

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(BlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    private static PublicationCacheService BuildService(
        IPublicationService? publicationService = null
    )
    {
        return new PublicationCacheService(
            publicationService: publicationService ?? Mock.Of<IPublicationService>(Strict)
        );
    }
}
