using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services.Utils;

public abstract class DataSetQueryNormalisationUtilTests
{
    public class NormaliseQueryTests : DataSetQueryNormalisationUtilTests
    {
        [Fact]
        public void ArrayPropertiesOrdering_Filters()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        In = ["Filter1", "Filter3", "Filter2"],
                        NotIn = ["Filter6", "Filter5", "Filter4"],
                    },
                },
            };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            var facets = (normalised.Criteria as DataSetQueryCriteriaFacets)!;
            Assert.Equal(["Filter1", "Filter2", "Filter3"], facets.Filters!.In);
            Assert.Equal(["Filter4", "Filter5", "Filter6"], facets.Filters!.NotIn);
        }

        [Fact]
        public void ArrayPropertiesOrdering_Locations_ByLevelAndKeyAndValue()
        {
            List<IDataSetQueryLocation> originalLocations =
            [
                new DataSetQueryLocationCode { Level = "Level4", Code = "Location2" },
                new DataSetQueryLocationCode { Level = "Level4", Code = "Location1" },
                new DataSetQueryLocationId { Level = "Level4", Id = "Location2" },
                new DataSetQueryLocationId { Level = "Level4", Id = "Location1" },
                new DataSetQueryLocationProviderUkprn { Level = "Level1", Ukprn = "Location2" },
                new DataSetQueryLocationProviderUkprn { Level = "Level1", Ukprn = "Location1" },
                new DataSetQueryLocationProviderUkprn { Level = "Level2", Ukprn = "Location3" },
                new DataSetQueryLocationSchoolUrn { Level = "Level2", Urn = "Location1" },
                new DataSetQueryLocationSchoolUrn { Level = "Level2", Urn = "Location3" },
                new DataSetQueryLocationSchoolUrn { Level = "Level3", Urn = "Location2" },
                new DataSetQueryLocationLocalAuthorityCode { Level = "Level1", Code = "Location1" },
                new DataSetQueryLocationLocalAuthorityCode { Level = "Level2", Code = "Location2" },
                new DataSetQueryLocationSchoolLaEstab { Level = "Level2", LaEstab = "Location2" },
                new DataSetQueryLocationSchoolLaEstab { Level = "Level2", LaEstab = "Location1" },
                new DataSetQueryLocationLocalAuthorityOldCode { Level = "Level2", OldCode = "Location1" },
                new DataSetQueryLocationLocalAuthorityOldCode { Level = "Level2", OldCode = "Location2" },
            ];

            List<IDataSetQueryLocation> expectedLocations =
            [
                new DataSetQueryLocationLocalAuthorityCode { Level = "Level1", Code = "Location1" },
                new DataSetQueryLocationProviderUkprn { Level = "Level1", Ukprn = "Location1" },
                new DataSetQueryLocationProviderUkprn { Level = "Level1", Ukprn = "Location2" },
                new DataSetQueryLocationLocalAuthorityCode { Level = "Level2", Code = "Location2" },
                new DataSetQueryLocationSchoolLaEstab { Level = "Level2", LaEstab = "Location1" },
                new DataSetQueryLocationSchoolLaEstab { Level = "Level2", LaEstab = "Location2" },
                new DataSetQueryLocationLocalAuthorityOldCode { Level = "Level2", OldCode = "Location1" },
                new DataSetQueryLocationLocalAuthorityOldCode { Level = "Level2", OldCode = "Location2" },
                new DataSetQueryLocationProviderUkprn { Level = "Level2", Ukprn = "Location3" },
                new DataSetQueryLocationSchoolUrn { Level = "Level2", Urn = "Location1" },
                new DataSetQueryLocationSchoolUrn { Level = "Level2", Urn = "Location3" },
                new DataSetQueryLocationSchoolUrn { Level = "Level3", Urn = "Location2" },
                new DataSetQueryLocationCode { Level = "Level4", Code = "Location1" },
                new DataSetQueryLocationCode { Level = "Level4", Code = "Location2" },
                new DataSetQueryLocationId { Level = "Level4", Id = "Location1" },
                new DataSetQueryLocationId { Level = "Level4", Id = "Location2" },
            ];

            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Locations = new DataSetQueryCriteriaLocations { In = originalLocations, NotIn = originalLocations },
                },
            };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            var facets = (normalised.Criteria as DataSetQueryCriteriaFacets)!;
            Assert.Equal(expectedLocations, facets.Locations!.In);
            Assert.Equal(expectedLocations, facets.Locations!.NotIn);
        }

        [Fact]
        public void ArrayPropertiesOrdering_GeographicLevels()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                    {
                        In = ["Level3", "Level1", "Level2"],
                        NotIn = ["Level6", "Level4", "Level5"],
                    },
                },
            };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            var facets = (normalised.Criteria as DataSetQueryCriteriaFacets)!;
            Assert.Equal(["Level1", "Level2", "Level3"], facets.GeographicLevels!.In);
            Assert.Equal(["Level4", "Level5", "Level6"], facets.GeographicLevels!.NotIn);
        }

        [Fact]
        public void ArrayPropertiesOrdering_TimePeriods_ByCodeAndPeriod()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        In =
                        [
                            new DataSetQueryTimePeriod { Period = "Period3", Code = "AY" },
                            new DataSetQueryTimePeriod { Period = "Period1", Code = "FY" },
                            new DataSetQueryTimePeriod { Period = "Period2", Code = "AY" },
                        ],
                        NotIn =
                        [
                            new DataSetQueryTimePeriod { Period = "Period6", Code = "FY" },
                            new DataSetQueryTimePeriod { Period = "Period5", Code = "AY" },
                            new DataSetQueryTimePeriod { Period = "Period4", Code = "AY" },
                        ],
                    },
                },
            };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            // Expect to see DataSetQueryTimePeriods ordered firstly by Code and then by Period.
            List<DataSetQueryTimePeriod> expectedIns =
            [
                new() { Period = "Period2", Code = "AY" },
                new() { Period = "Period3", Code = "AY" },
                new() { Period = "Period1", Code = "FY" },
            ];

            List<DataSetQueryTimePeriod> expectedNotIns =
            [
                new() { Period = "Period4", Code = "AY" },
                new() { Period = "Period5", Code = "AY" },
                new() { Period = "Period6", Code = "FY" },
            ];

            var facets = (normalised.Criteria as DataSetQueryCriteriaFacets)!;
            Assert.Equal(expectedIns, facets.TimePeriods!.In);
            Assert.Equal(expectedNotIns, facets.TimePeriods!.NotIn);
        }

        [Fact]
        public void ArrayPropertiesOrdering_Indicators()
        {
            var query = new DataSetQueryRequest { Indicators = ["Indicator1", "Indicator3", "Indicator2"] };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            Assert.Equal(["Indicator1", "Indicator2", "Indicator3"], normalised.Indicators);
        }

        [Fact]
        public void ArrayPropertiesOrdering_Sorts_OrderRetained()
        {
            List<DataSetQuerySort> originalSortOrders =
            [
                new() { Field = "Sort1", Direction = "Asc" },
                new() { Field = "Sort3", Direction = "Asc" },
                new() { Field = "Sort2", Direction = "Asc" },
            ];

            var query = new DataSetQueryRequest { Sorts = originalSortOrders };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            // Expect that the order of the Sorts is retained as the order sets the
            // primary, secondary, tertiary sort orders.
            Assert.Equal(originalSortOrders, normalised.Sorts);
        }

        [Fact]
        public void SubCriteriaOrdering_OrderedAlphabeticallyByJson()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            TimePeriods = new DataSetQueryCriteriaTimePeriods
                            {
                                Eq = new DataSetQueryTimePeriod { Period = "Period1", Code = "Code1" },
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter2" },
                        },
                        new DataSetQueryCriteriaAnd { And = [] },
                        new DataSetQueryCriteriaFacets { Filters = new DataSetQueryCriteriaFilters { Eq = "Filter2" } },
                        new DataSetQueryCriteriaOr { Or = [] },
                        new DataSetQueryCriteriaFacets { Filters = new DataSetQueryCriteriaFilters { Eq = "Filter1" } },
                        new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaFacets() },
                    ],
                },
            };

            var normalised = DataSetQueryNormalisationUtil.NormaliseQuery(query);

            // Assert that sub-criteria are ordered by the alphabetical order of their
            // JSON representations, with null elements removed.
            var expectedCriteria = new DataSetQueryCriteriaAnd
            {
                And = new List<IDataSetQueryCriteria>
                {
                    // First child element is "And".
                    new DataSetQueryCriteriaAnd { And = new List<IDataSetQueryCriteria>() },
                    // First child element is "Filters", followed by "Eq", followed by "Filter1".
                    new DataSetQueryCriteriaFacets { Filters = new DataSetQueryCriteriaFilters { Eq = "Filter1" } },
                    // First child element is "Filters", followed by "Eq", followed by "Filter2".
                    new DataSetQueryCriteriaFacets { Filters = new DataSetQueryCriteriaFilters { Eq = "Filter2" } },
                    // First child element is "Filters", followed by "NotEq", followed by "Filter2".
                    new DataSetQueryCriteriaFacets { Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter2" } },
                    // First child element is "Not".
                    new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaFacets() },
                    // First child element is "Or".
                    new DataSetQueryCriteriaOr { Or = new List<IDataSetQueryCriteria>() },
                    // First child element is "TimePeriods".
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Period = "Period1", Code = "Code1" },
                        },
                    },
                },
            };

            normalised.Criteria.AssertDeepEqualTo(expectedCriteria);
        }

        [Fact]
        public void DeeplyNestedCriteria_OrderedConsistently()
        {
            var normalisedQuery1 = DataSetQueryNormalisationUtil.NormaliseQuery(
                DataSetQueryRequestTestData.NestedQuery1
            );
            var normalisedQuery2 = DataSetQueryNormalisationUtil.NormaliseQuery(
                DataSetQueryRequestTestData.NestedQuery2
            );

            normalisedQuery1.Criteria.AssertDeepEqualTo(normalisedQuery2.Criteria);
        }
    }
}
