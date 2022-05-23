using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Newtonsoft.Json.Linq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.LocationViewModelBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class LocationViewModelBuilderTests
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
    public void BuildLocationAttributeViewModels_Flat()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northEast
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northWest
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _eastMidlands
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies: null);

        Assert.Equal(3, result.Count);

        var countries = Assert.Contains(GeographicLevel.Country, result);
        var regions = Assert.Contains(GeographicLevel.Region, result);
        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        var country1 = Assert.Single(countries);
        Assert.Equal(locations[0].Id, country1.Id);
        Assert.Equal(_england.Name, country1.Label);
        Assert.Equal(_england.Code, country1.Value);
        Assert.Null(country1.Level);
        Assert.Null(country1.Options);

        Assert.Equal(3, regions.Count);
        var region1 = regions[0];

        Assert.Equal(locations[1].Id, region1.Id);
        Assert.Equal(_northEast.Name, region1.Label);
        Assert.Equal(_northEast.Code, region1.Value);
        Assert.Null(region1.Level);
        Assert.Null(region1.Options);

        var region2 = regions[1];
        Assert.Equal(locations[2].Id, region2.Id);
        Assert.Equal(_northWest.Name, region2.Label);
        Assert.Equal(_northWest.Code, region2.Value);
        Assert.Null(region2.Level);
        Assert.Null(region2.Options);

        var region3 = regions[2];
        Assert.Equal(locations[3].Id, region3.Id);
        Assert.Equal(_eastMidlands.Name, region3.Label);
        Assert.Equal(_eastMidlands.Code, region3.Value);
        Assert.Null(region3.Level);
        Assert.Null(region3.Options);

        Assert.Equal(2, localAuthorities.Count);

        var la1 = localAuthorities[0];
        Assert.Equal(locations[4].Id, la1.Id);
        Assert.Equal(_derby.Name, la1.Label);
        Assert.Equal(_derby.Code, la1.Value);
        Assert.Null(la1.Level);
        Assert.Null(la1.Options);

        var la2 = localAuthorities[1];
        Assert.Equal(locations[5].Id, la2.Id);
        Assert.Equal(_nottingham.Name, la2.Label);
        Assert.Equal(_nottingham.Code, la2.Value);
        Assert.Null(la2.Level);
        Assert.Null(la2.Options);
    }

    [Fact]
    public void BuildLocationAttributeViewModels_Flat_GeoJsonProvided()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northEast
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northWest
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _eastMidlands
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _northEast,
                LocalAuthority = _sunderland
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            }
        };

        // Provide GeoJson for some but not all of the local authorities and not for any other level
        var geoJson = BuildGeoJson(_derby, _sunderland);

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies: null,
                geoJson);

        Assert.Equal(3, result.Count);

        var countries = Assert.Contains(GeographicLevel.Country, result);
        var regions = Assert.Contains(GeographicLevel.Region, result);
        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        // Except no GeoJson at country level 
        var country1 = Assert.Single(countries);
        Assert.Equal(locations[0].Id, country1.Id);
        Assert.Equal(_england.Name, country1.Label);
        Assert.Equal(_england.Code, country1.Value);
        Assert.Null(country1.GeoJson);

        // Except no GeoJson at region level 
        Assert.Equal(3, regions.Count);

        var region1 = regions[0];
        Assert.Equal(locations[1].Id, region1.Id);
        Assert.Equal(_northEast.Name, region1.Label);
        Assert.Equal(_northEast.Code, region1.Value);
        Assert.Null(region1.GeoJson);

        var region2 = regions[1];
        Assert.Equal(locations[2].Id, region2.Id);
        Assert.Equal(_northWest.Name, region2.Label);
        Assert.Equal(_northWest.Code, region2.Value);
        Assert.Null(region2.GeoJson);

        var region3 = regions[2];
        Assert.Equal(locations[3].Id, region3.Id);
        Assert.Equal(_eastMidlands.Name, region3.Label);
        Assert.Equal(_eastMidlands.Code, region3.Value);
        Assert.Null(region3.GeoJson);

        // Expect the local authorities to have the correct GeoJson if it was provided
        Assert.Equal(3, localAuthorities.Count);

        var la1 = localAuthorities[0];
        Assert.Equal(locations[5].Id, la1.Id);
        Assert.Equal(_derby.Name, la1.Label);
        Assert.Equal(_derby.Code, la1.Value);
        Assert.NotNull(la1.GeoJson);
        Assert.Equal(_derby.Code, la1.GeoJson["code"].ToString());

        var la2 = localAuthorities[1];
        Assert.Equal(locations[6].Id, la2.Id);
        Assert.Equal(_nottingham.Name, la2.Label);
        Assert.Equal(_nottingham.Code, la2.Value);
        // GeoJson is missing as it was not provided for this code
        Assert.Null(la2.GeoJson);

        var la3 = localAuthorities[2];
        Assert.Equal(locations[4].Id, la3.Id);
        Assert.Equal(_sunderland.Name, la3.Label);
        Assert.Equal(_sunderland.Code, la3.Value);
        Assert.NotNull(la3.GeoJson);
        Assert.Equal(_sunderland.Code, la3.GeoJson["code"].ToString());
    }

    [Fact]
    public void BuildLocationAttributeViewModels_Hierarchical()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northEast
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _northWest
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Country = _england,
                Region = _eastMidlands
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            }
        };

        var hierarchies = new Dictionary<GeographicLevel, List<string>>
        {
            {
                GeographicLevel.LocalAuthority,
                ListOf("Country", "Region")
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies);

        Assert.Equal(3, result.Count);

        var countries = Assert.Contains(GeographicLevel.Country, result);
        var regions = Assert.Contains(GeographicLevel.Region, result);
        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        // Expect no hierarchy within the Country level
        var country1 = Assert.Single(countries);
        Assert.Equal(locations[0].Id, country1.Id);
        Assert.Equal(_england.Name, country1.Label);
        Assert.Equal(_england.Code, country1.Value);
        Assert.Null(country1.Level);
        Assert.Null(country1.Options);

        // Expect no hierarchy within the Region level
        Assert.Equal(3, regions.Count);
        var region1 = regions[0];

        Assert.Equal(locations[1].Id, region1.Id);
        Assert.Equal(_northEast.Name, region1.Label);
        Assert.Equal(_northEast.Code, region1.Value);
        Assert.Null(region1.Level);
        Assert.Null(region1.Options);

        var region2 = regions[1];
        Assert.Equal(locations[2].Id, region2.Id);
        Assert.Equal(_northWest.Name, region2.Label);
        Assert.Equal(_northWest.Code, region2.Value);
        Assert.Null(region2.Level);
        Assert.Null(region2.Options);

        var region3 = regions[2];
        Assert.Equal(locations[3].Id, region3.Id);
        Assert.Equal(_eastMidlands.Name, region3.Label);
        Assert.Equal(_eastMidlands.Code, region3.Value);
        Assert.Null(region3.Level);
        Assert.Null(region3.Options);

        // Expect a hierarchy of Country-Region-LA within the Local Authority level
        var laOption1 = Assert.Single(localAuthorities);
        Assert.Null(laOption1.Id);
        Assert.Equal(_england.Name, laOption1.Label);
        Assert.Equal(_england.Code, laOption1.Value);
        Assert.Equal(GeographicLevel.Country, laOption1.Level);
        Assert.NotNull(laOption1.Options);

        var laOption1SubOption1 = Assert.Single(laOption1.Options);
        Assert.Null(laOption1SubOption1.Id);
        Assert.Equal(_eastMidlands.Name, laOption1SubOption1.Label);
        Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
        Assert.Equal(GeographicLevel.Region, laOption1SubOption1.Level);
        Assert.NotNull(laOption1SubOption1.Options);
        Assert.Equal(2, laOption1SubOption1.Options.Count);

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
    }

    [Fact]
    public void BuildLocationAttributeViewModels_Hierarchical_GeoJsonProvided()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _northEast,
                LocalAuthority = _sunderland
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            }
        };

        var hierarchies = new Dictionary<GeographicLevel, List<string>>
        {
            {
                GeographicLevel.LocalAuthority,
                ListOf("Region")
            }
        };

        // Provide GeoJson for some but not all of the local authorities and not for any other level
        var geoJson = BuildGeoJson(_sunderland, _derby);

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies,
                geoJson);

        Assert.Single(result);

        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        Assert.Equal(2, localAuthorities.Count);

        // Expect a hierarchy of Region-LA within the Local Authority level
        var laOption1 = localAuthorities[0];
        Assert.Null(laOption1.Id);
        Assert.Equal(_northEast.Name, laOption1.Label);
        Assert.Equal(_northEast.Code, laOption1.Value);
        Assert.Equal(GeographicLevel.Region, laOption1.Level);
        Assert.Null(laOption1.GeoJson);
        Assert.NotNull(laOption1.Options);

        var laOption1SubOption1 = Assert.Single(laOption1.Options);
        Assert.Equal(locations[0].Id, laOption1SubOption1.Id);
        Assert.Equal(_sunderland.Name, laOption1SubOption1.Label);
        Assert.Equal(_sunderland.Code, laOption1SubOption1.Value);
        Assert.NotNull(laOption1SubOption1.GeoJson);
        Assert.Equal(_sunderland.Code, laOption1SubOption1.GeoJson["code"].ToString());

        var laOption2 = localAuthorities[1];
        Assert.Null(laOption2.Id);
        Assert.Equal(_eastMidlands.Name, laOption2.Label);
        Assert.Equal(_eastMidlands.Code, laOption2.Value);
        Assert.Equal(GeographicLevel.Region, laOption2.Level);
        Assert.Null(laOption2.GeoJson);
        Assert.NotNull(laOption2.Options);
        Assert.Equal(2, laOption2.Options.Count);

        var laOption2SubOption1 = laOption2.Options[0];
        Assert.Equal(locations[1].Id, laOption2SubOption1.Id);
        Assert.Equal(_derby.Name, laOption2SubOption1.Label);
        Assert.Equal(_derby.Code, laOption2SubOption1.Value);
        Assert.NotNull(laOption2SubOption1.GeoJson);
        Assert.Equal(_derby.Code, laOption2SubOption1.GeoJson["code"].ToString());

        var laOption2SubOption2 = laOption2.Options[1];
        Assert.Equal(locations[2].Id, laOption2SubOption2.Id);
        Assert.Equal(_nottingham.Name, laOption2SubOption2.Label);
        Assert.Equal(_nottingham.Code, laOption2SubOption2.Value);
        // GeoJson is missing as it was not provided for this code
        Assert.Null(laOption2SubOption2.GeoJson);
    }

    [Fact]
    public void BuildLocationAttributeViewModels_LocalAuthorityOldCodeIsValue()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _cheshireOldCode
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies: null);

        Assert.Single(result);

        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        var la = Assert.Single(localAuthorities);

        // This LA does not have a new code, so check the old code is used as the fallback
        Assert.Equal(locations[0].Id, la.Id);
        Assert.Equal(_cheshireOldCode.Name, la.Label);
        Assert.Equal(_cheshireOldCode.OldCode, la.Value);
    }

    [Fact]
    public void BuildLocationAttributeViewModels_LocationRegionsOrderedByCode()
    {
        // Set up a mix of locations causing regions to be returned as both a flat list and in a hierarchy.
        // Regions have been ordered randomly, but we expect the returned view models to be ordered by
        // the region code.
        var locations = new List<Location>
        {
            // Flat Regions
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Region = _northWest
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Region = _eastMidlands
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Region,
                Region = _northEast
            },
            // Hierarchical Region-LA
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _northWest,
                LocalAuthority = _blackpool
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _northEast,
                LocalAuthority = _sunderland
            }
        };

        var hierarchies = new Dictionary<GeographicLevel, List<string>>
        {
            {
                GeographicLevel.LocalAuthority,
                ListOf("Region")
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies);

        Assert.Equal(2, result.Count);

        var regions = Assert.Contains(GeographicLevel.Region, result);
        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        // Expect no hierarchy within the Region level
        Assert.Equal(3, regions.Count);

        var region1 = regions[0];
        Assert.Equal(locations[2].Id, region1.Id);
        Assert.Equal(_northEast.Name, region1.Label);
        Assert.Equal(_northEast.Code, region1.Value);

        var region2 = regions[1];
        Assert.Equal(locations[0].Id, region2.Id);
        Assert.Equal(_northWest.Name, region2.Label);
        Assert.Equal(_northWest.Code, region2.Value);

        var region3 = regions[2];
        Assert.Equal(locations[1].Id, region3.Id);
        Assert.Equal(_eastMidlands.Name, region3.Label);
        Assert.Equal(_eastMidlands.Code, region3.Value);

        // Expect a hierarchy of Region-LA within the Local Authority level
        Assert.Equal(3, localAuthorities.Count);

        var laOption1 = localAuthorities[0];
        Assert.Null(laOption1.Id);
        Assert.Equal(_northEast.Name, laOption1.Label);
        Assert.Equal(_northEast.Code, laOption1.Value);
        Assert.Equal(GeographicLevel.Region, laOption1.Level);
        Assert.NotNull(laOption1.Options);

        var laOption1SubOption1 = Assert.Single(laOption1.Options);
        Assert.Equal(locations[5].Id, laOption1SubOption1.Id);
        Assert.Equal(_sunderland.Name, laOption1SubOption1.Label);
        Assert.Equal(_sunderland.Code, laOption1SubOption1.Value);

        var laOption2 = localAuthorities[1];
        Assert.Null(laOption2.Id);
        Assert.Equal(_northWest.Name, laOption2.Label);
        Assert.Equal(_northWest.Code, laOption2.Value);
        Assert.Equal(GeographicLevel.Region, laOption2.Level);
        Assert.NotNull(laOption2.Options);

        var laOption2SubOption1 = Assert.Single(laOption2.Options);
        Assert.Equal(locations[3].Id, laOption2SubOption1.Id);
        Assert.Equal(_blackpool.Name, laOption2SubOption1.Label);
        Assert.Equal(_blackpool.Code, laOption2SubOption1.Value);

        var laOption3 = localAuthorities[2];
        Assert.Null(laOption3.Id);
        Assert.Equal(_eastMidlands.Name, laOption3.Label);
        Assert.Equal(_eastMidlands.Code, laOption3.Value);
        Assert.Equal(GeographicLevel.Region, laOption3.Level);
        Assert.NotNull(laOption3.Options);

        var laOption3SubOption1 = Assert.Single(laOption3.Options);
        Assert.Equal(locations[4].Id, laOption3SubOption1.Id);
        Assert.Equal(_derby.Name, laOption3SubOption1.Label);
        Assert.Equal(_derby.Code, laOption3SubOption1.Value);
    }

    [Fact]
    public void BuildLocationAttributeViewModels_Flat_LocationsAreDeduplicated()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _derbyDupe
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _nottingham
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                null);

        Assert.Single(result);

        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        Assert.Equal(3, localAuthorities.Count);

        // There are two locations with a label of Derby, so we
        // de-duplicate these by appending the code (which is unique).
        var la1 = localAuthorities[0];
        Assert.Equal(locations[0].Id, la1.Id);
        Assert.Equal("Derby (E06000015)", la1.Label);
        Assert.Equal(_derby.Code, la1.Value);
        Assert.Null(la1.Level);
        Assert.Null(la1.Options);

        var la2 = localAuthorities[1];
        Assert.Equal(locations[1].Id, la2.Id);
        Assert.Equal("Derby (E06000016)", la2.Label);
        Assert.Equal(_derbyDupe.Code, la2.Value);
        Assert.Null(la2.Level);
        Assert.Null(la2.Options);

        var la3 = localAuthorities[2];
        Assert.Equal(locations[2].Id, la3.Id);
        Assert.Equal(_nottingham.Name, la3.Label);
        Assert.Equal(_nottingham.Code, la3.Value);
        Assert.Null(la3.Level);
        Assert.Null(la3.Options);
    }

    [Fact]
    public void BuildLocationAttributeViewModels_Hierarchical_LocationsAreDeduplicated()
    {
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _eastMidlands,
                LocalAuthority = _derbyDupe
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Region = _eastMidlands,
                LocalAuthority = _nottingham
            }
        };

        var hierarchies = new Dictionary<GeographicLevel, List<string>>
        {
            {
                GeographicLevel.LocalAuthority,
                ListOf("Region")
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies);

        Assert.Single(result);

        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        // Expect a hierarchy of Region-LA within the Local Authority level
        Assert.Single(localAuthorities);

        var laOption1 = localAuthorities[0];
        Assert.Null(laOption1.Id);
        Assert.Equal(_eastMidlands.Name, laOption1.Label);
        Assert.Equal(_eastMidlands.Code, laOption1.Value);
        Assert.Equal(GeographicLevel.Region, laOption1.Level);
        Assert.NotNull(laOption1.Options);
        Assert.Equal(3, laOption1.Options.Count);

        // There are two locations with a label of Derby, so we
        // de-duplicate these by appending the code (which is unique).
        var laOption1SubOption1 = laOption1.Options[0];
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

    [Fact]
    public void BuildLocationAttributeViewModels_DuplicateLocationsWithSameValueAreUntouched()
    {
        // Set up locations for the same level which have an identical principal attribute,
        // but differ by some other attribute.
        var locations = new List<Location>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                // Include region
                Region = _eastMidlands,
                LocalAuthority = _derby
            },
            new()
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                // Omit region
                Region = null,
                LocalAuthority = _derby
            }
        };

        var result =
            (IDictionary<GeographicLevel, List<LocationAttributeViewModel>>) BuildLocationAttributeViewModels(locations,
                hierarchies: null);

        Assert.Single(result);

        var localAuthorities = Assert.Contains(GeographicLevel.LocalAuthority, result);

        Assert.Equal(2, localAuthorities.Count);

        // There are two locations with a label of Derby but they also have the same code.
        // Appending the code won't deduplicate them so expect their labels to remain untouched.
        var la1 = localAuthorities[0];
        Assert.Equal(locations[0].Id, la1.Id);
        Assert.Equal(_derby.Name, la1.Label);
        Assert.Equal(_derby.Code, la1.Value);

        var la2 = localAuthorities[1];
        Assert.Equal(locations[1].Id, la2.Id);
        Assert.Equal(_derby.Name, la2.Label);
        Assert.Equal(_derby.Code, la2.Value);
    }

    private static Dictionary<GeographicLevel, Dictionary<string, GeoJson>> BuildGeoJson(
        params LocationAttribute[] locationAttributes)
    {
        return locationAttributes
            .GroupBy(locationAttribute => locationAttribute.GeographicLevel)
            .ToDictionary(grouping => grouping.Key,
                grouping => grouping.ToDictionary(locationAttribute => locationAttribute.Code,
                    locationAttribute => new GeoJson
                    {
                        // Set a minimal JSON string as the value which contains the location code.
                        // This should be enough to assert the correct GeoJson object has been selected
                        // when the the location view model was built.
                        Value = new JObject
                        {
                            {
                                "code", locationAttribute.Code
                            }
                        }.ToString()
                    }));
    }
}
