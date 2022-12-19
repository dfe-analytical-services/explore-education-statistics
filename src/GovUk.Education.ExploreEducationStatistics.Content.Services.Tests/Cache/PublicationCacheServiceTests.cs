#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

[Collection(CacheServiceTests)]
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
        Topic = new TopicViewModel(new ThemeViewModel(
            Guid.NewGuid(),
            Slug: "",
            Title: "",
            Summary: ""
        ))
    };

    [Fact]
    public async Task GetPublication_NoCachedPublication()
    {
        var cacheKey = new PublicationCacheKey(PublicationSlug);

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationCacheViewModel)))
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
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationCacheViewModel)))
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
            .Setup(s => s.GetItem(cacheKey, typeof(PublicationCacheViewModel)))
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
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        AnyLiveReleaseHasData = true
                    })
                }
            }
        });

        PublicBlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(null);

        var publicationService = new Mock<IPublicationService>(Strict);

        publicationService
            .Setup(s => s.GetPublicationTree())
            .ReturnsAsync(publicationTree);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(
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
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        AnyLiveReleaseHasData = true
                    })
                }
            }
        };

        PublicBlobCacheService
            .Setup(s => s.GetItem(
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
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        AnyLiveReleaseHasData = true
                    })
                }
            }
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.DataCatalogue);
    }

    [Fact]
    public async Task GetPublicationTree_DataCatalogue_NoLiveReleaseHasData_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        AnyLiveReleaseHasData = false
                    })
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.DataCatalogue);
    }

    [Fact]
    public async Task GetPublicationTree_DataTables_NonSupersededPublicationWithDataOnLatestRelease_Included()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        IsSuperseded = false,
                        LatestReleaseHasData = true
                    })
                }
            }
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.DataTables);
    }

    [Fact]
    public async Task GetPublicationTree_DataTables_SupersededPublicationWithDataOnLatestRelease_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        IsSuperseded = true,
                        LatestReleaseHasData = true
                    })
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.DataTables);
    }

    [Fact]
    public async Task GetPublicationTree_DataTables_NonSupersededPublicationWithNoDataOnLatestRelease_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        IsSuperseded = false,
                        LatestReleaseHasData = false
                    })
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.DataTables);
    }

    [Fact]
    public async Task GetPublicationTree_FastTrack_SomeLiveReleaseHasData_Included()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        AnyLiveReleaseHasData = true
                    })
                }
            }
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.FastTrack);
    }

    [Fact]
    public async Task GetPublicationTree_FastTrack_NoLiveReleaseHasData_Excluded()
    {
        var publicationTree = new PublicationTreeThemeViewModel
        {
            Title = "Theme A",
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreePublicationViewModel
                    {
                        Title = "Publication A",
                        AnyLiveReleaseHasData = false
                    })
                }
            }
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
            .Setup(s => s.SetItem<object>(cacheKey, _publicationViewModel))
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
            .Setup(s => s.SetItem<object>(
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
            Topics = new List<PublicationTreeTopicViewModel>
            {
                new()
                {
                    Publications = new List<PublicationTreePublicationViewModel>
                    {
                        new()
                    }
                }
            }
        };

        var converted = DeserializeObject<PublicationTreeThemeViewModel>(SerializeObject(publicationTree));
        converted.AssertDeepEqualTo(publicationTree);
    }

    private static async Task AssertPublicationTreeUnfiltered(
        PublicationTreeThemeViewModel publicationTree,
        PublicationTreeFilter filter)
    {
        PublicBlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(filter);

        VerifyAllMocks(PublicBlobCacheService);

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(ListOf(publicationTree));
    }

    private static async Task AssertPublicationTreeEmpty(
        PublicationTreeThemeViewModel publicationTree,
        PublicationTreeFilter filter)
    {
        PublicBlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<PublicationTreeThemeViewModel>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(filter);

        VerifyAllMocks(PublicBlobCacheService);

        var filteredTree = result.AssertRight();
        Assert.Empty(filteredTree);
    }

    private static PublicationCacheService BuildService(
        IPublicationService? publicationService = null
    )
    {
        return new PublicationCacheService(
            publicationService: publicationService ?? Mock.Of<IPublicationService>(Strict),
            Mock.Of<ILogger<PublicationCacheService>>()
        );
    }
}
