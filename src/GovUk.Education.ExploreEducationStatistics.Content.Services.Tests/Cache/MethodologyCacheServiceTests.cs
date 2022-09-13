#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
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
public class MethodologyCacheServiceTests : CacheServiceTestFixture
{
    private readonly List<AllMethodologiesThemeViewModel> _methodologyTree = ListOf(
        new AllMethodologiesThemeViewModel
        {
            Title = "Theme 1",
            Topics = ListOf(new AllMethodologiesTopicViewModel
            {
                Title = "Theme 1 Topic 1",
                Publications = ListOf(new AllMethodologiesPublicationViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme 1 Topic 1 Publication 1",
                    Methodologies = ListOf(new MethodologyVersionSummaryViewModel
                    {
                        Title = "Theme 1 Topic 1 Publication 1 Methodology 1",
                    })
                })
            })
        },
        new AllMethodologiesThemeViewModel
        {
            Title = "Theme 2",
            Topics = ListOf(new AllMethodologiesTopicViewModel
            {
                Title = "Theme 2 Topic 1",
                Publications = ListOf(new AllMethodologiesPublicationViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Theme 2 Topic 1 Publication 1",
                    Methodologies = ListOf(new MethodologyVersionSummaryViewModel
                    {
                        Title = "Theme 2 Topic 1 Publication 1 Methodology 1",
                    })
                })
            })
        });

    [Fact]
    public async Task GetSummariesTree_NoCachedTreeExists()
    {
        PublicBlobCacheService
            .Setup(s => s.GetItem(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);

        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree));

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(new AllMethodologiesCacheKey(), _methodologyTree))
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
            .Setup(s => s.GetItem(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(_methodologyTree);

        var service = SetupService();

        var result = await service.GetSummariesTree();

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task GetSummariesByPublication_NoCachedTreeExists()
    {
        var publicationId = _methodologyTree[1].Topics[0].Publications[0].Id;

        PublicBlobCacheService
            .Setup(s => s.GetItem(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);

        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree));

        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(methodologyService: methodologyService.Object);

        var result = await service.GetSummariesByPublication(publicationId);

        VerifyAllMocks(methodologyService, PublicBlobCacheService);

        var expectedMethodologiesByPublication =
            _methodologyTree[1].Topics[0].Publications[0].Methodologies;

        result.AssertRight(expectedMethodologiesByPublication);
    }

    [Fact]
    public async Task UpdateSummariesTree()
    {
        var methodologyService = new Mock<IMethodologyService>(Strict);

        methodologyService
            .Setup(s => s.GetSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(_methodologyTree));

        // We should not see any attempt to "get" the cached tree, but rather only see a fresh fetching
        // of the latest tree and then it being cached.
        PublicBlobCacheService
            .Setup(s => s.SetItem<object>(new AllMethodologiesCacheKey(), _methodologyTree))
            .Returns(Task.CompletedTask);

        var service = SetupService(methodologyService: methodologyService.Object);

        var result = await service.UpdateSummariesTree();

        VerifyAllMocks(methodologyService, PublicBlobCacheService);

        result.AssertRight(_methodologyTree);
    }

    [Fact]
    public async Task GetSummariesByPublication_CachedTreeExists()
    {
        var publicationId = _methodologyTree[1].Topics[0].Publications[0].Id;

        PublicBlobCacheService
            .Setup(s => s.GetItem(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(_methodologyTree);

        var service = SetupService();

        var result = await service.GetSummariesByPublication(publicationId);

        VerifyAllMocks(PublicBlobCacheService);

        var expectedMethodologiesByPublication =
            _methodologyTree[1].Topics[0].Publications[0].Methodologies;

        result.AssertRight(expectedMethodologiesByPublication);
    }

    [Fact]
    public async Task GetSummariesByPublication_PublicationNotFound()
    {
        var publicationId = Guid.NewGuid();

        PublicBlobCacheService
            .Setup(s => s.GetItem(new AllMethodologiesCacheKey(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(_methodologyTree);

        var service = SetupService();

        var result = await service.GetSummariesByPublication(publicationId);

        VerifyAllMocks(PublicBlobCacheService);

        result.AssertRight(new List<MethodologyVersionSummaryViewModel>());
    }

    [Fact]
    public void AllMethodologiesThemeViewModel_SerializeAndDeserialize()
    {
        var viewModel = new AllMethodologiesThemeViewModel
        {
            Id = Guid.NewGuid(),
            Title = "Publication title",
            Topics = new List<AllMethodologiesTopicViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Topic title",
                    Publications = new List<AllMethodologiesPublicationViewModel>
                    {
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
                                }
                            }
                        }
                    }
                }
            }
        };

        var converted = DeserializeObject<AllMethodologiesThemeViewModel>(SerializeObject(viewModel));
        converted.AssertDeepEqualTo(viewModel);
    }

    private static MethodologyCacheService SetupService(
        IMethodologyService? methodologyService = null)
    {
        return new(
            methodologyService: methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            Mock.Of<ILogger<MethodologyCacheService>>()
        );
    }
}
