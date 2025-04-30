using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Builders;
using Org.BouncyCastle.Bcpg;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

public class PublicationCacheServiceTests : CacheServiceTestFixture
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
            ContactTelNo = ""
        },
        ExternalMethodology = new ExternalMethodologyViewModel
        {
            Title = "",
            Url = ""
        },
        LatestReleaseId = Guid.NewGuid(),
        Releases =
        [
            new ReleaseTitleViewModel
            {
                Id = Guid.NewGuid(),
                Slug = "",
                Title = ""
            }
        ],
        ReleaseSeries =
        [
            new ReleaseSeriesItemViewModel
            {
                Description = "legacy link description",
                LegacyLinkUrl = "http://test.com/",
            }
        ],
        Theme = new ThemeViewModel(
            Guid.NewGuid(),
            Slug: "",
            Title: "",
            Summary: ""
        )
    };

    [Fact]
    public async Task GetPublication_NoCachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(PublicationCacheViewModel)))
            .ReturnsAsync(null);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(cacheKey, _publicationViewModel))
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
            .Setup(s => s.GetItemAsync(cacheKey, typeof(PublicationCacheViewModel)))
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
            .Setup(s => s.GetItemAsync(cacheKey, typeof(PublicationCacheViewModel)))
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
    public async Task GetPublicationTree_NoCachedTreeExists()
    {
        var publicationTree = ListOf(new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                AnyLiveReleaseHasData = true
            })
        });

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(null);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.GetPublicationTree())
            .ReturnsAsync(publicationTree);

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(
                new PublicationTreeCacheKey(), publicationTree))
            .Returns(Task.CompletedTask);

        var service = BuildService(publicationService.Object);

        var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);

        VerifyAllMocks(PublicBlobCacheService);

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(publicationTree);
    }

    [Fact]
    public async Task GetPublicationTree_CachedTreeExists()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                AnyLiveReleaseHasData = true
            })
        };

        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(PublicationTreeFilter.DataCatalogue);

        VerifyAllMocks(PublicBlobCacheService);

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(ListOf(publicationTree));
    }

    [Fact]
    public async Task GetPublicationTree_DataCatalogue_SomeLiveReleaseHasData_Included()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                AnyLiveReleaseHasData = true
            })
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.DataCatalogue);
    }

    [Fact]
    public async Task GetPublicationTree_DataCatalogue_NoLiveReleaseHasData_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                AnyLiveReleaseHasData = false
            })
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.DataCatalogue);
    }

    [Fact]
    public async Task GetPublicationTree_DataTables_NonSupersededPublicationWithDataOnLatestRelease_Included()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                IsSuperseded = false,
                LatestReleaseHasData = true
            })
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.DataTables);
    }

    [Fact]
    public async Task GetPublicationTree_DataTables_SupersededPublicationWithDataOnLatestRelease_Included()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                IsSuperseded = true,
                LatestReleaseHasData = true
            })
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.DataTables);
    }

    [Fact]
    public async Task GetPublicationTree_DataTables_NonSupersededPublicationWithNoDataOnLatestRelease_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                IsSuperseded = false,
                LatestReleaseHasData = false
            })
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.DataTables);
    }

    [Fact]
    public async Task GetPublicationTree_FastTrack_SomeLiveReleaseHasData_Included()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                AnyLiveReleaseHasData = true
            })
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.FastTrack);
    }

    [Fact]
    public async Task GetPublicationTree_FastTrack_NoLiveReleaseHasData_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Publications = ListOf(new PublicationTreePublicationViewModel
            {
                Title = "Publication A",
                AnyLiveReleaseHasData = false
            })
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.FastTrack);
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
            .Setup(s => s.SetItemAsync<object>(cacheKey, _publicationViewModel))
            .Returns(Task.CompletedTask);

        var service = BuildService(publicationService: publicationService.Object);

        var result = await service.UpdatePublication(PublicationSlug);

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(publicationService, PublicBlobCacheService);

        result.AssertRight(_publicationViewModel);
    }

    [Fact]
    public async Task UpdatePublicationTree()
    {
        var publicationTree = ListOf(new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
        });

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.GetPublicationTree())
            .ReturnsAsync(publicationTree);

        // We should not see any attempt to "get" the cached tree, but rather only see a fresh fetching
        // of the latest tree and then it being cached.
        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(
                new PublicationTreeCacheKey(), publicationTree))
            .Returns(Task.CompletedTask);

        var service = BuildService(publicationService.Object);

        var filteredTree = await service.UpdatePublicationTree();

        VerifyAllMocks(PublicBlobCacheService);

        publicationTree.AssertDeepEqualTo(filteredTree);
    }

    [Fact]
    public void PublicationCacheViewModel_SerializeAndDeserialize()
    {
        var converted = DeserializeObject<PublicationCacheViewModel>(SerializeObject(_publicationViewModel));
        converted.AssertDeepEqualTo(_publicationViewModel);
    }

    [Fact]
    public void PublicationTree_SerializeAndDeserialize()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Publications = new List<PublicationTreePublicationViewModel>
            {
                new()
            }
        };

        var converted = DeserializeObject<PublicationTreeThemeViewModel>(SerializeObject(publicationTree));
        converted.AssertDeepEqualTo(publicationTree);
    }

    [Fact]
    public async Task GivenThemeWithPublications_WhenInvalidatePublicationsByTheme_ThenShouldUpdateAllPublicationsForTheme()
    {
        // ARRANGE
        var themeId = Guid.NewGuid();
        var publicationService = new PublicationServiceMockBuilder();
        
        string[] themePublicationSlugs =
            [
                "publication-for-theme-1",
                "publication-for-theme-2",
                "publication-for-theme-3"
            ];

        // Setup the search results to return the publications for the theme
        publicationService
            .WhereListPublicationsReturns(
                themePublicationSlugs
                    .Select(slug => new PublicationSearchResultViewModelBuilder().WithSlug(slug).Build()));

        // Setup each publication viewmodel returned on Get. These are the values that should get cached.
        var publicationVMs = themePublicationSlugs
            .Select(slug => new PublicationCacheViewModel { Slug = slug })
            .ToArray();
        foreach (var publicationVM in publicationVMs)
        {
            publicationService.WhereGetPublicationReturns(publicationVM);
        }

        // Setup call for adding items to the Cache service
        PublicBlobCacheService
            .Setup(s => s.SetItemAsync(It.IsAny<IBlobCacheKey>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);
        
        // Our SUT
        var service = BuildService(
            publicationService: publicationService.Build()
            );

        // ACT
        var result = await service.InvalidatePublicationsByTheme(themeId);

        // ASSERT
        result.AssertRight();
        Assert.All(publicationVMs,
            expected =>
            {
                // Verify all viewmodels were refreshed in cache
                PublicBlobCacheService
                    .Verify(
                        s => s.SetItemAsync(
                            It.Is<IBlobCacheKey>(actual => new PublicationCacheKey(expected.Slug).Equals(actual)),
                            It.Is<object>(actual => ((PublicationCacheViewModel)actual) == expected)),
                        Times.Once
                        );
            });
        publicationService.Assert.SearchWasFor(themeId);
    }
    
    [Fact]
    public async Task GivenThemeWithNoPublications_WhenInvalidatePublicationsByTheme_ThenDoesNothing()
    {
        // ARRANGE
        var themeId = Guid.NewGuid();
        var publicationService = new PublicationServiceMockBuilder();
        
        // Setup the search results to return the publications for the theme
        publicationService
            .WhereListPublicationsReturnsNoResults();

        // Our SUT
        var service = BuildService(
            publicationService: publicationService.Build()
            );

        // ACT
        var result = await service.InvalidatePublicationsByTheme(themeId);

        // ASSERT
        result.AssertRight();
        
        // Verify nothing refreshed in cache
        PublicBlobCacheService
            .Verify(
                s => s.SetItemAsync(It.IsAny<IBlobCacheKey>(), It.IsAny<object>()),
                Times.Never());
        
    }

    private async Task AssertPublicationTreeUnfiltered(
        PublicationTreeThemeViewModel publicationTree,
        PublicationTreeFilter filter)
    {
        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(filter);

        VerifyAllMocks(PublicBlobCacheService);

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(ListOf(publicationTree));
    }

    private async Task AssertPublicationTreeEmpty(
        PublicationTreeThemeViewModel publicationTree,
        PublicationTreeFilter filter)
    {
        PublicBlobCacheService
            .Setup(s => s.GetItemAsync(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(filter);

        VerifyAllMocks(PublicBlobCacheService);

        var filteredTree = result.AssertRight();
        Assert.Empty(filteredTree);
    }

    private static PublicationCacheService BuildService(
        IPublicationService? publicationService = null,
        IPublicBlobStorageService? publicBlobStorageService = null
    )
    {
        return new PublicationCacheService(
            publicationService: publicationService ?? Mock.Of<IPublicationService>(Strict),
            publicBlobStorageService: publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict),
            Mock.Of<ILogger<PublicationCacheService>>()
        );
    }
}
