#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
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
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    [Collection(BlobCacheServiceTests)]
    public class SubjectMetaServiceTests : BlobCacheServiceTestFixture
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _blackpool = new("E06000009", "", "Blackpool");
        private readonly LocalAuthority _cheshireOldCode = new(null, "875", "Cheshire (Pre LGR 2009)");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _derbyDupe = new("E06000016", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");
        private readonly LocalAuthority _sunderland = new("E08000024", "", "Sunderland");

        [Fact]
        public async Task GetSubjectMeta_SubjectNotFound()
        {
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(It.IsAny<Guid>()))
                .ReturnsAsync((ReleaseSubject?) null);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId);
            var service = BuildSubjectMetaService(
                statisticsDbContext,
                releaseSubjectRepository: releaseSubjectRepository.Object);

            var result = await service.GetSubjectMeta(Guid.NewGuid());

            VerifyAllMocks(releaseSubjectRepository);

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetSubjectMetaRestricted_SubjectNoAccess()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(MockBehavior.Strict);

            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Subject>(resource => resource.Id == subject.Id),
                    DataSecurityPolicies.CanViewSubjectData))
                .ReturnsAsync(false);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    userService: userService.Object);

                var result = await service.GetSubjectMetaRestricted(statisticsRelease.Id, subject.Id);
                VerifyAllMocks(userService);

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task GetSubjectMeta_EmptyModelReturnedForSubject()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);
            
            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
                .ReturnsAsync(new List<Location>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    contentDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
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
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
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

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
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
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
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
                var regionOption2 = regions.Options[1];
                var regionOption3 = regions.Options[2];

                Assert.Equal(locations[1].Id, regionOption1.Id);
                Assert.Equal(_northEast.Name, regionOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1.Value);
                Assert.Null(regionOption1.Level);
                Assert.Null(regionOption1.Options);

                Assert.Equal(locations[2].Id, regionOption2.Id);
                Assert.Equal(_northWest.Name, regionOption2.Label);
                Assert.Equal(_northWest.Code, regionOption2.Value);
                Assert.Null(regionOption2.Level);
                Assert.Null(regionOption2.Options);

                Assert.Equal(locations[3].Id, regionOption3.Id);
                Assert.Equal(_eastMidlands.Name, regionOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption3.Value);
                Assert.Null(regionOption3.Level);
                Assert.Null(regionOption3.Options);

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                var laOption1 = Assert.Single(localAuthorities.Options);
                Assert.NotNull(laOption1);
                Assert.Null(laOption1.Id);
                Assert.Equal(_england.Name, laOption1.Label);
                Assert.Equal(_england.Code, laOption1.Value);
                Assert.Equal("country", laOption1.Level);

                var laOption1SubOption1 = Assert.Single(laOption1.Options!);
                Assert.NotNull(laOption1SubOption1);
                Assert.Null(laOption1SubOption1.Id);
                Assert.Equal(_eastMidlands.Name, laOption1SubOption1.Label);
                Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
                Assert.Equal("region", laOption1SubOption1.Level);
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
        public async Task GetSubjectMeta_LocationsForSubject_LocationAttributeOfHierarchyIsMissing()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            // Setup a hierarchy of Country-Region-LA data within the Local Authority level where one of the attributes
            // of the hierarchy is not present (possible if the data was not provided, e.g. LA data supplied without Regions).
            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = _england,
                    LocalAuthority = _derby
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Country = _england,
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

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
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
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                // Local Authority is the only level in the data
                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                var laOption1 = Assert.Single(localAuthorities.Options);
                Assert.NotNull(laOption1);
                Assert.Null(laOption1.Id);
                Assert.Equal(_england.Name, laOption1.Label);
                Assert.Equal(_england.Code, laOption1.Value);
                Assert.Equal("country", laOption1.Level);

                // Expect an empty Region option grouping the Local Authorities
                var laOption1SubOption1 = Assert.Single(laOption1.Options!);
                Assert.NotNull(laOption1SubOption1);
                Assert.Null(laOption1SubOption1.Id);
                Assert.Equal(string.Empty, laOption1SubOption1!.Label);
                Assert.Equal(string.Empty, laOption1SubOption1.Value);
                Assert.Equal("region", laOption1SubOption1.Level);
                Assert.Equal(2, laOption1SubOption1.Options!.Count);

                var laOption1SubOption1SubOption1 = laOption1SubOption1.Options[0];
                Assert.Equal(locations[0].Id, laOption1SubOption1SubOption1.Id);
                Assert.Equal(_derby.Name, laOption1SubOption1SubOption1.Label);
                Assert.Equal(_derby.Code, laOption1SubOption1SubOption1.Value);
                Assert.Null(laOption1SubOption1SubOption1.Level);
                Assert.Null(laOption1SubOption1SubOption1.Options);

                var laOption1SubOption1SubOption2 = laOption1SubOption1.Options[1];
                Assert.Equal(locations[1].Id, laOption1SubOption1SubOption2.Id);
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
        public async Task GetSubjectMeta_LocationsForSpecialCases()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            // Setup multiple geographic levels of data where some but not all of the levels have a hierarchy applied.
            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    LocalAuthority = _cheshireOldCode
                });

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>()
            });

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
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
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                Assert.Single(locationViewModels);

                var localAuthorities = locationViewModels["localAuthority"];

                // This Cheshire LA does not have a new code, so we fallback to
                // providing its old code the option value.
                var laOption1 = localAuthorities.Options[0];
                Assert.Equal(locations[0].Id, laOption1.Id);
                Assert.Equal(_cheshireOldCode.Name, laOption1.Label);
                Assert.Equal(_cheshireOldCode.OldCode, laOption1.Value);
                Assert.Null(laOption1.Level);
                Assert.Null(laOption1.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationsAreDeduplicated_Flat()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    LocalAuthority = _derby
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    LocalAuthority = _derbyDupe
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    LocalAuthority = _nottingham
                });

            var options = Options.Create(new LocationsOptions());

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
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
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect no hierarchy within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                Assert.Equal(3, localAuthorities.Options.Count);

                var laOption1 = localAuthorities.Options[0];

                // There are two locations with a label of Derby, so we
                // de-duplicate these by appending the code (which is unique).
                Assert.Equal(locations[0].Id, laOption1.Id);
                Assert.Equal("Derby (E06000015)", laOption1.Label);
                Assert.Equal(_derby.Code, laOption1.Value);
                Assert.Null(laOption1.Level);
                Assert.Null(laOption1.Options);

                var laOption2 = localAuthorities.Options[1];
                Assert.Equal(locations[1].Id, laOption2.Id);
                Assert.Equal("Derby (E06000016)", laOption2.Label);
                Assert.Equal(_derbyDupe.Code, laOption2.Value);
                Assert.Null(laOption2.Level);
                Assert.Null(laOption2.Options);

                var laOption3 = localAuthorities.Options[2];
                Assert.Equal(locations[2].Id, laOption3.Id);
                Assert.Equal(_nottingham.Name, laOption3.Label);
                Assert.Equal(_nottingham.Code, laOption3.Value);
                Assert.Null(laOption3.Level);
                Assert.Null(laOption3.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationsAreDeduplicated_Hierarchy()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Region = _eastMidlands,
                    LocalAuthority = _derby
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Region = _eastMidlands,
                    LocalAuthority = _derbyDupe
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Region = _eastMidlands,
                    LocalAuthority = _nottingham
                });

            var options = Options.Create(
                new LocationsOptions
                {
                    Hierarchies = new Dictionary<GeographicLevel, List<string>>
                    {
                        {
                            GeographicLevel.LocalAuthority,
                            new List<string>
                            {
                                "Region"
                            }
                        }
                    }
                }
            );

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
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
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect a hierarchy of Region within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                var laOption1 = Assert.Single(localAuthorities.Options);
                Assert.NotNull(laOption1);
                Assert.Null(laOption1.Id);
                Assert.Equal(_eastMidlands.Name, laOption1.Label);
                Assert.Equal(_eastMidlands.Code, laOption1.Value);
                Assert.Equal("region", laOption1.Level);

                Assert.Equal(3, laOption1.Options!.Count);

                var laOption1SubOption1 = laOption1.Options[0];

                // There are two locations with a label of Derby, so we
                // de-duplicate these by appending the code (which is unique).
                Assert.Equal(locations[0].Id, laOption1SubOption1.Id);
                Assert.Equal("Derby (E06000015)", laOption1SubOption1.Label);
                Assert.Equal(_derby.Code, laOption1SubOption1.Value);
                Assert.Null(laOption1SubOption1.Level);
                Assert.Null(laOption1SubOption1.Options);

                var laOption1SubOption2 = laOption1.Options[1];
                Assert.Equal(locations[1].Id, laOption1SubOption2.Id);
                Assert.Equal("Derby (E06000016)", laOption1SubOption2.Label);
                Assert.Equal(_derbyDupe.Code, laOption1SubOption2.Value);
                Assert.Null(laOption1SubOption2.Level);
                Assert.Null(laOption1SubOption2.Options);

                var laOption1SubOption3 = laOption1.Options[2];
                Assert.Equal(locations[2].Id, laOption1SubOption3.Id);
                Assert.Equal(_nottingham.Name, laOption1SubOption3.Label);
                Assert.Equal(_nottingham.Code, laOption1SubOption3.Value);
                Assert.Null(laOption1SubOption3.Level);
                Assert.Null(laOption1SubOption3.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationRegionsOrderedByCode()
        {
            var statisticsRelease = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject
            };

            // Regions have been ordered randomly, but we expect the returned
            // view models to be ordered by the region's location code.
            var locations = ListOf(
                // Flat Regions
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.Region,
                    Region = _northWest
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.Region,
                    Region = _eastMidlands
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.Region,
                    Region = _northEast
                },
                // Hierarchical Regions - LA
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Region = _northWest,
                    LocalAuthority = _blackpool
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Region = _eastMidlands,
                    LocalAuthority = _derby
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Region = _northEast,
                    LocalAuthority = _sunderland
                });

            var options = Options.Create(new LocationsOptions
            {
                Hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {
                        GeographicLevel.LocalAuthority,
                        new List<string>
                        {
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

            var contentRelease = new Content.Model.Release
            {
                Id = statisticsRelease.Id,
                Slug = "release-slug",
                Publication = new Publication
                {
                    Slug = "publication-slug"
                }
            };

            var cacheKey = GetCacheKey(contentRelease, releaseSubject);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }
            
            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            CacheService
                .Setup(s => s.GetItem(cacheKey, typeof(SubjectMetaViewModel)))
                .ReturnsAsync(null);

            CacheService
                .Setup(s => s.SetItem<object>(cacheKey, It.IsAny<SubjectMetaViewModel>()))
                .Returns(Task.CompletedTask);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(new List<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(new List<IndicatorGroup>());

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            locationRepository
                .Setup(s => s.GetDistinctForSubject(subject.Id))
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
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    releaseSubjectRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                // Result has Region and Local Authority levels
                Assert.Equal(2, locationViewModels.Count);
                Assert.True(locationViewModels.ContainsKey("region"));
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect no hierarchy within the Region level
                var regions = locationViewModels["region"];

                Assert.Equal("Regional", regions.Legend);
                Assert.Equal(3, regions.Options.Count);
                var regionOption1 = regions.Options[0];
                var regionOption2 = regions.Options[1];
                var regionOption3 = regions.Options[2];

                Assert.Equal(locations[2].Id, regionOption1.Id);
                Assert.Equal(_northEast.Name, regionOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1.Value);

                Assert.Equal(locations[0].Id, regionOption2.Id);
                Assert.Equal(_northWest.Name, regionOption2.Label);
                Assert.Equal(_northWest.Code, regionOption2.Value);

                Assert.Equal(locations[1].Id, regionOption3.Id);
                Assert.Equal(_eastMidlands.Name, regionOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption3.Value);

                // Expect a hierarchy of Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                Assert.Equal(3, localAuthorities.Options.Count);

                var laOption1 = localAuthorities.Options[0];
                Assert.NotNull(laOption1);
                Assert.Null(laOption1.Id);
                Assert.Equal(_northEast.Name, laOption1.Label);
                Assert.Equal(_northEast.Code, laOption1.Value);
                Assert.Equal("region", laOption1.Level);
                Assert.Single(laOption1.Options!);

                var laOption2 = localAuthorities.Options[1];
                Assert.NotNull(laOption2);
                Assert.Null(laOption2.Id);
                Assert.Equal(_northWest.Name, laOption2.Label);
                Assert.Equal(_northWest.Code, laOption2.Value);
                Assert.Equal("region", laOption2.Level);
                Assert.Single(laOption2.Options!);

                var laOption3 = localAuthorities.Options[2];
                Assert.NotNull(laOption3);
                Assert.Null(laOption3.Id);
                Assert.Equal(_eastMidlands.Name, laOption3.Label);
                Assert.Equal(_eastMidlands.Code, laOption3.Value);
                Assert.Equal("region", laOption3.Level);
                Assert.Single(laOption3.Options!);
            }
        }

        [Fact]
        public async Task GetSubjectMetaForQuery_TimePeriods()
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

                var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

                releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                    .ReturnsAsync(releaseSubject);

                var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

                timePeriodService
                    .Setup(s => s.GetTimePeriods(It.Is<IQueryable<Observation>>(
                        observationsWihMatchingLocations => observationsWihMatchingLocations
                            .ToList()
                            .Select(o => o.Id)
                            .SequenceEqual(observationsWihMatchingLocations.Select(m => m.Id)))))
                    .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>
                    {
                        (2012, TimeIdentifier.April),
                        (2012, TimeIdentifier.May),
                        (2012, TimeIdentifier.June)
                    });

                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await service.GetSubjectMeta(query, cancellationToken);

                VerifyAllMocks(
                    releaseSubjectRepository,
                    timePeriodService);

                var meta = result.AssertRight();
                Assert.Empty(meta.Locations);
                Assert.Empty(meta.Filters);
                Assert.Empty(meta.Indicators);

                var periods = meta.TimePeriod.Options.ToList();
                Assert.Equal(3, periods.Count());
                Assert.Equal(2012, periods[0].Year);
                Assert.Equal(TimeIdentifier.April, periods[0].Code);
                Assert.Equal(2012, periods[1].Year);
                Assert.Equal(TimeIdentifier.May, periods[1].Code);
                Assert.Equal(2012, periods[2].Year);
                Assert.Equal(TimeIdentifier.June, periods[2].Code);
            }
        }

        [Fact]
        public async Task GetSubjectMetaForQuery_FiltersAndIndicators()
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
                var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

                releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                    .ReturnsAsync(releaseSubject);

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
                    .Returns(indicatorGroups);

                var service = BuildSubjectMetaService(
                    statisticsDbContext,
                    observationService: observationService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    filterItemRepository: filterItemRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object);

                var result = await service.GetSubjectMeta(query, cancellationToken);

                VerifyAllMocks(
                    filterItemRepository,
                    indicatorGroupRepository,
                    observationService,
                    releaseSubjectRepository);

                var meta = result.AssertRight();
                meta.TimePeriod.AssertDeepEqualTo(new TimePeriodsMetaViewModel());
                Assert.Empty(meta.Locations);

                meta.Filters.AssertDeepEqualTo(new Dictionary<string, FilterMetaViewModel>
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

                meta.Indicators.AssertDeepEqualTo(new Dictionary<string, IndicatorGroupMetaViewModel>
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
        public async Task GetSubjectMetaForQuery_InvalidCombination_NoTimePeriodsOrLocations()
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

            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(MockBehavior.Strict);

            releaseSubjectRepository.Setup(mock => mock.GetReleaseSubjectForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildSubjectMetaService(statisticsDbContext,
                    releaseSubjectRepository: releaseSubjectRepository.Object);

                var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                    () => service.GetSubjectMeta(query, default));

                VerifyAllMocks(releaseSubjectRepository);

                Assert.Equal("Unable to determine which SubjectMeta information has requested " +
                             "(Parameter 'subjectMetaStep')", exception.Message);
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

        private static SubjectMetaCacheKey GetCacheKey(Content.Model.Release release, ReleaseSubject releaseSubject)
        {
            return new SubjectMetaCacheKey(release.Publication.Slug, release.Slug, releaseSubject.SubjectId);
        }

        private static SubjectMetaService BuildSubjectMetaService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IFilterRepository? filterRepository = null,
            IFilterItemRepository? filterItemRepository = null,
            IIndicatorGroupRepository? indicatorGroupRepository = null,
            ILocationRepository? locationRepository = null,
            IObservationService? observationService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            ITimePeriodService? timePeriodService = null,
            IUserService? userService = null,
            IOptions<LocationsOptions>? options = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(MockBehavior.Strict),
                statisticsDbContext,
                filterRepository ?? Mock.Of<IFilterRepository>(MockBehavior.Strict),
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
                indicatorGroupRepository ?? Mock.Of<IIndicatorGroupRepository>(MockBehavior.Strict),
                locationRepository ?? Mock.Of<ILocationRepository>(MockBehavior.Strict),
                Mock.Of<ILogger<SubjectMetaService>>(),
                observationService ?? Mock.Of<IObservationService>(MockBehavior.Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(MockBehavior.Strict),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                timePeriodService ?? Mock.Of<ITimePeriodService>(MockBehavior.Strict),
                userService ?? AlwaysTrueUserService().Object,
                options ?? DefaultLocationOptions()
            );
        }
    }
}
