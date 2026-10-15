#nullable enable
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

public abstract class ParquetDataSetWriterTests : IDisposable
{
    private readonly ParquetTestDataSet _testDataSet = new();

    private IParquetDataSetPathResolver PathResolver => _testDataSet.PathResolver;

    public void Dispose() => _testDataSet.Dispose();

    public class WriteParquetFilesTests : ParquetDataSetWriterTests
    {
        [Fact]
        public async Task WritesParquetFilesAndRemovesStagingDb()
        {
            await _testDataSet.Write();

            var subjectId = _testDataSet.SubjectId;

            Assert.True(File.Exists(PathResolver.DataPath(subjectId)));
            Assert.True(File.Exists(PathResolver.FilterOptionsPath(subjectId)));
            Assert.True(File.Exists(PathResolver.FiltersPath(subjectId)));
            Assert.True(File.Exists(PathResolver.IndicatorsPath(subjectId)));
            Assert.True(File.Exists(PathResolver.LocationOptionsPath(subjectId)));
            Assert.True(File.Exists(PathResolver.LocationsPath(subjectId)));
            Assert.True(File.Exists(PathResolver.TimePeriodsPath(subjectId)));

            Assert.False(File.Exists(PathResolver.StagingDbPath(subjectId)));
            Assert.Empty(Directory.GetFiles(PathResolver.DirectoryPath(subjectId), "*.tmp"));
        }

        [Fact]
        public async Task TimePeriods_OrderedAndFormattedAsPublicApi()
        {
            await _testDataSet.Write();

            var timePeriods = await Query<(int Id, string Period, string Identifier, int Year, string TimeIdentifier)>(
                $"""
                SELECT id, period, identifier, ees_year, ees_time_identifier
                FROM '{PathResolver.TimePeriodsPath(_testDataSet.SubjectId)}'
                ORDER BY id
                """
            );

            Assert.Equal(
                [
                    (1, "202122", "Academic year", 2021, "AY"),
                    (2, "2022", "Calendar year", 2022, "CY"),
                    (3, "202223", "Academic year", 2022, "AY"),
                ],
                timePeriods
            );
        }

        [Fact]
        public async Task FilterOptions_OnePerFilterItemOrderedByLabelWithinFilter()
        {
            await _testDataSet.Write();

            var filterOptions = await Query<FilterOptionRow>(
                $"""
                SELECT
                    id AS Id,
                    label AS Label,
                    public_id AS PublicId,
                    filter_id AS FilterId,
                    filter_column AS FilterColumn,
                    ees_filter_item_id AS FilterItemId,
                    ees_filter_group_id AS FilterGroupId,
                    ees_filter_group_label AS FilterGroupLabel,
                    ees_filter_id AS EesFilterId
                FROM '{PathResolver.FilterOptionsPath(_testDataSet.SubjectId)}'
                ORDER BY id
                """
            );

            var ncYearPublicId = SqidEncoder.Encode(1);
            var schoolTypePublicId = SqidEncoder.Encode(2);

            Assert.Equal(
                [
                    (1, "Total", SqidEncoder.Encode(1), ncYearPublicId, "ncyear", _testDataSet.KeyStage1Total.Id),
                    (2, "Total", SqidEncoder.Encode(2), ncYearPublicId, "ncyear", _testDataSet.KeyStage2Total.Id),
                    (3, "Year 1", SqidEncoder.Encode(3), ncYearPublicId, "ncyear", _testDataSet.Year1.Id),
                    (4, "Year 2", SqidEncoder.Encode(4), ncYearPublicId, "ncyear", _testDataSet.Year2.Id),
                    (
                        5,
                        "State-funded primary",
                        SqidEncoder.Encode(5),
                        schoolTypePublicId,
                        "school_type",
                        _testDataSet.Primary.Id
                    ),
                    (
                        6,
                        "State-funded secondary",
                        SqidEncoder.Encode(6),
                        schoolTypePublicId,
                        "school_type",
                        _testDataSet.Secondary.Id
                    ),
                ],
                filterOptions.Select(o => (o.Id, o.Label, o.PublicId, o.FilterId, o.FilterColumn, o.FilterItemId))
            );

            var keyStage2Total = filterOptions[1];
            Assert.Equal(_testDataSet.KeyStage2.Id, keyStage2Total.FilterGroupId);
            Assert.Equal("Key stage 2", keyStage2Total.FilterGroupLabel);
            Assert.Equal(_testDataSet.NcYear.Id, keyStage2Total.EesFilterId);

            var filters = await Query<(Guid Id, string PublicId, string Column)>(
                $"""
                SELECT id, public_id, "column"
                FROM '{PathResolver.FiltersPath(_testDataSet.SubjectId)}'
                ORDER BY public_id
                """
            );

            Assert.Contains((_testDataSet.NcYear.Id, ncYearPublicId, "ncyear"), filters);
            Assert.Contains((_testDataSet.SchoolType.Id, schoolTypePublicId, "school_type"), filters);
        }

        [Fact]
        public async Task Indicators_WrittenWithPublicApiColumns()
        {
            await _testDataSet.Write();

            var indicators = await Query<(
                string Id,
                string Column,
                string Label,
                string Unit,
                int? DecimalPlaces,
                Guid IndicatorId,
                Guid IndicatorGroupId
            )>(
                $"""
                SELECT
                    id, "column", label, unit, CAST(decimal_places AS INTEGER),
                    ees_indicator_id, ees_indicator_group_id
                FROM '{PathResolver.IndicatorsPath(_testDataSet.SubjectId)}'
                ORDER BY "column"
                """
            );

            Assert.Equal(
                [
                    (
                        SqidEncoder.Encode(1),
                        "enrolments",
                        "Enrolments",
                        "",
                        null,
                        _testDataSet.Enrolments.Id,
                        _testDataSet.DefaultIndicatorGroup.Id
                    ),
                    (
                        SqidEncoder.Encode(2),
                        "notes",
                        "Notes",
                        "string",
                        null,
                        _testDataSet.Notes.Id,
                        _testDataSet.OtherIndicatorGroup.Id
                    ),
                    (
                        SqidEncoder.Encode(3),
                        "sess_authorised_percent",
                        "Percentage of authorised sessions",
                        "%",
                        2,
                        _testDataSet.AuthorisedPercent.Id,
                        _testDataSet.DefaultIndicatorGroup.Id
                    ),
                ],
                indicators
            );
        }

        [Fact]
        public async Task LocationOptions_OnePerLocationAtEachLevel()
        {
            await _testDataSet.Write();

            var locationOptions = (
                await Query<LocationOptionRow>(
                    $"""
                    SELECT
                        id AS Id,
                        label AS Label,
                        level AS Level,
                        public_id AS PublicId,
                        code AS Code,
                        old_code AS OldCode,
                        urn AS Urn,
                        laestab AS LaEstab,
                        ukprn AS Ukprn
                    FROM '{PathResolver.LocationOptionsPath(_testDataSet.SubjectId)}'
                    ORDER BY id
                    """
                )
            ).Select(o => (o.Id, o.Label, o.Level, o.PublicId, o.Code, o.OldCode, o.Urn, o.LaEstab, o.Ukprn));

            Assert.Equal(
                [
                    (1, "Newcastle upon Tyne", "LA", SqidEncoder.Encode(1), "E08000021", "391", null, null, null),
                    (2, "England", "NAT", SqidEncoder.Encode(2), "E92000001", null, null, null, null),
                    (3, "North East", "REG", SqidEncoder.Encode(3), "E12000001", null, null, null, null),
                    (
                        4,
                        "Newcastle Primary School",
                        "SCH",
                        SqidEncoder.Encode(4),
                        null,
                        null,
                        "100000",
                        "3912000",
                        null
                    ),
                ],
                locationOptions
            );
        }

        [Fact]
        public async Task Data_OrderedAsPublicApiWithOptionIdsAndEesColumns()
        {
            await _testDataSet.Write();

            var rows = await Query<DataRow>(
                $"""
                SELECT
                    CAST(id AS BIGINT) AS Id,
                    time_period_id AS TimePeriodId,
                    geographic_level AS GeographicLevel,
                    locations_la_id AS La,
                    locations_nat_id AS Nat,
                    locations_reg_id AS Reg,
                    locations_sch_id AS Sch,
                    ncyear AS NcYear,
                    school_type AS SchoolType,
                    enrolments AS Enrolments,
                    notes AS Notes,
                    sess_authorised_percent AS Percent,
                    ees_observation_id AS ObservationId,
                    ees_location_id AS LocationId,
                    ees_csv_row AS CsvRow
                FROM '{PathResolver.DataPath(_testDataSet.SubjectId)}'
                ORDER BY id
                """
            );

            Assert.Equal(
                [
                    (1L, 3, "Local authority", (1, 2, 3, 0), (3, 5)),
                    (2L, 3, "National", (0, 2, 0, 0), (4, 5)),
                    (3L, 3, "National", (0, 2, 0, 0), (2, 6)),
                    (4L, 1, "National", (0, 2, 0, 0), (3, 5)),
                    (5L, 1, "Regional", (0, 2, 3, 0), (1, 6)),
                    (6L, 2, "School", (1, 2, 3, 4), (3, 5)),
                ],
                rows.Select(r =>
                    (r.Id, r.TimePeriodId, r.GeographicLevel, (r.La, r.Nat, r.Reg, r.Sch), (r.NcYear, r.SchoolType))
                )
            );

            Assert.Equal(
                [
                    ("5", "n", "5.5", Observation(4).Id, _testDataSet.Newcastle.Id, 6L),
                    ("200", null, "2.50", Observation(1).Id, _testDataSet.England.Id, 3L),
                    ("300", "c", "x", Observation(2).Id, _testDataSet.England.Id, 4L),
                    ("100", "note 1", "1.50", Observation(0).Id, _testDataSet.England.Id, 2L),
                    ("40", "", "4", Observation(3).Id, _testDataSet.NorthEast.Id, 5L),
                    ("6", "n", "6", Observation(5).Id, _testDataSet.School.Id, 7L),
                ],
                rows.Select(r => (r.Enrolments, r.Notes, r.Percent, r.ObservationId, r.LocationId, r.CsvRow))
            );
        }

        [Fact]
        public async Task ObservationWithoutFilterItem_WrittenWithZeroOptionId()
        {
            var observation = _testDataSet.Observations[0];
            observation.FilterItems.RemoveAll(item => item.FilterId == _testDataSet.NcYear.Id);

            await _testDataSet.Write();

            var ncYearOptionId = await QuerySingle<int>(
                $"""
                SELECT ncyear FROM '{PathResolver.DataPath(_testDataSet.SubjectId)}'
                WHERE ees_observation_id = '{observation.Id}'
                """
            );

            Assert.Equal(0, ncYearOptionId);
        }

        [Fact]
        public async Task FilterColumnIsReserved_Throws()
        {
            _testDataSet.NcYear.Name = "time_period_id";

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _testDataSet.Write());
            Assert.Contains("time_period_id", exception.Message);
        }

        [Fact]
        public async Task StagedObservationLocationNotProvided_Throws()
        {
            var writer = _testDataSet.BuildWriter();
            var subjectId = _testDataSet.SubjectId;

            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);
            await writer.AppendObservations(
                subjectId,
                _testDataSet.Filters,
                _testDataSet.Indicators,
                _testDataSet.Observations
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                writer.WriteParquetFiles(
                    subjectId,
                    _testDataSet.Filters,
                    _testDataSet.IndicatorGroups,
                    [_testDataSet.England]
                )
            );

            Assert.False(File.Exists(PathResolver.DataPath(subjectId)));
        }

        private Observation Observation(int index) => _testDataSet.Observations[index];
    }

    public class StagingTests : ParquetDataSetWriterTests
    {
        [Fact]
        public async Task CountAndListLocationIds_ReturnStagedObservations()
        {
            var writer = _testDataSet.BuildWriter();
            var subjectId = _testDataSet.SubjectId;

            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);
            await writer.AppendObservations(
                subjectId,
                _testDataSet.Filters,
                _testDataSet.Indicators,
                _testDataSet.Observations
            );

            Assert.Equal(6, await writer.CountStagedObservations(subjectId));
            Assert.Equal(
                _testDataSet.Locations.Select(location => location.Id).Order(),
                (await writer.ListStagedLocationIds(subjectId)).Order()
            );
        }

        [Fact]
        public async Task Resume_RemovesObservationsStagedFromCsvRow()
        {
            var writer = _testDataSet.BuildWriter();
            var subjectId = _testDataSet.SubjectId;

            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);
            await writer.AppendObservations(
                subjectId,
                _testDataSet.Filters,
                _testDataSet.Indicators,
                _testDataSet.Observations
            );

            // Observations are staged from CSV rows 2 to 7.
            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: 5);

            Assert.Equal(3, await writer.CountStagedObservations(subjectId));
        }

        [Fact]
        public async Task StartWithoutResume_DiscardsExistingStagedObservations()
        {
            var writer = _testDataSet.BuildWriter();
            var subjectId = _testDataSet.SubjectId;

            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);
            await writer.AppendObservations(
                subjectId,
                _testDataSet.Filters,
                _testDataSet.Indicators,
                _testDataSet.Observations
            );

            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);

            Assert.Equal(0, await writer.CountStagedObservations(subjectId));
        }
    }

    private class FilterOptionRow
    {
        public int Id { get; init; }
        public string Label { get; init; } = "";
        public string PublicId { get; init; } = "";
        public string FilterId { get; init; } = "";
        public string FilterColumn { get; init; } = "";
        public Guid FilterItemId { get; init; }
        public Guid FilterGroupId { get; init; }
        public string FilterGroupLabel { get; init; } = "";
        public Guid EesFilterId { get; init; }
    }

    private class LocationOptionRow
    {
        public int Id { get; init; }
        public string Label { get; init; } = "";
        public string Level { get; init; } = "";
        public string PublicId { get; init; } = "";
        public string? Code { get; init; }
        public string? OldCode { get; init; }
        public string? Urn { get; init; }
        public string? LaEstab { get; init; }
        public string? Ukprn { get; init; }
    }

    private class DataRow
    {
        public long Id { get; init; }
        public int TimePeriodId { get; init; }
        public string GeographicLevel { get; init; } = "";
        public int La { get; init; }
        public int Nat { get; init; }
        public int Reg { get; init; }
        public int Sch { get; init; }
        public int NcYear { get; init; }
        public int SchoolType { get; init; }
        public string Enrolments { get; init; } = "";
        public string? Notes { get; init; }
        public string Percent { get; init; } = "";
        public Guid ObservationId { get; init; }
        public Guid LocationId { get; init; }
        public long CsvRow { get; init; }
    }

    private static async Task<List<T>> Query<T>(string sql)
    {
        await using var connection = new DuckDbConnection();
        connection.Open();
        return (await connection.QueryAsync<T>(sql)).AsList();
    }

    private static async Task<T> QuerySingle<T>(string sql)
    {
        await using var connection = new DuckDbConnection();
        connection.Open();
        return await connection.QuerySingleAsync<T>(sql);
    }
}
