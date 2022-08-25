#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;
 
[Collection(CacheServiceTests)]
public class ThemeCacheServiceTests : CacheServiceTestFixture
{
    [Fact]
    public async Task GetPublicationTree_NoCachedTreeExists()
    {
        var publicationTree = ListOf(new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
                    {
                        Title = "Publication A",
                        IsSuperseded = false,
                        HasLiveRelease = true
                    })
                }
            }
        });
        
        BlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<ThemeTree<PublicationTreeNode>>)))
            .ReturnsAsync(null);

        var themeService = new Mock<IThemeService>(Strict);

        themeService
            .Setup(s => s.GetPublicationTree())
            .ReturnsAsync(publicationTree);
        
        BlobCacheService
            .Setup(s => s.SetItem<object>(
                new PublicationTreeCacheKey(), publicationTree))
            .Returns(Task.CompletedTask);
        
        var service = BuildService(themeService.Object);

        var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);

        VerifyAllMocks(BlobCacheService);

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(publicationTree);
    }
    
    [Fact]
    public async Task GetPublicationTree_CachedTreeExists()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
                    {
                        Title = "Publication A",
                        IsSuperseded = false,
                        HasLiveRelease = true
                    })
                }
            }
        };
        
        BlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<ThemeTree<PublicationTreeNode>>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(PublicationTreeFilter.FindStatistics);

        VerifyAllMocks(BlobCacheService);   

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(ListOf(publicationTree));
    }
    
    [Fact]
    public async Task GetPublicationTree_FindStatistics_NonSupersededPublicationWithLiveRelease_Included()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
                    {
                        Title = "Publication A",
                        IsSuperseded = false,
                        HasLiveRelease = true
                    })
                }
            }
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.FindStatistics);
    }
    
    [Fact]
    public async Task GetPublicationTree_FindStatistics_NonSupersededPublicationWithLegacyRelease_Included()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
                    {
                        Title = "Publication A",
                        IsSuperseded = false,
                        Type = PublicationType.Legacy
                    })
                }
            }
        };

        await AssertPublicationTreeUnfiltered(publicationTree, PublicationTreeFilter.FindStatistics);
    }

    [Fact]
    public async Task GetPublicationTree_FindStatistics_SupersededPublicationWithLiveRelease_Excluded()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
                    {
                        Title = "Publication A",
                        IsSuperseded = true,
                        HasLiveRelease = true
                    })
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.FindStatistics);
    }
    
    [Fact]
    public async Task GetPublicationTree_FindStatistics_SupersededPublicationWithLegacyRelease_Excluded()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
                    {
                        Title = "Publication A",
                        IsSuperseded = true,
                        Type = PublicationType.Legacy
                    })
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.FindStatistics);
    }
    
    [Fact]
    public async Task GetPublicationTree_DataCatalogue_SomeLiveReleaseHasData_Included()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
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
        var publication = new PublicationTreeNode
        {
            Title = "Publication A",
            AnyLiveReleaseHasData = false
        };
        
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(publication)
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.DataCatalogue);
    }
    
    [Fact]
    public async Task GetPublicationTree_DataTables_NonSupersededPublicationWithDataOnLatestRelease_Included()
    {
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
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
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
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
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
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
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(new PublicationTreeNode
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
        var publication = new PublicationTreeNode
        {
            Title = "Publication A",
            AnyLiveReleaseHasData = false
        };
        
        var publicationTree = new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
            Topics = new List<TopicTree<PublicationTreeNode>>
            {
                new()
                {
                    Title = "Topic A",
                    Publications = ListOf(publication)
                }
            }
        };

        await AssertPublicationTreeEmpty(publicationTree, PublicationTreeFilter.FastTrack);
    }
    
    [Fact]
    public async Task UpdatePublicationTree()
    {
        var publicationTree = ListOf(new ThemeTree<PublicationTreeNode>
        {
            Title = "Theme A",
        });
        
        var themeService = new Mock<IThemeService>(Strict);

        themeService
            .Setup(s => s.GetPublicationTree())
            .ReturnsAsync(publicationTree);
        
        // We should not see any attempt to "get" the cached tree, but rather only see a fresh fetching
        // of the latest tree and then it being cached.
        BlobCacheService
            .Setup(s => s.SetItem<object>(
                new PublicationTreeCacheKey(), publicationTree))
            .Returns(Task.CompletedTask);
        
        var service = BuildService(themeService.Object);

        var filteredTree = await service.UpdatePublicationTree();

        VerifyAllMocks(BlobCacheService);
        
        publicationTree.AssertDeepEqualTo(filteredTree);
    }

    private static async Task AssertPublicationTreeUnfiltered(
        ThemeTree<PublicationTreeNode> publicationTree,
        PublicationTreeFilter filter)
    {
        BlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<ThemeTree<PublicationTreeNode>>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(filter);

        VerifyAllMocks(BlobCacheService);

        var filteredTree = result.AssertRight();
        filteredTree.AssertDeepEqualTo(ListOf(publicationTree));
    }

    private static async Task AssertPublicationTreeEmpty(
        ThemeTree<PublicationTreeNode> publicationTree,
        PublicationTreeFilter filter)
    {
        BlobCacheService
            .Setup(s => s.GetItem(
                new PublicationTreeCacheKey(), typeof(IList<ThemeTree<PublicationTreeNode>>)))
            .ReturnsAsync(ListOf(publicationTree));

        var service = BuildService();

        var result = await service.GetPublicationTree(filter);

        VerifyAllMocks(BlobCacheService);

        var filteredTree = result.AssertRight();
        Assert.Empty(filteredTree);
    }
    
    private static ThemeCacheService BuildService(IThemeService? themeService = null)
    {
        return new(
            themeService: themeService ?? Mock.Of<IThemeService>(Strict),
            Mock.Of<ILogger<ThemeCacheService>>()
        );
    }
}