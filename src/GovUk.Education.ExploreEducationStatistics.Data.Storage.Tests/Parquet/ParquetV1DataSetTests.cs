#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

public abstract class ParquetV1DataSetTests : IDisposable
{
    private readonly ParquetTestDataSet _testDataSet = new();

    public void Dispose() => _testDataSet.Dispose();

    public class ListObservationsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task NoCriteria_ReturnsAllObservationsWithLocationFilterItemsAndMeasures()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations([], [], null);

            Assert.Equal(6, result.Count);

            foreach (var expected in _testDataSet.Observations)
            {
                var observation = Assert.Single(result, o => o.Id == expected.Id);

                Assert.Equal(_testDataSet.SubjectId, observation.SubjectId);
                Assert.Equal(expected.LocationId, observation.LocationId);
                Assert.Equal(expected.Location, observation.Location);
                Assert.Equal(expected.Year, observation.Year);
                Assert.Equal(expected.TimeIdentifier, observation.TimeIdentifier);
                Assert.Equal(expected.CsvRow, observation.CsvRow);
                Assert.Equal(expected.Measures, observation.Measures);
                Assert.Equal(
                    expected.FilterItems.Select(item => (item.ObservationId, item.FilterItemId, item.FilterId)).Order(),
                    observation
                        .FilterItems.Select(item => (item.ObservationId, item.FilterItemId, item.FilterId))
                        .Order()
                );
            }
        }

        [Fact]
        public async Task FilterItemsOfSameFilter_MatchAnyItem()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations([_testDataSet.Year1.Id, _testDataSet.Year2.Id], [], null);

            AssertObservations([0, 1, 4, 5], result);
        }

        [Fact]
        public async Task FilterItemsOfDifferentFilters_MatchEveryFilter()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations(
                [_testDataSet.Secondary.Id, _testDataSet.KeyStage1Total.Id],
                [],
                null
            );

            AssertObservations([3], result);
        }

        [Fact]
        public async Task EveryItemOfFilterSelected_FilterIgnored()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations(
                [_testDataSet.Primary.Id, _testDataSet.Secondary.Id, _testDataSet.Year1.Id],
                [],
                null
            );

            AssertObservations([0, 4, 5], result);
        }

        [Fact]
        public async Task FilterItemsOfOtherDataSet_Ignored()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations([Guid.NewGuid()], [], null);

            Assert.Equal(6, result.Count);
        }

        [Fact]
        public async Task Locations_MatchAnyLocation()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations(
                [],
                [_testDataSet.NorthEast.Id, _testDataSet.Newcastle.Id],
                null
            );

            AssertObservations([3, 4], result);
        }

        [Fact]
        public async Task TimePeriod_MatchesObservationsInRange()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations(
                [],
                [],
                new TimePeriodQuery(2022, AcademicYear, 2022, AcademicYear)
            );

            AssertObservations([1, 2, 4], result);
        }

        [Fact]
        public async Task TimePeriodNotInDataSet_ReturnsNothing()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations(
                [],
                [],
                new TimePeriodQuery(2010, AcademicYear, 2011, AcademicYear)
            );

            Assert.Empty(result);
        }

        [Fact]
        public async Task AllCriteria_MatchesEveryCriterion()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListObservations(
                [_testDataSet.Primary.Id],
                [_testDataSet.England.Id],
                new TimePeriodQuery(2022, AcademicYear, 2022, AcademicYear)
            );

            AssertObservations([1], result);
        }

        private void AssertObservations(int[] expectedIndexes, List<Observation> result)
        {
            Assert.Equal(
                expectedIndexes.Select(index => _testDataSet.Observations[index].Id).Order(),
                result.Select(observation => observation.Id).Order()
            );
        }
    }

    public class ListObservationBatchesTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task SixObservationsWithBatchSizeFour_YieldsTwoBatches()
        {
            var dataSet = await _testDataSet.Write();

            var batches = await dataSet.ListObservationBatches([], [], null, batchSize: 4).ToListAsync();

            Assert.Equal([4, 2], batches.Select(batch => batch.Count));
            Assert.Equal(6, batches.SelectMany(batch => batch).Select(o => o.Id).Distinct().Count());
        }

        [Fact]
        public async Task NoMatchedObservations_YieldsNothing()
        {
            var dataSet = await _testDataSet.Write();

            var batches = await dataSet.ListObservationBatches([], [Guid.NewGuid()], null, batchSize: 4).ToListAsync();

            Assert.Empty(batches);
        }
    }

    public class ListFilterItemsForQueryTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task ReturnsFilterItemsOfMatchedObservationsWithFilterGroupAndFilter()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilterItemsForQuery([_testDataSet.England.Id], null);

            Assert.Equal(
                new[]
                {
                    _testDataSet.Primary.Id,
                    _testDataSet.Secondary.Id,
                    _testDataSet.Year1.Id,
                    _testDataSet.Year2.Id,
                    _testDataSet.KeyStage2Total.Id,
                }.Order(),
                result.Select(item => item.Id).Order()
            );

            var keyStage2Total = Assert.Single(result, item => item.Id == _testDataSet.KeyStage2Total.Id);
            Assert.Equal("Total", keyStage2Total.Label);
            Assert.Equal(_testDataSet.KeyStage2.Id, keyStage2Total.FilterGroupId);
            Assert.Equal("Key stage 2", keyStage2Total.FilterGroup.Label);
            Assert.Equal(_testDataSet.NcYear.Id, keyStage2Total.FilterGroup.Filter.Id);
            Assert.Equal("ncyear", keyStage2Total.FilterGroup.Filter.Name);
        }

        [Fact]
        public async Task TimePeriod_ReturnsFilterItemsOfObservationsInRange()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilterItemsForQuery(
                [],
                new TimePeriodQuery(2021, AcademicYear, 2021, AcademicYear)
            );

            Assert.Equal(
                new[]
                {
                    _testDataSet.Primary.Id,
                    _testDataSet.Secondary.Id,
                    _testDataSet.Year1.Id,
                    _testDataSet.KeyStage1Total.Id,
                }.Order(),
                result.Select(item => item.Id).Order()
            );
        }

        [Fact]
        public async Task NoObservationsMatched_ReturnsNoFilterItems()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilterItemsForQuery([Guid.NewGuid()], null);

            Assert.Empty(result);
        }
    }

    public class ListFilterItemsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task FilterItemIds_ReturnedWithFilterGroupAndFilterForDataSetOnly()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilterItems([
                _testDataSet.Year2.Id,
                _testDataSet.Primary.Id,
                Guid.NewGuid(),
            ]);

            Assert.Equal(2, result.Count);

            var year2 = Assert.Single(result, item => item.Id == _testDataSet.Year2.Id);
            Assert.Equal("Year 2", year2.Label);
            Assert.Equal(_testDataSet.KeyStage1.Id, year2.FilterGroup.Id);
            Assert.Equal(_testDataSet.NcYear.Id, year2.FilterGroup.Filter.Id);
            Assert.Equal(_testDataSet.NcYear.Id, year2.FilterGroup.FilterId);

            var primary = Assert.Single(result, item => item.Id == _testDataSet.Primary.Id);
            Assert.Equal(_testDataSet.SchoolType.Id, primary.FilterGroup.Filter.Id);
        }
    }

    public class ListFiltersTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task FiltersReturnedWithoutFilterGroups()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilters();

            Assert.Equal(2, result.Count);

            var schoolType = Assert.Single(result, filter => filter.Id == _testDataSet.SchoolType.Id);
            Assert.Equal("School type", schoolType.Label);
            Assert.Equal("school_type", schoolType.Name);
            Assert.Equal("Type of school", schoolType.Hint);
            Assert.Null(schoolType.GroupCsvColumn);
            Assert.Equal(_testDataSet.Primary.Id, schoolType.AutoSelectFilterItemId);
            Assert.Equal(_testDataSet.Primary.Label, schoolType.AutoSelectFilterItemLabel);
            Assert.Equal(_testDataSet.SubjectId, schoolType.SubjectId);
            Assert.Empty(schoolType.FilterGroups);

            var ncYear = Assert.Single(result, filter => filter.Id == _testDataSet.NcYear.Id);
            Assert.Equal("key_stage", ncYear.GroupCsvColumn);
            Assert.Null(ncYear.Hint);
            Assert.Null(ncYear.AutoSelectFilterItemId);
        }

        [Fact]
        public async Task IncludeItems_ReturnedWithFilterGroupsAndFilterItems()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilters(includeItems: true);

            var ncYear = Assert.Single(result, filter => filter.Id == _testDataSet.NcYear.Id);

            Assert.Equal(
                new[] { _testDataSet.KeyStage1.Id, _testDataSet.KeyStage2.Id }.Order(),
                ncYear.FilterGroups.Select(group => group.Id).Order()
            );

            var keyStage1 = Assert.Single(ncYear.FilterGroups, group => group.Id == _testDataSet.KeyStage1.Id);
            Assert.Equal("Key stage 1", keyStage1.Label);
            Assert.Equal(
                new[] { _testDataSet.Year1.Id, _testDataSet.Year2.Id, _testDataSet.KeyStage1Total.Id }.Order(),
                keyStage1.FilterItems.Select(item => item.Id).Order()
            );
            Assert.All(keyStage1.FilterItems, item => Assert.Equal(keyStage1.Id, item.FilterGroupId));
        }
    }

    public class ListFilterItemRelationshipsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task ParentAndChildFilterItemsOnSameObservation_ReturnedAsDistinctPairs()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilterItemRelationships(_testDataSet.SchoolType.Id, _testDataSet.NcYear.Id);

            Assert.Equal(
                new[]
                {
                    (_testDataSet.Primary.Id, _testDataSet.Year1.Id),
                    (_testDataSet.Primary.Id, _testDataSet.Year2.Id),
                    (_testDataSet.Secondary.Id, _testDataSet.KeyStage1Total.Id),
                    (_testDataSet.Secondary.Id, _testDataSet.KeyStage2Total.Id),
                }.Order(),
                result.Order()
            );
        }

        [Fact]
        public async Task FiltersBelongToOtherDataSet_ReturnsEmpty()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListFilterItemRelationships(Guid.NewGuid(), _testDataSet.NcYear.Id);

            Assert.Empty(result);
        }
    }

    public class ListIndicatorsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task NoIds_ReturnsAllIndicators()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListIndicators();

            Assert.Equal(3, result.Count);

            var authorisedPercent = Assert.Single(
                result,
                indicator => indicator.Id == _testDataSet.AuthorisedPercent.Id
            );
            Assert.Equal("Percentage of authorised sessions", authorisedPercent.Label);
            Assert.Equal("sess_authorised_percent", authorisedPercent.Name);
            Assert.Equal(IndicatorUnit.Percent, authorisedPercent.Unit);
            Assert.Equal(2, authorisedPercent.DecimalPlaces);
            Assert.Equal(_testDataSet.DefaultIndicatorGroup.Id, authorisedPercent.IndicatorGroupId);

            var enrolments = Assert.Single(result, indicator => indicator.Id == _testDataSet.Enrolments.Id);
            Assert.Equal(IndicatorUnit.None, enrolments.Unit);
            Assert.Null(enrolments.DecimalPlaces);

            var notes = Assert.Single(result, indicator => indicator.Id == _testDataSet.Notes.Id);
            Assert.Equal(IndicatorUnit.String, notes.Unit);
        }

        [Fact]
        public async Task Ids_ReturnsMatchingIndicatorsForDataSetOnly()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListIndicators([_testDataSet.Notes.Id, Guid.NewGuid()]);

            Assert.Equal(_testDataSet.Notes.Id, Assert.Single(result).Id);
        }

        [Fact]
        public async Task EmptyIds_ReturnsNoIndicators()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListIndicators([]);

            Assert.Empty(result);
        }
    }

    public class ListIndicatorGroupsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task ReturnedWithIndicators()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListIndicatorGroups();

            Assert.Equal(2, result.Count);

            var defaultGroup = Assert.Single(result, group => group.Id == _testDataSet.DefaultIndicatorGroup.Id);
            Assert.Equal("Default", defaultGroup.Label);
            Assert.Equal(_testDataSet.SubjectId, defaultGroup.SubjectId);
            Assert.Equal(
                new[] { _testDataSet.Enrolments.Id, _testDataSet.AuthorisedPercent.Id }.Order(),
                defaultGroup.Indicators.Select(indicator => indicator.Id).Order()
            );
            Assert.All(defaultGroup.Indicators, indicator => Assert.Same(defaultGroup, indicator.IndicatorGroup));
        }
    }

    public class ListLocationsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task NoIds_ReturnsLocationsOfObservations()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListLocations();

            Assert.Equal(4, result.Count);

            foreach (var expected in _testDataSet.Locations)
            {
                Assert.Equal(expected, Assert.Single(result, location => location.Id == expected.Id));
            }
        }

        [Fact]
        public async Task Ids_ReturnsMatchingLocationsOfDataSet()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListLocations([_testDataSet.School.Id, Guid.NewGuid()]);

            Assert.Equal(_testDataSet.School, Assert.Single(result));
        }
    }

    public class ListGeographicLevelsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task ReturnsDistinctGeographicLevels()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListGeographicLevels();

            Assert.Equal(
                new[]
                {
                    GeographicLevel.Country,
                    GeographicLevel.LocalAuthority,
                    GeographicLevel.Region,
                    GeographicLevel.School,
                }.Order(),
                result.Order()
            );
        }
    }

    public class ListTimePeriodsTests : ParquetV1DataSetTests
    {
        [Fact]
        public async Task NoLocationIds_ReturnsDistinctTimePeriodsOrderedByDefinition()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListTimePeriods();

            Assert.Equal([(2021, AcademicYear), (2022, AcademicYear), (2022, CalendarYear)], result);
        }

        [Fact]
        public async Task LocationIds_ReturnsTimePeriodsAtLocationsOnly()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListTimePeriods([_testDataSet.NorthEast.Id, _testDataSet.School.Id]);

            Assert.Equal([(2021, AcademicYear), (2022, CalendarYear)], result);
        }

        [Fact]
        public async Task EmptyLocationIds_ReturnsNoTimePeriods()
        {
            var dataSet = await _testDataSet.Write();

            var result = await dataSet.ListTimePeriods([]);

            Assert.Empty(result);
        }

        [Fact]
        public async Task MonthlyTimePeriods_OrderedByDefinition()
        {
            _testDataSet.Observations[0].TimeIdentifier = December;
            _testDataSet.Observations[1].TimeIdentifier = January;
            _testDataSet.Observations[1].Year = 2021;
            _testDataSet.Observations[2].TimeIdentifier = February;
            _testDataSet.Observations[2].Year = 2021;
            _testDataSet.Observations.RemoveRange(3, 3);

            var writer = _testDataSet.BuildWriter();
            var subjectId = _testDataSet.SubjectId;

            await writer.StartStaging(subjectId, _testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);
            await writer.AppendObservations(
                subjectId,
                _testDataSet.Filters,
                _testDataSet.Indicators,
                _testDataSet.Observations
            );
            await writer.WriteParquetFiles(
                subjectId,
                _testDataSet.Filters,
                _testDataSet.IndicatorGroups,
                [_testDataSet.England]
            );

            var result = await _testDataSet.BuildDataSet().ListTimePeriods();

            Assert.Equal([(2021, January), (2021, February), (2021, December)], result);
        }
    }
}
