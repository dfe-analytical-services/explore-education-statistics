using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

public class GlossaryCacheServiceTests
{
    private readonly List<GlossaryCategoryViewModel> _glossary =
    [
        new(Heading: 'A', Entries: [new GlossaryEntryViewModel(Title: "A title", Slug: "A slug", Body: "A body")]),
    ];

    [Fact]
    public async Task GetGlossary_NoCachedGlossary()
    {
        var cacheKey = new GlossaryCacheKey();

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(cacheKey, typeof(List<GlossaryCategoryViewModel>)))
            .ReturnsAsync((object?)null);

        publicBlobCacheService.Setup(s => s.SetItemAsync<object>(cacheKey, _glossary)).Returns(Task.CompletedTask);

        var glossaryService = new Mock<IGlossaryService>(Strict);

        glossaryService.Setup(s => s.GetGlossary()).ReturnsAsync(_glossary);

        var service = BuildService(
            glossaryService: glossaryService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.GetGlossary();

        VerifyAllMocks(glossaryService, publicBlobCacheService);

        Assert.Equal(_glossary, result);
    }

    [Fact]
    public async Task GetGlossary_CachedGlossary()
    {
        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(new GlossaryCacheKey(), typeof(List<GlossaryCategoryViewModel>)))
            .ReturnsAsync(_glossary);

        var service = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

        var result = await service.GetGlossary();

        VerifyAllMocks(publicBlobCacheService);

        Assert.Equal(_glossary, result);
    }

    [Fact]
    public async Task UpdateGlossary()
    {
        var glossaryService = new Mock<IGlossaryService>(Strict);

        glossaryService.Setup(s => s.GetGlossary()).ReturnsAsync(_glossary);

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(new GlossaryCacheKey(), _glossary))
            .Returns(Task.CompletedTask);

        var service = BuildService(
            glossaryService: glossaryService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.UpdateGlossary();

        // There should be no attempt on the cache service to get the cached resource

        VerifyAllMocks(glossaryService, publicBlobCacheService);

        Assert.Equal(_glossary, result);
    }

    [Fact]
    public void GlossaryCategoryViewModel_SerializeAndDeserialize()
    {
        var original = new GlossaryCategoryViewModel(
            Heading: 'A',
            Entries: new List<GlossaryEntryViewModel> { new(Body: "A body", Slug: "A slug", Title: "A title") }
        );

        var converted = DeserializeObject<GlossaryCategoryViewModel>(SerializeObject(original));
        converted.AssertDeepEqualTo(original);
    }

    private static GlossaryCacheService BuildService(
        IGlossaryService? glossaryService = null,
        IPublicBlobCacheService? publicBlobCacheService = null
    )
    {
        return new GlossaryCacheService(
            glossaryService: glossaryService ?? Mock.Of<IGlossaryService>(Strict),
            publicBlobCacheService: publicBlobCacheService ?? Mock.Of<IPublicBlobCacheService>(Strict),
            logger: Mock.Of<ILogger<GlossaryCacheService>>()
        );
    }
}
