using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
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

public class MethodologyCacheServiceTests : CacheServiceTestFixture
{
    private readonly List<AllMethodologiesThemeViewModel> _methodologyTree =
    [
        new AllMethodologiesThemeViewModel
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
        new AllMethodologiesThemeViewModel
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
        PublicBlobCacheService
            .Setup(s =>
                s.GetItemAsync(
                    new AllMethodologiesCacheKey(),
                    typeof(List<AllMethodologiesThemeViewModel>)
                )
            )
            .ReturnsAsync((object?)null);

        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(
                new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree)
            );

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(methodologyService: methodologyService.Object);

        var result = await service.GetSummariesTree();

        VerifyAllMocks(methodologyService, PublicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task GetSummariesTree_CachedTreeExists()
    {
        PublicBlobCacheService
            .Setup(s =>
                s.GetItemAsync(
                    new AllMethodologiesCacheKey(),
                    typeof(List<AllMethodologiesThemeViewModel>)
                )
            )
            .ReturnsAsync(_methodologyTree);

        var service = SetupService();

        var result = await service.GetSummariesTree();

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task GetSummariesByPublication_NoCachedTreeExists()
    {
        var publicationId = _methodologyTree[1].Publications[0].Id;

        PublicBlobCacheService
            .Setup(s =>
                s.GetItemAsync(
                    new AllMethodologiesCacheKey(),
                    typeof(List<AllMethodologiesThemeViewModel>)
                )
            )
            .ReturnsAsync((object?)null);

        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(
                new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree)
            );

        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(methodologyService: methodologyService.Object);

        var result = await service.GetSummariesByPublication(publicationId);

        VerifyAllMocks(methodologyService, PublicBlobCacheService);

        var expectedMethodologiesByPublication = _methodologyTree[1].Publications[0].Methodologies;

        result.AssertRight(expectedMethodologiesByPublication);
    }

    [Fact]
    public async Task UpdateSummariesTree()
    {
        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(
                new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree)
            );

        // We should not see any attempt to "get" the cached tree, but rather only see a fresh fetching
        // of the latest tree and then it being cached.
        PublicBlobCacheService
            .Setup(s => s.SetItemAsync<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(methodologyService: methodologyService.Object);

        var result = await service.UpdateSummariesTree();

        VerifyAllMocks(methodologyService, PublicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task GetSummariesByPublication_CachedTreeExists()
    {
        var publicationId = _methodologyTree[1].Publications[0].Id;

        PublicBlobCacheService
            .Setup(s =>
                s.GetItemAsync(
                    new AllMethodologiesCacheKey(),
                    typeof(List<AllMethodologiesThemeViewModel>)
                )
            )
            .ReturnsAsync(_methodologyTree);

        var service = SetupService();

        var result = await service.GetSummariesByPublication(publicationId);

        VerifyAllMocks(PublicBlobCacheService);

        var expectedMethodologiesByPublication = _methodologyTree[1].Publications[0].Methodologies;

        result.AssertRight(expectedMethodologiesByPublication);
    }

    [Fact]
    public async Task GetSummariesByPublication_PublicationNotFound()
    {
        var publicationId = Guid.NewGuid();

        PublicBlobCacheService
            .Setup(s =>
                s.GetItemAsync(
                    new AllMethodologiesCacheKey(),
                    typeof(List<AllMethodologiesThemeViewModel>)
                )
            )
            .ReturnsAsync(_methodologyTree);

        var service = SetupService();

        var result = await service.GetSummariesByPublication(publicationId);

        VerifyAllMocks(PublicBlobCacheService);

        var viewModels = result.AssertRight();
        Assert.Empty(viewModels);
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

        var converted = DeserializeObject<AllMethodologiesThemeViewModel>(
            SerializeObject(viewModel)
        );
        converted.AssertDeepEqualTo(viewModel);
    }

    private static MethodologyCacheService SetupService(
        IMethodologyService? methodologyService = null
    )
    {
        return new(
            methodologyService: methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            Mock.Of<ILogger<MethodologyCacheService>>()
        );
    }
}
