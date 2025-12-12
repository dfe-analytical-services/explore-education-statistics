#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class SubjectResultMetaServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly Country _england = new("E92000001", "England");
    private readonly Region _northEast = new("E12000001", "North East");
    private readonly Region _northWest = new("E12000002", "North West");
    private readonly Region _eastMidlands = new("E12000004", "East Midlands");

    private static readonly BoundaryLevel _countriesBoundaryLevel = new()
    {
        Id = 1,
        Label = "Countries November 2021",
        Level = GeographicLevel.Country,
    };

    private readonly BoundaryLevel _regionsBoundaryLevel = new()
    {
        Id = 2,
        Label = "Regions November 2021",
        Level = GeographicLevel.Region,
    };

    [Fact]
    public async Task GetSubjectMeta_SubjectNotFound()
    {
        var query = new FullTableQuery();

        var contextId = Guid.NewGuid().ToString();

        await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
        var service = BuildService(statisticsDbContext);

        var result = await service.GetSubjectMeta(
            releaseVersionId: Guid.NewGuid(),
            query,
            observations: [],
            isCroppedTable: false
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetSubjectMeta_EmptyModelReturnedForSubject()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1));

        var releaseVersion = publication.ReleaseVersions[0];

        Subject subject = _dataFixture.DefaultSubject();

        ReleaseSubject releaseSubject = _dataFixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _dataFixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            )
            .WithSubject(subject);

        ReleaseFile releaseFile = _dataFixture.DefaultReleaseFile();

        var observations = new List<Observation>();

        var query = new FullTableQuery { Indicators = new List<Guid>(), SubjectId = subject.Id };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(Strict);
        var filterItemRepository = new Mock<IFilterItemRepository>(Strict);
        var locationService = new Mock<ILocationService>(Strict);
        var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
        var indicatorRepository = new Mock<IIndicatorRepository>(Strict);
        var locationRepository = new Mock<ILocationRepository>(Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);
        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(new List<GeographicLevel>())).Returns([]);

        filterItemRepository.Setup(s => s.GetFilterItemsFromObservations(observations)).ReturnsAsync([]);

        locationService
            .Setup(s =>
                s.GetLocationViewModels(
                    It.IsAny<List<Location>>(),
                    It.IsAny<Dictionary<GeographicLevel, List<string>>>(),
                    null
                )
            )
            .ReturnsAsync([]);

        footnoteRepository
            .Setup(s => s.GetFilteredFootnotes(releaseVersion.Id, subject.Id, new List<Guid>(), query.Indicators))
            .ReturnsAsync([]);

        indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators)).Returns([]);

        releaseDataFileRepository.Setup(s => s.GetBySubject(releaseVersion.Id, subject.Id)).ReturnsAsync(releaseFile);

        timePeriodService.Setup(s => s.GetTimePeriodRange(observations)).Returns([]);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                boundaryLevelRepository: boundaryLevelRepository.Object,
                filterItemRepository: filterItemRepository.Object,
                locationService: locationService.Object,
                footnoteRepository: footnoteRepository.Object,
                indicatorRepository: indicatorRepository.Object,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = await service.GetSubjectMeta(releaseVersion.Id, query, observations, isCroppedTable: false);

            VerifyAllMocks(
                boundaryLevelRepository,
                filterItemRepository,
                footnoteRepository,
                indicatorRepository,
                locationRepository,
                releaseDataFileRepository,
                timePeriodService
            );

            var viewModel = result.AssertRight();

            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Footnotes);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.Locations);
            Assert.Empty(viewModel.BoundaryLevels);
            Assert.Empty(viewModel.TimePeriodRange);
            Assert.Equal(publication.Title, viewModel.PublicationName);
            Assert.Equal(releaseFile.Name, viewModel.SubjectName);
            Assert.False(viewModel.GeoJsonAvailable);
        }
    }

    [Fact]
    public async Task GetSubjectMeta_BoundaryLevelViewModelsReturnedForSubject()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture.DefaultRelease(publishedVersions: 1).Generate(1));

        var releaseVersion = publication.ReleaseVersions[0];

        Subject subject = _dataFixture.DefaultSubject();

        ReleaseSubject releaseSubject = _dataFixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _dataFixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            )
            .WithSubject(subject);

        var observations = ListOf(
            new Observation
            {
                Location = new Location { GeographicLevel = GeographicLevel.Country, Country = _england },
            },
            new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Region,
                    Country = _england,
                    Region = _northWest,
                },
            },
            new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Region,
                    Country = _england,
                    Region = _northEast,
                },
            },
            new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Region,
                    Country = _england,
                    Region = _eastMidlands,
                },
            }
        );

        var options = new LocationsOptions
        {
            Hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                { GeographicLevel.Region, ["Country", "Region"] },
            },
        }.ToOptionsWrapper();

        var query = new FullTableQuery { Indicators = new List<Guid>(), SubjectId = subject.Id };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(Strict);
        var filterItemRepository = new Mock<IFilterItemRepository>(Strict);
        var footnoteRepository = new Mock<IFootnoteRepository>(Strict);
        var indicatorRepository = new Mock<IIndicatorRepository>(Strict);
        var locationService = new Mock<ILocationService>(Strict);
        var locationRepository = new Mock<ILocationRepository>(Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);
        var timePeriodService = new Mock<ITimePeriodService>(Strict);

        locationService
            .Setup(s =>
                s.GetLocationViewModels(
                    It.IsAny<List<Location>>(),
                    It.IsAny<Dictionary<GeographicLevel, List<string>>>(),
                    null
                )
            )
            .ReturnsAsync([]);

        boundaryLevelRepository
            .Setup(s =>
                s.FindByGeographicLevels(new List<GeographicLevel> { GeographicLevel.Country, GeographicLevel.Region })
            )
            .Returns(new List<BoundaryLevel> { _countriesBoundaryLevel, _regionsBoundaryLevel });

        filterItemRepository.Setup(s => s.GetFilterItemsFromObservations(observations)).ReturnsAsync([]);

        footnoteRepository
            .Setup(s => s.GetFilteredFootnotes(releaseVersion.Id, subject.Id, new List<Guid>(), query.Indicators))
            .ReturnsAsync([]);

        indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators)).Returns([]);

        releaseDataFileRepository
            .Setup(s => s.GetBySubject(releaseVersion.Id, subject.Id))
            .ReturnsAsync(new ReleaseFile());

        timePeriodService.Setup(s => s.GetTimePeriodRange(observations)).Returns([]);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                boundaryLevelRepository: boundaryLevelRepository.Object,
                locationService: locationService.Object,
                filterItemRepository: filterItemRepository.Object,
                footnoteRepository: footnoteRepository.Object,
                indicatorRepository: indicatorRepository.Object,
                releaseDataFileRepository: releaseDataFileRepository.Object,
                timePeriodService: timePeriodService.Object,
                options: options
            );

            var result = await service.GetSubjectMeta(releaseVersion.Id, query, observations, isCroppedTable: false);

            VerifyAllMocks(
                boundaryLevelRepository,
                filterItemRepository,
                footnoteRepository,
                indicatorRepository,
                locationRepository,
                releaseDataFileRepository,
                timePeriodService
            );

            var viewModel = result.AssertRight();

            var boundaryLevels = viewModel.BoundaryLevels;

            Assert.Equal(2, boundaryLevels.Count);
            Assert.Equal(1, boundaryLevels[0].Id);
            Assert.Equal("Countries November 2021", boundaryLevels[0].Label);
            Assert.Equal(2, boundaryLevels[1].Id);
            Assert.Equal("Regions November 2021", boundaryLevels[1].Label);
            Assert.True(viewModel.GeoJsonAvailable);
        }
    }

    private static IOptions<LocationsOptions> DefaultLocationsOptions()
    {
        return new LocationsOptions().ToOptionsWrapper();
    }

    private static SubjectResultMetaService BuildService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
        IFilterItemRepository? filterItemRepository = null,
        ILocationService? locationService = null,
        IBoundaryLevelRepository? boundaryLevelRepository = null,
        IFootnoteRepository? footnoteRepository = null,
        IIndicatorRepository? indicatorRepository = null,
        ITimePeriodService? timePeriodService = null,
        IUserService? userService = null,
        ISubjectRepository? subjectRepository = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null,
        IOptions<LocationsOptions>? options = null
    )
    {
        return new(
            contentDbContext ?? InMemoryContentDbContext(),
            statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
            boundaryLevelRepository ?? Mock.Of<IBoundaryLevelRepository>(Strict),
            filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
            footnoteRepository ?? Mock.Of<IFootnoteRepository>(Strict),
            indicatorRepository ?? Mock.Of<IIndicatorRepository>(Strict),
            locationService ?? Mock.Of<ILocationService>(Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict),
            userService ?? AlwaysTrueUserService().Object,
            subjectRepository ?? new SubjectRepository(statisticsDbContext),
            releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>(Strict),
            options ?? DefaultLocationsOptions(),
            Mock.Of<ILogger<SubjectResultMetaService>>()
        );
    }
}
