#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

[Collection(CacheServiceTests)]
public class PublicationCacheServiceTests : CacheServiceTestFixture
{
    private const string PublicationSlug = "publication-slug";

    private readonly PublicationViewModel _publicationViewModel = new()
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
            ContactTelNo = ""
        },
        ExternalMethodology = new ExternalMethodologyViewModel
        {
            Title = "",
            Url = ""
        },
        LatestReleaseId = Guid.NewGuid(),
        Methodologies = new List<MethodologyVersionSummaryViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Slug = "",
                Title = ""
            }
        },
        LegacyReleases = new List<LegacyReleaseViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Description = "",
                Url = ""
            }
        },
        Releases = new List<ReleaseTitleViewModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Slug = "",
                Title = ""
            }
        },
        Topic = new TopicViewModel(new ThemeViewModel(""))
    };

    [Fact]
    public async Task GetPublication_NoCachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
            .ReturnsAsync(null);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _publicationViewModel))
            .Returns(Task.CompletedTask);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.Get(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        var service = BuildService(publicationService: publicationService.Object);

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(publicationService, PublicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public async Task GetPublication_CachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
            .ReturnsAsync(_publicationViewModel);

        var service = BuildService();

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public async Task GetPublication_PublicationNotFound()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationViewModel)))
            .ReturnsAsync(null);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.Get(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(publicationService: publicationService.Object);

        var result = await service.GetPublication(PublicationSlug);

        VerifyAllMocks(publicationService, PublicBlobCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdatePublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.Get(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _publicationViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(publicationService: publicationService.Object);

        var result = await service.UpdatePublication(PublicationSlug);

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(publicationService, PublicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public void PublicationViewModel_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<PublicationViewModel>(SerializeObject(_publicationViewModel));
        converted.AssertDeepEqualTo(_publicationViewModel);
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
