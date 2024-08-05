using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

[Collection("Set FluentValidation property name camel case configuration")]
public class DataSetQueryCriteriaOrValidatorTests
{
    private readonly DataSetQueryCriteriaOr.Validator _validator = new();

    [Fact]
    public void Empty_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or = [],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(q => q.Or)
            .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
            .Only();
    }

    [Fact]
    public void Nulls_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
            [
                null!,
                new DataSetQueryCriteriaFacets()
            ],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("or[0]")
            .WithErrorCode(FluentValidationKeys.NotNullValidator)
            .Only();
    }

    [Fact]
    public void SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaOr
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
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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
            .ShouldHaveValidationErrorFor("or[0].filters.eq")
            .Only();
    }

    [Fact]
    public void MultipleFacets_Success()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void MultipleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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

        Assert.Equal(2, result.Errors.Count);

        result.ShouldHaveValidationErrorFor("or[0].filters.eq");
        result.ShouldHaveValidationErrorFor("or[1].timePeriods.gte.code");
    }

    [Fact]
    public void SingleCondition_Empty_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
            [
                new DataSetQueryCriteriaOr
                {
                    Or = [],
                },
            ],
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("or[0].or")
            .Only();
    }

    [Fact]
    public void SingleCondition_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SingleCondition_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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
            .ShouldHaveValidationErrorFor("or[0].or[0].filters.eq")
            .Only();
    }

    [Fact]
    public void MultipleConditions_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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
                    Not = new DataSetQueryCriteriaFacets
                    {
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            In = [new DataSetQueryLocationId { Level = "NAT", Id = "12345" }],
                        },
                    },
                },
            ],
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MultipleConditions_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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
                    Not = new DataSetQueryCriteriaFacets
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

        Assert.Equal(3, result.Errors.Count);

        result.ShouldHaveValidationErrorFor("or[0].and[0].filters.eq");
        result.ShouldHaveValidationErrorFor("or[1].or[0].timePeriods.gte.period");
        result.ShouldHaveValidationErrorFor("or[2].not.locations.in[0].level");
    }

    [Fact]
    public void MixedCriteria_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MixedCriteria_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaOr
        {
            Or =
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

        Assert.Equal(2, result.Errors.Count);

        result.ShouldHaveValidationErrorFor("or[0].and[0].filters.eq");
        result.ShouldHaveValidationErrorFor("or[1].timePeriods.gte.period");
    }
}
