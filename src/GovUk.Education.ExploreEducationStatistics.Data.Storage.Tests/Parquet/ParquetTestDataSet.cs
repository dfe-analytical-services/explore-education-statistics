#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

/// <summary>
/// A small data set written to parquet files in a temporary directory, which is deleted on disposal.
/// </summary>
public sealed class ParquetTestDataSet : IDisposable
{
    public Guid SubjectId { get; } = Guid.NewGuid();

    public IParquetDataSetPathResolver PathResolver { get; }

    public Filter SchoolType { get; }
    public FilterItem Primary { get; }
    public FilterItem Secondary { get; }

    public Filter NcYear { get; }
    public FilterGroup KeyStage1 { get; }
    public FilterGroup KeyStage2 { get; }
    public FilterItem Year1 { get; }
    public FilterItem Year2 { get; }
    public FilterItem KeyStage1Total { get; }
    public FilterItem KeyStage2Total { get; }

    public IndicatorGroup DefaultIndicatorGroup { get; }
    public IndicatorGroup OtherIndicatorGroup { get; }
    public Indicator Enrolments { get; }
    public Indicator AuthorisedPercent { get; }
    public Indicator Notes { get; }

    public Location England { get; }
    public Location NorthEast { get; }
    public Location Newcastle { get; }
    public Location School { get; }

    public List<Filter> Filters => [SchoolType, NcYear];
    public List<IndicatorGroup> IndicatorGroups => [DefaultIndicatorGroup, OtherIndicatorGroup];
    public List<Indicator> Indicators => IndicatorGroups.SelectMany(group => group.Indicators).ToList();
    public List<Location> Locations => [England, NorthEast, Newcastle, School];

    public List<Observation> Observations { get; }

    public ParquetTestDataSet()
    {
        PathResolver = new TestParquetDataSetPathResolver(
            Path.Combine(Path.GetTempPath(), $"parquet-data-set-tests-{Guid.NewGuid()}")
        );

        SchoolType = CreateFilter("school_type", "School type", hint: "Type of school");
        var schoolTypeDefault = CreateFilterGroup(SchoolType, FilterGroup.NotSpecifiedFilterGroupLabel);
        Primary = CreateFilterItem(schoolTypeDefault, "State-funded primary");
        Secondary = CreateFilterItem(schoolTypeDefault, "State-funded secondary");
        SchoolType.AutoSelectFilterItemId = Primary.Id;
        SchoolType.AutoSelectFilterItemLabel = Primary.Label;

        NcYear = CreateFilter("ncyear", "National curriculum year", groupCsvColumn: "key_stage");
        KeyStage1 = CreateFilterGroup(NcYear, "Key stage 1");
        KeyStage2 = CreateFilterGroup(NcYear, "Key stage 2");
        Year1 = CreateFilterItem(KeyStage1, "Year 1");
        Year2 = CreateFilterItem(KeyStage1, "Year 2");
        // The same label in different groups is a different filter item.
        KeyStage1Total = CreateFilterItem(KeyStage1, "Total");
        KeyStage2Total = CreateFilterItem(KeyStage2, "Total");

        DefaultIndicatorGroup = CreateIndicatorGroup("Default");
        OtherIndicatorGroup = CreateIndicatorGroup("Other");
        Enrolments = CreateIndicator(DefaultIndicatorGroup, "enrolments", "Enrolments", IndicatorUnit.None, null);
        AuthorisedPercent = CreateIndicator(
            DefaultIndicatorGroup,
            "sess_authorised_percent",
            "Percentage of authorised sessions",
            IndicatorUnit.Percent,
            2
        );
        Notes = CreateIndicator(OtherIndicatorGroup, "notes", "Notes", IndicatorUnit.String, null);

        var englandAttribute = new Country("E92000001", "England");
        var northEastAttribute = new Region("E12000001", "North East");
        var newcastleAttribute = new LocalAuthority("E08000021", "391", "Newcastle upon Tyne");

        England = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = englandAttribute,
        };
        NorthEast = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Region,
            Country = englandAttribute,
            Region = northEastAttribute,
        };
        Newcastle = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = englandAttribute,
            Region = northEastAttribute,
            LocalAuthority = newcastleAttribute,
        };
        School = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.School,
            Country = englandAttribute,
            Region = northEastAttribute,
            LocalAuthority = newcastleAttribute,
            School = new School("100000", "Newcastle Primary School", "3912000"),
        };

        var csvRow = 2;

        Observations =
        [
            CreateObservation(csvRow++, England, 2021, AcademicYear, [Primary, Year1], ["100", "1.50", "note 1"]),
            CreateObservation(csvRow++, England, 2022, AcademicYear, [Primary, Year2], ["200", "2.50", null]),
            CreateObservation(csvRow++, England, 2022, AcademicYear, [Secondary, KeyStage2Total], ["300", "x", "c"]),
            CreateObservation(csvRow++, NorthEast, 2021, AcademicYear, [Secondary, KeyStage1Total], ["40", "4", ""]),
            CreateObservation(csvRow++, Newcastle, 2022, AcademicYear, [Primary, Year1], ["5", "5.5", "n"]),
            CreateObservation(csvRow++, School, 2022, CalendarYear, [Primary, Year1], ["6", "6", "n"]),
        ];
    }

    public ParquetDataSetWriter BuildWriter() => new(PathResolver, NullLogger<ParquetDataSetWriter>.Instance);

    public ParquetV1DataSet BuildDataSet() => new(SubjectId, PathResolver);

    /// <summary>
    /// Stages the observations in two batches and writes the parquet files.
    /// </summary>
    public async Task<ParquetV1DataSet> Write()
    {
        var writer = BuildWriter();

        await writer.StartStaging(SubjectId, Filters, Indicators, resumeFromCsvRow: null);
        await writer.AppendObservations(SubjectId, Filters, Indicators, Observations.Take(3));
        await writer.AppendObservations(SubjectId, Filters, Indicators, Observations.Skip(3));
        await writer.WriteParquetFiles(SubjectId, Filters, IndicatorGroups, Locations);

        return BuildDataSet();
    }

    public void Dispose()
    {
        if (Directory.Exists(PathResolver.BasePath()))
        {
            Directory.Delete(PathResolver.BasePath(), recursive: true);
        }
    }

    private Filter CreateFilter(string name, string label, string? hint = null, string? groupCsvColumn = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Label = label,
            Hint = hint,
            GroupCsvColumn = groupCsvColumn,
            SubjectId = SubjectId,
        };

    private static FilterGroup CreateFilterGroup(Filter filter, string label)
    {
        var group = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = label,
            Filter = filter,
            FilterId = filter.Id,
        };
        filter.FilterGroups.Add(group);
        return group;
    }

    private static FilterItem CreateFilterItem(FilterGroup group, string label)
    {
        var item = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = label,
            FilterGroup = group,
            FilterGroupId = group.Id,
        };
        group.FilterItems.Add(item);
        return item;
    }

    private IndicatorGroup CreateIndicatorGroup(string label) =>
        new()
        {
            Id = Guid.NewGuid(),
            Label = label,
            SubjectId = SubjectId,
        };

    private static Indicator CreateIndicator(
        IndicatorGroup group,
        string name,
        string label,
        IndicatorUnit unit,
        int? decimalPlaces
    )
    {
        var indicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Name = name,
            Label = label,
            Unit = unit,
            DecimalPlaces = decimalPlaces,
            IndicatorGroup = group,
            IndicatorGroupId = group.Id,
        };
        group.Indicators.Add(indicator);
        return indicator;
    }

    private Observation CreateObservation(
        int csvRow,
        Location location,
        int year,
        TimeIdentifier timeIdentifier,
        List<FilterItem> filterItems,
        List<string?> measures
    )
    {
        var observationId = Guid.NewGuid();

        return new Observation
        {
            Id = observationId,
            SubjectId = SubjectId,
            CsvRow = csvRow,
            Location = location,
            LocationId = location.Id,
            Year = year,
            TimeIdentifier = timeIdentifier,
            FilterItems = filterItems
                .Select(item => new ObservationFilterItem
                {
                    ObservationId = observationId,
                    FilterItemId = item.Id,
                    FilterId = item.FilterGroup.FilterId,
                })
                .ToList(),
            Measures = Indicators.Zip(measures).ToDictionary(pair => pair.First.Id, pair => pair.Second!),
        };
    }
}
