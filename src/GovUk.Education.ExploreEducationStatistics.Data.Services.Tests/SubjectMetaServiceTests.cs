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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class SubjectMetaServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _derbyDupe = new("E06000016", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        [Fact]
        public async Task GetSubjectMeta_SubjectNotFound()
        {
            var contextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            var service = BuildSubjectMetaService(statisticsDbContext);

            var result = await service.GetSubjectMeta(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetSubjectMetaRestricted_SubjectNoAccess()
        {
            var release = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var userService = new Mock<IUserService>(MockBehavior.Strict);

            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Subject>(resource => resource.Id == subject.Id),
                    DataSecurityPolicies.CanViewSubjectData))
                .ReturnsAsync(false);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext: statisticsDbContext,
                    userService: userService.Object);

                var result = await service.GetSubjectMetaRestricted(subject.Id);
                MockUtils.VerifyAllMocks(userService);

                result.AssertForbidden();
            }
        }

        [Fact]
        public async Task GetSubjectMeta_EmptyModelReturnedForSubject()
        {
            var release = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(Enumerable.Empty<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(Enumerable.Empty<IndicatorGroup>());

            locationRepository
                .Setup(s => s.GetLocationAttributesHierarchical(
                    subject.Id,
                    new Dictionary<GeographicLevel, List<string>>()))
                .ReturnsAsync(new Dictionary<GeographicLevel, List<LocationAttributeNode>>());

            timePeriodService.Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext: statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
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
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject()
        {
            var release = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

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

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(Enumerable.Empty<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(Enumerable.Empty<IndicatorGroup>());

            locationRepository
                .Setup(s => s.GetLocationAttributesHierarchical(subject.Id, options.Value.Hierarchies))
                .ReturnsAsync(locations);

            timePeriodService.Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext: statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
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
                Assert.Equal(_england.Name, countryOption1!.Label);
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

                Assert.Equal(_northEast.Name, regionOption1.Label);
                Assert.Equal(_northEast.Code, regionOption1.Value);
                Assert.Null(regionOption1.Level);
                Assert.Null(regionOption1.Options);

                Assert.Equal(_northWest.Name, regionOption2.Label);
                Assert.Equal(_northWest.Code, regionOption2.Value);
                Assert.Null(regionOption2.Level);
                Assert.Null(regionOption2.Options);

                Assert.Equal(_eastMidlands.Name, regionOption3.Label);
                Assert.Equal(_eastMidlands.Code, regionOption3.Value);
                Assert.Null(regionOption3.Level);
                Assert.Null(regionOption3.Options);

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                var laOption1 = Assert.Single(localAuthorities.Options);
                Assert.NotNull(laOption1);
                Assert.Equal(_england.Name, laOption1!.Label);
                Assert.Equal(_england.Code, laOption1.Value);
                Assert.Equal("Country", laOption1.Level);

                var laOption1SubOption1 = Assert.Single(laOption1.Options!);
                Assert.NotNull(laOption1SubOption1);
                Assert.Equal(_eastMidlands.Name, laOption1SubOption1!.Label);
                Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
                Assert.Equal("Region", laOption1SubOption1.Level);
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

                Assert.Empty(viewModel.Filters);
                Assert.Empty(viewModel.Indicators);
                Assert.Empty(viewModel.TimePeriod.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationViewModelsReturnedForSubject_LocationAttributeOfHierarchyIsMissing()
        {
            var release = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            // Setup a hierarchy of Country-Region-LA data within the Local Authority level where one of the attributes
            // of the hierarchy is not present (possible if the data was not provided, e.g. LA data supplied without Regions).
            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.LocalAuthority,
                    new List<LocationAttributeNode>
                    {
                        new(_england)
                        {
                            Children = new List<LocationAttributeNode>
                            {
                                // Omit the Region to simulate the Region data not being provided
                                new(Region.Empty())
                                {
                                    Attribute = Region.Empty(),
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

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(Enumerable.Empty<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(Enumerable.Empty<IndicatorGroup>());

            locationRepository
                .Setup(s => s.GetLocationAttributesHierarchical(subject.Id, options.Value.Hierarchies))
                .ReturnsAsync(locations);

            timePeriodService.Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext: statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
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
                Assert.Equal(_england.Name, laOption1!.Label);
                Assert.Equal(_england.Code, laOption1.Value);
                Assert.Equal("Country", laOption1.Level);

                // Expect an empty Region option grouping the Local Authorities
                var laOption1SubOption1 = Assert.Single(laOption1.Options!);
                Assert.NotNull(laOption1SubOption1);
                Assert.Equal(string.Empty, laOption1SubOption1!.Label);
                Assert.Equal(string.Empty, laOption1SubOption1.Value);
                Assert.Equal("Region", laOption1SubOption1.Level);
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

                Assert.Empty(viewModel.Filters);
                Assert.Empty(viewModel.Indicators);
                Assert.Empty(viewModel.TimePeriod.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationsAreDeduplicated_Flat()
        {
            var release = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.LocalAuthority,
                    new List<LocationAttributeNode>
                    {
                        new(_derby),
                        // Include a duplicate Derby that has a different code
                        new(_derbyDupe),
                        new(_nottingham)
                    }
                }
            };

            var options = Options.Create(new LocationsOptions());

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(Enumerable.Empty<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(Enumerable.Empty<IndicatorGroup>());

            locationRepository
                .Setup(s => s.GetLocationAttributesHierarchical(subject.Id, options.Value.Hierarchies))
                .ReturnsAsync(locations);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext: statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect a hierarchy of Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                Assert.Equal(3, localAuthorities.Options.Count);

                var laOption1 = localAuthorities.Options[0];

                // There are two locations with a label of Derby, so we
                // de-duplicate these by appending the code (which is unique).
                Assert.Equal("Derby (E06000015)", laOption1.Label);
                Assert.Equal(_derby.Code, laOption1.Value);
                Assert.Null(laOption1.Level);
                Assert.Null(laOption1.Options);

                var laOption2 = localAuthorities.Options[1];
                Assert.Equal("Derby (E06000016)", laOption2.Label);
                Assert.Equal(_derbyDupe.Code, laOption2.Value);
                Assert.Null(laOption2.Level);
                Assert.Null(laOption2.Options);

                var laOption3 = localAuthorities.Options[2];
                Assert.Equal(_nottingham.Name, laOption3.Label);
                Assert.Equal(_nottingham.Code, laOption3.Value);
                Assert.Null(laOption3.Level);
                Assert.Null(laOption3.Options);
            }
        }

        [Fact]
        public async Task GetSubjectMeta_LocationsAreDeduplicated_Hierarchy()
        {
            var release = new Release();
            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var locations = new Dictionary<GeographicLevel, List<LocationAttributeNode>>
            {
                {
                    GeographicLevel.LocalAuthority,
                    // Region-LA hierarchy in the LA level data
                    new List<LocationAttributeNode>
                    {
                        new(_eastMidlands)
                        {
                            Children = new List<LocationAttributeNode>
                            {
                                new(_derby),
                                // Include a duplicate Derby that has a different code
                                new(_derbyDupe),
                                new(_nottingham)
                            }
                        }
                    }
                }
            };

            var options = Options.Create(
                new LocationsOptions
                {
                    Hierarchies = new Dictionary<GeographicLevel, List<string>>
                    {
                        {
                            GeographicLevel.LocalAuthority,
                            new List<string>
                            {
                                "Region",
                                "LocalAuthority"
                            }
                        }
                    }
                }
            );

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var filterRepository = new Mock<IFilterRepository>(MockBehavior.Strict);
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>(MockBehavior.Strict);
            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            var timePeriodService = new Mock<ITimePeriodService>(MockBehavior.Strict);

            filterRepository
                .Setup(s => s.GetFiltersIncludingItems(subject.Id))
                .Returns(Enumerable.Empty<Filter>());

            indicatorGroupRepository
                .Setup(s => s.GetIndicatorGroups(subject.Id))
                .Returns(Enumerable.Empty<IndicatorGroup>());

            locationRepository
                .Setup(s => s.GetLocationAttributesHierarchical(subject.Id, options.Value.Hierarchies))
                .ReturnsAsync(locations);

            timePeriodService
                .Setup(s => s.GetTimePeriods(subject.Id))
                .Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildSubjectMetaService(
                    statisticsDbContext: statisticsDbContext,
                    filterRepository: filterRepository.Object,
                    indicatorGroupRepository: indicatorGroupRepository.Object,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    options: options
                );

                var result = (await service.GetSubjectMeta(subject.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    filterRepository,
                    indicatorGroupRepository,
                    locationRepository,
                    timePeriodService);

                var viewModel = Assert.IsAssignableFrom<SubjectMetaViewModel>(result);

                var locationViewModels = viewModel.Locations;

                Assert.Single(locationViewModels);
                Assert.True(locationViewModels.ContainsKey("localAuthority"));

                // Expect a hierarchy of Region-LA within the Local Authority level
                var localAuthorities = locationViewModels["localAuthority"];
                var laOption1 = Assert.Single(localAuthorities.Options);
                Assert.NotNull(laOption1);
                Assert.Equal(_eastMidlands.Name, laOption1!.Label);
                Assert.Equal(_eastMidlands.Code, laOption1.Value);
                Assert.Equal("Region", laOption1.Level);

                Assert.Equal(3, laOption1.Options!.Count);

                var laOption1SubOption1 = laOption1.Options[0];

                // There are two locations with a label of Derby, so we
                // de-duplicate these by appending the code (which is unique).
                Assert.Equal("Derby (E06000015)", laOption1SubOption1.Label);
                Assert.Equal(_derby.Code, laOption1SubOption1.Value);
                Assert.Null(laOption1SubOption1.Level);
                Assert.Null(laOption1SubOption1.Options);

                var laOption1SubOption2 = laOption1.Options[1];
                Assert.Equal("Derby (E06000016)", laOption1SubOption2.Label);
                Assert.Equal(_derbyDupe.Code, laOption1SubOption2.Value);
                Assert.Null(laOption1SubOption2.Level);
                Assert.Null(laOption1SubOption2.Options);

                var laOption1SubOption3 = laOption1.Options[2];
                Assert.Equal(_nottingham.Name, laOption1SubOption3.Label);
                Assert.Equal(_nottingham.Code, laOption1SubOption3.Value);
                Assert.Null(laOption1SubOption3.Level);
                Assert.Null(laOption1SubOption3.Options);
            }
        }

        private static IOptions<LocationsOptions> DefaultLocationOptions()
        {
            return Options.Create(new LocationsOptions());
        }

        private static SubjectMetaService BuildSubjectMetaService(
            StatisticsDbContext statisticsDbContext,
            IFilterRepository? filterRepository = null,
            IFilterItemRepository? filterItemRepository = null,
            IIndicatorGroupRepository? indicatorGroupRepository = null,
            ILocationRepository? locationRepository = null,
            IObservationService? observationService = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            ITimePeriodService? timePeriodService = null,
            IUserService? userService = null,
            IOptions<LocationsOptions>? options = null)
        {
            return new(
                filterRepository ?? Mock.Of<IFilterRepository>(MockBehavior.Strict),
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
                indicatorGroupRepository ?? Mock.Of<IIndicatorGroupRepository>(MockBehavior.Strict),
                locationRepository ?? Mock.Of<ILocationRepository>(MockBehavior.Strict),
                Mock.Of<ILogger<SubjectMetaService>>(),
                observationService ?? Mock.Of<IObservationService>(MockBehavior.Strict),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                timePeriodService ?? Mock.Of<ITimePeriodService>(MockBehavior.Strict),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                options ?? DefaultLocationOptions()
            );
        }
    }
}
