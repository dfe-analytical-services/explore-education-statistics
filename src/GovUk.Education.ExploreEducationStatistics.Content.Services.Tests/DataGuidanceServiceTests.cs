using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class DataGuidanceServiceTests
{
    private static readonly List<DataGuidanceDataSetViewModel> DataGuidanceDataSets =
        new()
        {
            new DataGuidanceDataSetViewModel
            {
                FileId = Guid.NewGuid(),
                Content = "Data guidance",
                Filename = "data.csv",
                Name = "Data set name",
                GeographicLevels = new List<string>
                {
                    "National", "Local authority", "Local authority district"
                },
                TimePeriods = new TimePeriodLabels("2020_AYQ3", "2021_AYQ1"),
                Variables = new List<LabelValue>
                {
                    new("Filter label", "test_filter"),
                    new("Indicator label", "test_indicator")
                }
            }
        };

    [Fact]
    public async Task GetDataGuidance()
    {
        var publicationId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();

        const string publicationSlug = "test-publication";
        const string releaseSlug = "2016-17";

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

        var service = SetupService(
            dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object
        );

        publicationCacheService.Setup(mock => mock.GetPublication(publicationSlug))
            .ReturnsAsync(
                new PublicationCacheViewModel
                {
                    Id = publicationId,
                    Title = "Test publication",
                    Slug = publicationSlug,
                }
            );

        releaseCacheService.Setup(mock => mock.GetRelease(publicationSlug, releaseSlug))
            .ReturnsAsync(
                new ReleaseCacheViewModel(releaseVersionId)
                {
                    Title = "2016-17",
                    Slug = "2016-17",
                    DataGuidance = "Release Guidance"
                }
            );

        dataGuidanceDataSetService.Setup(
                mock => mock.ListDataSets(releaseVersionId, null, default)
            )
            .ReturnsAsync(DataGuidanceDataSets);

        var result = await service.GetDataGuidance(publicationSlug, releaseSlug);

        VerifyAllMocks(publicationCacheService,
            releaseCacheService,
            dataGuidanceDataSetService);

        var viewModel = result.AssertRight();

        Assert.Equal(releaseVersionId, viewModel.Id);
        Assert.Equal("2016-17", viewModel.Title);
        Assert.Equal("2016-17", viewModel.Slug);
        Assert.Equal("Release Guidance", viewModel.DataGuidance);
        Assert.Equal(DataGuidanceDataSets, viewModel.DataSets);

        Assert.Equal(publicationId, viewModel.Publication!.Id);
        Assert.Equal("Test publication", viewModel.Publication.Title);
        Assert.Equal("test-publication", viewModel.Publication.Slug);
    }

    [Fact]
    public async Task GetDataGuidance_NotFoundForPublicationSlug()
    {
        const string publicationSlug = "incorrect-publication-slug";
        const string releaseSlug = "2016-17";

        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        var service = SetupService(
            publicationCacheService: publicationCacheService.Object
        );

        publicationCacheService.Setup(mock => mock.GetPublication(publicationSlug))
            .ReturnsAsync(new NotFoundResult());

        var result = await service.GetDataGuidance(publicationSlug, releaseSlug);

        VerifyAllMocks(publicationCacheService);

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetDataGuidance_NotFoundForReleaseSlug()
    {
        const string publicationSlug = "test-publication";
        const string releaseSlug = "incorrect-release-slug";

        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
        var releaseCacheService = new Mock<IReleaseCacheService>(Strict);

        var service = SetupService(
            publicationCacheService: publicationCacheService.Object,
            releaseCacheService: releaseCacheService.Object
        );

        publicationCacheService.Setup(mock => mock.GetPublication(publicationSlug))
            .ReturnsAsync(new PublicationCacheViewModel());

        releaseCacheService.Setup(mock => mock.GetRelease(publicationSlug, releaseSlug))
            .ReturnsAsync(
                new NotFoundResult()
            );

        var result = await service.GetDataGuidance(publicationSlug, releaseSlug);

        VerifyAllMocks(publicationCacheService, releaseCacheService);

        result.AssertNotFound();
    }

    private static DataGuidanceService SetupService(
        IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
        IPublicationCacheService? publicationCacheService = null,
        IReleaseCacheService? releaseCacheService = null)
    {
        return new DataGuidanceService(
            dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(Strict),
            publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
            releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict)
        );
    }
}
