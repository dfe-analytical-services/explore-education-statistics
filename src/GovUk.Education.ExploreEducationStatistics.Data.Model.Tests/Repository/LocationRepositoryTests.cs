#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class LocationRepositoryTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        private readonly List<string> _countryRegionLaHierarchy = ListOf("Country", "Region", "LocalAuthority");

        [Fact]
        public async Task GetLocationAttributesHierarchical_NoLocationsFoundForSubject()
        {
            // Test a scenario where there is no Subject data

            // Add a Subject/Observation/Location not related to the Subject that will be tested
            var observation = new Observation
            {
                Location = new Location
                {
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _nottingham
                },
                GeographicLevel = GeographicLevel.LocalAuthority,
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Observation.AddAsync(observation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var repository = BuildRepository(statisticsDbContext);

                var result = await repository.GetLocationAttributesHierarchical(Guid.NewGuid());

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetLocationAttributesHierarchical_HierarchyContainsInvalidAttributeName()
        {
            // Test a scenario where a hierarchy is defined but mentions an attribute not part of the Location type

            var subject = new Subject();

            var observation = new Observation
            {
                Location = new Location
                {
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _nottingham
                },
                GeographicLevel = GeographicLevel.LocalAuthority,
                Subject = subject
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Observation.AddAsync(observation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    // Hierarchy will not be matched since Subject has no School level data
                    {GeographicLevel.LocalAuthority, ListOf("Country", "Region", "NotAPropertyOfLocation")}
                };

                var repository = BuildRepository(statisticsDbContext);

                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    async () => { await repository.GetLocationAttributesHierarchical(subject.Id, hierarchies); }
                );

                Assert.Equal($"Location does not have a property NotAPropertyOfLocation", exception.Message);
            }
        }

        [Fact]
        public async Task GetLocationAttributesHierarchical_GeographicLevelsHaveNoRelevantHierarchy()
        {
            // Test a scenario where hierarchies are defined but none are relevant to the Subject data

            var subject = new Subject();

            var observations = new List<Observation>
            {
                new()
                {
                    Location = new Location
                    {
                        Country = _england
                    },
                    GeographicLevel = GeographicLevel.Country,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _northEast
                    },
                    GeographicLevel = GeographicLevel.Region,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _northWest
                    },
                    GeographicLevel = GeographicLevel.Region,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _derby
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    // School hierarchy that is insignificant since Subject has no School level data
                    {GeographicLevel.School, ListOf("LocalAuthority", "School")}
                };

                var repository = BuildRepository(statisticsDbContext);
                var result = await repository.GetLocationAttributesHierarchical(subject.Id, hierarchies);

                // Data has Country, Region and Local Authority levels
                Assert.Equal(3, result.Count);
                Assert.True(result.ContainsKey(GeographicLevel.Country));
                Assert.True(result.ContainsKey(GeographicLevel.Region));
                Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

                // Expect no hierarchy within any of the levels

                var countries = result[GeographicLevel.Country];

                Assert.Single(countries);
                Assert.Equal(_england, countries[0].Attribute);
                Assert.Empty(countries[0].Children);

                var regions = result[GeographicLevel.Region];

                Assert.Equal(2, regions.Count);
                Assert.Equal(_northEast, regions[0].Attribute);
                Assert.Empty(regions[0].Children);
                Assert.Equal(_northWest, regions[1].Attribute);
                Assert.Empty(regions[1].Children);

                var localAuthorities = result[GeographicLevel.LocalAuthority];
                Assert.Single(localAuthorities);
                Assert.Equal(_derby, localAuthorities[0].Attribute);
                Assert.Empty(localAuthorities[0].Children);
            }
        }

        [Fact]
        public async Task GetLocationAttributesHierarchical_GeographicLevelsHaveRelevantHierarchy()
        {
            // Test a scenario where a hierarchy is defined that can be applied to the Subject data

            var subject = new Subject();

            var observations = new List<Observation>
            {
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _derby
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _nottingham
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
                };

                var repository = BuildRepository(statisticsDbContext);
                var result = await repository.GetLocationAttributesHierarchical(subject.Id, hierarchies);

                // Local Authority is the only level in the data
                Assert.Single(result);
                Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = result[GeographicLevel.LocalAuthority];
                Assert.Single(localAuthorities);

                Assert.Equal(_england, localAuthorities[0].Attribute);
                Assert.Single(localAuthorities[0].Children);

                Assert.Equal(_eastMidlands, localAuthorities[0].Children[0].Attribute);
                Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

                Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

                Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);
            }
        }

        [Fact]
        public async Task GetLocationAttributesHierarchical_AllLocationsHaveFewerAttributesThanHierarchy()
        {
            /*
             * Test a scenario where all Observations have Locations that don't have attributes present in the hierarchy.
             * E.g a Country-Region-LA hierarchy is defined but no Locations have Country or Region:
             * 
             * geographic_level    country_code    country_name    region_code    region_name    la_code    la_name
             * ====================================================================================================
             * Local authority                                                                   E06000015  Derby
             * Local authority                                                                   E06000018  Nottingham
             */

            var subject = new Subject();

            var observations = new List<Observation>
            {
                new()
                {
                    Location = new Location
                    {
                        LocalAuthority = _derby
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        LocalAuthority = _nottingham
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
                };

                var repository = BuildRepository(statisticsDbContext);
                var result = await repository.GetLocationAttributesHierarchical(subject.Id, hierarchies);

                // Local Authority is the only level in the data
                Assert.Single(result);
                Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = result[GeographicLevel.LocalAuthority];
                Assert.Single(localAuthorities);

                // Country attribute at depth 1 is empty as not defined for any of the LA data 
                Assert.Equal(Country.Empty(), localAuthorities[0].Attribute);
                Assert.Single(localAuthorities[0].Children);

                // Region attribute at depth 2 is empty as not defined for any of the LA data
                Assert.Equal(Region.Empty(), localAuthorities[0].Children[0].Attribute);
                Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

                Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

                Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);
            }
        }

        [Fact]
        public async Task GetLocationAttributesHierarchical_SomeLocationsHaveFewerAttributesThanHierarchy()
        {
            /*
             * Test a scenario where *some* Observations have Locations that don't have attributes present in the hierarchy.
             * E.g a Country-Region-LA hierarchy is defined but Derby appears with and without a Region.
             * 
             * geographic_level    country_code    country_name    region_code    region_name    la_code    la_name
             * ====================================================================================================
             * Local authority     E92000001       England         E12000004      East Midlands  E06000015  Derby
             * Local authority     E92000001       England                                       E06000015  Derby
             * Local authority     E92000001       England         E12000004      East Midlands  E06000018  Nottingham
             */

            var subject = new Subject();

            var observations = new List<Observation>
            {
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _derby
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        LocalAuthority = _derby
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _nottingham
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var hierarchies = new Dictionary<GeographicLevel, List<string>>
                {
                    {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
                };

                var repository = BuildRepository(statisticsDbContext);
                var result = await repository.GetLocationAttributesHierarchical(subject.Id, hierarchies);

                // Local Authority is the only level in the data
                Assert.Single(result);
                Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = result[GeographicLevel.LocalAuthority];
                Assert.Single(localAuthorities);

                Assert.Equal(_england, localAuthorities[0].Attribute);
                Assert.Equal(2, localAuthorities[0].Children.Count);

                // First Region attribute at depth 2 exists for East Midlands and contains children Derby and Nottingham
                Assert.Equal(_eastMidlands, localAuthorities[0].Children[0].Attribute);
                Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

                Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

                Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);

                // A second Region attribute at depth 2 also exists and is empty but contains child Derby
                Assert.Equal(Region.Empty(), localAuthorities[0].Children[1].Attribute);
                Assert.Single(localAuthorities[0].Children[1].Children);

                Assert.Equal(_derby, localAuthorities[0].Children[1].Children[0].Attribute);
                Assert.Empty(localAuthorities[0].Children[1].Children[0].Children);
            }
        }

        [Fact]
        public async Task GetLocationAttributesHierarchical_MultipleGeographicLevels_NotEveryLevelHasHierarchy()
        {
            /*
             * Test a scenario where the Subject data has multiple geographic levels and some but not all of the levels
             * have a hierarchy that can be applied.
             * E.g. Country, Region and LA data exists and a Country-Region-LA hierarchy is defined for the LA level.
             *
             * No hierarchy should be applied to the Countries from the Country level data.
             * No hierarchy should be applied to the Regions from the Regional level data.
             * The Country-Region-LA hierarchy should be applied to the LA level data.
             */

            var subject = new Subject();

            var observations = new List<Observation>
            {
                new()
                {
                    Location = new Location
                    {
                        Country = _england
                    },
                    GeographicLevel = GeographicLevel.Country,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _northEast
                    },
                    GeographicLevel = GeographicLevel.Region,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _northWest
                    },
                    GeographicLevel = GeographicLevel.Region,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands
                    },
                    GeographicLevel = GeographicLevel.Region,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _derby
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                },
                new()
                {
                    Location = new Location
                    {
                        Country = _england,
                        Region = _eastMidlands,
                        LocalAuthority = _nottingham
                    },
                    GeographicLevel = GeographicLevel.LocalAuthority,
                    Subject = subject
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.SaveChangesAsync();
            }

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
            };

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var repository = BuildRepository(statisticsDbContext);
                var result = await repository.GetLocationAttributesHierarchical(subject.Id, hierarchies);

                // Result has Country, Region and Local Authority levels
                Assert.Equal(3, result.Count);
                Assert.True(result.ContainsKey(GeographicLevel.Country));
                Assert.True(result.ContainsKey(GeographicLevel.Region));
                Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

                // Expect no hierarchy within the Country level
                var countries = result[GeographicLevel.Country];

                Assert.Single(countries);
                Assert.Equal(_england, countries[0].Attribute);
                Assert.Empty(countries[0].Children);

                // Expect no hierarchy within the Region level
                var regions = result[GeographicLevel.Region];

                Assert.Equal(3, regions.Count);
                Assert.Equal(_northEast, regions[0].Attribute);
                Assert.Empty(regions[0].Children);
                Assert.Equal(_northWest, regions[1].Attribute);
                Assert.Empty(regions[1].Children);
                Assert.Equal(_eastMidlands, regions[2].Attribute);
                Assert.Empty(regions[2].Children);

                // Expect a hierarchy of Country-Region-LA within the Local Authority level
                var localAuthorities = result[GeographicLevel.LocalAuthority];
                Assert.Single(localAuthorities);

                Assert.Equal(_england, localAuthorities[0].Attribute);
                Assert.Single(localAuthorities[0].Children);

                Assert.Equal(_eastMidlands, localAuthorities[0].Children[0].Attribute);
                Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

                Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

                Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
                Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);
            }
        }

        private static LocationRepository BuildRepository(
            StatisticsDbContext? statisticsDbContext = null)
        {
            return new(
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>());
        }
    }
}
