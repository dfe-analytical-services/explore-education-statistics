#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class ParquetV1QueryServiceTests : IAsyncLifetime
{
    // Mirrors what the importer reads from a data CSV: labels are matched after trimming and ignoring case,
    // blank filter values become the "Not specified" filter item and blank indicator values are kept as empty.
    private const string Csv = """
        time_period,time_identifier,geographic_level,country_code,country_name,region_code,region_name,school_type,sex_group,sex,enrolments,attendance_rate
        2019,Academic year,National,E92000001,England,,,Primary,Default,Male,100,95.5
        2019,Academic year,National,E92000001,England,,,Primary,Default,Female,110,
        202021,Academic year,National,E92000001,England,,,Secondary,Default,Male,120,90
        2020,Academic year,Regional,E92000001,England,E12000001,North East,Primary, Default ,  MALE  ,30,80
        2021,Academic year,Regional,E92000001,England,E12000001,North East,Primary,Default,,40,70
        """;

    private readonly string _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();
    private readonly Guid _subjectId = Guid.NewGuid();

    private readonly File _dataFile = new()
    {
        Id = Guid.NewGuid(),
        RootPath = Guid.NewGuid(),
        Type = FileType.Data,
    };

    private readonly Location _england = new()
    {
        Id = Guid.NewGuid(),
        GeographicLevel = GeographicLevel.Country,
        Country = new Country("E92000001", "England"),
    };

    private readonly Location _northEast = new()
    {
        Id = Guid.NewGuid(),
        GeographicLevel = GeographicLevel.Region,
        Country = new Country("E92000001", "England"),
        Region = new Region("E12000001", "North East"),
    };

    private readonly Location _london = new()
    {
        Id = Guid.NewGuid(),
        GeographicLevel = GeographicLevel.Region,
        Country = new Country("E92000001", "England"),
        Region = new Region("E12000007", "London"),
    };

    private readonly Filter _schoolType;
    private readonly Filter _sex;
    private readonly FilterItem _primary;
    private readonly FilterItem _secondary;
    private readonly FilterItem _male;
    private readonly FilterItem _female;
    private readonly FilterItem _notSpecified;
    private readonly Indicator _enrolments;
    private readonly Indicator _attendanceRate;

    public ParquetV1QueryServiceTests()
    {
        _schoolType = new Filter(
            hint: null,
            label: "School type",
            name: "school_type",
            groupCsvColumn: null,
            parentFilter: null,
            autoSelectFilterItemLabel: null,
            subjectId: _subjectId
        );
        var schoolTypeGroup = new FilterGroup(_schoolType.Id, FilterGroup.NotSpecifiedFilterGroupLabel);
        _primary = new FilterItem("Primary", schoolTypeGroup);
        _secondary = new FilterItem("Secondary", schoolTypeGroup);
        schoolTypeGroup.FilterItems = [_primary, _secondary];
        _schoolType.FilterGroups = [schoolTypeGroup];

        _sex = new Filter(
            hint: null,
            label: "Sex",
            name: "sex",
            groupCsvColumn: "sex_group",
            parentFilter: null,
            autoSelectFilterItemLabel: null,
            subjectId: _subjectId
        );
        var sexGroup = new FilterGroup(_sex.Id, "Default");
        _male = new FilterItem("Male", sexGroup);
        _female = new FilterItem("Female", sexGroup);
        _notSpecified = new FilterItem(FilterItem.NotSpecifiedFilterItemLabel, sexGroup);
        sexGroup.FilterItems = [_male, _female, _notSpecified];
        _sex.FilterGroups = [sexGroup];

        _enrolments = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Enrolments",
            Name = "enrolments",
        };
        _attendanceRate = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Attendance rate",
            Name = "attendance_rate",
        };
    }

    private string ParquetPath => Path.Combine(_directory, "data.parquet");

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);

        var csvPath = Path.Combine(_directory, "data.csv");
        await System.IO.File.WriteAllTextAsync(csvPath, Csv);

        await using (var duckDbConnection = new DuckDbConnection())
        {
            await duckDbConnection.OpenAsync();
            await duckDbConnection.ExecuteNonQueryAsync(
                $"""
                COPY (SELECT * FROM read_csv('{csvPath}', ALL_VARCHAR = true, HEADER = true))
                TO '{ParquetPath}' (FORMAT PARQUET)
                """
            );
        }

        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        statisticsDbContext.Location.AddRange(_england, _northEast, _london);
        statisticsDbContext.Filter.AddRange(_schoolType, _sex);
        statisticsDbContext.IndicatorGroup.Add(
            new IndicatorGroup
            {
                Id = Guid.NewGuid(),
                SubjectId = _subjectId,
                Label = "Default",
                Indicators = [_enrolments, _attendanceRate],
            }
        );
        await statisticsDbContext.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        Directory.Delete(_directory, recursive: true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListTimePeriods()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        var service = BuildService(statisticsDbContext);

        Assert.Equal(
            [(2019, AcademicYear), (2020, AcademicYear)],
            await service.ListTimePeriods(_dataFile, [_england.Id])
        );

        Assert.Equal(
            [(2020, AcademicYear), (2021, AcademicYear)],
            await service.ListTimePeriods(_dataFile, [_northEast.Id, _london.Id])
        );

        Assert.Equal(
            [(2019, AcademicYear), (2020, AcademicYear), (2021, AcademicYear)],
            await service.ListTimePeriods(_dataFile, [_england.Id, _northEast.Id])
        );

        Assert.Empty(await service.ListTimePeriods(_dataFile, [_london.Id]));
        Assert.Empty(await service.ListTimePeriods(_dataFile, [Guid.NewGuid()]));
    }

    [Fact]
    public async Task ListFilterItems()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        var service = BuildService(statisticsDbContext);

        var result = await service.ListFilterItems(
            _dataFile,
            new FullTableQuery
            {
                SubjectId = _subjectId,
                LocationIds = [_england.Id],
                TimePeriod = new TimePeriodQuery(2019, AcademicYear, 2019, AcademicYear),
            }
        );

        AssertFilterItems([_primary, _male, _female], result);
        Assert.All(result, filterItem => Assert.NotNull(filterItem.FilterGroup.Filter));

        result = await service.ListFilterItems(
            _dataFile,
            new FullTableQuery
            {
                SubjectId = _subjectId,
                LocationIds = [_northEast.Id, _london.Id],
                TimePeriod = new TimePeriodQuery(2020, AcademicYear, 2021, AcademicYear),
            }
        );

        AssertFilterItems([_primary, _male, _notSpecified], result);

        result = await service.ListFilterItems(
            _dataFile,
            new FullTableQuery { SubjectId = _subjectId, LocationIds = [_england.Id] }
        );

        AssertFilterItems([_primary, _secondary, _male, _female], result);

        Assert.Empty(
            await service.ListFilterItems(
                _dataFile,
                new FullTableQuery { SubjectId = _subjectId, LocationIds = [_london.Id] }
            )
        );
    }

    [Fact]
    public async Task ListObservations()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        var service = BuildService(statisticsDbContext);

        var result = await service.ListObservations(
            _dataFile,
            new FullTableQuery
            {
                SubjectId = _subjectId,
                LocationIds = [_england.Id, _northEast.Id],
                TimePeriod = new TimePeriodQuery(2020, AcademicYear, 2021, AcademicYear),
                Filters = [_primary.Id, _male.Id],
                Indicators = [_enrolments.Id],
            }
        );

        var observation = Assert.Single(result);

        Assert.Equal(_subjectId, observation.SubjectId);
        Assert.Equal(_northEast.Id, observation.LocationId);
        Assert.Equal(_northEast.Id, observation.Location.Id);
        Assert.Equal(GeographicLevel.Region, observation.Location.GeographicLevel);
        Assert.Equal(2020, observation.Year);
        Assert.Equal(AcademicYear, observation.TimeIdentifier);
        Assert.Equal(new Dictionary<Guid, string> { { _enrolments.Id, "30" } }, observation.Measures);
        AssertFilterItemIds([_primary, _male], observation);
    }

    [Fact]
    public async Task ListObservations_NoFilters()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        var service = BuildService(statisticsDbContext);

        var result = await service.ListObservations(
            _dataFile,
            new FullTableQuery
            {
                SubjectId = _subjectId,
                LocationIds = [_england.Id],
                TimePeriod = new TimePeriodQuery(2019, AcademicYear, 2019, AcademicYear),
                Indicators = [_enrolments.Id, _attendanceRate.Id],
            }
        );

        var observations = result.OrderBy(observation => observation.Measures[_enrolments.Id]).ToList();

        Assert.Equal(2, observations.Count);

        Assert.Equal(_england.Id, observations[0].LocationId);
        Assert.Equal(2019, observations[0].Year);
        Assert.Equal(
            new Dictionary<Guid, string> { { _enrolments.Id, "100" }, { _attendanceRate.Id, "95.5" } },
            observations[0].Measures
        );
        AssertFilterItemIds([_primary, _male], observations[0]);

        Assert.Equal(_england.Id, observations[1].LocationId);
        Assert.Equal(2019, observations[1].Year);
        Assert.Equal(
            new Dictionary<Guid, string> { { _enrolments.Id, "110" }, { _attendanceRate.Id, "" } },
            observations[1].Measures
        );
        AssertFilterItemIds([_primary, _female], observations[1]);
    }

    [Fact]
    public async Task ListObservations_NotSpecifiedFilterItem()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        var service = BuildService(statisticsDbContext);

        var result = await service.ListObservations(
            _dataFile,
            new FullTableQuery
            {
                SubjectId = _subjectId,
                LocationIds = [_northEast.Id],
                Filters = [_notSpecified.Id],
                Indicators = [_enrolments.Id],
            }
        );

        var observation = Assert.Single(result);

        Assert.Equal(_northEast.Id, observation.LocationId);
        Assert.Equal(2021, observation.Year);
        Assert.Equal(new Dictionary<Guid, string> { { _enrolments.Id, "40" } }, observation.Measures);
        AssertFilterItemIds([_primary, _notSpecified], observation);
    }

    [Fact]
    public async Task ListObservations_NoMatchingLocations()
    {
        await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);
        var service = BuildService(statisticsDbContext);

        Assert.Empty(
            await service.ListObservations(
                _dataFile,
                new FullTableQuery
                {
                    SubjectId = _subjectId,
                    LocationIds = [_london.Id],
                    Indicators = [_enrolments.Id],
                }
            )
        );

        Assert.Empty(
            await service.ListObservations(
                _dataFile,
                new FullTableQuery
                {
                    SubjectId = _subjectId,
                    LocationIds = [Guid.NewGuid()],
                    Indicators = [_enrolments.Id],
                }
            )
        );
    }

    private ParquetV1QueryService BuildService(StatisticsDbContext statisticsDbContext)
    {
        var dataFilesPathResolver = new Mock<IDataFilesPathResolver>(Strict);
        dataFilesPathResolver.Setup(s => s.ParquetV1Path(_dataFile)).Returns(ParquetPath);

        return new ParquetV1QueryService(
            statisticsDbContext,
            new FilterRepository(statisticsDbContext),
            new IndicatorRepository(statisticsDbContext),
            dataFilesPathResolver.Object,
            Mock.Of<ILogger<ParquetV1QueryService>>()
        );
    }

    private static void AssertFilterItems(IEnumerable<FilterItem> expected, IEnumerable<FilterItem> actual)
    {
        Assert.Equal(expected.Select(fi => fi.Id).Order(), actual.Select(fi => fi.Id).Order());
    }

    private static void AssertFilterItemIds(IEnumerable<FilterItem> expected, Observation observation)
    {
        Assert.Equal(
            expected.Select(fi => fi.Id).Order(),
            observation.FilterItems.Select(ofi => ofi.FilterItemId).Order()
        );
        Assert.All(observation.FilterItems, ofi => Assert.Equal(observation.Id, ofi.ObservationId));
    }
}
