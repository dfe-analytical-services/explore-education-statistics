#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Cache;

public class ReleaseCacheServiceTests
{
    private const string PublicationSlug = "publication-slug";
    private const string ReleaseSlug = "2022";
    private static readonly Guid ReleaseId = Guid.NewGuid();
    private static readonly Guid PublicationId = Guid.NewGuid();

    private readonly PublicationViewModel _publicationViewModel = new()
    {
        Id = PublicationId,
        Title = "Test publication",
        Slug = PublicationSlug,
        Releases = new List<ReleaseTitleViewModel>
        {
            new()
            {
                Id = ReleaseId
            },
            new()
            {
                Id = Guid.NewGuid()
            }
        }
    };

    private readonly List<MethodologyVersionSummaryViewModel> _methodologies = new()
    {
        new MethodologyVersionSummaryViewModel
        {
            Id = Guid.NewGuid(),
        }
    };

    private readonly CachedReleaseViewModel _releaseViewModel = new(ReleaseId)
    {
        Type = new ReleaseTypeViewModel
        {
            Title = "National Statistics"
        }
    };

    [Fact]
    public async Task GetRelease()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/releases/{ReleaseSlug}.json"))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(
            blobStorageService: blobStorageService.Object);

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(blobStorageService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_NotFound()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/releases/{ReleaseSlug}.json"))
            .ReturnsAsync((CachedReleaseViewModel?) null);

        var service = BuildService(blobStorageService: blobStorageService.Object);

        var result = await service.GetRelease(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(blobStorageService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetRelease_LatestRelease()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/latest-release.json"))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(
            blobStorageService: blobStorageService.Object);

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(blobStorageService);

        result.AssertRight(_releaseViewModel);
    }

    [Fact]
    public async Task GetRelease_LatestRelease_NotFound()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/latest-release.json"))
            .ReturnsAsync((CachedReleaseViewModel?) null);

        var service = BuildService(blobStorageService: blobStorageService.Object);

        var result = await service.GetRelease(PublicationSlug);

        VerifyAllMocks(blobStorageService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseAndPublication()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(PublicationId))
            .ReturnsAsync(_methodologies);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/releases/{ReleaseSlug}.json"))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object,
            methodologyCacheService: methodologyCacheService.Object);

        var result = await service.GetReleaseAndPublication(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(blobStorageService,
            methodologyCacheService,
            publicationCacheService);

        var releaseViewModel = result.AssertRight();

        Assert.Equal(ReleaseId, releaseViewModel.Id);
        Assert.Equal(ReleaseType.NationalStatistics, releaseViewModel.Type);

        var expectedPublicationViewModel = _publicationViewModel with
        {
            Methodologies = _methodologies,
            Releases = _publicationViewModel.Releases
                .Where(vm => vm.Id != ReleaseId)
                .ToList()
        };

        releaseViewModel.Publication.AssertDeepEqualTo(expectedPublicationViewModel);
    }

    [Fact]
    public async Task GetReleaseAndPublication_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(publicationCacheService: publicationCacheService.Object);

        var result = await service.GetReleaseAndPublication(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseAndPublication_ReleaseNotFound()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(PublicationId))
            .ReturnsAsync(_methodologies);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/releases/{ReleaseSlug}.json"))
            .ReturnsAsync((CachedReleaseViewModel?) null);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object,
            methodologyCacheService: methodologyCacheService.Object);

        var result = await service.GetReleaseAndPublication(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(blobStorageService,
            methodologyCacheService,
            publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseAndPublication_LatestRelease()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(PublicationId))
            .ReturnsAsync(_methodologies);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/latest-release.json"))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object,
            methodologyCacheService: methodologyCacheService.Object);

        var result = await service.GetReleaseAndPublication(PublicationSlug);

        VerifyAllMocks(blobStorageService,
            methodologyCacheService,
            publicationCacheService);

        var releaseViewModel = result.AssertRight();

        Assert.Equal(ReleaseId, releaseViewModel.Id);
        Assert.Equal(ReleaseType.NationalStatistics, releaseViewModel.Type);

        var expectedPublicationViewModel = _publicationViewModel with
        {
            Methodologies = _methodologies,
            Releases = _publicationViewModel.Releases
                .Where(vm => vm.Id != ReleaseId)
                .ToList()
        };

        releaseViewModel.Publication.AssertDeepEqualTo(expectedPublicationViewModel);
    }

    [Fact]
    public async Task GetReleaseAndPublication_LatestRelease_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(publicationCacheService: publicationCacheService.Object);

        var result = await service.GetReleaseAndPublication(PublicationSlug);

        VerifyAllMocks(publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseAndPublication_LatestRelease_ReleaseNotFound()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        methodologyCacheService.Setup(mock => mock.GetSummariesByPublication(PublicationId))
            .ReturnsAsync(ListOf(new MethodologyVersionSummaryViewModel()));

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/latest-release.json"))
            .ReturnsAsync((CachedReleaseViewModel?) null);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object,
            methodologyCacheService: methodologyCacheService.Object);

        var result = await service.GetReleaseAndPublication(PublicationSlug);

        VerifyAllMocks(blobStorageService,
            methodologyCacheService,
            publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseSummary()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/releases/{ReleaseSlug}.json"))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object
        );

        var result = await service.GetReleaseSummary(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(blobStorageService, publicationCacheService);

        var releaseSummaryViewModel = result.AssertRight();

        Assert.Equal(ReleaseId, releaseSummaryViewModel.Id);
        Assert.Equal(ReleaseType.NationalStatistics, releaseSummaryViewModel.Type);

        var publication = releaseSummaryViewModel.Publication;
        Assert.NotNull(publication);
        Assert.Equal(PublicationId, publication!.Id);
        Assert.Equal(PublicationSlug, publication.Slug);
        Assert.Equal("Test publication", publication.Title);
    }

    [Fact]
    public async Task GetReleaseSummary_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(publicationCacheService: publicationCacheService.Object);

        var result = await service.GetReleaseSummary(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseSummary_ReleaseNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/releases/{ReleaseSlug}.json"))
            .ReturnsAsync((CachedReleaseViewModel?) null);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object);

        var result = await service.GetReleaseSummary(PublicationSlug, ReleaseSlug);

        VerifyAllMocks(blobStorageService, publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseSummary_LatestRelease()
    {
        var blobStorageService = new Mock<IBlobStorageService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/latest-release.json"))
            .ReturnsAsync(_releaseViewModel);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object
        );

        var result = await service.GetReleaseSummary(PublicationSlug);

        VerifyAllMocks(blobStorageService, publicationCacheService);

        var releaseSummaryViewModel = result.AssertRight();

        Assert.Equal(ReleaseId, releaseSummaryViewModel.Id);
        Assert.Equal(ReleaseType.NationalStatistics, releaseSummaryViewModel.Type);

        var publication = releaseSummaryViewModel.Publication;
        Assert.NotNull(publication);
        Assert.Equal(PublicationId, publication!.Id);
        Assert.Equal(PublicationSlug, publication.Slug);
        Assert.Equal("Test publication", publication.Title);
    }

    [Fact]
    public async Task GetReleaseSummary_LatestRelease_PublicationNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var service = BuildService(publicationCacheService: publicationCacheService.Object);

        var result = await service.GetReleaseSummary(PublicationSlug);

        VerifyAllMocks(publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetReleaseSummary_LatestRelease_ReleaseNotFound()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        publicationCacheService.Setup(mock => mock.GetPublication(PublicationSlug))
            .ReturnsAsync(_publicationViewModel);

        blobStorageService
            .Setup(s => s.GetDeserializedJson<CachedReleaseViewModel>(
                BlobContainers.PublicContent,
                $"publications/{PublicationSlug}/latest-release.json"))
            .ReturnsAsync((CachedReleaseViewModel?) null);

        var service = BuildService(
            blobStorageService: blobStorageService.Object,
            publicationCacheService: publicationCacheService.Object);

        var result = await service.GetReleaseSummary(PublicationSlug);

        VerifyAllMocks(blobStorageService, publicationCacheService);

        result.AssertNotFound();
    }

    private static ReleaseCacheService BuildService(
        IBlobStorageService? blobStorageService = null,
        IMethodologyCacheService? methodologyCacheService = null,
        IPublicationCacheService? publicationCacheService = null
    )
    {
        return new ReleaseCacheService(
            blobStorageService: blobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
            methodologyCacheService: methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            publicationCacheService: publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict)
        );
    }
}
