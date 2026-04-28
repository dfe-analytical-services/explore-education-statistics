using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

public class PublicationCacheServiceTests
{
    private const string PublicationSlug = "publication-slug";

    private readonly PublicationCacheViewModel _publicationViewModel = new()
    {
        Id = Guid.NewGuid(),
        Slug = "",
        Title = "",
        IsSuperseded = false,
        Contact = new ContactViewModel
        {
            ContactName = "",
            TeamEmail = "",
            TeamName = "",
            ContactTelNo = "",
        },
        ExternalMethodology = new ExternalMethodologyViewModel { Title = "", Url = "" },
        LatestReleaseId = Guid.NewGuid(),
        Releases =
        [
            new ReleaseTitleViewModel
            {
                Id = Guid.NewGuid(),
                Slug = "",
                Title = "",
            },
        ],
        ReleaseSeries =
        [
            new ReleaseSeriesItemViewModel
            {
                Description = "legacy link description",
                LegacyLinkUrl = "http://test.com/",
            },
        ],
        Theme = new ThemeViewModel
        {
            Id = Guid.NewGuid(),
            Slug = "",
            Title = "",
            Summary = "",
        },
    };

    [Fact]
    public async Task GetPublication_NoCachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(PublicationCacheViewModel)))
            .ReturnsAsync(null!);

        publicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _publicationViewModel))
            .Returns(Task.CompletedTask);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService.Setup(s => s.Get(PublicationSlug)).ReturnsAsync(_publicationViewModel);

        var service = BuildService(
            publicationService: publicationService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(publicationService, publicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public async Task GetPublication_CachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(PublicationCacheViewModel)))
            .ReturnsAsync(_publicationViewModel);

        var service = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(publicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public async Task GetPublication_PublicationNotFound()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(PublicationCacheViewModel)))
            .ReturnsAsync((object?)null);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService.Setup(s => s.Get(PublicationSlug)).ReturnsAsync(new NotFoundResult());

        var service = BuildService(
            publicationService: publicationService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(publicationService, publicBlobCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdatePublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService.Setup(s => s.Get(PublicationSlug)).ReturnsAsync(_publicationViewModel);

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _publicationViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(
            publicationService: publicationService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.UpdatePublication(PublicationSlug);

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(publicationService, publicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public void PublicationCacheViewModel_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<PublicationCacheViewModel>(SerializeObject(_publicationViewModel));
        converted.AssertDeepEqualTo(_publicationViewModel);
    }

    private static PublicationCacheService BuildService(
        IPublicationService? publicationService = null,
        IPublicBlobStorageService? publicBlobStorageService = null,
        IPublicBlobCacheService? publicBlobCacheService = null
    )
    {
        return new PublicationCacheService(
            publicationService: publicationService ?? Mock.Of<IPublicationService>(Strict),
            publicBlobStorageService: publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict),
            publicBlobCacheService: publicBlobCacheService ?? Mock.Of<IPublicBlobCacheService>(Strict),
            Mock.Of<ILogger<PublicationCacheService>>()
        );
    }
}
