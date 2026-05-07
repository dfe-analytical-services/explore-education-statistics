using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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

public class MethodologyCacheServiceTests
{
    private readonly List<AllMethodologiesThemeViewModel> _methodologyTree =
    [
        new()
        {
            Title = "Theme 1",
            Publications =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme 1 Publication 1",
                    Methodologies = [new() { Title = "Theme 1 Publication 1 Methodology 1" }],
                },
            ],
        },
        new()
        {
            Title = "Theme 2",
            Publications =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme 2 Publication 1",
                    Methodologies = [new() { Title = "Theme 2 Publication 1 Methodology 1" }],
                },
            ],
        },
    ];

    [Fact]
    public async Task GetSummariesTree_NoCachedTreeExists()
    {
        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync((object?)null);

        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree));

        publicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(
            methodologyService: methodologyService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.GetSummariesTree();

        VerifyAllMocks(methodologyService, publicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task GetSummariesTree_CachedTreeExists()
    {
        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        publicBlobCacheService
            .Setup(s => s.GetItemAsync(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(_methodologyTree);

        var service = SetupService(publicBlobCacheService: publicBlobCacheService.Object);

        var result = await service.GetSummariesTree();

        VerifyAllMocks(publicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task UpdateSummariesTree()
    {
        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree));

        var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

        // We should not see any attempt to "get" the cached tree, but rather only see a fresh fetching
        // of the latest tree and then it being cached.
        publicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(
            methodologyService: methodologyService.Object,
            publicBlobCacheService: publicBlobCacheService.Object
        );

        var result = await service.UpdateSummariesTree();

        VerifyAllMocks(methodologyService, publicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public void AllMethodologiesThemeViewModel_SerializeAndDeserialize()
    {
        var viewModel = new AllMethodologiesThemeViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Publications =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication title",
                    Methodologies = new List<MethodologyVersionSummaryViewModel>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Slug = "methodology-slug",
                            Title = "Methodology title",
                        },
                    },
                },
            ],
        };

        var converted = DeserializeObject<AllMethodologiesThemeViewModel>(SerializeObject(viewModel));
        converted.AssertDeepEqualTo(viewModel);
    }

    private static MethodologyCacheService SetupService(
        IMethodologyService? methodologyService = null,
        IPublicBlobCacheService? publicBlobCacheService = null
    )
    {
        return new(
            methodologyService: methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            publicBlobCacheService: publicBlobCacheService ?? Mock.Of<IPublicBlobCacheService>(Strict),
            Mock.Of<ILogger<MethodologyCacheService>>()
        );
    }
}
