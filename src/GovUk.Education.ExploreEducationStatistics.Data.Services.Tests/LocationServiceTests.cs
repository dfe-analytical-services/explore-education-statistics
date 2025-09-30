#nullable enable
using GeoJSON.Net.Geometry;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class LocationServiceTests
{
    private readonly LocationService _sut;
    private readonly StatisticsDbContext _context;
    private readonly Mock<IBoundaryDataRepository> _boundaryDataRepository;

    public LocationServiceTests()
    {
        _context = InMemoryStatisticsDbContext();
        _boundaryDataRepository = new Mock<IBoundaryDataRepository>(Strict);
        _sut = new LocationService(_context, _boundaryDataRepository.Object);
    }

    private readonly Country _england = new("E92000001", "England");
    private readonly Region _northEast = new("E12000001", "North East");
    private readonly Region _northWest = new("E12000002", "North West");
    private readonly Region _eastMidlands = new("E12000004", "East Midlands");
    private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
    private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

    private static readonly BoundaryLevel _countriesBoundaryLevel = new()
    {
        Id = 1,
        Label = "Countries November 2021",
        Level = GeographicLevel.Country
    };

    private static readonly Dictionary<string, object> _properties = new()
    {
        { "OBJECTID", 1 },
        { "ctry17cd", "E92000001" },
        { "ctry17nm", "England" },
        { "ctry17nmw", "Lloegr" },
        { "bng_e", 394881 },
        { "bng_n", 370341 },
        { "long", -2 },
        { "lat", 50 },
        { "GlobalID", "1277674d-5d60-457d-9322-833b31f77014" },
    };

    private static readonly BoundaryData _boundaryData = new()
    {
        BoundaryLevel = _countriesBoundaryLevel,
        Code = "E92000001",
        Name = "England",
        GeoJson = new(new MultiPolygon([[[
                    [-71.17351189255714, 42.350224666504324],
            [-71.1677360907197, 42.34671571695422],
            [-71.16970919072628, 42.35326835618748],
            [-71.14341516047716, 42.36174674733808],
            [-71.17559093981981, 42.368232175909064],
            [-71.17351189255714, 42.350224666504324]]]]),
                    _properties, "1"
                ),
    };

    [Fact]
    public async Task GetLocationViewModels_NoBoundaryLevel_ReturnsViewModelsWithoutGeoJson()
    {
        // Arrange
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

        var locations = new List<Location>
        {
            location1, location2, location3, location4, location5, location6
        };

        var hierarchies = new Dictionary<GeographicLevel, List<string>>
        {
            {
                GeographicLevel.LocalAuthority,
                [
                    "Country",
                    "Region"
                ]
            }
        };

        _context.BoundaryLevel.Add(_countriesBoundaryLevel);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLocationViewModels(
            locations,
            hierarchies);

        // Assert
        // Result has Country, Region and Local Authority levels
        Assert.Equal(3, result.Count);
        Assert.True(result.ContainsKey("country"));
        Assert.True(result.ContainsKey("region"));
        Assert.True(result.ContainsKey("localAuthority"));

        // Expect no hierarchy within the Country level
        var countries = result["country"];

        var countryOption1 = Assert.Single(countries);
        Assert.Equal(location1.Id, countryOption1.Id);
        Assert.Equal(_england.Name, countryOption1.Label);
        Assert.Equal(_england.Code, countryOption1.Value);
        Assert.Null(countryOption1.Level);
        Assert.Null(countryOption1.Options);
        Assert.Null(countryOption1.GeoJson);

        // Expect no hierarchy within the Region level
        var regions = result["region"];
        Assert.Equal(3, regions.Count);

        var regionOption1 = regions[0];
        Assert.Equal(location2.Id, regionOption1.Id);
        Assert.Equal(_northEast.Name, regionOption1.Label);
        Assert.Equal(_northEast.Code, regionOption1.Value);
        Assert.Null(regionOption1.Level);
        Assert.Null(regionOption1.Options);
        Assert.Null(regionOption1.GeoJson);

        var regionOption2 = regions[1];
        Assert.Equal(location3.Id, regionOption2.Id);
        Assert.Equal(_northWest.Name, regionOption2.Label);
        Assert.Equal(_northWest.Code, regionOption2.Value);
        Assert.Null(regionOption2.Level);
        Assert.Null(regionOption2.Options);
        Assert.Null(regionOption2.GeoJson);

        var regionOption3 = regions[2];
        Assert.Equal(location4.Id, regionOption3.Id);
        Assert.Equal(_eastMidlands.Name, regionOption3.Label);
        Assert.Equal(_eastMidlands.Code, regionOption3.Value);
        Assert.Null(regionOption3.Level);
        Assert.Null(regionOption3.Options);
        Assert.Null(regionOption3.GeoJson);

        // Expect a hierarchy of Country-Region-LA within the Local Authority level
        var localAuthorities = result["localAuthority"];

        var laOption1 = Assert.Single(localAuthorities);
        Assert.NotNull(laOption1);
        Assert.Null(laOption1.Id);
        Assert.Equal(_england.Name, laOption1.Label);
        Assert.Equal(_england.Code, laOption1.Value);
        Assert.Equal(GeographicLevel.Country, laOption1.Level);
        Assert.NotNull(laOption1.Options);
        Assert.Null(laOption1.GeoJson);

        var laOption1SubOption1 = Assert.Single(laOption1.Options!);
        Assert.NotNull(laOption1SubOption1);
        Assert.Null(laOption1SubOption1.Id);
        Assert.Equal(_eastMidlands.Name, laOption1SubOption1.Label);
        Assert.Equal(_eastMidlands.Code, laOption1SubOption1.Value);
        Assert.Equal(GeographicLevel.Region, laOption1SubOption1.Level);
        Assert.NotNull(laOption1SubOption1.Options);
        Assert.Equal(2, laOption1SubOption1.Options!.Count);
        Assert.Null(laOption1SubOption1.GeoJson);

        var laOption1SubOption1SubOption1 = laOption1SubOption1.Options[0];
        Assert.Equal(location5.Id, laOption1SubOption1SubOption1.Id);
        Assert.Equal(_derby.Name, laOption1SubOption1SubOption1.Label);
        Assert.Equal(_derby.Code, laOption1SubOption1SubOption1.Value);
        Assert.Null(laOption1SubOption1SubOption1.Level);
        Assert.Null(laOption1SubOption1SubOption1.Options);
        Assert.Null(laOption1SubOption1SubOption1.GeoJson);

        var laOption1SubOption1SubOption2 = laOption1SubOption1.Options[1];
        Assert.Equal(location6.Id, laOption1SubOption1SubOption2.Id);
        Assert.Equal(_nottingham.Name, laOption1SubOption1SubOption2.Label);
        Assert.Equal(_nottingham.Code, laOption1SubOption1SubOption2.Value);
        Assert.Null(laOption1SubOption1SubOption2.Level);
        Assert.Null(laOption1SubOption1SubOption2.Options);
        Assert.Null(laOption1SubOption1SubOption2.GeoJson);
    }

    [Fact]
    public async Task GetLocationViewModels_WithBoundaryLevel_ReturnsViewModelsWithGeoJson()
    {

        var location1 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Region,
            Country = _england,
            Region = _northEast
        };

        var location2 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Region,
            Country = _england,
            Region = _northWest
        };

        var location3 = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Region,
            Country = _england,
            Region = _eastMidlands
        };

        var locations = new List<Location>
        {
            location1, location2, location3
        };

        var hierarchies = new Dictionary<GeographicLevel, List<string>>
        {
            {
                GeographicLevel.Region, ["Country"]
            }
        };

        _context.BoundaryLevel.Add(new() { Id = 123, Label = "Boundary Level 1", Level = GeographicLevel.Region });
        await _context.SaveChangesAsync();

        _boundaryDataRepository
            .Setup(s => s.FindByBoundaryLevelAndCodes(
                It.IsAny<long>(),
                new List<string> { _northEast.Code!, _northWest.Code!, _eastMidlands.Code! }))
            .Returns(new Dictionary<string, BoundaryData>
                {
                    {
                        _northEast.Code!,
                        _boundaryData
                    },
                    {
                        _northWest.Code!,
                        _boundaryData
                    },
                    {
                        _eastMidlands.Code!,
                        _boundaryData
                    }
                });

        // Act
        var result = await _sut.GetLocationViewModels(
            locations,
            hierarchies,
            boundaryLevelId: 123);

        // Assert
        VerifyAllMocks(_boundaryDataRepository);

        // Result only has a Region level
        Assert.Single(result);
        Assert.True(result.ContainsKey("region"));

        // Expect a hierarchy of Country-Region within the Region level
        var regions = result["region"];

        // Country option that groups the Regions does not have GeoJson
        var regionOption1 = Assert.Single(regions);
        Assert.Null(regionOption1.Id);
        Assert.Equal(_england.Name, regionOption1.Label);
        Assert.Equal(_england.Code, regionOption1.Value);
        Assert.Null(regionOption1.GeoJson);
        Assert.Equal(GeographicLevel.Country, regionOption1.Level);
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
