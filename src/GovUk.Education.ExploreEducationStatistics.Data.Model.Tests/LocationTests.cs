using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests
{
    public class LocationTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _northEast = new("E12000001", "North East");
        private readonly Region _northWest = new("E12000002", "North West");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        private readonly List<string> _countryRegionLaHierarchy = ListOf("Country", "Region");

        [Fact]
        public void GetAttributes()
        {
            var location = new Location
            {
                Id = Guid.NewGuid(),
                Country = _england,
                Region = _northWest,
                LocalAuthority = _nottingham,
                GeographicLevel = GeographicLevel.LocalAuthority
            };

            Assert.Equal(3, location.GetAttributes().Count());

            var attributes = location.GetAttributes().ToHashSet();
            Assert.Contains(_england, attributes);
            Assert.Contains(_northWest, attributes);
            Assert.Contains(_nottingham, attributes);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_NoLocations()
        {
            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                // Hierarchy will not be matched since Subject has no School level data
                {GeographicLevel.LocalAuthority, ListOf("Country", "Region")}
            };

            // Test a scenario where there are no Locations e.g. if there was no Subject data
            var result = new List<Location>().GetLocationAttributesHierarchical(hierarchies);
            Assert.Empty(result);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_HierarchyContainsInvalidAttributeName()
        {
            // Test a scenario where a hierarchy is defined but mentions an attribute not part of the Location type
            var locations = ListOf(new Location
            {
                Id = Guid.NewGuid(),
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham,
                GeographicLevel = GeographicLevel.LocalAuthority
            });

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                // Hierarchy will not be matched since Subject has no School level data
                {GeographicLevel.LocalAuthority, ListOf("Country", "Region", "NotAPropertyOfLocation")}
            };

            var exception = Assert.Throws<ArgumentException>(
                () => { locations.GetLocationAttributesHierarchical(hierarchies); }
            );

            Assert.Equal($"Location does not have a property NotAPropertyOfLocation", exception.Message);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_GeographicLevelsHaveNoRelevantHierarchy()
        {
            // Test a scenario where hierarchies are defined but none are relevant to the Subject data
            var locations = ListOf(new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    GeographicLevel = GeographicLevel.Country
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _northEast,
                    GeographicLevel = GeographicLevel.Region
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _northWest,
                    GeographicLevel = GeographicLevel.Region
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _derby,
                    GeographicLevel = GeographicLevel.LocalAuthority
                });

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                // School hierarchy that is insignificant since Subject has no School level data
                {GeographicLevel.School, ListOf("LocalAuthority", "School")}
            };

            var result = locations.GetLocationAttributesHierarchical(hierarchies);

            // Data has Country, Region and Local Authority levels
            Assert.Equal(3, result.Count);
            Assert.True(result.ContainsKey(GeographicLevel.Country));
            Assert.True(result.ContainsKey(GeographicLevel.Region));
            Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

            // Expect no hierarchy within any of the levels
            var countries = result[GeographicLevel.Country];

            Assert.Single(countries);
            Assert.Equal(_england, countries[0].Attribute);
            Assert.Equal(locations[0].Id, countries[0].LocationId);
            Assert.Empty(countries[0].Children);

            var regions = result[GeographicLevel.Region];

            Assert.Equal(2, regions.Count);
            Assert.Equal(_northEast, regions[0].Attribute);
            Assert.Equal(locations[1].Id, regions[0].LocationId);
            Assert.Empty(regions[0].Children);
            Assert.Equal(_northWest, regions[1].Attribute);
            Assert.Equal(locations[2].Id, regions[1].LocationId);
            Assert.Empty(regions[1].Children);

            var localAuthorities = result[GeographicLevel.LocalAuthority];
            Assert.Single(localAuthorities);
            Assert.Equal(_derby, localAuthorities[0].Attribute);
            Assert.Equal(locations[3].Id, localAuthorities[0].LocationId);
            Assert.Empty(localAuthorities[0].Children);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_GeographicLevelsHaveRelevantHierarchy()
        {
            // Test a scenario where a hierarchy is defined that can be applied to the Subject data
            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _derby,
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _nottingham,
                    GeographicLevel = GeographicLevel.LocalAuthority
                });

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
            };

            var result = locations.GetLocationAttributesHierarchical(hierarchies);

            // Local Authority is the only level in the data
            Assert.Single(result);
            Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

            // Expect a hierarchy of Country-Region-LA within the Local Authority level
            var localAuthorities = result[GeographicLevel.LocalAuthority];
            Assert.Single(localAuthorities);

            Assert.Equal(_england, localAuthorities[0].Attribute);
            Assert.Null(localAuthorities[0].LocationId);
            Assert.Single(localAuthorities[0].Children);

            Assert.Equal(_eastMidlands, localAuthorities[0].Children[0].Attribute);
            Assert.Null(localAuthorities[0].Children[0].LocationId);
            Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

            Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
            Assert.Equal(locations[0].Id, localAuthorities[0].Children[0].Children[0].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

            Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
            Assert.Equal(locations[1].Id, localAuthorities[0].Children[0].Children[1].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_AllLocationsHaveFewerAttributesThanHierarchy()
        {
            /*
             * Test a scenario where all Observations have Locations that don't have attributes present in the hierarchy.
             * E.g a Country-Region-LA hierarchy is defined for the LA level but no Locations have Country or Region:
             *
             * geographic_level    country_code    country_name    region_code    region_name    la_code    la_name
             * ====================================================================================================
             * Local authority                                                                   E06000015  Derby
             * Local authority                                                                   E06000018  Nottingham
             */

            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    LocalAuthority = _derby,
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    LocalAuthority = _nottingham,
                    GeographicLevel = GeographicLevel.LocalAuthority
                });

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
            };

            var result = locations.GetLocationAttributesHierarchical(hierarchies);

            // Local Authority is the only level in the data
            Assert.Single(result);
            Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

            // Expect a hierarchy of Country-Region-LA within the Local Authority level
            var localAuthorities = result[GeographicLevel.LocalAuthority];
            Assert.Single(localAuthorities);

            // Country attribute at depth 1 is empty as not defined for any of the LA data
            Assert.Equal(new Country(null, null), localAuthorities[0].Attribute);
            Assert.Null(localAuthorities[0].LocationId);
            Assert.Single(localAuthorities[0].Children);

            // Region attribute at depth 2 is empty as not defined for any of the LA data
            Assert.Equal(new Region( null, null), localAuthorities[0].Children[0].Attribute);
            Assert.Null(localAuthorities[0].Children[0].LocationId);
            Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

            Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
            Assert.Equal(locations[0].Id, localAuthorities[0].Children[0].Children[0].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

            Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
            Assert.Equal(locations[1].Id, localAuthorities[0].Children[0].Children[1].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_SomeLocationsHaveFewerAttributesThanHierarchy()
        {
            /*
             * Test a scenario where *some* Observations have Locations that don't have attributes present in the hierarchy.
             * E.g a Country-Region-LA hierarchy is defined for the LA level but Derby appears with and without a Region.
             *
             * geographic_level    country_code    country_name    region_code    region_name    la_code    la_name
             * ====================================================================================================
             * Local authority     E92000001       England         E12000004      East Midlands  E06000015  Derby
             * Local authority     E92000001       England                                       E06000015  Derby
             * Local authority     E92000001       England         E12000004      East Midlands  E06000018  Nottingham
             */

            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _derby,
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    LocalAuthority = _derby,
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _nottingham,
                    GeographicLevel = GeographicLevel.LocalAuthority
                });

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
            };

            var result = locations.GetLocationAttributesHierarchical(hierarchies);

            // Local Authority is the only level in the data
            Assert.Single(result);
            Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

            // Expect a hierarchy of Country-Region-LA within the Local Authority level
            var localAuthorities = result[GeographicLevel.LocalAuthority];
            Assert.Single(localAuthorities);

            Assert.Equal(_england, localAuthorities[0].Attribute);
            Assert.Null(localAuthorities[0].LocationId);
            Assert.Equal(2, localAuthorities[0].Children.Count);

            // First Region attribute at depth 2 exists for East Midlands and contains children Derby and Nottingham
            // The Location id for this Derby matches the Location *with* the Region
            Assert.Equal(_eastMidlands, localAuthorities[0].Children[0].Attribute);
            Assert.Null(localAuthorities[0].Children[0].LocationId);
            Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

            Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
            Assert.Equal(locations[0].Id, localAuthorities[0].Children[0].Children[0].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

            Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
            Assert.Equal(locations[2].Id, localAuthorities[0].Children[0].Children[1].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);

            // A second Region attribute at depth 2 also exists and is empty but contains child Derby
            // The Location id for this Derby here matches the Location *without* the Region
            Assert.Equal(new Region(null, null), localAuthorities[0].Children[1].Attribute);
            Assert.Null(localAuthorities[0].Children[1].LocationId);
            Assert.Single(localAuthorities[0].Children[1].Children);

            Assert.Equal(_derby, localAuthorities[0].Children[1].Children[0].Attribute);
            Assert.Equal(locations[1].Id, localAuthorities[0].Children[1].Children[0].LocationId);
            Assert.Empty(localAuthorities[0].Children[1].Children[0].Children);
        }

        [Fact]
        public void GetLocationAttributesHierarchical_MultipleGeographicLevels_NotEveryLevelHasHierarchy()
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

            var locations = ListOf(
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    GeographicLevel = GeographicLevel.Country
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _northEast,
                    GeographicLevel = GeographicLevel.Region
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _northWest,
                    GeographicLevel = GeographicLevel.Region
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    GeographicLevel = GeographicLevel.Region
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _derby,
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                new Location
                {
                    Id = Guid.NewGuid(),
                    Country = _england,
                    Region = _eastMidlands,
                    LocalAuthority = _nottingham,
                    GeographicLevel = GeographicLevel.LocalAuthority
                });

            var hierarchies = new Dictionary<GeographicLevel, List<string>>
            {
                {GeographicLevel.LocalAuthority, _countryRegionLaHierarchy}
            };

            var result = locations.GetLocationAttributesHierarchical(hierarchies);

            // Result has Country, Region and Local Authority levels
            Assert.Equal(3, result.Count);
            Assert.True(result.ContainsKey(GeographicLevel.Country));
            Assert.True(result.ContainsKey(GeographicLevel.Region));
            Assert.True(result.ContainsKey(GeographicLevel.LocalAuthority));

            // Expect no hierarchy within the Country level
            var countries = result[GeographicLevel.Country];

            Assert.Single(countries);
            Assert.Equal(_england, countries[0].Attribute);
            Assert.Equal(locations[0].Id, countries[0].LocationId);
            Assert.Empty(countries[0].Children);

            // Expect no hierarchy within the Region level
            var regions = result[GeographicLevel.Region];

            Assert.Equal(3, regions.Count);
            Assert.Equal(_northEast, regions[0].Attribute);
            Assert.Equal(locations[1].Id, regions[0].LocationId);
            Assert.Empty(regions[0].Children);
            Assert.Equal(_northWest, regions[1].Attribute);
            Assert.Equal(locations[2].Id, regions[1].LocationId);
            Assert.Empty(regions[1].Children);
            Assert.Equal(_eastMidlands, regions[2].Attribute);
            Assert.Equal(locations[3].Id, regions[2].LocationId);
            Assert.Empty(regions[2].Children);

            // Expect a hierarchy of Country-Region-LA within the Local Authority level
            var localAuthorities = result[GeographicLevel.LocalAuthority];
            Assert.Single(localAuthorities);

            Assert.Equal(_england, localAuthorities[0].Attribute);
            Assert.Null(localAuthorities[0].LocationId);
            Assert.Single(localAuthorities[0].Children);

            Assert.Equal(_eastMidlands, localAuthorities[0].Children[0].Attribute);
            Assert.Null(localAuthorities[0].Children[0].LocationId);
            Assert.Equal(2, localAuthorities[0].Children[0].Children.Count);

            Assert.Equal(_derby, localAuthorities[0].Children[0].Children[0].Attribute);
            Assert.Equal(locations[4].Id, localAuthorities[0].Children[0].Children[0].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[0].Children);

            Assert.Equal(_nottingham, localAuthorities[0].Children[0].Children[1].Attribute);
            Assert.Equal(locations[5].Id, localAuthorities[0].Children[0].Children[1].LocationId);
            Assert.Empty(localAuthorities[0].Children[0].Children[1].Children);
        }
    }
}
