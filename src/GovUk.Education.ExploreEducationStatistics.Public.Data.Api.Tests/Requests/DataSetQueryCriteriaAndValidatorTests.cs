using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryCriteriaAndValidatorTests
{
    private readonly DataSetQueryCriteriaAnd.Validator _validator = new();

    [Fact]
    public void Empty()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And = [],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(q => q.And);
    }

    [Fact]
    public void SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = "12345"
                    },
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = ""
                    },
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("And[0].Filters.Eq");
    }

    [Fact]
    public void MultipleFacets_Success()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = "12345"
                    }
                },
                new DataSetQueryCriteriaFacets
                {
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        NotEq = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" }
                    },
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void MultipleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = ""
                    },
                },
                new DataSetQueryCriteriaFacets
                {
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Gte = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "Invalid" }
                    },
                },
            ],
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor("And[0].Filters.Eq");
        result.ShouldHaveValidationErrorFor("And[1].TimePeriods.Gte.Code");
    }

    [Fact]
    public void SingleCondition_Empty_Failure()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaOr
                {
                    Or = [],
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("And[0].Or");
    }

    [Fact]
    public void SingleCondition_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = "12345"
                            },
                        },
                    ],
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SingleCondition_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = new string('x', 11)
                            },
                        },
                    ],
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("And[0].Or[0].Filters.Eq");
    }

    [Fact]
    public void MultipleConditions_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = "12345"
                            },
                        },
                    ],
                },
                new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            TimePeriods = new DataSetQueryCriteriaTimePeriods
                            {
                                Gte = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" }
                            },
                        },
                    ],
                },
                new DataSetQueryCriteriaNot
                {
                    Not =new DataSetQueryCriteriaFacets
                    {
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            In = [new DataSetQueryLocationId { Level = "NAT", Id = "12345" }],
                        },
                    },
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MultipleConditions_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = new string('x', 11)
                            },
                        },
                    ],
                },
                new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            TimePeriods = new DataSetQueryCriteriaTimePeriods
                            {
                                Gte = new DataSetQueryTimePeriod { Period = "", Code = "AY" }
                            },
                        },
                    ],
                },
                new DataSetQueryCriteriaNot
                {
                    Not =new DataSetQueryCriteriaFacets
                    {
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            In = [new DataSetQueryLocationId { Level = "nat", Id = "12345" }],
                        },
                    },
                },
            ],
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor("And[0].And[0].Filters.Eq");
        result.ShouldHaveValidationErrorFor("And[1].Or[0].TimePeriods.Gte.Period");
        result.ShouldHaveValidationErrorFor("And[2].Not.Locations.In[0].Level");
    }

    [Fact]
    public void MixedCriteria_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = "12345"
                            },
                        },
                    ],
                },
                new DataSetQueryCriteriaFacets
                {
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Gte = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" }
                    },
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MixedCriteria_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaAnd
        {
            And =
            [
                new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = new string('x', 11)
                            },
                        },
                    ],
                },
                new DataSetQueryCriteriaFacets
                {
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Gte = new DataSetQueryTimePeriod { Period = "", Code = "AY" }
                    },
                },
            ],
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor("And[0].And[0].Filters.Eq");
        result.ShouldHaveValidationErrorFor("And[1].TimePeriods.Gte.Period");
    }
}
