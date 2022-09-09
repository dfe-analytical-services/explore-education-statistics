#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

[Collection(CacheServiceTests)]
public class GlossaryCacheServiceTests : CacheServiceTestFixture
{
    private readonly List<GlossaryCategoryViewModel> _glossary = new()
    {
        new GlossaryCategoryViewModel()
    };

    [Fact]
    public async Task GetGlossary_NoCachedGlossary()
    {
        var cacheKey = new GlossaryCacheKey();

        PublicBlobCacheService
            .Setup(s => s.GetItem(cacheKey, typeof(List<GlossaryCategoryViewModel>)))
            .ReturnsAsync(null);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(cacheKey, _glossary))
            .Returns(Task.CompletedTask);

        var glossaryService = new Mock<IGlossaryService>(Strict);

        glossaryService
            .Setup(s => s.GetAllGlossaryEntries())
            .ReturnsAsync(_glossary);

        var service = BuildService(glossaryService: glossaryService.Object);

        var result = await service.GetGlossary();

        VerifyAllMocks(glossaryService, PublicBlobCacheService);

        Assert.Equal(_glossary, result);
    }

    [Fact]
    public async Task GetGlossary_CachedGlossary()
    {
        PublicBlobCacheService
            .Setup(s => s.GetItem(new GlossaryCacheKey(), typeof(List<GlossaryCategoryViewModel>)))
            .ReturnsAsync(_glossary);

        var service = BuildService();

        var result = await service.GetGlossary();

        VerifyAllMocks(PublicBlobCacheService);

        Assert.Equal(_glossary, result);
    }

    [Fact]
    public async Task UpdateGlossary()
    {
        var glossaryService = new Mock<IGlossaryService>(Strict);

        glossaryService
            .Setup(s => s.GetAllGlossaryEntries())
            .ReturnsAsync(_glossary);

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(new GlossaryCacheKey(), _glossary))
            .Returns(Task.CompletedTask);

        var service = BuildService(glossaryService: glossaryService.Object);

        var result = await service.UpdateGlossary();

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(glossaryService, PublicBlobCacheService);

        Assert.Equal(_glossary, result);
    }

    [Fact]
    public void GlossaryCategoryViewModel_SerializeAndDeserialize()
    {
        var original = new GlossaryCategoryViewModel
        {
            Heading = "Glossary Category 1",
            Entries = new List<GlossaryEntryViewModel>
            {
                new()
                {
                    Body = "A body",
                    Slug = "A slug",
                    Title = "A title"
                }
            }
        };

        var converted = DeserializeObject<GlossaryCategoryViewModel>(SerializeObject(original));
        converted.AssertDeepEqualTo(original);
    }

    private static GlossaryCacheService BuildService(
        IGlossaryService? glossaryService = null
    )
    {
        return new GlossaryCacheService(
            glossaryService: glossaryService ?? Mock.Of<IGlossaryService>(Strict)
        );
    }
}
