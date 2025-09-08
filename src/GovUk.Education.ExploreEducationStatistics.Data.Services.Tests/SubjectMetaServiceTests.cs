#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Filter = GovUk.Education.ExploreEducationStatistics.Data.Model.Filter;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class SubjectMetaServiceTests
{
    private readonly Country _england = new("E92000001", "England");
    private readonly Region _northEast = new("E12000001", "North East");
    private readonly Region _northWest = new("E12000002", "North West");
    private readonly Region _eastMidlands = new("E12000004", "East Midlands");
    private readonly LocalAuthority _blackpool = new("E06000009", "", "Blackpool");
    private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
    private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");
    private readonly LocalAuthority _sunderland = new("E08000024", "", "Sunderland");

    [Fact]
    public async Task GetSubjectMeta_ReleaseSubjectNotFound()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext();

        var service = BuildSubjectMetaService(
            statisticsDbContext);

        var result = await service.GetSubjectMeta(releaseVersionId: Guid.NewGuid(),
            subjectId: Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetSubjectMeta_EmptyModelReturned()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
        var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
        var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

        filterRepository
            .Setup(s => s.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(new List<Filter>());

        indicatorGroupRepository
            .Setup(s => s.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(new List<IndicatorGroup>());

        timePeriodService
            .Setup(s => s.GetTimePeriods(releaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        locationRepository
            .Setup(s => s.GetDistinctForSubject(releaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object,
                indicatorGroupRepository: indicatorGroupRepository.Object,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object
            );

            var result = (await service.GetSubjectMeta(
                    releaseVersionId: releaseSubject.ReleaseVersionId,
                    subjectId: releaseSubject.SubjectId))
                .AssertRight();

            VerifyAllMocks(
                filterRepository,
                indicatorGroupRepository,
                locationRepository,
                timePeriodService);

            var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

            Assert.Empty(viewModel.Locations);
            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.TimePeriod.Options);
        }
    }

    [Fact]
    public async Task GetSubjectMeta_LocationsForSubject()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
                FilterHierarchies = null,
            },
        };

        // Setup multiple geographic levels of data where some but not all of the levels have a hierarchy applied.
        var locations = ListOf(
            new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england,
            },
            new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northEast,
            },
            new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northWest,
            },
            new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _eastMidlands,
            },
            new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            });

        var options = new LocationsOptions
        {
            Hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                {
                    GeographicLevel.LocalAuthority,
                    [
                        "Country",
                        "Region"
                    ]
                }
            }
        }.ToOptionsWrapper();

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
        var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
        var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

        filterRepository
            .Setup(s => s.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(new List<Filter>());

        indicatorGroupRepository
            .Setup(s => s.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(new List<IndicatorGroup>());

        timePeriodService
            .Setup(s => s.GetTimePeriods(releaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        locationRepository
            .Setup(s => s.GetDistinctForSubject(releaseSubject.SubjectId))
            .ReturnsAsync(locations);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object,
                indicatorGroupRepository: indicatorGroupRepository.Object,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                options: options
            );

            var result = (await service.GetSubjectMeta(releaseVersionId: releaseSubject.ReleaseVersionId,
                    subjectId: releaseSubject.SubjectId))
                .AssertRight();

            VerifyAllMocks(
                filterRepository,
                indicatorGroupRepository,
                locationRepository,
                timePeriodService);

            var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

            var locationViewModels = viewModel.Locations;

            // Result has Country, Region and Local Authority levels
            Assert.Equal(3, locationViewModels.Count);
            Assert.True(locationViewModels.ContainsKey("country"));
            Assert.True(locationViewModels.ContainsKey("region"));
            Assert.True(locationViewModels.ContainsKey("localAuthority"));

            // Expect no hierarchy within the Country level
            var countries = locationViewModels["country"];
            Assert.Equal("National", countries.Legend);

            var countryOption1 = Assert.Single(countries.Options);
            Assert.NotNull(countryOption1);
            Assert.Equal(locations[0].Id, countryOption1.Id);
            Assert.Equal(_england.Name, countryOption1.Label);
            Assert.Equal(_england.Code, countryOption1.Value);
            Assert.Null(countryOption1.Level);
            Assert.Null(countryOption1.Options);

            // Expect no hierarchy within the Region level
            var regions = locationViewModels["region"];
            Assert.Equal("Regional", regions.Legend);

            Assert.Equal(3, regions.Options.Count);

            var regionOption1 = regions.Options[0];
            Assert.Equal(locations[1].Id, regionOption1.Id);
            Assert.Equal(_northEast.Name, regionOption1.Label);
            Assert.Equal(_northEast.Code, regionOption1.Value);
            Assert.Null(regionOption1.Level);
            Assert.Null(regionOption1.Options);

            var regionOption2 = regions.Options[1];
            Assert.Equal(locations[2].Id, regionOption2.Id);
            Assert.Equal(_northWest.Name, regionOption2.Label);
            Assert.Equal(_northWest.Code, regionOption2.Value);
            Assert.Null(regionOption2.Level);
            Assert.Null(regionOption2.Options);

            var regionOption3 = regions.Options[2];
            Assert.Equal(locations[3].Id, regionOption3.Id);
            Assert.Equal(_eastMidlands.Name, regionOption3.Label);
            Assert.Equal(_eastMidlands.Code, regionOption3.Value);
            Assert.Null(regionOption3.Level);
            Assert.Null(regionOption3.Options);

            // Expect a hierarchy of Country-Region-LA within the Local Authority level
            var localAuthorities = locationViewModels["localAuthority"];
            Assert.Equal("Local authority", localAuthorities.Legend);

            var laOption1 = Assert.Single(localAuthorities.Options);
            Assert.NotNull(laOption1);
            Assert.Null(laOption1.Id);
            Assert.Equal(_england.Name, laOption1.Label);
            Assert.Equal(_england.Code, laOption1.Value);
            Assert.Equal(GeographicLevel.Country, laOption1.Level);

            var laOption1SubOption1 = Assert.Single(laOption1.Options!);
            Assert.NotNull(laOption1SubOption1);
            Assert.Null(laOption1SubOption1.Id);
            Assert.Equal(_eastMidlands.Name, laOption1SubOption1.Label);
            Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
            Assert.Equal(GeographicLevel.Region, laOption1SubOption1.Level);
            Assert.Equal(2, laOption1SubOption1.Options!.Count);

            var laOption1SubOption1SubOption1 = laOption1SubOption1.Options[0];
            Assert.Equal(locations[4].Id, laOption1SubOption1SubOption1.Id);
            Assert.Equal(_derby.Name, laOption1SubOption1SubOption1.Label);
            Assert.Equal(_derby.Code, laOption1SubOption1SubOption1.Value);
            Assert.Null(laOption1SubOption1SubOption1.Level);
            Assert.Null(laOption1SubOption1SubOption1.Options);

            var laOption1SubOption1SubOption2 = laOption1SubOption1.Options[1];
            Assert.Equal(locations[5].Id, laOption1SubOption1SubOption2.Id);
            Assert.Equal(_nottingham.Name, laOption1SubOption1SubOption2.Label);
            Assert.Equal(_nottingham.Code, laOption1SubOption1SubOption2.Value);
            Assert.Null(laOption1SubOption1SubOption2.Level);
            Assert.Null(laOption1SubOption1SubOption2.Options);

            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.TimePeriod.Options);
            Assert.Null(viewModel.FilterHierarchies);
        }
    }

    [Fact]
    public async Task GetSubjectMeta_FilterHierarchies()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var filter1Id = Guid.NewGuid();
        var filter2Id = Guid.NewGuid();
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
                FilterHierarchies =
                    [
                        new DataSetFileFilterHierarchy(
                            [filter1Id, filter2Id],
                            [
                                new Dictionary<Guid, List<Guid>>
                                {
                                    { Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]},
                                    { Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]}
                                },
                                new Dictionary<Guid, List<Guid>>
                                {
                                    { Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]},
                                    { Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]}
                                }
                            ]
                        ),
                    ],
            },
        };

        var options = new LocationsOptions
        {
            Hierarchies = []
        }.ToOptionsWrapper();

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseSubject.Add(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
        var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
        var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

        filterRepository
            .Setup(s => s.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(new List<Filter>());

        indicatorGroupRepository
            .Setup(s => s.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(new List<IndicatorGroup>());

        timePeriodService
            .Setup(s => s.GetTimePeriods(releaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        locationRepository
            .Setup(s => s.GetDistinctForSubject(releaseSubject.SubjectId))
            .ReturnsAsync([]);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object,
                indicatorGroupRepository: indicatorGroupRepository.Object,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                options: options
            );

            var result = (await service.GetSubjectMeta(releaseVersionId: releaseSubject.ReleaseVersionId,
                    subjectId: releaseSubject.SubjectId))
                .AssertRight();

            VerifyAllMocks(
                filterRepository,
                indicatorGroupRepository,
                locationRepository,
                timePeriodService);

            var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

            Assert.Empty(viewModel.Locations);
            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.TimePeriod.Options);

            var fileFilterHierarchy = releaseFile.File.FilterHierarchies[0];
            viewModel.FilterHierarchies.AssertDeepEqualTo([
                [
                    new DataSetFileFilterHierarchyTierViewModel
                    (
                       0,
                       filter1Id,
                       filter2Id,
                       fileFilterHierarchy.Tiers[0]
                    ),
                    new DataSetFileFilterHierarchyTierViewModel
                    (
                        1,
                        filter2Id,
                        null,
                        fileFilterHierarchy.Tiers[1]
                    ),
                ],
            ]);
        }
    }

    [Fact]
    public async Task FilterSubjectMeta_EmptyModelReturned()
    {
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion(),
            Subject = new Subject(),
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

        var cancellationToken = new CancellationTokenSource().Token;

        timePeriodService
            .Setup(s => s.GetTimePeriods(It.IsAny<IQueryable<Observation>>()))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var request = new LocationsOrTimePeriodsQueryRequest
            {
                SubjectId = releaseSubject.SubjectId,
                LocationIds = ListOf(Guid.NewGuid())
            };

            var service = BuildSubjectMetaService(
                statisticsDbContext,
                timePeriodService: timePeriodService.Object
            );

            var result =
                (await service.FilterSubjectMeta(releaseSubject.ReleaseVersionId, request, cancellationToken))
                .AssertRight();

            VerifyAllMocks(timePeriodService);

            var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

            Assert.Empty(viewModel.Locations);
            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.TimePeriod.Options);
        }
    }

    [Fact]
    public async Task FilterSubjectMeta_LatestRelease_EmptyModelReturned()
    {
        var releaseVersion = new Content.Model.ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1)
        };

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion
            {
                Id = releaseVersion.Id
            },
            Subject = new Subject()
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

        var cancellationToken = new CancellationTokenSource().Token;

        timePeriodService
            .Setup(s => s.GetTimePeriods(It.IsAny<IQueryable<Observation>>()))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var request = new LocationsOrTimePeriodsQueryRequest
            {
                SubjectId = releaseSubject.SubjectId,
                LocationIds = ListOf(Guid.NewGuid()),
            };

            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                timePeriodService: timePeriodService.Object
            );

            var result = (await service.FilterSubjectMeta(null, request, cancellationToken))
                .AssertRight();

            VerifyAllMocks(timePeriodService);

            var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

            Assert.Empty(viewModel.Locations);
            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.TimePeriod.Options);
        }
    }

    [Fact]
    public async Task FilterSubjectMeta_TimePeriods()
    {
        var subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion(),
            Subject = subject
        };

        var location1 = new Location
        {
            Id = Guid.NewGuid(),
            LocalAuthority = _blackpool
        };

        var location2 = new Location
        {
            Id = Guid.NewGuid(),
            LocalAuthority = _derby
        };

        var location3 = new Location
        {
            Id = Guid.NewGuid(),
            Country = _england
        };

        var location4 = new Location
        {
            Id = Guid.NewGuid(),
            LocalAuthority = _nottingham
        };

        var location5 = new Location
        {
            Id = Guid.NewGuid(),
            LocalAuthority = _sunderland
        };

        var request = new LocationsOrTimePeriodsQueryRequest
        {
            SubjectId = subject.Id,
            LocationIds = ListOf(location1.Id, location2.Id, location3.Id)
        };

        var observations = ListOf(
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location1
            },
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location2
            },
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location3
            });

        var observationsWithDifferentLocations = ListOf(
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location4
            },
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location5
            });

        var observationsFromAnotherSubject = ListOf(
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location2
            },
            new Observation
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                Location = location3
            });

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.Observation.AddRangeAsync(observations);
            await statisticsDbContext.Observation.AddRangeAsync(observationsWithDifferentLocations);
            await statisticsDbContext.Observation.AddRangeAsync(observationsFromAnotherSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            timePeriodService
                .Setup(s => s.GetTimePeriods(It.Is<IQueryable<Observation>>(
                    observationsWihMatchingLocations => observationsWihMatchingLocations
                        .ToList()
                        .Select(o => o.Id)
                        .SequenceEqual(observationsWihMatchingLocations.Select(m => m.Id)))))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2012, TimeIdentifier.April),
                    (2012, TimeIdentifier.May),
                    (2012, TimeIdentifier.June)
                });

            var service = BuildSubjectMetaService(
                statisticsDbContext,
                timePeriodService: timePeriodService.Object);

            var result =
                (await service.FilterSubjectMeta(releaseSubject.ReleaseVersionId, request, cancellationToken))
                .AssertRight();

            VerifyAllMocks(timePeriodService);

            Assert.Empty(result.Locations);
            Assert.Empty(result.Filters);
            Assert.Empty(result.Indicators);

            var periods = result.TimePeriod.Options.ToList();
            Assert.Equal(3, periods.Count);
            Assert.Equal(2012, periods[0].Year);
            Assert.Equal(TimeIdentifier.April, periods[0].Code);
            Assert.Equal(2012, periods[1].Year);
            Assert.Equal(TimeIdentifier.May, periods[1].Code);
            Assert.Equal(2012, periods[2].Year);
            Assert.Equal(TimeIdentifier.June, periods[2].Code);
        }
    }

    [Fact]
    public async Task FilterSubjectMeta_FiltersAndIndicators()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion
            {
                Id = releaseSubject.ReleaseVersion.Id,
                Published = DateTime.UtcNow,
            },
            File =  new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
                FilterHierarchies = null, // BuildFilterHierarchyViewModel is tested via GetSubjectMeta, so no need to test it here
            },
        };

        var request = new LocationsOrTimePeriodsQueryRequest
        {
            SubjectId = releaseSubject.SubjectId,
            LocationIds = ListOf(Guid.NewGuid()),
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2012,
                StartCode = TimeIdentifier.AcademicYear,
                EndYear = 2012,
                EndCode = TimeIdentifier.AcademicYear
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.MatchedObservations.AddRangeAsync(
                new MatchedObservation(Guid.NewGuid()),
                new MatchedObservation(Guid.NewGuid()),
                new MatchedObservation(Guid.NewGuid()));
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var observationService = new Mock<IObservationService>(MockBehavior.Strict);

            observationService
                .Setup(s => s.GetMatchedObservations(
                    It.Is<FullTableQuery>(ctx => ctx
                            .Equals(request.AsFullTableQuery())),
                    cancellationToken))
                .ReturnsAsync(statisticsDbContext.MatchedObservations);

            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);

            var filter1 = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = releaseSubject.SubjectId,
                Label = "Filter 1"
            };
            filter1.FilterGroups = CreateFilterGroups(filter1, 2, 1);

            var filter2 = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = releaseSubject.SubjectId,
                Label = "Filter 2"
            };
            filter2.FilterGroups = CreateFilterGroups(filter2, 2);

            var filter1FilterItems = filter1
                .FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var filter2FilterItems = filter2
                .FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var allFilterItems = filter1FilterItems.Concat(filter2FilterItems);

            filterItemRepository
                .Setup(s => s.GetFilterItemsFromMatchedObservationIds(
                    // ReSharper disable once AccessToDisposedClosure
                    releaseSubject.SubjectId, statisticsDbContext.MatchedObservations))
                .ReturnsAsync(allFilterItems);

            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

            var indicatorGroups = ListOf(
                new IndicatorGroup
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator Group 2",
                    Indicators = ListOf(
                        new Indicator
                        {
                            Id = Guid.NewGuid(),
                            Unit = IndicatorUnit.None,
                            Label = "Indicator 2"
                        })
                },
                new IndicatorGroup
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator Group 1",
                    Indicators = ListOf(
                        new Indicator
                        {
                            Id = Guid.NewGuid(),
                            Unit = IndicatorUnit.Percent,
                            Label = "Indicator 1"
                        })
                });

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(releaseSubject.SubjectId))
                .ReturnsAsync(indicatorGroups);

            var service = BuildSubjectMetaService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                observationService: observationService.Object,
                filterItemRepository: filterItemRepository.Object,
                indicatorGroupRepository: indicatorGroupRepository.Object);

            var result =
                (await service.FilterSubjectMeta(releaseSubject.ReleaseVersionId, request, cancellationToken))
                .AssertRight();

            VerifyAllMocks(
                filterItemRepository,
                indicatorGroupRepository,
                observationService);

            result.TimePeriod.AssertDeepEqualTo(new TimePeriodsMetaViewModel());
            Assert.Empty(result.Locations);

            result.Filters.AssertDeepEqualTo(new Dictionary<string, FilterMetaViewModel>
            {
                {
                    "Filter1", new FilterMetaViewModel
                    {
                        Id = filter1.Id,
                        Legend = "Filter 1",
                        Options = new Dictionary<string, FilterGroupMetaViewModel>
                        {
                            {
                                "FilterGroup1", new FilterGroupMetaViewModel
                                {
                                    Id = filter1.FilterGroups[0].Id,
                                    Label = "Filter Group 1",
                                    Options = new List<FilterItemMetaViewModel>
                                    {
                                        new("Filter Item 1", filter1FilterItems[0].Id),
                                        new("Filter Item 2", filter1FilterItems[1].Id)
                                    },
                                    Order = 0
                                }
                            },
                            {
                                "FilterGroup2", new FilterGroupMetaViewModel
                                {
                                    Id = filter1.FilterGroups[1].Id,
                                    Label = "Filter Group 2",
                                    Options = new List<FilterItemMetaViewModel>
                                    {
                                        new("Filter Item 1", filter1FilterItems[2].Id)
                                    },
                                    Order = 1
                                }
                            }
                        },
                        Order = 0
                    }
                },
                {
                    "Filter2", new FilterMetaViewModel
                    {
                        Id = filter2.Id,
                        Legend = "Filter 2",
                        Options = new Dictionary<string, FilterGroupMetaViewModel>
                        {
                            {
                                "FilterGroup1", new FilterGroupMetaViewModel
                                {
                                    Id = filter2.FilterGroups[0].Id,
                                    Label = "Filter Group 1",
                                    Options = new List<FilterItemMetaViewModel>
                                    {
                                        new("Filter Item 1", filter2FilterItems[0].Id),
                                        new("Filter Item 2", filter2FilterItems[1].Id)
                                    },
                                    Order = 0
                                }
                            }
                        },
                        Order = 1
                    }
                },
            });

            result.Indicators.AssertDeepEqualTo(new Dictionary<string, IndicatorGroupMetaViewModel>
            {
                {
                    "IndicatorGroup1", new IndicatorGroupMetaViewModel
                    {
                        Id = indicatorGroups[1].Id,
                        Label = "Indicator Group 1",
                        Options = ListOf(
                            new IndicatorMetaViewModel
                            {
                                Label = "Indicator 1",
                                Unit = IndicatorUnit.Percent,
                                Value = indicatorGroups[1].Indicators[0].Id
                            }),
                        Order = 0
                    }
                },
                {
                    "IndicatorGroup2", new IndicatorGroupMetaViewModel
                    {
                        Id = indicatorGroups[0].Id,
                        Label = "Indicator Group 2",
                        Options = ListOf(
                            new IndicatorMetaViewModel
                            {
                                Label = "Indicator 2",
                                Unit = IndicatorUnit.None,
                                Value = indicatorGroups[0].Indicators[0].Id
                            }),
                        Order = 1
                    }
                }
            });

            Assert.Null(result.FilterHierarchies);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            },
                            new()
                            {
                                Id = Guid.NewGuid()
                            },
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            },
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Create a request with identical filters, filter groups and filter items
        var request = filters.Select(filter =>
            new FilterUpdateViewModel
            {
                Id = filter.Id,
                FilterGroups = filter.FilterGroups.Select(filterGroup => new FilterGroupUpdateViewModel
                {
                    Id = filterGroup.Id,
                    FilterItems = filterGroup.FilterItems.Select(filterItem => filterItem.Id).ToList()
                }).ToList()
            }).ToList();

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        cacheService
            .Setup(service => service.DeleteItemAsync(new PrivateSubjectMetaCacheKey(
                releaseSubject.ReleaseVersionId,
                releaseSubject.SubjectId)))
            .Returns(Task.CompletedTask);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                cacheService: cacheService.Object,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(cacheService, filterRepository);

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseSubject.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            var savedSequence = savedReleaseFile.FilterSequence;

            Assert.NotNull(savedSequence);
            Assert.Equal(2, savedSequence!.Count);

            // Filter 1
            var savedFilter1 = savedSequence[0];
            Assert.Equal(request[0].Id, savedFilter1.Id);

            Assert.Equal(2, savedFilter1.ChildSequence.Count);
            var savedFilter1Group1 = savedFilter1.ChildSequence[0];
            var savedFilter1Group2 = savedFilter1.ChildSequence[1];

            Assert.Equal(request[0].FilterGroups[0].Id, savedFilter1Group1.Id);
            Assert.Equal(request[0].FilterGroups[1].Id, savedFilter1Group2.Id);

            Assert.Equal(2, savedFilter1Group1.ChildSequence.Count);
            Assert.Equal(request[0].FilterGroups[0].FilterItems[0], savedFilter1Group1.ChildSequence[0]);
            Assert.Equal(request[0].FilterGroups[0].FilterItems[1], savedFilter1Group1.ChildSequence[1]);

            Assert.Equal(2, savedFilter1Group2.ChildSequence.Count);
            Assert.Equal(request[0].FilterGroups[1].FilterItems[0], savedFilter1Group2.ChildSequence[0]);
            Assert.Equal(request[0].FilterGroups[1].FilterItems[1], savedFilter1Group2.ChildSequence[1]);

            // Filter 2
            var savedFilter2 = savedSequence[1];
            Assert.Equal(request[1].Id, savedFilter2.Id);

            Assert.Single(savedFilter2.ChildSequence);
            var savedFilter2Group1 = savedFilter2.ChildSequence[0];

            Assert.Equal(request[1].FilterGroups[0].Id, savedFilter2Group1.Id);

            Assert.Single(savedFilter2Group1.ChildSequence);
            Assert.Equal(request[1].FilterGroups[0].FilterItems[0], savedFilter2Group1.ChildSequence[0]);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_FilterMissing()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Request has the second filter missing
        var request = new List<FilterUpdateViewModel>
        {
            new()
            {
                Id = filters[0].Id,
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = filters[0].FilterGroups[0].Id,
                        FilterItems = new List<Guid>
                        {
                            filters[0].FilterGroups[0].FilterItems[0].Id
                        }
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(filterRepository);

            result.AssertBadRequest(FiltersDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseSubject.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseSubject remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_FilterGroupMissing()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion{ Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Request has the second filter group missing
        var request = new List<FilterUpdateViewModel>
        {
            new()
            {
                Id = filters[0].Id,
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = filters[0].FilterGroups[0].Id,
                        FilterItems = new List<Guid>
                        {
                            filters[0].FilterGroups[0].FilterItems[0].Id
                        }
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(filterRepository);

            result.AssertBadRequest(FilterGroupsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_FilterItemMissing()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            },
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Request has the second filter item missing
        var request = new List<FilterUpdateViewModel>
        {
            new()
            {
                Id = filters[0].Id,
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = filters[0].FilterGroups[0].Id,
                        FilterItems = new List<Guid>
                        {
                            filters[0].FilterGroups[0].FilterItems[0].Id
                        }
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(filterRepository);

            result.AssertBadRequest(FilterItemsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_FilterNotForSubject()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Request has a filter not for this subject
        var request = new List<FilterUpdateViewModel>
        {
            new()
            {
                Id = filters[0].Id,
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = filters[0].FilterGroups[0].Id,
                        FilterItems = new List<Guid>
                        {
                            filters[0].FilterGroups[0].FilterItems[0].Id
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<Guid>
                        {
                            Guid.NewGuid()
                        }
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(filterRepository);

            result.AssertBadRequest(FiltersDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_FilterGroupNotForSubject()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Request has a filter group not for this subject
        var request = new List<FilterUpdateViewModel>
        {
            new()
            {
                Id = filters[0].Id,
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = filters[0].FilterGroups[0].Id,
                        FilterItems = new List<Guid>
                        {
                            filters[0].FilterGroups[0].FilterItems[0].Id
                        }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<Guid>
                        {
                            Guid.NewGuid()
                        }
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(filterRepository);

            result.AssertBadRequest(FilterGroupsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_FilterItemNotForSubject()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var filters = new List<Filter>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid()
                            }
                        }
                    }
                }
            }
        };

        // Request has a filter item not for this subject
        var request = new List<FilterUpdateViewModel>
        {
            new()
            {
                Id = filters[0].Id,
                FilterGroups = new List<FilterGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = filters[0].FilterGroups[0].Id,
                        FilterItems = new List<Guid>
                        {
                            filters[0].FilterGroups[0].FilterItems[0].Id,
                            Guid.NewGuid()
                        }
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

        filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
            .ReturnsAsync(filters);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                filterRepository: filterRepository.Object);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(filterRepository);

            result.AssertBadRequest(FilterItemsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_ReleaseNotFound()
    {
        // Create a ReleaseSubject but for a different release than the one which will be used in the update
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(statisticsDbContext);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: Guid.NewGuid(),
                subjectId: releaseSubject.SubjectId,
                new List<FilterUpdateViewModel>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            );

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectFilters_SubjectNotFound()
    {
        // Create a ReleaseSubject but for a different release than the one which will be used in the update
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(statisticsDbContext);

            var result = await service.UpdateSubjectFilters(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: Guid.NewGuid(),
                new List<FilterUpdateViewModel>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            );

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.FilterSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var indicatorGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    },
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            }
        };

        // Create a request with identical indicator groups and indicators
        var request = indicatorGroups.Select(indicatorGroup =>
            new IndicatorGroupUpdateViewModel
            {
                Id = indicatorGroup.Id,
                Indicators = indicatorGroup.Indicators.Select(indicator => indicator.Id).ToList()
            }).ToList();

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

        cacheService
            .Setup(service => service.DeleteItemAsync(new PrivateSubjectMetaCacheKey(
                releaseSubject.ReleaseVersionId,
                releaseSubject.SubjectId)))
            .Returns(Task.CompletedTask);

        indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(indicatorGroups);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                cacheService: cacheService.Object,
                indicatorGroupRepository: indicatorGroupRepository.Object);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: releaseSubject.ReleaseVersion.Id,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(cacheService, indicatorGroupRepository);

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersion.Id
                && rf.File.SubjectId == releaseSubject.SubjectId
                && rf.File.Type == FileType.Data);

            var savedSequence = savedReleaseFile.IndicatorSequence;

            Assert.NotNull(savedSequence);
            Assert.Equal(2, savedSequence!.Count);

            // Indicator Group 1
            var savedIndicatorGroup1 = savedSequence[0];
            Assert.Equal(request[0].Id, savedIndicatorGroup1.Id);

            Assert.Equal(2, savedIndicatorGroup1.ChildSequence.Count);
            Assert.Equal(request[0].Indicators[0], savedIndicatorGroup1.ChildSequence[0]);
            Assert.Equal(request[0].Indicators[1], savedIndicatorGroup1.ChildSequence[1]);

            // Indicator Group 2
            var savedIndicatorGroup2 = savedSequence[1];
            Assert.Equal(request[1].Id, savedIndicatorGroup2.Id);

            Assert.Single(savedIndicatorGroup2.ChildSequence);
            Assert.Equal(request[1].Indicators[0], savedIndicatorGroup2.ChildSequence[0]);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators_IndicatorGroupMissing()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var indicatorGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            }
        };

        // Request has the second indicator group missing
        var request = new List<IndicatorGroupUpdateViewModel>
        {
            new()
            {
                Id = indicatorGroups[0].Id,
                Indicators = new List<Guid>
                {
                    indicatorGroups[0].Indicators[0].Id
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

        indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(indicatorGroups);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                indicatorGroupRepository: indicatorGroupRepository.Object);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(indicatorGroupRepository);

            result.AssertBadRequest(IndicatorGroupsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.IndicatorSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators_IndicatorMissing()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var indicatorGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    },
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            }
        };

        // Request has the second indicator missing
        var request = new List<IndicatorGroupUpdateViewModel>
        {
            new()
            {
                Id = indicatorGroups[0].Id,
                Indicators = new List<Guid>
                {
                    indicatorGroups[0].Indicators[0].Id
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

        indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(indicatorGroups);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                indicatorGroupRepository: indicatorGroupRepository.Object);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(indicatorGroupRepository);

            result.AssertBadRequest(IndicatorsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.IndicatorSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators_IndicatorGroupNotForSubject()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var indicatorGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            }
        };

        // Request has an indicator group not for this subject
        var request = new List<IndicatorGroupUpdateViewModel>
        {
            new()
            {
                Id = indicatorGroups[0].Id,
                Indicators = new List<Guid>
                {
                    indicatorGroups[0].Indicators[0].Id
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Guid>
                {
                    Guid.NewGuid()
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

        indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(indicatorGroups);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                indicatorGroupRepository: indicatorGroupRepository.Object);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(indicatorGroupRepository);

            result.AssertBadRequest(IndicatorGroupsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.IndicatorSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators_IndicatorNotForSubject()
    {
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var indicatorGroups = new List<IndicatorGroup>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            }
        };

        // Request has an indicator not for this subject
        var request = new List<IndicatorGroupUpdateViewModel>
        {
            new()
            {
                Id = indicatorGroups[0].Id,
                Indicators = new List<Guid>
                {
                    indicatorGroups[0].Indicators[0].Id,
                    Guid.NewGuid()
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

        indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
            .ReturnsAsync(indicatorGroups);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                contentDbContext,
                indicatorGroupRepository: indicatorGroupRepository.Object);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: releaseSubject.SubjectId,
                request
            );

            VerifyAllMocks(indicatorGroupRepository);

            result.AssertBadRequest(IndicatorsDifferFromSubject);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.IndicatorSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators_ReleaseNotFound()
    {
        // Create a ReleaseSubject but for a different release than the one which will be used in the update
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(statisticsDbContext);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: Guid.NewGuid(),
                subjectId: releaseSubject.SubjectId,
                new List<IndicatorGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            );

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.IndicatorSequence);
        }
    }

    [Fact]
    public async Task UpdateSubjectIndicators_SubjectNotFound()
    {
        // Create a ReleaseSubject but for a different release than the one which will be used in the update
        var subject = new Subject { Id = Guid.NewGuid(), };
        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            SubjectId = subject.Id,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new Content.Model.ReleaseVersion { Id = releaseSubject.ReleaseVersion.Id, },
            File = new File
            {
                SubjectId = releaseSubject.SubjectId,
                Type = FileType.Data,
            },
        };


        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = BuildSubjectMetaService(statisticsDbContext);

            var result = await service.UpdateSubjectIndicators(
                releaseVersionId: releaseSubject.ReleaseVersionId,
                subjectId: Guid.NewGuid(),
                new List<IndicatorGroupUpdateViewModel>
                {
                    new()
                    {
                        Id = Guid.NewGuid()
                    }
                }
            );

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var savedReleaseFile = contentDbContext.ReleaseFiles.Single(rf =>
                rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                && rf.File.SubjectId == releaseSubject.SubjectId);

            // Verify that the ReleaseFile remains untouched
            Assert.Null(savedReleaseFile.IndicatorSequence);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ExcludeFiltersUsedForGrouping_ExcludesGroupingFilters(bool metadataSpecifiesGrouping)
    {
        // Arrange
        List<Filter> filtersRaw =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Hint = "Filter a hint",
                Label = "Filter a",
                Name = "filter_a",
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group a",
                        FilterItems =
                            new List<FilterItem>
                            {
                                new() { Id = Guid.NewGuid(), Label = "Item a" },
                                new() { Id = Guid.NewGuid(), Label = "Item b" }
                            }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group b",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item c" },
                            new() { Id = Guid.NewGuid(), Label = "Item d" }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Hint = "Filter b hint",
                Label = "Filter b",
                Name = "filter_b",
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group c",
                        FilterItems =
                            new List<FilterItem>
                            {
                                new() { Id = Guid.NewGuid(), Label = "Item e" },
                                new() { Id = Guid.NewGuid(), Label = "Item f" }
                            }
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group d",
                        FilterItems = new List<FilterItem>
                        {
                            new() { Id = Guid.NewGuid(), Label = "Item g" },
                            new() { Id = Guid.NewGuid(), Label = "Item h" }
                        }
                    }
                }
            },
            new() { Id = Guid.NewGuid(), Hint = "Group a hint", Label = "Group a", Name = "group_a" },
            new() { Id = Guid.NewGuid(), Hint = "Group c hint", Label = "Group c", Name = "group_c" }
        ];

        if (metadataSpecifiesGrouping)
        {
            filtersRaw[0].GroupCsvColumn = "group_a";
            filtersRaw[1].GroupCsvColumn = "group_c";
        }

        var filters = new Dictionary<string, FilterMetaViewModel>
        {
            { "FilterA", new (filtersRaw[0], 0) }, 
            { "FilterB", new (filtersRaw[1], 1) },
            { "GroupA", new (filtersRaw[2], 2) },
            { "GroupC", new (filtersRaw[3], 3) }
        };

        // Act
        var result = SubjectMetaService.ExcludeFiltersUsedForGrouping(filters);

        if (!metadataSpecifiesGrouping)
        {
            // Returns all filters
            Assert.Equal(4, result.Count);
            return;
        }
        
        // Returns only filters not used for grouping
        Assert.Equal(2, result.Count);
        // Only "filter_a" and "filter_b" should be present, as "group_a", "group_c" are grouping filters
        Assert.Contains(result.Values, f => f.Name == filtersRaw[0].Name);
        Assert.Contains(result.Values, f => f.Name == filtersRaw[1].Name);
        Assert.DoesNotContain(result.Values, f => f.Name == filtersRaw[2].Name);
        Assert.DoesNotContain(result.Values, f => f.Name == filtersRaw[3].Name);
    }
    
    private static IOptions<LocationsOptions> DefaultLocationOptions()
    {
        return new LocationsOptions().ToOptionsWrapper();
    }

    private static List<FilterGroup> CreateFilterGroups(Filter filter,
        params int[] numberOfFilterItemsPerFilterGroup)
    {
        return numberOfFilterItemsPerFilterGroup
            .Select((filterItemCount, index) =>
            {
                var filterGroup = new FilterGroup
                {
                    Id = Guid.NewGuid(),
                    Label = $"Filter Group {index + 1}",
                    Filter = filter,
                    FilterItems = Enumerable
                        .Range(0, filterItemCount)
                        .Select(filterItemIndex =>
                            new FilterItem
                            {
                                Id = Guid.NewGuid(),
                                Label = $"Filter Item {filterItemIndex + 1}",
                            })
                        .ToList()
                };

                filterGroup.FilterItems.ForEach(filterItem => filterItem.FilterGroup = filterGroup);

                return filterGroup;
            })
            .ToList();
    }

    private static SubjectMetaService BuildSubjectMetaService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext? contentDbContext = null,
        IBlobCacheService? cacheService = null,
        IReleaseSubjectService? releaseSubjectService = null,
        IFilterRepository? filterRepository = null,
        IFilterItemRepository? filterItemRepository = null,
        IIndicatorGroupRepository? indicatorGroupRepository = null,
        ILocationRepository? locationRepository = null,
        IObservationService? observationService = null,
        ITimePeriodService? timePeriodService = null,
        IUserService? userService = null,
        IOptions<LocationsOptions>? options = null)
    {
        var contentDbContextInstance = contentDbContext ?? InMemoryContentDbContext();

        return new(
            statisticsDbContext,
            contentDbContextInstance,
            cacheService ?? Mock.Of<IBlobCacheService>(MockBehavior.Strict),
            releaseSubjectService ?? new ReleaseSubjectService(statisticsDbContext, contentDbContextInstance),
            filterRepository ?? Mock.Of<IFilterRepository>(MockBehavior.Strict),
            filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
            indicatorGroupRepository ?? Mock.Of<IIndicatorGroupRepository>(MockBehavior.Strict),
            locationRepository ?? Mock.Of<ILocationRepository>(MockBehavior.Strict),
            Mock.Of<ILogger<SubjectMetaService>>(),
            observationService ?? Mock.Of<IObservationService>(MockBehavior.Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(MockBehavior.Strict),
            userService ?? AlwaysTrueUserService().Object,
            options ?? DefaultLocationOptions()
        );
    }
}
