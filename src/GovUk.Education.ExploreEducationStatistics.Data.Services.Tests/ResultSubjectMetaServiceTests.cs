#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ResultSubjectMetaServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _cheshireOldCode = new(null, "875", "Cheshire (Pre LGR 2009)");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        private readonly BoundaryLevel _countriesBoundaryLevel = new()
        {
            Id = 1,
            Label = "Countries November 2021"
        };
        private readonly BoundaryLevel _regionsBoundaryLevel = new()
        {
            Id = 2,
            Label = "Regions November 2021"
        };

        private readonly GeoJson _geoJson = new()
        {
            Value = "[]"
        };

        [Fact]
        public async Task GetSubjectMeta_SubjectNotFound()
        {
            var query = new SubjectMetaQueryContext();

            var contextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            var service = BuildResultSubjectMetaService(statisticsDbContext);

            var result = await service.GetSubjectMeta(
                releaseId: Guid.NewGuid(),
                query,
                new List<Observation>().AsQueryable());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetSubjectMeta_EmptyModelReturnedForSubject()
        {
            var publication = new Publication
            {
                Title = "Test Publication"
            };

            var releaseFile = new ReleaseFile
            {
                Name = "Test File"
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var releaseId = Guid.NewGuid();

            var observations = new List<Observation>().AsQueryable();

            var query = new SubjectMetaQueryContext
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
                await statisticsDbContext.Subject.AddAsync(subject);
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

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    new Dictionary<GeographicLevel, List<string>>()))
                .ReturnsAsync(new Dictionary<GeographicLevel, List<LocationAttributeNode>>());

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(releaseFile);

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
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
                Assert.Empty(viewModel.LocationsHierarchical);
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

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Country,
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                    }
                },
                {
                    GeographicLevel.Region,
                    new List<LocationAttributeNode>
                    {
                        new(_northEast),
                        new(_northWest),
                        new(_eastMidlands)
                    }
                }
            };

            var query = new SubjectMetaQueryContext
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
                await statisticsDbContext.Subject.AddAsync(subject);
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

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Country))
                .Returns(_countriesBoundaryLevel);

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

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    new Dictionary<GeographicLevel, List<string>>()))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
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
        // TODO EES-2992 Remove this when the location hierarchies work is complete
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Country,
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                    }
                },
                {
                    GeographicLevel.Region,
                    new List<LocationAttributeNode>
                    {
                        new(_northEast),
                        new(_northWest),
                        new(_eastMidlands)
                    }
                }
            };

            var query = new SubjectMetaQueryContext
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
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var featureManager = new Mock<IFeatureManager>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(It.IsAny<GeographicLevel>()))
                .Returns((BoundaryLevel?) null);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.Country,
                        GeographicLevel.Region
                    }))
                .Returns(new List<BoundaryLevel>());

            // Setup the location hierarchies featured as disabled
            featureManager.Setup(s => s.IsEnabledAsync("LocationHierarchies"))
                .ReturnsAsync(false);

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    null))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    featureManager: featureManager.Object,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

                var viewModel = result.AssertRight();

                // With the location hierarchies feature turned off, hierarchical locations should be empty
                Assert.Empty(viewModel.LocationsHierarchical);

                // Locations should be populated in the legacy locations field
                var locationViewModels = viewModel.Locations;

                Assert.Equal(4, locationViewModels.Count);

                Assert.Equal(_england.Name, locationViewModels[0].Label);
                Assert.Equal(_england.Code, locationViewModels[0].Value);
                Assert.Equal(GeographicLevel.Country, locationViewModels[0].Level);
                Assert.Null(locationViewModels[0].GeoJson);

                Assert.Equal(_eastMidlands.Name, locationViewModels[1].Label);
                Assert.Equal(_eastMidlands.Code, locationViewModels[1].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[1].Level);
                Assert.Null(locationViewModels[1].GeoJson);

                Assert.Equal(_northEast.Name, locationViewModels[2].Label);
                Assert.Equal(_northEast.Code, locationViewModels[2].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[2].Level);
                Assert.Null(locationViewModels[2].GeoJson);

                Assert.Equal(_northWest.Name, locationViewModels[3].Label);
                Assert.Equal(_northWest.Code, locationViewModels[3].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[3].Level);
                Assert.Null(locationViewModels[3].GeoJson);
            }
        }

        [Fact]
        // TODO EES-2992 Remove this when the location hierarchies work is complete
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject_IncludeGeoJson()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Country,
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                    }
                },
                {
                    GeographicLevel.Region,
                    new List<LocationAttributeNode>
                    {
                        new(_northEast),
                        new(_northWest),
                        new(_eastMidlands)
                    }
                }
            };

            // Setup a query requesting geoJson but not with any specific boundary level id
            var query = new SubjectMetaQueryContext
            {
                BoundaryLevel = null,
                IncludeGeoJson = true,
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
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var featureManager = new Mock<IFeatureManager>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var geoJsonRepository = new Mock<IGeoJsonRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Country))
                .Returns(_countriesBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Region))
                .Returns(_regionsBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.Country,
                        GeographicLevel.Region
                    }))
                .Returns(new List<BoundaryLevel>());

            // Setup the location hierarchies featured as disabled
            featureManager.Setup(s => s.IsEnabledAsync("LocationHierarchies"))
                .ReturnsAsync(false);

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            geoJsonRepository.Setup(s => s.FindByBoundaryLevelAndCodes(
                _countriesBoundaryLevel.Id,
                new List<string>
                {
                    _england.Code!
                })).Returns(new Dictionary<string, GeoJson>
            {
                {
                    _england.Code!,
                    _geoJson
                }
            });

            geoJsonRepository.Setup(s => s.FindByBoundaryLevelAndCodes(
                    _regionsBoundaryLevel.Id,
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

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    null))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    featureManager: featureManager.Object,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    geoJsonRepository: geoJsonRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
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

                // With the location hierarchies feature turned off, hierarchical locations should be empty
                Assert.Empty(viewModel.LocationsHierarchical);

                // Locations should be populated in the legacy locations field
                var locationViewModels = viewModel.Locations;

                Assert.Equal(4, locationViewModels.Count);

                // Expect all results to have GeoJson

                Assert.Equal(_england.Name, locationViewModels[0].Label);
                Assert.Equal(_england.Code, locationViewModels[0].Value);
                Assert.Equal(GeographicLevel.Country, locationViewModels[0].Level);
                Assert.NotNull(locationViewModels[0].GeoJson);

                Assert.Equal(_eastMidlands.Name, locationViewModels[1].Label);
                Assert.Equal(_eastMidlands.Code, locationViewModels[1].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[1].Level);
                Assert.NotNull(locationViewModels[1].GeoJson);

                Assert.Equal(_northEast.Name, locationViewModels[2].Label);
                Assert.Equal(_northEast.Code, locationViewModels[2].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[2].Level);
                Assert.NotNull(locationViewModels[2].GeoJson);

                Assert.Equal(_northWest.Name, locationViewModels[3].Label);
                Assert.Equal(_northWest.Code, locationViewModels[3].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[3].Level);
                Assert.NotNull(locationViewModels[3].GeoJson);
            }
        }

        [Fact]
        // TODO EES-2992 Remove this when the location hierarchies work is complete
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject_SpecificBoundaryLevelId()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Region,
                    new List<LocationAttributeNode>
                    {
                        new(_northEast),
                        new(_northWest),
                        new(_eastMidlands)
                    }
                }
            };

            // Setup a query requesting geoJson with a specific boundary level id
            var query = new SubjectMetaQueryContext
            {
                BoundaryLevel = 123,
                IncludeGeoJson = true,
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
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
            var featureManager = new Mock<IFeatureManager>(MockBehavior.Strict);
            var filterItemRepository = new Mock<IFilterItemRepository>(MockBehavior.Strict);
            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            var geoJsonRepository = new Mock<IGeoJsonRepository>(MockBehavior.Strict);
            var indicatorRepository = new Mock<IIndicatorRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Region))
                .Returns(_regionsBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindRelatedByBoundaryLevel(query.BoundaryLevel.Value))
                .Returns(new List<BoundaryLevel>
                {
                    _regionsBoundaryLevel
                });

            // Setup the location hierarchies featured as disabled
            featureManager.Setup(s => s.IsEnabledAsync("LocationHierarchies"))
                .ReturnsAsync(false);

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

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

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    null))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    featureManager: featureManager.Object,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    geoJsonRepository: geoJsonRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
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

                // With the location hierarchies feature turned off, hierarchical locations should be empty
                Assert.Empty(viewModel.LocationsHierarchical);

                // Locations should be populated in the legacy locations field
                var locationViewModels = viewModel.Locations;

                Assert.Equal(3, locationViewModels.Count);

                // Expect all results to have GeoJson

                Assert.Equal(_eastMidlands.Name, locationViewModels[0].Label);
                Assert.Equal(_eastMidlands.Code, locationViewModels[0].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[0].Level);
                Assert.NotNull(locationViewModels[0].GeoJson);

                Assert.Equal(_northEast.Name, locationViewModels[1].Label);
                Assert.Equal(_northEast.Code, locationViewModels[1].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[1].Level);
                Assert.NotNull(locationViewModels[1].GeoJson);

                Assert.Equal(_northWest.Name, locationViewModels[2].Label);
                Assert.Equal(_northWest.Code, locationViewModels[2].Value);
                Assert.Equal(GeographicLevel.Region, locationViewModels[2].Level);
                Assert.NotNull(locationViewModels[2].GeoJson);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_HierarchicalLocationViewModelsReturnedForSubject()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            // Setup multiple geographic levels of data where some but not all of the levels have a hierarchy applied.
            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Country,
                    // No hierarchy in Country level data
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                    }
                },
                {
                    GeographicLevel.Region,
                    // No hierarchy in Regional level data
                    new List<LocationAttributeNode>
                    {
                        new(_northEast),
                        new(_northWest),
                        new(_eastMidlands)
                    }
                },
                {
                    GeographicLevel.LocalAuthority,
                    // Country-Region-LA hierarchy in the LA level data
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                        {
                            Children = new List<LocationAttributeNode>
                            {
                                new(_eastMidlands)
                                {
                                    Children = new List<LocationAttributeNode>
                                    {
                                        new(_derby),
                                        new(_nottingham)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {
                        GeographicLevel.LocalAuthority,
                        new List<string>
                        {
                            "Country",
                            "Region",
                            "LocalAuthority"
                        }
                    }
                }
            });

            var query = new SubjectMetaQueryContext
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
                await statisticsDbContext.Subject.AddAsync(subject);
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

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(It.IsAny<GeographicLevel>()))
                .Returns((BoundaryLevel?) null);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.Country,
                        GeographicLevel.Region,
                        GeographicLevel.LocalAuthority
                    }))
                .Returns(new List<BoundaryLevel>());

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    options.Value.Hierarchies))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

                var viewModel = result.AssertRight();

                // With the location hierarchies feature turned on, legacy locations should be empty
                Assert.Empty(viewModel.Locations);

                var locationViewModels = viewModel.LocationsHierarchical;

                // Result has Country, Region and Local Authority levels
                Assert.Equal(3, locationViewModels.Count);
                Assert.True(locationViewModels.ContainsKey("country"));
                Assert.True(locationViewModels.ContainsKey("region"));
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect no hierarchy within the Country level
                var countries = locationViewModels["country"];

                var countryOption1 = Assert.Single(countries);
                Assert.NotNull(countryOption1);
                Assert.Equal(_england.Name, countryOption1!.Label);
                Assert.Equal(_england.Code, countryOption1.Value);
                Assert.Null(countryOption1.GeoJson);
                Assert.Null(countryOption1.Level);
                Assert.Null(countryOption1.Options);

                // Expect no hierarchy within the Region level
                var regions = locationViewModels["region"];

                Assert.Equal(3, regions.Count);
                var regionOption1 = regions[0];
                var regionOption2 = regions[1];
                var regionOption3 = regions[2];
                Assert.Null(countryOption1.GeoJson);
                Assert.Equal(_northEast.Name, regionOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1.Value);
                Assert.Null(regionOption1.Level);
                Assert.Null(regionOption1.Options);
                Assert.Null(regionOption2.GeoJson);
                Assert.Equal(_northWest.Name, regionOption2.Label);
                Assert.Equal(_northWest.Code, regionOption2.Value);
                Assert.Null(regionOption2.Level);
                Assert.Null(regionOption2.Options);
                Assert.Null(regionOption3.GeoJson);
                Assert.Equal(_eastMidlands.Name, regionOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption3.Value);
                Assert.Null(regionOption3.Level);
                Assert.Null(regionOption3.Options);

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];

                var laOption1 = Assert.Single(localAuthorities);
                Assert.NotNull(laOption1);
                Assert.Equal(_england.Name, laOption1!.Label);
                Assert.Equal(_england.Code, laOption1.Value);
                Assert.Equal("Country", laOption1.Level);
                Assert.NotNull(laOption1.Options);

                var laOption1SubOption1 = Assert.Single(laOption1.Options!);
                Assert.NotNull(laOption1SubOption1);
                Assert.Equal(_eastMidlands.Name, laOption1SubOption1!.Label);
                Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
                Assert.Equal("Region", laOption1SubOption1.Level);
                Assert.NotNull(laOption1SubOption1.Options);
                Assert.Equal(2, laOption1SubOption1.Options!.Count);

                var laOption1SubOption1SubOption1 = laOption1SubOption1.Options[0];
                Assert.Equal(_derby.Name, laOption1SubOption1SubOption1.Label);
                Assert.Equal(_derby.Code, laOption1SubOption1SubOption1.Value);
                Assert.Null(laOption1SubOption1SubOption1.Level);
                Assert.Null(laOption1SubOption1SubOption1.Options);

                var laOption1SubOption1SubOption2 = laOption1SubOption1.Options[1];
                Assert.Equal(_nottingham.Name, laOption1SubOption1SubOption2.Label);
                Assert.Equal(_nottingham.Code, laOption1SubOption1SubOption2.Value);
                Assert.Null(laOption1SubOption1SubOption2.Level);
                Assert.Null(laOption1SubOption1SubOption2.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_HierarchicalLocationViewModelsReturnedForSubject_IncludeGeoJson()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Country,
                    // No hierarchy in Country level data
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                    }
                },
                {
                    GeographicLevel.Region,
                    // Country-Region hierarchy in the Region level data
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                        {
                            Children = new List<LocationAttributeNode>
                            {
                                new(_northEast),
                                new(_northWest),
                                new(_eastMidlands)
                            }
                        }
                    }
                }
            };

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

            // Setup a query requesting geoJson but not with any specific boundary level id
            var query = new SubjectMetaQueryContext
            {
                BoundaryLevel = null,
                IncludeGeoJson = true,
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
                await statisticsDbContext.Subject.AddAsync(subject);
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

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Country))
                .Returns(_countriesBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Region))
                .Returns(_regionsBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.Country,
                        GeographicLevel.Region
                    }))
                .Returns(new List<BoundaryLevel>());

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            geoJsonRepository.Setup(s => s.FindByBoundaryLevelAndCodes(
                _countriesBoundaryLevel.Id,
                new List<string>
                {
                    _england.Code!
                })).Returns(new Dictionary<string, GeoJson>
            {
                {
                    _england.Code!,
                    _geoJson
                }
            });

            geoJsonRepository.Setup(s => s.FindByBoundaryLevelAndCodes(
                    _regionsBoundaryLevel.Id,
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

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    options.Value.Hierarchies))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    geoJsonRepository: geoJsonRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
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

                // With the location hierarchies feature turned on, legacy locations should be empty
                Assert.Empty(viewModel.Locations);

                var locationViewModels = viewModel.LocationsHierarchical;

                // Result has Country and Region levels
                Assert.Equal(2, locationViewModels.Count);
                Assert.True(locationViewModels.ContainsKey("country"));
                Assert.True(locationViewModels.ContainsKey("region"));

                // Expect no hierarchy within the Country level but the option should have GeoJson
                var countries = locationViewModels["country"];

                var countryOption1 = Assert.Single(countries);
                Assert.NotNull(countryOption1);
                Assert.Equal(_england.Name, countryOption1!.Label);
                Assert.Equal(_england.Code, countryOption1.Value);
                Assert.NotNull(countryOption1.GeoJson);
                Assert.Null(countryOption1.Level);
                Assert.Null(countryOption1.Options);

                // Expect a hierarchy of Country-Region within the Region level
                var regions = locationViewModels["region"];

                // Country option that groups the Regions does not have GeoJson
                var regionOption1 = Assert.Single(regions);
                Assert.NotNull(regionOption1);
                Assert.Equal(_england.Name, regionOption1!.Label);
                Assert.Equal(_england.Code, regionOption1.Value);
                Assert.Null(regionOption1.GeoJson);
                Assert.Equal("Country", regionOption1.Level);
                Assert.NotNull(regionOption1.Options);
                Assert.Equal(3, regionOption1.Options!.Count);

                // Each Region option should have GeoJson
                var regionOption1SubOption1 = regionOption1.Options[0];
                Assert.Equal(_northEast.Name, regionOption1SubOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1SubOption1.Value);
                Assert.NotNull(regionOption1SubOption1.GeoJson);
                Assert.Null(regionOption1SubOption1.Level);
                Assert.Null(regionOption1SubOption1.Options);

                var regionOption1SubOption2 = regionOption1.Options[1];
                Assert.Equal(_northWest.Name, regionOption1SubOption2.Label);
                Assert.Equal(_northWest.Code, regionOption1SubOption2.Value);
                Assert.NotNull(regionOption1SubOption2.GeoJson);
                Assert.Null(regionOption1SubOption2.Level);
                Assert.Null(regionOption1SubOption2.Options);

                var regionOption1SubOption3 = regionOption1.Options[2];
                Assert.Equal(_eastMidlands.Name, regionOption1SubOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption1SubOption3.Value);
                Assert.NotNull(regionOption1SubOption3.GeoJson);
                Assert.Null(regionOption1SubOption3.Level);
                Assert.Null(regionOption1SubOption3.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_HierarchicalLocationViewModelsReturnedForSubject_SpecificBoundaryLevelId()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.Region,
                    // Country-Region hierarchy in the Region level data
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                        {
                            Children = new List<LocationAttributeNode>
                            {
                                new(_northEast),
                                new(_northWest),
                                new(_eastMidlands)
                            }
                        }
                    }
                }
            };

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

            // Setup a query requesting geoJson with a specific boundary level id
            var query = new SubjectMetaQueryContext
            {
                BoundaryLevel = 123,
                IncludeGeoJson = true,
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
                await statisticsDbContext.Subject.AddAsync(subject);
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

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(GeographicLevel.Region))
                .Returns(_regionsBoundaryLevel);

            boundaryLevelRepository.Setup(s => s.FindRelatedByBoundaryLevel(query.BoundaryLevel.Value))
                .Returns(new List<BoundaryLevel>
                {
                    _regionsBoundaryLevel
                });

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

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

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    options.Value.Hierarchies))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    geoJsonRepository: geoJsonRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
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

                // With the location hierarchies feature turned on, legacy locations should be empty
                Assert.Empty(viewModel.Locations);

                var locationViewModels = viewModel.LocationsHierarchical;

                // Result only has a Region level
                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("region"));

                // Expect a hierarchy of Country-Region within the Region level
                var regions = locationViewModels["region"];

                // Country option that groups the Regions does not have GeoJson
                var regionOption1 = Assert.Single(regions);
                Assert.NotNull(regionOption1);
                Assert.Equal(_england.Name, regionOption1!.Label);
                Assert.Equal(_england.Code, regionOption1.Value);
                Assert.Null(regionOption1.GeoJson);
                Assert.Equal("Country", regionOption1.Level);
                Assert.NotNull(regionOption1.Options);
                Assert.Equal(3, regionOption1.Options!.Count);

                // Each Region option should have GeoJson
                var regionOption1SubOption1 = regionOption1.Options[0];
                Assert.Equal(_northEast.Name, regionOption1SubOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1SubOption1.Value);
                Assert.NotNull(regionOption1SubOption1.GeoJson);
                Assert.Null(regionOption1SubOption1.Level);
                Assert.Null(regionOption1SubOption1.Options);

                var regionOption1SubOption2 = regionOption1.Options[1];
                Assert.Equal(_northWest.Name, regionOption1SubOption2.Label);
                Assert.Equal(_northWest.Code, regionOption1SubOption2.Value);
                Assert.NotNull(regionOption1SubOption2.GeoJson);
                Assert.Null(regionOption1SubOption2.Level);
                Assert.Null(regionOption1SubOption2.Options);

                var regionOption1SubOption3 = regionOption1.Options[2];
                Assert.Equal(_eastMidlands.Name, regionOption1SubOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption1SubOption3.Value);
                Assert.NotNull(regionOption1SubOption3.GeoJson);
                Assert.Null(regionOption1SubOption3.Level);
                Assert.Null(regionOption1SubOption3.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationsForSpecialCases()
        {
            var publication = new Publication();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var observations = new List<Observation>().AsQueryable();

            var releaseId = Guid.NewGuid();

            // Setup multiple geographic levels of data where some but not all of the levels have a hierarchy applied.
            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.LocalAuthority,
                    new List<LocationAttributeNode>
                    {
                        new(_cheshireOldCode),
                        new(_derby)
                    }
                }
            };

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>()
            });

            var query = new SubjectMetaQueryContext
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
                await statisticsDbContext.Subject.AddAsync(subject);
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

            boundaryLevelRepository.Setup(s => s.FindLatestByGeographicLevel(It.IsAny<GeographicLevel>()))
                .Returns((BoundaryLevel?) null);

            boundaryLevelRepository.Setup(s => s.FindByGeographicLevels(
                    new List<GeographicLevel>
                    {
                        GeographicLevel.LocalAuthority
                    }))
                .Returns(new List<BoundaryLevel>());

            filterItemRepository.Setup(s => s.GetFilterItems(
                    subject.Id,
                    observations,
                    true))
                .Returns(Enumerable.Empty<FilterItem>());

            footnoteRepository.Setup(s => s.GetFilteredFootnotes(
                    releaseId,
                    subject.Id,
                    observations,
                    query.Indicators))
                .Returns(Enumerable.Empty<Footnote>());

            indicatorRepository.Setup(s => s.GetIndicators(subject.Id, query.Indicators))
                .Returns(Enumerable.Empty<Indicator>());

            locationRepository.Setup(s => s.GetLocationAttributesHierarchical(
                    observations,
                    options.Value.Hierarchies))
                .ReturnsAsync(locations);

            releaseDataFileRepository.Setup(s => s.GetBySubject(releaseId, subject.Id))
                .ReturnsAsync(new ReleaseFile());

            subjectRepository.Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(publication.Id);

            timePeriodService.Setup(s => s.GetTimePeriodRange(observations))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildResultSubjectMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    boundaryLevelRepository: boundaryLevelRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    footnoteRepository: footnoteRepository.Object,
                    indicatorRepository: indicatorRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = await service.GetSubjectMeta(
                    releaseId: releaseId,
                    query,
                    observations);

                MockUtils.VerifyAllMocks(
                    boundaryLevelRepository,
                    filterItemRepository,
                    footnoteRepository,
                    indicatorRepository,
                    locationRepository,
                    releaseDataFileRepository,
                    subjectRepository,
                    timePeriodService);

                var viewModel = result.AssertRight();

                // With the location hierarchies feature turned on, legacy locations should be empty
                Assert.Empty(viewModel.Locations);

                var locationViewModels = viewModel.LocationsHierarchical;

                Assert.Single(locationViewModels);

                var localAuthorities = locationViewModels["localAuthority"];

                // This Cheshire LA does not have a new code, so we fallback to
                // providing its old code the option value.
                var laOption1 = localAuthorities[0];
                Assert.Equal(_cheshireOldCode.Name, laOption1.Label);
                Assert.Equal(_cheshireOldCode.OldCode, laOption1.Value);
                Assert.Null(laOption1.Level);
            }
        }

        private static IFeatureManager DefaultFeatureManager()
        {
            var featureManager = new Mock<IFeatureManager>(MockBehavior.Strict);
            featureManager.Setup(s => s.IsEnabledAsync("LocationHierarchies"))
                .ReturnsAsync(true);
            return featureManager.Object;
        }

        private static IOptions<LocationsOptions> DefaultLocationOptions()
        {
            return Options.Create(new LocationsOptions());
        }

        private static ResultSubjectMetaService BuildResultSubjectMetaService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IFeatureManager? featureManager = null,
            IFilterItemRepository? filterItemRepository = null,
            IBoundaryLevelRepository? boundaryLevelRepository = null,
            IFootnoteRepository? footnoteRepository = null,
            IGeoJsonRepository? geoJsonRepository = null,
            IIndicatorRepository? indicatorRepository = null,
            ILocationRepository? locationRepository = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            ITimePeriodService? timePeriodService = null,
            IUserService? userService = null,
            ISubjectRepository? subjectRepository = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null,
            IOptions<LocationsOptions>? options = null)
        {
            return new(
                featureManager ?? DefaultFeatureManager(),
                contentDbContext ?? InMemoryContentDbContext(),
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
                boundaryLevelRepository ?? Mock.Of<IBoundaryLevelRepository>(MockBehavior.Strict),
                footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
                geoJsonRepository ?? Mock.Of<IGeoJsonRepository>(MockBehavior.Strict),
                indicatorRepository ?? Mock.Of<IIndicatorRepository>(MockBehavior.Strict),
                locationRepository ?? Mock.Of<ILocationRepository>(MockBehavior.Strict),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                timePeriodService ?? Mock.Of<ITimePeriodService>(MockBehavior.Strict),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                releaseDataFileRepository ?? Mock.Of<IReleaseDataFileRepository>(MockBehavior.Strict),
                options ?? DefaultLocationOptions(),
                Mock.Of<ILogger<ResultSubjectMetaService>>()
            );
        }
    }
}
