using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

internal static class DataSetQueryRequestTestData
{
    public static readonly DataSetQueryRequest NestedQuery1 = new()
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
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period3",
                                                Code = "Code",
                                            },
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period4",
                                                Code = "Code",
                                            },
                                        ],
                                    },
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                                    {
                                        In =
                                        [
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period1",
                                                Code = "Code",
                                            },
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period2",
                                                Code = "Code",
                                            },
                                        ],
                                    },
                                },
                            ],
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { Eq = "Filter3" },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                In = ["Filter1", "Filter2"],
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter4"] },
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
                                                    Level = "Level",
                                                    Ukprn = "Location2",
                                                },
                                                new DataSetQueryLocationId
                                                {
                                                    Level = "Level",
                                                    Id = "Location3",
                                                },
                                            ],
                                        },
                                    },
                                    new DataSetQueryCriteriaFacets
                                    {
                                        Locations = new DataSetQueryCriteriaLocations
                                        {
                                            NotIn =
                                            [
                                                new DataSetQueryLocationProviderUkprn
                                                {
                                                    Level = "Level",
                                                    Ukprn = "Location4",
                                                },
                                                new DataSetQueryLocationId
                                                {
                                                    Level = "Level",
                                                    Id = "Location1",
                                                },
                                            ],
                                        },
                                    },
                                ],
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter5" },
                        },
                    ],
                },
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { Eq = "Filter9" },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                In = ["Filter7", "Filter8"],
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter6"] },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter10" },
                        },
                    ],
                },
            ],
        },
        Sorts =
        [
            new DataSetQuerySort { Field = "Field2", Direction = "ASC" },
            new DataSetQuerySort { Field = "Field1", Direction = "ASC" },
        ],
        Indicators = ["Indicator3", "Indicator1", "Indicator2"],
        Debug = true,
        Page = 3,
        PageSize = 300,
    };

    /// <summary>
    /// This query request is logically the same as NestedQuery1 but with sub-criteria and
    /// list elements in different orders.
    /// </summary>
    public static readonly DataSetQueryRequest NestedQuery2 = new()
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
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter10" },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                In = ["Filter7", "Filter8"],
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter6"] },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { Eq = "Filter9" },
                        },
                    ],
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
                                                    Level = "Level",
                                                    Ukprn = "Location4",
                                                },
                                                new DataSetQueryLocationId
                                                {
                                                    Level = "Level",
                                                    Id = "Location1",
                                                },
                                            ],
                                        },
                                    },
                                    new DataSetQueryCriteriaFacets
                                    {
                                        Locations = new DataSetQueryCriteriaLocations
                                        {
                                            NotIn =
                                            [
                                                new DataSetQueryLocationProviderUkprn
                                                {
                                                    Level = "Level",
                                                    Ukprn = "Location2",
                                                },
                                                new DataSetQueryLocationId
                                                {
                                                    Level = "Level",
                                                    Id = "Location3",
                                                },
                                            ],
                                        },
                                    },
                                ],
                            },
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
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period2",
                                                Code = "Code",
                                            },
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period1",
                                                Code = "Code",
                                            },
                                        ],
                                    },
                                },
                                new DataSetQueryCriteriaFacets
                                {
                                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                                    {
                                        NotIn =
                                        [
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period3",
                                                Code = "Code",
                                            },
                                            new DataSetQueryTimePeriod
                                            {
                                                Period = "Period4",
                                                Code = "Code",
                                            },
                                        ],
                                    },
                                },
                            ],
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                In = ["Filter2", "Filter1"],
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { Eq = "Filter3" },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotEq = "Filter5" },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters { NotIn = ["Filter4"] },
                        },
                    ],
                },
            ],
        },
        Sorts =
        [
            new DataSetQuerySort { Field = "Field2", Direction = "ASC" },
            new DataSetQuerySort { Field = "Field1", Direction = "ASC" },
        ],
        Indicators = ["Indicator1", "Indicator3", "Indicator2"],
        Debug = true,
        Page = 3,
        PageSize = 300,
    };
}
