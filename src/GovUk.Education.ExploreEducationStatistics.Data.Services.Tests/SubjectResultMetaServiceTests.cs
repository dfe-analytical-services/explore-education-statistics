#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class SubjectResultMetaServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        private readonly BoundaryLevel _countriesBoundaryLevel = new()
        {
            Id = 1,
            Label = "Countries November 2021",
            Level = GeographicLevel.Country
        };

        private readonly BoundaryLevel _regionsBoundaryLevel = new()
        {
            Id = 2,
            Label = "Regions November 2021",
            Level = GeographicLevel.Region
        };

        private readonly GeoJson _geoJson = new()
        {
            Value = "[]"
        };

        [Fact]
        public async Task GetSubjectMeta_SubjectNotFound()
        {
            var query = new ObservationQueryContext();

            var contextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            var service = BuildService(statisticsDbContext);

            var result = await service.GetSubjectMeta(
                releaseId: Guid.NewGuid(),
                query,
                new List<Observation>());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetSubjectMeta_EmptyModelReturnedForSubject()
        {
            var publication = new Publication
            {
                Title = "Test Publication"
            };
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var releaseFile = new ReleaseFile
            {
                Name = "Test File"
            };

            var observations = new List<Observation>();

            var query = new ObservationQueryContext
            {
                Indicators = new List<Guid>(),
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(new List<GeographicLevel>()))
                .Returns(Enumerable.Empty<BoundaryLevel>());

            filterItemRepository.Setup(s => s.GetFilterItemsFromObservations(observations))
                .ReturnsAsync(new List<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    release.Id,
                    subject.Id,
                    new List<Guid>(),
                    query.Indicators))
                .ReturnsAsync(new List<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            releaseDataFileRepository.Setup(s => s.GetBySubject(release.Id, subject.Id))
                .ReturnsAsync(releaseFile);

            subjectRepository.Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService
                .Setup(s => s.GetTimePeriodRange(observations))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.GetSubjectMeta(
                    release.Id,
                    query,
                    observations);

                VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

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
            var publication = new Publication();
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var observations = ListOf(
                new Observation
                {
                    Location = new Location
                    {
                        GeographicLevel = GeographicLevel.Country,
                        Country = _england,
                    }
                },
                new Observation
                {
                    Location = new Location
                    {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = _northWest
                    }
                },
                new Observation
                {
                    Location = new Location
                    {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = _northEast
                    }
                },
                new Observation
                {
                    Location = new Location
                    {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = _eastMidlands
                    }
                });

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {
                        GeographicLevel.Region,
                        new List<string>
                        {
                            "Country",
                            "Region"
                        }
                    }
                }
            });

            var query = new ObservationQueryContext
            {
                Indicators = new List<Guid>(),
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.Country,
                        GeographicLevel.Region
                    }))
                .Returns(new List<BoundaryLevel>
                {
                    _countriesBoundaryLevel,
                    _regionsBoundaryLevel
                });

            filterItemRepository.Setup(s => s.GetFilterItemsFromObservations(observations))
                .ReturnsAsync(new List<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    release.Id,
                    subject.Id,
                    new List<Guid>(),
                    query.Indicators))
                .ReturnsAsync(new List<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            releaseDataFileRepository.Setup(s => s.GetBySubject(release.Id, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService
                .Setup(s => s.GetTimePeriodRange(observations))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    release.Id,
                    query,
                    observations);

                VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

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

        [Fact]
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject()
        {
            var publication = new Publication();
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            // Setup multiple geographic levels of data where some but not all of the levels have a hierarchy applied.

            var location1 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var location2 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northEast
            };

            var location3 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northWest
            };

            var location4 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _eastMidlands
            };

            var location5 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _derby
            };

            var location6 = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            };

            var observations = ListOf(
                // No hierarchy in Country level data
                new Observation
                {
                    Location = location1
                },
                // No hierarchy in Regional level data
                new Observation
                {
                    Location = location3
                },
                new Observation
                {
                    Location = location2
                },
                // A duplicate Location is here
                new Observation
                {
                    Location = location3
                },
                new Observation
                {
                    Location = location4
                },
                // Country-Region-LA hierarchy in the LA level data
                new Observation
                {
                    Location = location5
                },
                new Observation
                {
                    Location = location6
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

            var query = new ObservationQueryContext
            {
                Indicators = new List<Guid>(),
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.Country,
                        GeographicLevel.Region,
                        GeographicLevel.LocalAuthority
                    }))
                .Returns(new List<BoundaryLevel>());

            filterItemRepository
                .Setup(s => s.GetFilterItemsFromObservations(observations))
                .ReturnsAsync(new List<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    release.Id,
                    subject.Id,
                    new List<Guid>(),
                    query.Indicators))
                .ReturnsAsync(new List<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            releaseDataFileRepository.Setup(s => s.GetBySubject(release.Id, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService
                .Setup(s => s.GetTimePeriodRange(observations))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    release.Id,
                    query,
                    observations);

                VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

                var viewModel = result.AssertRight();

                var locationViewModels = viewModel.Locations;

                // Result has Country, Region and Local Authority levels
                Assert.Equal(3, locationViewModels.Count);
                Assert.True(locationViewModels.ContainsKey("country"));
                Assert.True(locationViewModels.ContainsKey("region"));
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect no hierarchy within the Country level
                var countries = locationViewModels["country"];

                var countryOption1 = Assert.Single(countries);
                Assert.NotNull(countryOption1);
                Assert.Equal(location1.Id, countryOption1.Id);
                Assert.Equal(_england.Name, countryOption1.Label);
                Assert.Equal(_england.Code, countryOption1.Value);
                Assert.Null(countryOption1.Level);
                Assert.Null(countryOption1.Options);

                // Expect no hierarchy within the Region level
                var regions = locationViewModels["region"];
                Assert.Equal(3, regions.Count);

                var regionOption1 = regions[0];
                Assert.Equal(location2.Id, regionOption1.Id);
                Assert.Equal(_northEast.Name, regionOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1.Value);
                Assert.Null(regionOption1.Level);
                Assert.Null(regionOption1.Options);

                var regionOption2 = regions[1];
                Assert.Equal(location3.Id, regionOption2.Id);
                Assert.Equal(_northWest.Name, regionOption2.Label);
                Assert.Equal(_northWest.Code, regionOption2.Value);
                Assert.Null(regionOption2.Level);
                Assert.Null(regionOption2.Options);

                var regionOption3 = regions[2];
                Assert.Equal(location4.Id, regionOption3.Id);
                Assert.Equal(_eastMidlands.Name, regionOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption3.Value);
                Assert.Null(regionOption3.Level);
                Assert.Null(regionOption3.Options);

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];

                var laOption1 = Assert.Single(localAuthorities);
                Assert.NotNull(laOption1);
                Assert.Null(laOption1.Id);
                Assert.Equal(_england.Name, laOption1.Label);
                Assert.Equal(_england.Code, laOption1.Value);
                Assert.Equal(GeographicLevel.Country, laOption1.Level);
                Assert.NotNull(laOption1.Options);

                var laOption1SubOption1 = Assert.Single(laOption1.Options!);
                Assert.NotNull(laOption1SubOption1);
                Assert.Null(laOption1SubOption1.Id);
                Assert.Equal(_eastMidlands.Name, laOption1SubOption1.Label);
                Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
                Assert.Equal(GeographicLevel.Region, laOption1SubOption1.Level);
                Assert.NotNull(laOption1SubOption1.Options);
                Assert.Equal(2, laOption1SubOption1.Options!.Count);

                var laOption1SubOption1SubOption1 = laOption1SubOption1.Options[0];
                Assert.Equal(location5.Id, laOption1SubOption1SubOption1.Id);
                Assert.Equal(_derby.Name, laOption1SubOption1SubOption1.Label);
                Assert.Equal(_derby.Code, laOption1SubOption1SubOption1.Value);
                Assert.Null(laOption1SubOption1SubOption1.Level);
                Assert.Null(laOption1SubOption1SubOption1.Options);

                var laOption1SubOption1SubOption2 = laOption1SubOption1.Options[1];
                Assert.Equal(location6.Id, laOption1SubOption1SubOption2.Id);
                Assert.Equal(_nottingham.Name, laOption1SubOption1SubOption2.Label);
                Assert.Equal(_nottingham.Code, laOption1SubOption1SubOption2.Value);
                Assert.Null(laOption1SubOption1SubOption2.Level);
                Assert.Null(laOption1SubOption1SubOption2.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject_SpecificBoundaryLevelId()
        {
            var publication = new Publication();
            var release = new Release();
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var observations = ListOf(
                new Observation
                {
                    Location = new Location
                    {
                        Id = Guid.NewGuid(),
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = _northEast
                    }
                },
                new Observation
                {
                    Location = new Location
                    {
                        Id = Guid.NewGuid(),
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = _northWest
                    }
                },
                new Observation
                {
                    Location = new Location
                    {
                        Id = Guid.NewGuid(),
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = _eastMidlands
                    }
                });

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {
                        GeographicLevel.Region,
                        new List<string>
                        {
                            "Country"
                        }
                    }
                }
            });

            // Setup a query requesting GeoJSON (by virtue of having a boundary level set)
            var query = new ObservationQueryContext
            {
                BoundaryLevel = 123,
                Indicators = new List<Guid>(),
                SubjectId = subject.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var geoJsonRepository = new Mock<IGeoJsonRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.Get(query.BoundaryLevel.Value))
                .ReturnsAsync(_regionsBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(ItIs.ListSequenceEqualTo(ListOf(GeographicLevel.Region))))
                .Returns(ListOf(_regionsBoundaryLevel));

            filterItemRepository
                .Setup(s => s.GetFilterItemsFromObservations(observations))
                .ReturnsAsync(new List<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    release.Id,
                    subject.Id,
                    new List<Guid>(),
                    query.Indicators))
                .ReturnsAsync(new List<Footnote>());

            geoJsonRepository.Setup(s => s.FindByBoundaryLevelAndCodes(
                    query.BoundaryLevel.Value,
                    new List<string>
                    {
                        _northEast.Code!, _northWest.Code!, _eastMidlands.Code!
                    }))
                .Returns(new Dictionary<string, GeoJson>
                {
                    {
                        _northEast.Code!,
                        _geoJson
                    },
                    {
                        _northWest.Code!,
                        _geoJson
                    },
                    {
                        _eastMidlands.Code!,
                        _geoJson
                    }
                });

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            releaseDataFileRepository.Setup(s => s.GetBySubject(release.Id, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService
                .Setup(s => s.GetTimePeriodRange(observations))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    geoJsonRepository: geoJsonRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    release.Id,
                    query,
                    observations);

                VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    geoJsonRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

                var viewModel = result.AssertRight();

                Assert.True(viewModel.GeoJsonAvailable);

                var locationViewModels = viewModel.Locations;

                // Result only has a Region level
                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("region"));

                // Expect a hierarchy of Country-Region within the Region level
                var regions = locationViewModels["region"];

                // Country option that groups the Regions does not have GeoJson
                var regionOption1 = Assert.Single(regions);
                Assert.NotNull(regionOption1);
                Assert.Null(regionOption1.Id);
                Assert.Equal(_england.Name, regionOption1.Label);
                Assert.Equal(_england.Code, regionOption1.Value);
                Assert.Null(regionOption1.GeoJson);
                Assert.Equal(GeographicLevel.Country, regionOption1.Level);
                Assert.NotNull(regionOption1.Options);
                Assert.Equal(3, regionOption1.Options!.Count);

                // Each Region option should have GeoJson
                var regionOption1SubOption1 = regionOption1.Options[0];
                Assert.Equal(observations[0].Location.Id, regionOption1SubOption1.Id);
                Assert.Equal(_northEast.Name, regionOption1SubOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1SubOption1.Value);
                Assert.NotNull(regionOption1SubOption1.GeoJson);
                Assert.Null(regionOption1SubOption1.Level);
                Assert.Null(regionOption1SubOption1.Options);

                var regionOption1SubOption2 = regionOption1.Options[1];
                Assert.Equal(observations[1].Location.Id, regionOption1SubOption2.Id);
                Assert.Equal(_northWest.Name, regionOption1SubOption2.Label);
                Assert.Equal(_northWest.Code, regionOption1SubOption2.Value);
                Assert.NotNull(regionOption1SubOption2.GeoJson);
                Assert.Null(regionOption1SubOption2.Level);
                Assert.Null(regionOption1SubOption2.Options);

                var regionOption1SubOption3 = regionOption1.Options[2];
                Assert.Equal(observations[2].Location.Id, regionOption1SubOption3.Id);
                Assert.Equal(_eastMidlands.Name, regionOption1SubOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption1SubOption3.Value);
                Assert.NotNull(regionOption1SubOption3.GeoJson);
                Assert.Null(regionOption1SubOption3.Level);
                Assert.Null(regionOption1SubOption3.Options);
            }
        }

        private static IOptions<LocationsOptions> DefaultLocationOptions()
        {
            return Options.Create(new LocationsOptions());
        }

        private static SubjectResultMetaService BuildService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            IFilterItemRepository? filterItemRepository = null,
            IBoundaryLevelRepository? boundaryLevelRepository = null,
            IFootnoteRepository? footnoteRepository = null,
            IGeoJsonRepository? geoJsonRepository = null,
            IIndicatorRepository? indicatorRepository = null,
            ITimePeriodService? timePeriodService = null,
            IUserService? userService = null,
            ISubjectRepository? subjectRepository = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null,
            IOptions<LocationsOptions>? options = null)
        {
            return new(
                contentDbContext ?? InMemoryContentDbContext(),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                boundaryLevelRepository ?? Mock.Of<IBoundaryLevelRepository>(MockBehavior.Strict),
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
                footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
                geoJsonRepository ?? Mock.Of<IGeoJsonRepository>(MockBehavior.Strict),
                indicatorRepository ?? Mock.Of<IIndicatorRepository>(MockBehavior.Strict),
                timePeriodService ?? Mock.Of<ITimePeriodService>(MockBehavior.Strict),
                userService ?? AlwaysTrueUserService().Object,
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>(MockBehavior.Strict),
                options ?? DefaultLocationOptions(),
                Mock.Of<ILogger<SubjectResultMetaService>>()
            );
        }
    }
}
