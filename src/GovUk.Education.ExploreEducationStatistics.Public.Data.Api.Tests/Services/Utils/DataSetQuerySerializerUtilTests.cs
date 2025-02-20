using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using Snapshooter.Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services.Utils;

public abstract class DataSetQuerySerializerUtilTests
{
    private static readonly DataSetQueryRequest NestedQuery = new()
    {
        Criteria = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaOr
                        {
                            Or =
                            [
                                new DataSetQueryCriteriaFacets
                                {
                                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                                    {
                                        NotIn =
                                        [
                                            new DataSetQueryTimePeriod { Period = "Period3", Code = "Code" },
                                            new DataSetQueryTimePeriod { Period = "Period4", Code = "Code" }
                                        ]
                                    }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                                    {
                                        In =
                                        [
                                            new DataSetQueryTimePeriod { Period = "Period1", Code = "Code" },
                                            new DataSetQueryTimePeriod { Period = "Period2", Code = "Code" }
                                        ]
                                    }
                                }
                            ]
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { Eq = "Filter3" }
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { In = ["Filter1", "Filter2"] }
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter4"] }
                        },
                        new DataSetQueryCriteriaNot
                        {
                            Not = new DataSetQueryCriteriaOr
                            {
                                Or =
                                [
                                    new DataSetQueryCriteriaFacets
                                    {
                                        Locations = new DataSetQueryCriteriaLocations
                                        {
                                            NotIn =
                                            [
                                                new DataSetQueryLocationProviderUkprn
                                                {
                                                    Level = "Level", Ukprn = "Location2"
                                                },
                                                new DataSetQueryLocationId { Level = "Level", Id = "Location3" }
                                            ]
                                        }
                                    },
                                    new DataSetQueryCriteriaFacets
                                    {
                                        Locations = new DataSetQueryCriteriaLocations
                                        {
                                            NotIn =
                                            [
                                                new DataSetQueryLocationProviderUkprn
                                                {
                                                    Level = "Level", Ukprn = "Location4"
                                                },
                                                new DataSetQueryLocationId { Level = "Level", Id = "Location1" }
                                            ]
                                        }
                                    },
                                ]
                            }
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter5" }
                        }
                    ]
                },
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { Eq = "Filter9" }
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { In = ["Filter7", "Filter8"] }
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter6"] }
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter10" }
                        }
                    ]
                }
            ]
        },
        
    };
    
    public class NormaliseQueryTests : DataSetQuerySerializerUtilTests
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
                        In = ["Filter1", "Filter3", "Filter2"], NotIn = ["Filter6", "Filter5", "Filter4"]
                    }
                }
            };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

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
                new DataSetQueryLocationLocalAuthorityOldCode { Level = "Level2", OldCode = "Location2" }
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
                new DataSetQueryLocationId { Level = "Level4", Id = "Location2" }
            ];

            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Locations = new DataSetQueryCriteriaLocations
                    {
                        In = originalLocations, NotIn = originalLocations
                    }
                }
            };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

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
                        In = ["Level3", "Level1", "Level2"], NotIn = ["Level6", "Level4", "Level5"],
                    }
                }
            };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

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
                            new DataSetQueryTimePeriod { Period = "Period4", Code = "AY" }
                        ]
                    }
                }
            };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

            // Expect to see DataSetQueryTimePeriods ordered firstly by Code and then by Period.
            List<DataSetQueryTimePeriod> expectedIns =
            [
                new() { Period = "Period2", Code = "AY" },
                new() { Period = "Period3", Code = "AY" },
                new() { Period = "Period1", Code = "FY" }
            ];

            List<DataSetQueryTimePeriod> expectedNotIns =
            [
                new() { Period = "Period4", Code = "AY" },
                new() { Period = "Period5", Code = "AY" },
                new() { Period = "Period6", Code = "FY" }
            ];

            var facets = (normalised.Criteria as DataSetQueryCriteriaFacets)!;
            Assert.Equal(expectedIns, facets.TimePeriods!.In);
            Assert.Equal(expectedNotIns, facets.TimePeriods!.NotIn);
        }

        [Fact]
        public void ArrayPropertiesOrdering_Indicators()
        {
            var query = new DataSetQueryRequest { Indicators = ["Indicator1", "Indicator3", "Indicator2"] };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

            Assert.Equal(["Indicator1", "Indicator2", "Indicator3"], normalised.Indicators);
        }

        [Fact]
        public void ArrayPropertiesOrdering_Sorts()
        {
            var query = new DataSetQueryRequest
            {
                Sorts =
                [
                    new DataSetQuerySort { Field = "Sort1", Direction = "Asc" },
                    new DataSetQuerySort { Field = "Sort3", Direction = "Asc" },
                    new DataSetQuerySort { Field = "Sort2", Direction = "Asc" }
                ]
            };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

            List<DataSetQuerySort> expectedSorts =
            [
                new() { Field = "Sort1", Direction = "Asc" },
                new() { Field = "Sort2", Direction = "Asc" },
                new() { Field = "Sort3", Direction = "Asc" }
            ];

            Assert.Equal(expectedSorts, normalised.Sorts);
        }

        [Fact]
        public void SubCriteriaOrdering()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaFacets() },
                        new DataSetQueryCriteriaOr { Or = [] },
                        new DataSetQueryCriteriaFacets(),
                        new DataSetQueryCriteriaAnd { And = [] },
                        new DataSetQueryCriteriaOr { Or = [] },
                        new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaFacets() }
                    ]
                }
            };

            var normalised = DataSetQuerySerializerUtil.NormaliseQuery(query);

            // Assert that sub-criteria are ordered by the alphabetical order of their
            // criteria types (e.g. "And" before "Facets", "Facets" before "Not").
            var expectedCriteria = new DataSetQueryCriteriaAnd
            {
                And = new List<IDataSetQueryCriteria>
                {
                    new DataSetQueryCriteriaAnd { And = new List<IDataSetQueryCriteria>() },
                    new DataSetQueryCriteriaFacets(),
                    new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaFacets() },
                    new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaFacets() },
                    new DataSetQueryCriteriaOr { Or = new List<IDataSetQueryCriteria>() },
                    new DataSetQueryCriteriaOr { Or = new List<IDataSetQueryCriteria>() }
                }
            };

            normalised.Criteria.AssertDeepEqualTo(expectedCriteria);
        }

        [Fact]
        public void DeeplyNestedCriteria_OrderedConsistently()
        {
            

            // Create another query that is logically the same as the first query
            // but with difference criteria ordering.
            var query2 = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaAnd
                        {
                            And =
                            [
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter10" }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { In = ["Filter7", "Filter8"] }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter6"] }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { Eq = "Filter9" }
                                }
                            ]
                        },
                        new DataSetQueryCriteriaAnd
                        {
                            And =
                            [
                                new DataSetQueryCriteriaNot
                                {
                                    Not = new DataSetQueryCriteriaOr
                                    {
                                        Or =
                                        [
                                            new DataSetQueryCriteriaFacets
                                            {
                                                Locations = new DataSetQueryCriteriaLocations
                                                {
                                                    NotIn =
                                                    [
                                                        new DataSetQueryLocationProviderUkprn
                                                        {
                                                            Level = "Level", Ukprn = "Location4"
                                                        },
                                                        new DataSetQueryLocationId { Level = "Level", Id = "Location1" }
                                                    ]
                                                }
                                            },
                                            new DataSetQueryCriteriaFacets
                                            {
                                                Locations = new DataSetQueryCriteriaLocations
                                                {
                                                    NotIn =
                                                    [
                                                        new DataSetQueryLocationProviderUkprn
                                                        {
                                                            Level = "Level", Ukprn = "Location2"
                                                        },
                                                        new DataSetQueryLocationId { Level = "Level", Id = "Location3" }
                                                    ]
                                                }
                                            },
                                        ]
                                    }
                                },
                                new DataSetQueryCriteriaOr
                                {
                                    Or =
                                    [
                                        new DataSetQueryCriteriaFacets
                                        {
                                            TimePeriods = new DataSetQueryCriteriaTimePeriods
                                            {
                                                In =
                                                [
                                                    new DataSetQueryTimePeriod { Period = "Period2", Code = "Code" },
                                                    new DataSetQueryTimePeriod { Period = "Period1", Code = "Code" }
                                                ]
                                            }
                                        },
                                        new DataSetQueryCriteriaFacets
                                        {
                                            TimePeriods = new DataSetQueryCriteriaTimePeriods
                                            {
                                                NotIn =
                                                [
                                                    new DataSetQueryTimePeriod { Period = "Period3", Code = "Code" },
                                                    new DataSetQueryTimePeriod { Period = "Period4", Code = "Code" }
                                                ]
                                            }
                                        }
                                    ]
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { In = ["Filter2", "Filter1"] }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { Eq = "Filter3" }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter5" }
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter4"] }
                                }
                            ]
                        }
                    ]
                }
            };

            var normalisedQuery1 = DataSetQuerySerializerUtil.NormaliseQuery(NestedQuery);
            var normalisedQuery2 = DataSetQuerySerializerUtil.NormaliseQuery(query2);

            normalisedQuery1.Criteria.AssertDeepEqualTo(normalisedQuery2.Criteria);
        }
    }

    public class GetSortableStringTests : DataSetQuerySerializerUtilTests
    {
        [Fact]
        public void DataSetQueryCriteriaFilters()
        {
            var criteria = new DataSetQueryCriteriaFilters
            {
                Eq = "Option1", NotEq = "Option2", In = ["Option3", "Option4"], NotIn = ["Option5", "Option6"]
            };

            var expected =
                "DataSetQueryCriteriaFilters { " +
                "Eq: Option1, " +
                "NotEq: Option2, " +
                "In: [Option3,Option4], " +
                "NotIn: [Option5,Option6] }";

            Assert.Equal(expected, criteria.GetSortableString());
        }

        [Fact]
        public void DataSetQueryCriteriaGeographicLevels()
        {
            var criteria = new DataSetQueryCriteriaGeographicLevels
            {
                Eq = "Option1", NotEq = "Option2", In = ["Option3", "Option4"], NotIn = ["Option5", "Option6"]
            };

            var expected =
                "DataSetQueryCriteriaGeographicLevels { " +
                "Eq: Option1, " +
                "NotEq: Option2, " +
                "In: [Option3,Option4], " +
                "NotIn: [Option5,Option6] }";

            Assert.Equal(expected, criteria.GetSortableString());
        }

        [Fact]
        public void DataSetQueryCriteriaLocations()
        {
            var criteria = new DataSetQueryCriteriaLocations
            {
                Eq = new DataSetQueryLocationCode { Level = "Level1", Code = "Value1" },
                NotEq = new DataSetQueryLocationId { Level = "Level2", Id = "Value2" },
                In =
                [
                    new DataSetQueryLocationProviderUkprn { Level = "Level3", Ukprn = "Value3" },
                    new DataSetQueryLocationSchoolUrn { Level = "Level4", Urn = "Value4" }
                ],
                NotIn =
                [
                    new DataSetQueryLocationLocalAuthorityCode { Level = "Level5", Code = "Value5" },
                    new DataSetQueryLocationSchoolLaEstab { Level = "Level6", LaEstab = "Value6" },
                    new DataSetQueryLocationLocalAuthorityOldCode { Level = "Level7", OldCode = "Value7" }
                ]
            };

            var expected =
                "DataSetQueryCriteriaLocations { " +
                "Eq: DataSetQueryLocationCode { Code = Value1, Level = Level1 }, " +
                "NotEq: DataSetQueryLocationId { Id = Value2, Level = Level2 }, " +
                "In: [" +
                "DataSetQueryLocationProviderUkprn { Ukprn = Value3, Level = Level3 }," +
                "DataSetQueryLocationSchoolUrn { Urn = Value4, Level = Level4 }" +
                "], " +
                "NotIn: [" +
                "DataSetQueryLocationLocalAuthorityCode { Code = Value5, Level = Level5 }," +
                "DataSetQueryLocationSchoolLaEstab { LaEstab = Value6, Level = Level6 }," +
                "DataSetQueryLocationLocalAuthorityOldCode { OldCode = Value7, Level = Level7 }" +
                "] }";

            Assert.Equal(expected, criteria.GetSortableString());
        }

        [Fact]
        public void DataSetQueryCriteriaTimePeriods()
        {
            var criteria = new DataSetQueryCriteriaTimePeriods
            {
                Eq = new DataSetQueryTimePeriod { Period = "Period1", Code = "Code1" },
                NotEq = new DataSetQueryTimePeriod { Period = "Period2", Code = "Code2" },
                In = [new DataSetQueryTimePeriod { Period = "Period3", Code = "Code3" }],
                NotIn = [new DataSetQueryTimePeriod { Period = "Period4", Code = "Code4" }],
                Gt = new DataSetQueryTimePeriod { Period = "Period5", Code = "Code5" },
                Gte = new DataSetQueryTimePeriod { Period = "Period6", Code = "Code6" },
                Lt = new DataSetQueryTimePeriod { Period = "Period7", Code = "Code7" },
                Lte = new DataSetQueryTimePeriod { Period = "Period8", Code = "Code8" },
            };

            var expected =
                "DataSetQueryCriteriaTimePeriods { " +
                "Eq: DataSetQueryTimePeriod { Period = Period1, Code = Code1 }, " +
                "NotEq: DataSetQueryTimePeriod { Period = Period2, Code = Code2 }, " +
                "In: [DataSetQueryTimePeriod { Period = Period3, Code = Code3 }], " +
                "NotIn: [DataSetQueryTimePeriod { Period = Period4, Code = Code4 }], " +
                "Gt: DataSetQueryTimePeriod { Period = Period5, Code = Code5 }, " +
                "Gte: DataSetQueryTimePeriod { Period = Period6, Code = Code6 }, " +
                "Lt: DataSetQueryTimePeriod { Period = Period7, Code = Code7 }, " +
                "Lte: DataSetQueryTimePeriod { Period = Period8, Code = Code8 } }";

            Assert.Equal(expected, criteria.GetSortableString());
        }

        [Fact]
        public void DataSetQueryCriteriaFacets()
        {
            var filters = new DataSetQueryCriteriaFilters { Eq = "Option1" };
            var geographicLevels = new DataSetQueryCriteriaGeographicLevels { Eq = "Option1" };
            var locations = new DataSetQueryCriteriaLocations
            {
                Eq = new DataSetQueryLocationCode { Level = "Level1", Code = "Value1" }
            };
            var timePeriods = new DataSetQueryCriteriaTimePeriods
            {
                Eq = new DataSetQueryTimePeriod { Period = "Period1", Code = "Code1" }
            };
            
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = filters,
                GeographicLevels = geographicLevels,
                Locations = locations,
                TimePeriods = timePeriods
            };

            var expected =
                "DataSetQueryCriteriaFacets { " +
                $"Filters: {filters.GetSortableString()}, " +
                $"GeographicLevels: {geographicLevels.GetSortableString()}, " +
                $"Locations: {locations.GetSortableString()}, " +
                $"TimePeriods: {timePeriods.GetSortableString()} }}";


            Assert.Equal(expected, criteria.GetSortableString());
        }
        
        [Fact]
        public void DataSetQueryCriteriaAnd()
        {
            var facets1 = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters { Eq = "Option1" }
            };
            var facets2 = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters { Eq = "Option2" }
            };

            var criteria = new DataSetQueryCriteriaAnd
            {
                And =
                [
                    facets1,
                    facets2
                ]
            };

            Assert.Equal(
                $"DataSetQueryCriteriaAnd {{ And: [{facets1.GetSortableString()},{facets2.GetSortableString()}] }}",
                criteria.GetSortableString());
        }
        
        [Fact]
        public void DataSetQueryCriteriaOr()
        {
            var facets1 = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters { Eq = "Option1" }
            };
            var facets2 = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters { Eq = "Option2" }
            };

            var criteria = new DataSetQueryCriteriaOr
            {
                Or =
                [
                    facets1,
                    facets2
                ]
            };

            Assert.Equal(
                $"DataSetQueryCriteriaOr {{ Or: [{facets1.GetSortableString()},{facets2.GetSortableString()}] }}",
                criteria.GetSortableString());
        }
        
        [Fact]
        public void DataSetQueryCriteriaNot()
        {
            var facets = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters { Eq = "Option1" }
            };

            var criteria = new DataSetQueryCriteriaNot
            {
                Not = facets
            };

            Assert.Equal(
                $"DataSetQueryCriteriaNot {{ Not: {facets.GetSortableString()} }}",
                criteria.GetSortableString());
        }
    }

    public class SerializeTests : DataSetQuerySerializerUtilTests
    {
        [Fact]
        public void SerializedJsonMatchesSnapshot()
        {
            var query = NestedQuery with
            {
                Sorts = [
                    new DataSetQuerySort
                    {
                        Field = "Field2", 
                        Direction = "ASC",
                    },
                    new DataSetQuerySort
                    {
                        Field = "Field1", 
                        Direction = "ASC",
                    }
                ],
                Indicators = ["Indicator3", "Indicator1", "Indicator2"],
                Debug = true,
                Page = 3,
                PageSize = 300
            };
            
            var serialized = DataSetQuerySerializerUtil.SerializeQuery(query);

            // Compare against the snapshot showing that:
            //
            // * properties are displayed alphabetically
            // * output is indented
            // * null fields are excluded
            Snapshot.Match(serialized);
        }
    }
}
