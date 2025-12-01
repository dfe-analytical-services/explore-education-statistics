#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.Extensions.Options;
using Moq;
using Snapshooter.Xunit;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class TableBuilderServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task Query_LatestRelease()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var indicator1Id = Guid.NewGuid();
        var indicator2Id = Guid.NewGuid();
        var location1Id = Guid.NewGuid();
        var location2Id = Guid.NewGuid();
        var location3Id = Guid.NewGuid();

        var observations = new List<Observation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Location = new Location { Id = location1Id, GeographicLevel = GeographicLevel.Country },
                Measures = new Dictionary<Guid, string> { { indicator1Id, "123" }, { indicator2Id, "456" } },
                FilterItems = ListOf(
                    new ObservationFilterItem { FilterItem = new FilterItem("Filter Item 1", Guid.NewGuid()) }
                ),
                Year = 2019,
                TimeIdentifier = AcademicYear,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Location = new Location { Id = location2Id, GeographicLevel = GeographicLevel.Institution },
                Measures = new Dictionary<Guid, string> { { indicator1Id, "678" } },
                FilterItems = ListOf(
                    new ObservationFilterItem { FilterItem = new FilterItem("Filter Item 2", Guid.NewGuid()) }
                ),
                Year = 2020,
                TimeIdentifier = AcademicYear,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Location = new Location { Id = location3Id, GeographicLevel = GeographicLevel.Provider },
                Measures = new Dictionary<Guid, string>
                {
                    { indicator1Id, "789" },
                    { Guid.NewGuid(), "1123" },
                    { Guid.NewGuid(), "1456" },
                },
                FilterItems = ListOf(
                    new ObservationFilterItem { FilterItem = new FilterItem("Filter Item 3", Guid.NewGuid()) }
                ),
                Year = 2020,
                TimeIdentifier = AcademicYear,
            },
        };

        var subjectMeta = new SubjectResultMetaViewModel
        {
            Indicators = new List<IndicatorMetaViewModel> { new() { Label = "Test indicator" } },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            statisticsDbContext.Observation.AddRange(observations);
            statisticsDbContext.MatchedObservations.AddRange(
                new MatchedObservation(observations[0].Id),
                new MatchedObservation(observations[2].Id)
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var query = new FullTableQuery
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = new[] { indicator1Id, indicator2Id },
                LocationIds = ListOf(location1Id, location2Id, location3Id),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = AcademicYear,
                    EndYear = 2020,
                    EndCode = AcademicYear,
                },
            };

            var tableBuilderQueryOptimiser = new Mock<ITableBuilderQueryOptimiser>(Strict);
            var observationService = new Mock<IObservationService>(Strict);
            var matchedObservationsTable = Mock.Of<ITempTableReference>(Strict);
            var subjectResultMetaService = new Mock<ISubjectResultMetaService>(Strict);

            tableBuilderQueryOptimiser
                .Setup(mock => mock.IsCroppingRequired(It.IsAny<FullTableQuery>()))
                .ReturnsAsync(false);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationsTable);

            subjectResultMetaService
                .Setup(s =>
                    s.GetSubjectMeta(releaseVersion.Id, query, It.IsAny<IList<Observation>>(), It.IsAny<bool>())
                )
                .ReturnsAsync(subjectMeta);

            var service = BuildTableBuilderService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                observationService: observationService.Object,
                subjectResultMetaService: subjectResultMetaService.Object,
                tableBuilderQueryOptimiser: tableBuilderQueryOptimiser.Object
            );

            var result = await service.Query(query);

            VerifyAllMocks(observationService, subjectResultMetaService, tableBuilderQueryOptimiser);

            var observationResults = result.AssertRight().Results.ToList();

            Assert.Equal(2, observationResults.Count);

            Assert.Equal(observations[0].Id, observationResults[0].Id);
            Assert.Equal(GeographicLevel.Country, observationResults[0].GeographicLevel);
            Assert.Equal(location1Id, observationResults[0].LocationId);
            Assert.Equal("2019_AY", observationResults[0].TimePeriod);
            Assert.Equal(2, observationResults[0].Measures.Count);
            Assert.Equal("123", observationResults[0].Measures[indicator1Id]);
            Assert.Equal("456", observationResults[0].Measures[indicator2Id]);
            Assert.Equal(ListOf(observations[0].FilterItems.ToList()[0].FilterItemId), observationResults[0].Filters);

            Assert.Equal(observations[2].Id, observationResults[1].Id);
            Assert.Equal(GeographicLevel.Provider, observationResults[1].GeographicLevel);
            Assert.Equal(location3Id, observationResults[1].LocationId);
            Assert.Equal("2020_AY", observationResults[1].TimePeriod);
            Assert.Single(observationResults[1].Measures);
            Assert.Equal("789", observationResults[1].Measures[indicator1Id]);
            Assert.Equal(ListOf(observations[2].FilterItems.ToList()[0].FilterItemId), observationResults[1].Filters);

            Assert.Equal(subjectMeta, result.Right.SubjectMeta);
        }
    }

    [Fact]
    public async Task Query_LatestRelease_SubjectNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext, contentDbContext: contentDbContext);

            var query = new FullTableQuery
            {
                // Does not match the saved release subject
                SubjectId = Guid.NewGuid(),
            };

            var result = await service.Query(query);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Query_LatestRelease_PublicationNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        // Set up a ReleaseSubject that references a non-existent publication
        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(Guid.NewGuid())
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext, contentDbContext: contentDbContext);

            var query = new FullTableQuery { SubjectId = releaseSubject.SubjectId };

            var result = await service.Query(query);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Query_LatestRelease_ReleaseNotFound()
    {
        // Set up a ReleaseSubject that references a non-existent release version
        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(_fixture.DefaultStatsReleaseVersion());

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext, contentDbContext: contentDbContext);

            // SubjectId exists, but no Content.Model.Release
            var query = new FullTableQuery { SubjectId = releaseSubject.SubjectId };

            var result = await service.Query(query);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Query_ReleaseId()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var indicator1Id = Guid.NewGuid();
        var indicator2Id = Guid.NewGuid();
        var location1Id = Guid.NewGuid();
        var location2Id = Guid.NewGuid();
        var location3Id = Guid.NewGuid();

        var observations = new List<Observation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Location = new Location { Id = location1Id, GeographicLevel = GeographicLevel.Country },
                Measures = new Dictionary<Guid, string> { { indicator1Id, "123" }, { indicator2Id, "456" } },
                FilterItems = ListOf(
                    new ObservationFilterItem { FilterItem = new FilterItem("Filter Item 1", Guid.NewGuid()) }
                ),
                Year = 2019,
                TimeIdentifier = AcademicYear,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Location = new Location { Id = location2Id, GeographicLevel = GeographicLevel.Institution },
                Measures = new Dictionary<Guid, string> { { indicator1Id, "678" } },
                FilterItems = ListOf(
                    new ObservationFilterItem { FilterItem = new FilterItem("Filter Item 2", Guid.NewGuid()) }
                ),
                Year = 2020,
                TimeIdentifier = AcademicYear,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Location = new Location { Id = location3Id, GeographicLevel = GeographicLevel.Provider },
                Measures = new Dictionary<Guid, string>
                {
                    { indicator1Id, "789" },
                    { Guid.NewGuid(), "1123" },
                    { Guid.NewGuid(), "1456" },
                },
                FilterItems = ListOf(
                    new ObservationFilterItem { FilterItem = new FilterItem("Filter Item 3", Guid.NewGuid()) }
                ),
                Year = 2020,
                TimeIdentifier = AcademicYear,
            },
        };

        var subjectMeta = new SubjectResultMetaViewModel
        {
            Indicators = new List<IndicatorMetaViewModel> { new() { Label = "Test indicator" } },
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject);
            statisticsDbContext.Observation.AddRange(observations);
            statisticsDbContext.MatchedObservations.AddRange(
                new MatchedObservation(observations[0].Id),
                new MatchedObservation(observations[2].Id)
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var query = new FullTableQuery
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = new[] { indicator1Id, indicator2Id },
                LocationIds = ListOf(location1Id, location2Id, location3Id),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = AcademicYear,
                    EndYear = 2020,
                    EndCode = AcademicYear,
                },
            };

            var tableBuilderQueryOptimiser = new Mock<ITableBuilderQueryOptimiser>(Strict);
            var observationService = new Mock<IObservationService>(Strict);
            var matchedObservationsTable = Mock.Of<ITempTableReference>(Strict);
            var subjectResultMetaService = new Mock<ISubjectResultMetaService>(Strict);

            tableBuilderQueryOptimiser
                .Setup(mock => mock.IsCroppingRequired(It.IsAny<FullTableQuery>()))
                .ReturnsAsync(false);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationsTable);

            subjectResultMetaService
                .Setup(s =>
                    s.GetSubjectMeta(
                        releaseSubject.ReleaseVersionId,
                        query,
                        It.IsAny<IList<Observation>>(),
                        It.IsAny<bool>()
                    )
                )
                .ReturnsAsync(subjectMeta);

            var service = BuildTableBuilderService(
                statisticsDbContext: statisticsDbContext,
                observationService: observationService.Object,
                subjectResultMetaService: subjectResultMetaService.Object,
                tableBuilderQueryOptimiser: tableBuilderQueryOptimiser.Object
            );

            var result = await service.Query(releaseSubject.ReleaseVersionId, query);

            VerifyAllMocks(observationService, subjectResultMetaService, tableBuilderQueryOptimiser);

            var observationResults = result.AssertRight().Results.ToList();

            Assert.Equal(2, observationResults.Count);

            Assert.Equal(observations[0].Id, observationResults[0].Id);
            Assert.Equal(GeographicLevel.Country, observationResults[0].GeographicLevel);
            Assert.Equal(location1Id, observationResults[0].LocationId);
            Assert.Equal("2019_AY", observationResults[0].TimePeriod);
            Assert.Equal(2, observationResults[0].Measures.Count);
            Assert.Equal("123", observationResults[0].Measures[indicator1Id]);
            Assert.Equal("456", observationResults[0].Measures[indicator2Id]);
            Assert.Equal(ListOf(observations[0].FilterItems.ToList()[0].FilterItemId), observationResults[0].Filters);

            Assert.Equal(observations[2].Id, observationResults[1].Id);
            Assert.Equal(GeographicLevel.Provider, observationResults[1].GeographicLevel);
            Assert.Equal(location3Id, observationResults[1].LocationId);
            Assert.Equal("2020_AY", observationResults[1].TimePeriod);
            Assert.Single(observationResults[1].Measures);
            Assert.Equal("789", observationResults[1].Measures[indicator1Id]);
            Assert.Equal(ListOf(observations[2].FilterItems.ToList()[0].FilterItemId), observationResults[1].Filters);

            Assert.Equal(subjectMeta, result.Right.SubjectMeta);
        }
    }

    [Fact]
    public async Task Query_ReleaseVersionId_ReleaseVersionNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext);

            var query = new FullTableQuery { SubjectId = releaseSubject.SubjectId };

            // Query using a non-existent release version id
            var result = await service.Query(releaseVersionId: Guid.NewGuid(), query);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Query_ReleaseVersionId_SubjectNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext);

            var query = new FullTableQuery
            {
                // Does not match saved release subject
                SubjectId = Guid.NewGuid(),
            };

            var result = await service.Query(releaseVersionId: releaseVersion.Id, query);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task QueryToCsvStream_LatestRelease()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var filters = _fixture
            .DefaultFilter()
            .ForIndex(
                0,
                s =>
                    s.SetGroupCsvColumn("filter_0_grouping")
                        .SetFilterGroups(
                            _fixture
                                .DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(setters =>
                                    setters.Set(
                                        fg => fg.Label,
                                        (_, _, context) => $"Filter group {context.FixtureTypeIndex}"
                                    )
                                )
                                .Generate(2)
                        )
            )
            .ForIndex(
                1,
                s =>
                    s.SetGroupCsvColumn("filter_1_grouping")
                        .SetFilterGroups(
                            _fixture
                                .DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(setters =>
                                    setters.Set(
                                        fg => fg.Label,
                                        (_, _, context) => $"Filter group {context.FixtureTypeIndex}"
                                    )
                                )
                                .Generate(2)
                        )
            )
            .ForIndex(2, s => s.SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2).Generate(1)))
            .GenerateList();

        var filter0Items = filters[0].FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

        var filter1Items = filters[1].FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

        var filter2Items = filters[2].FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

        var indicatorGroups = _fixture
            .DefaultIndicatorGroup()
            .ForIndex(0, i => i.SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, i => i.SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups.SelectMany(ig => ig.Indicators).ToList();

        var locations = _fixture
            .DefaultLocation()
            .ForRange(..2, l => l.SetPresetRegion().SetGeographicLevel(GeographicLevel.Region))
            .ForRange(
                2..4,
                l => l.SetPresetRegionAndLocalAuthority().SetGeographicLevel(GeographicLevel.LocalAuthority)
            )
            .GenerateList();

        var observations = _fixture
            .DefaultObservation()
            .WithMeasures(indicators)
            .ForRange(
                ..2,
                o =>
                    o.SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                        .SetLocation(locations[0])
                        .SetTimePeriod(2022, AcademicYear)
            )
            .ForRange(
                2..4,
                o =>
                    o.SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                        .SetLocation(locations[1])
                        .SetTimePeriod(2022, AcademicYear)
            )
            .ForRange(
                4..6,
                o =>
                    o.SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                        .SetLocation(locations[2])
                        .SetTimePeriod(2023, AcademicYear)
            )
            .ForRange(
                6..8,
                o =>
                    o.SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                        .SetLocation(locations[3])
                        .SetTimePeriod(2023, AcademicYear)
            )
            .GenerateList();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture
                    .DefaultStatsReleaseVersion()
                    .WithId(publication.Releases[0].Versions[0].Id)
                    .WithPublicationId(publication.Id)
            )
            .WithSubject(
                _fixture
                    .DefaultSubject()
                    .WithFilters(filters)
                    .WithIndicatorGroups(indicatorGroups)
                    .WithObservations(observations)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            statisticsDbContext.Observation.AddRange(observations);
            statisticsDbContext.MatchedObservations.AddRange(
                observations.Select(o => new MatchedObservation(o.Id)).ToArray()
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var query = new FullTableQuery
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = indicators.Select(i => i.Id).ToList(),
                LocationIds = locations.Select(l => l.Id).ToList(),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2022,
                    StartCode = AcademicYear,
                    EndYear = 2023,
                    EndCode = AcademicYear,
                },
            };

            var tableBuilderQueryOptimiser = new Mock<ITableBuilderQueryOptimiser>(Strict);
            var observationService = new Mock<IObservationService>(Strict);
            var matchedObservationsTable = Mock.Of<ITempTableReference>(Strict);
            var subjectCsvMetaService = new Mock<ISubjectCsvMetaService>(Strict);

            tableBuilderQueryOptimiser
                .Setup(mock => mock.IsCroppingRequired(It.IsAny<FullTableQuery>()))
                .ReturnsAsync(false);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationsTable);

            var subjectCsvMeta = new SubjectCsvMetaViewModel
            {
                Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(
                    filter0Items.Concat(filter1Items).Concat(filter2Items)
                ),
                Indicators = indicators.Select(i => new IndicatorCsvMetaViewModel(i)).ToDictionary(i => i.Name),
                Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                Headers =
                [
                    "time_period",
                    "time_identifier",
                    "geographic_level",
                    "country_code",
                    "country_name",
                    "region_code",
                    "region_name",
                    "new_la_code",
                    "old_la_code",
                    "la_name",
                    "filter_0_grouping",
                    "filter_1_grouping",
                    filters[0].Name,
                    filters[1].Name,
                    filters[2].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name,
                ],
            };

            subjectCsvMetaService
                .Setup(s =>
                    s.GetSubjectCsvMeta(
                        It.Is<ReleaseSubject>(rs =>
                            rs.ReleaseVersionId == releaseSubject.ReleaseVersionId
                            && rs.SubjectId == releaseSubject.SubjectId
                        ),
                        query,
                        It.IsAny<IList<Observation>>(),
                        default
                    )
                )
                .ReturnsAsync(subjectCsvMeta);

            var service = BuildTableBuilderService(
                statisticsDbContext,
                contentDbContext,
                observationService: observationService.Object,
                subjectCsvMetaService: subjectCsvMetaService.Object,
                tableBuilderQueryOptimiser: tableBuilderQueryOptimiser.Object
            );

            using var stream = new MemoryStream();

            var result = await service.QueryToCsvStream(query, stream);

            VerifyAllMocks(observationService, subjectCsvMetaService, tableBuilderQueryOptimiser);

            result.AssertRight();

            stream.SeekToBeginning();

            Snapshot.Match(stream.ReadToEnd());
        }
    }

    [Fact]
    public async Task QueryToCsvStream_LatestRelease_ReleaseVersionNotFound()
    {
        // Set up a ReleaseSubject that references a non-existent release
        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(_fixture.DefaultStatsReleaseVersion());

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext
            );

            // SubjectId exists, but no Content.Model.Release
            var query = new FullTableQuery { SubjectId = releaseSubject.SubjectId };

            using var stream = new MemoryStream();

            var result = await service.QueryToCsvStream(query, stream);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task QueryToCsvStream_LatestRelease_SubjectNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext, contentDbContext);

            // SubjectId does not exist
            var query = new FullTableQuery { SubjectId = Guid.NewGuid() };

            using var stream = new MemoryStream();

            var result = await service.QueryToCsvStream(query, stream);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task QueryToCsvStream_ReleaseVersionId()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        var filters = _fixture
            .DefaultFilter()
            .ForIndex(
                0,
                s =>
                    s.SetGroupCsvColumn("filter_0_grouping")
                        .SetFilterGroups(
                            _fixture
                                .DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(setters =>
                                    setters.Set(
                                        fg => fg.Label,
                                        (_, _, context) => $"Filter group {context.FixtureTypeIndex}"
                                    )
                                )
                                .Generate(2)
                        )
            )
            .ForIndex(
                1,
                s =>
                    s.SetGroupCsvColumn("filter_1_grouping")
                        .SetFilterGroups(
                            _fixture
                                .DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(setters =>
                                    setters.Set(
                                        fg => fg.Label,
                                        (_, _, context) => $"Filter group {context.FixtureTypeIndex}"
                                    )
                                )
                                .Generate(2)
                        )
            )
            .ForIndex(2, s => s.SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2).Generate(1)))
            .GenerateList();

        var filter0Items = filters[0].FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

        var filter1Items = filters[1].FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

        var filter2Items = filters[2].FilterGroups.SelectMany(fg => fg.FilterItems).ToList();

        var indicatorGroups = _fixture
            .DefaultIndicatorGroup()
            .ForIndex(0, i => i.SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, i => i.SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups.SelectMany(ig => ig.Indicators).ToList();

        var locations = _fixture
            .DefaultLocation()
            .WithPresetRegion()
            .WithGeographicLevel(GeographicLevel.Region)
            .GenerateList(2);

        var observations = _fixture
            .DefaultObservation()
            .WithMeasures(indicators)
            .ForRange(
                ..2,
                o =>
                    o.SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                        .SetLocation(locations[0])
                        .SetTimePeriod(2022, AcademicYear)
            )
            .ForRange(
                2..4,
                o =>
                    o.SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                        .SetLocation(locations[1])
                        .SetTimePeriod(2023, AcademicYear)
            )
            .GenerateList();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            )
            .WithSubject(
                _fixture
                    .DefaultSubject()
                    .WithFilters(filters)
                    .WithIndicatorGroups(indicatorGroups)
                    .WithObservations(observations)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            statisticsDbContext.Observation.AddRange(observations);
            statisticsDbContext.MatchedObservations.AddRange(
                observations.Select(o => new MatchedObservation(o.Id)).ToArray()
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var query = new FullTableQuery
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = indicators.Select(i => i.Id).ToList(),
                LocationIds = locations.Select(l => l.Id).ToList(),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2022,
                    StartCode = AcademicYear,
                    EndYear = 2023,
                    EndCode = AcademicYear,
                },
            };

            var tableBuilderQueryOptimiser = new Mock<ITableBuilderQueryOptimiser>(Strict);
            var observationService = new Mock<IObservationService>(Strict);
            var matchedObservationsTable = Mock.Of<ITempTableReference>(Strict);
            var subjectCsvMetaService = new Mock<ISubjectCsvMetaService>(Strict);

            tableBuilderQueryOptimiser
                .Setup(mock => mock.IsCroppingRequired(It.IsAny<FullTableQuery>()))
                .ReturnsAsync(false);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationsTable);

            var subjectCsvMeta = new SubjectCsvMetaViewModel
            {
                Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(
                    filter0Items.Concat(filter1Items).Concat(filter2Items)
                ),
                Indicators = indicators.Select(i => new IndicatorCsvMetaViewModel(i)).ToDictionary(i => i.Name),
                Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                Headers = new List<string>
                {
                    "time_period",
                    "time_identifier",
                    "geographic_level",
                    "country_code",
                    "country_name",
                    "region_code",
                    "region_name",
                    "filter_0_grouping",
                    "filter_1_grouping",
                    filters[0].Name,
                    filters[1].Name,
                    filters[2].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name,
                },
            };

            subjectCsvMetaService
                .Setup(s =>
                    s.GetSubjectCsvMeta(
                        It.Is<ReleaseSubject>(rs =>
                            rs.ReleaseVersionId == releaseSubject.ReleaseVersionId
                            && rs.SubjectId == releaseSubject.SubjectId
                        ),
                        query,
                        It.IsAny<IList<Observation>>(),
                        default
                    )
                )
                .ReturnsAsync(subjectCsvMeta);

            var service = BuildTableBuilderService(
                statisticsDbContext,
                contentDbContext,
                observationService: observationService.Object,
                subjectCsvMetaService: subjectCsvMetaService.Object,
                tableBuilderQueryOptimiser: tableBuilderQueryOptimiser.Object
            );

            using var stream = new MemoryStream();

            var result = await service.QueryToCsvStream(releaseSubject.ReleaseVersionId, query, stream);

            VerifyAllMocks(observationService, subjectCsvMetaService, tableBuilderQueryOptimiser);

            result.AssertRight();

            stream.SeekToBeginning();

            Snapshot.Match(stream.ReadToEnd());
        }
    }

    [Fact]
    public async Task QueryToCsvStream_ReleaseVersionId_NoFilters()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            )
            .WithSubject(_fixture.DefaultSubject());

        var indicators = _fixture
            .DefaultIndicator()
            .WithIndicatorGroup(_fixture.DefaultIndicatorGroup().WithSubject(releaseSubject.Subject))
            .GenerateList(3);

        var locations = _fixture
            .DefaultLocation()
            .WithPresetRegion()
            .WithGeographicLevel(GeographicLevel.Region)
            .GenerateList(2);

        var observations = _fixture
            .DefaultObservation()
            .WithSubject(releaseSubject.Subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o.SetLocation(locations[0]).SetTimePeriod(2022, AcademicYear))
            .ForRange(2..4, o => o.SetLocation(locations[1]).SetTimePeriod(2022, AcademicYear))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            statisticsDbContext.Observation.AddRange(observations);
            statisticsDbContext.MatchedObservations.AddRange(
                observations.Select(o => new MatchedObservation(o.Id)).ToArray()
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var query = new FullTableQuery
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = indicators.Select(i => i.Id).ToList(),
                LocationIds = locations.Select(l => l.Id).ToList(),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2022,
                    StartCode = AcademicYear,
                    EndYear = 2023,
                    EndCode = AcademicYear,
                },
            };

            var tableBuilderQueryOptimiser = new Mock<ITableBuilderQueryOptimiser>(Strict);
            var observationService = new Mock<IObservationService>(Strict);
            var matchedObservationsTable = Mock.Of<ITempTableReference>(Strict);
            var subjectCsvMetaService = new Mock<ISubjectCsvMetaService>(Strict);

            tableBuilderQueryOptimiser
                .Setup(mock => mock.IsCroppingRequired(It.IsAny<FullTableQuery>()))
                .ReturnsAsync(false);

            observationService
                .Setup(s => s.GetMatchedObservations(query, default))
                .ReturnsAsync(matchedObservationsTable);

            var subjectCsvMeta = new SubjectCsvMetaViewModel
            {
                Indicators = indicators.Select(i => new IndicatorCsvMetaViewModel(i)).ToDictionary(i => i.Name),
                Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                Headers = new List<string>
                {
                    "time_period",
                    "time_identifier",
                    "geographic_level",
                    "country_code",
                    "country_name",
                    "region_code",
                    "region_name",
                    indicators[0].Name,
                    indicators[1].Name,
                },
            };

            subjectCsvMetaService
                .Setup(s =>
                    s.GetSubjectCsvMeta(
                        It.Is<ReleaseSubject>(rs =>
                            rs.ReleaseVersionId == releaseSubject.ReleaseVersionId
                            && rs.SubjectId == releaseSubject.SubjectId
                        ),
                        query,
                        It.IsAny<IList<Observation>>(),
                        default
                    )
                )
                .ReturnsAsync(subjectCsvMeta);

            var service = BuildTableBuilderService(
                statisticsDbContext,
                contentDbContext,
                observationService: observationService.Object,
                subjectCsvMetaService: subjectCsvMetaService.Object,
                tableBuilderQueryOptimiser: tableBuilderQueryOptimiser.Object
            );

            using var stream = new MemoryStream();

            var result = await service.QueryToCsvStream(releaseSubject.ReleaseVersionId, query, stream);

            VerifyAllMocks(observationService, subjectCsvMetaService, tableBuilderQueryOptimiser);

            result.AssertRight();

            stream.SeekToBeginning();

            Snapshot.Match(stream.ReadToEnd());
        }
    }

    [Fact]
    public async Task QueryToCsvStream_ReleaseVersionId_ReleaseVersionNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext, contentDbContext);

            var query = new FullTableQuery { SubjectId = releaseSubject.SubjectId };

            using var stream = new MemoryStream();

            // Query using a non-existent release version id
            var result = await service.QueryToCsvStream(releaseVersionId: Guid.NewGuid(), query, stream);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task QueryToCsvStream_ReleaseVersionId_SubjectNotFound()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1)]);

        var releaseVersion = publication.Releases.Single().Versions.Single();

        ReleaseSubject releaseSubject = _fixture
            .DefaultReleaseSubject()
            .WithReleaseVersion(
                _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).WithPublicationId(publication.Id)
            );

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();

            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildTableBuilderService(statisticsDbContext, contentDbContext);

            // SubjectId does not exist
            var query = new FullTableQuery { SubjectId = Guid.NewGuid() };

            using var stream = new MemoryStream();

            var result = await service.QueryToCsvStream(releaseSubject.ReleaseVersionId, query, stream);

            result.AssertNotFound();
        }
    }

    private static IOptions<TableBuilderOptions> DefaultTableBuilderOptions()
    {
        return new TableBuilderOptions { MaxTableCellsAllowed = 25000, CroppedTableMaxRows = 1000 }.ToOptionsWrapper();
    }

    private static IOptions<LocationsOptions> DefaultLocationOptions()
    {
        return new LocationsOptions
        {
            Hierarchies = new()
            {
                { GeographicLevel.LocalAuthority, ["Region"] },
                { GeographicLevel.LocalAuthorityDistrict, ["Region"] },
                { GeographicLevel.School, ["LocalAuthority"] },
            },
        }.ToOptionsWrapper();
    }

    private static TableBuilderService BuildTableBuilderService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext? contentDbContext = null,
        ILocationService? locationService = null,
        IObservationService? observationService = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
        ISubjectResultMetaService? subjectResultMetaService = null,
        ISubjectCsvMetaService? subjectCsvMetaService = null,
        ISubjectRepository? subjectRepository = null,
        ITableBuilderQueryOptimiser? tableBuilderQueryOptimiser = null,
        IUserService? userService = null,
        IOptions<TableBuilderOptions>? tableBuilderOptions = null,
        IOptions<LocationsOptions>? locationsOptions = null
    )
    {
        return new(
            statisticsDbContext,
            contentDbContext ?? InMemoryContentDbContext(),
            locationService ?? Mock.Of<ILocationService>(Strict),
            observationService ?? Mock.Of<IObservationService>(Strict),
            statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
            subjectResultMetaService ?? Mock.Of<ISubjectResultMetaService>(Strict),
            subjectCsvMetaService ?? Mock.Of<ISubjectCsvMetaService>(Strict),
            subjectRepository ?? new SubjectRepository(statisticsDbContext),
            tableBuilderQueryOptimiser ?? Mock.Of<ITableBuilderQueryOptimiser>(Strict),
            userService ?? AlwaysTrueUserService().Object,
            tableBuilderOptions ?? DefaultTableBuilderOptions(),
            locationsOptions ?? DefaultLocationOptions()
        );
    }
}
