#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using Unit = GovUk.Education.ExploreEducationStatistics.Data.Model.Unit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using ContentRelease = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
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

            var result = await service.GetSubjectMeta(releaseId: Guid.NewGuid(),
                subjectId: Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetSubjectMeta_EmptyModelReturned()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
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
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = (await service.GetSubjectMeta(releaseId: releaseSubject.ReleaseId,
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
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {
                        GeographicLevel.LocalAuthority,
                        new List<string>
                        {
                            "Country",
                            "Region"
                        }
                    }
                }
            });

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(releaseId: releaseSubject.ReleaseId,
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
            }
        }

        [Fact]
        public async Task FilterSubjectMeta_EmptyModelReturned()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.SubjectId,
                    LocationIds = ListOf(Guid.NewGuid())
                };

                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    timePeriodService: timePeriodService.Object
                );

                var result = (await service.FilterSubjectMeta(releaseSubject.ReleaseId, query, cancellationToken))
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
            var release = new ContentRelease
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddDays(-1)
            };
            
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Id = release.Id
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
                await contentDbContext.Releases.AddAsync(release);
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
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.SubjectId,
                    LocationIds = ListOf(Guid.NewGuid())
                };

                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    contentDbContext,
                    timePeriodService: timePeriodService.Object
                );

                var result = (await service.FilterSubjectMeta(null, query, cancellationToken))
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
                Release = new Release(),
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

            var query = new ObservationQueryContext
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

                var result = (await service.FilterSubjectMeta(releaseSubject.ReleaseId, query, cancellationToken))
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
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = subject
            };

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id,
                LocationIds = ListOf(Guid.NewGuid()),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2012,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2012,
                    EndCode = TimeIdentifier.AcademicYear
                }
            };

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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var cancellationToken = new CancellationTokenSource().Token;

                var observationService = new Mock<IObservationService>(MockBehavior.Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, cancellationToken))
                    .ReturnsAsync(statisticsDbContext.MatchedObservations);

                var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);

                var filter1 = new Filter
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subject.Id,
                    Label = "Filter 1"
                };
                filter1.FilterGroups = CreateFilterGroups(filter1, 2, 1);

                var filter2 = new Filter
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subject.Id,
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
                        subject.Id, statisticsDbContext.MatchedObservations))
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
                                Unit = Unit.Number,
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
                                Unit = Unit.Percent,
                                Label = "Indicator 1"
                            })
                    });

                indicatorGroupRepository
                    .Setup(s => s.GetIndicatorGroups(subject.Id))
                    .ReturnsAsync(indicatorGroups);

                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    observationService: observationService.Object,
                    filterItemRepository: filterItemRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = (await service.FilterSubjectMeta(releaseSubject.ReleaseId, query, cancellationToken))
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
                                    Unit = Unit.Percent,
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
                                    Unit = Unit.Number,
                                    Value = indicatorGroups[0].Indicators[0].Id
                                }),
                            Order = 1
                        }
                    }
                });
            }
        }

        [Fact]
        public async Task FilterSubjectMeta_InvalidCombination_NoTimePeriodsOrLocations()
        {
            var statisticsRelease = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id,
                LocationIds = new List<Guid>(),
                TimePeriod = null
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext);

                var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => service.FilterSubjectMeta(releaseSubject.ReleaseId, query, default));

                Assert.Equal("Unable to determine which SubjectMeta information has requested " +
                             "(Parameter 'subjectMetaStep')", exception.Message);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            cacheService
                .Setup(service => service.DeleteItem(new PrivateSubjectMetaCacheKey(releaseSubject.ReleaseId,
                    releaseSubject.SubjectId)))
                .Returns(Task.CompletedTask);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    cacheService: cacheService.Object,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(cacheService, filterRepository);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                var savedSequence = savedReleaseSubject.FilterSequence;

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
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(filterRepository);

                result.AssertBadRequest(FiltersDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_FilterGroupMissing()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(filterRepository);

                result.AssertBadRequest(FilterGroupsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_FilterItemMissing()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(filterRepository);

                result.AssertBadRequest(FilterItemsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_FilterNotForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(filterRepository);

                result.AssertBadRequest(FiltersDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_FilterGroupNotForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(filterRepository);

                result.AssertBadRequest(FilterGroupsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_FilterItemNotForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);

            filterRepository.Setup(mock => mock.GetFiltersIncludingItems(releaseSubject.SubjectId))
                .ReturnsAsync(filters);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    filterRepository: filterRepository.Object);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(filterRepository);

                result.AssertBadRequest(FilterItemsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_ReleaseNotFound()
        {
            // Create a ReleaseSubject but for a different release than the one which will be used in the update
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext);

                var result = await service.UpdateSubjectFilters(
                    releaseId: Guid.NewGuid(),
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectFilters_SubjectNotFound()
        {
            // Create a ReleaseSubject but for a different release than the one which will be used in the update
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext);

                var result = await service.UpdateSubjectFilters(
                    releaseId: releaseSubject.ReleaseId,
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.FilterSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectIndicators()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

            cacheService
                .Setup(service => service.DeleteItem(new PrivateSubjectMetaCacheKey(releaseSubject.ReleaseId,
                    releaseSubject.SubjectId)))
                .Returns(Task.CompletedTask);

            indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
                .ReturnsAsync(indicatorGroups);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    cacheService: cacheService.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(cacheService, indicatorGroupRepository);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                var savedSequence = savedReleaseSubject.IndicatorSequence;

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
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

            indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
                .ReturnsAsync(indicatorGroups);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(indicatorGroupRepository);

                result.AssertBadRequest(IndicatorGroupsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.IndicatorSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectIndicators_IndicatorMissing()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

            indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
                .ReturnsAsync(indicatorGroups);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(indicatorGroupRepository);

                result.AssertBadRequest(IndicatorsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.IndicatorSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectIndicators_IndicatorGroupNotForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

            indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
                .ReturnsAsync(indicatorGroups);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(indicatorGroupRepository);

                result.AssertBadRequest(IndicatorGroupsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.IndicatorSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectIndicators_IndicatorNotForSubject()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
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

            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);

            indicatorGroupRepository.Setup(mock => mock.GetIndicatorGroups(releaseSubject.SubjectId))
                .ReturnsAsync(indicatorGroups);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: releaseSubject.ReleaseId,
                    subjectId: releaseSubject.SubjectId,
                    request
                );

                VerifyAllMocks(indicatorGroupRepository);

                result.AssertBadRequest(IndicatorsDifferFromSubject);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.IndicatorSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectIndicators_ReleaseNotFound()
        {
            // Create a ReleaseSubject but for a different release than the one which will be used in the update
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: Guid.NewGuid(),
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.IndicatorSequence);
            }
        }

        [Fact]
        public async Task UpdateSubjectIndicators_SubjectNotFound()
        {
            // Create a ReleaseSubject but for a different release than the one which will be used in the update
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext);

                var result = await service.UpdateSubjectIndicators(
                    releaseId: releaseSubject.ReleaseId,
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var savedReleaseSubject = statisticsDbContext.ReleaseSubject.Single(rs =>
                    rs.ReleaseId == releaseSubject.ReleaseId
                    && rs.SubjectId == releaseSubject.SubjectId);

                // Verify that the ReleaseSubject remains untouched
                Assert.Null(savedReleaseSubject.IndicatorSequence);
            }
        }

        private static IOptions<LocationsOptions> DefaultLocationOptions()
        {
            return Options.Create(new LocationsOptions());
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
}
