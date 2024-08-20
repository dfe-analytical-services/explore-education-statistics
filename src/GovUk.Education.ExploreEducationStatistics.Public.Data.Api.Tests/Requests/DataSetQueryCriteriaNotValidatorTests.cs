using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

[Collection("Set FluentValidation property name camel case configuration")]
public class DataSetQueryCriteriaNotValidatorTests
{
    private readonly DataSetQueryCriteriaNot.Validator _validator = new();

    [Fact]
    public void Null_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = null!
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("not")
            .WithErrorCode(FluentValidationKeys.NotNullValidator)
            .Only();
    }

    [Fact]
    public void Facets_Success()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters
                {
                    Eq = "12345"
                },
            },
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Facets_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaFacets
            {
                Filters = new DataSetQueryCriteriaFilters
                {
                    Eq = ""
                },
            },
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("not.filters.eq")
            .Only();
    }

    [Fact]
    public void Condition_Empty_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaOr
            {
                Or = [],
            },
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("not.or")
            .Only();
    }

    [Fact]
    public void Condition_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaOr
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
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Condition_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaOr
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
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("not.or[0].filters.eq")
            .Only();
    }

    [Fact]
    public void Condition_MultipleFacets_Success()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaAnd
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
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Gte = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" }
                        },
                    },
                ],
            },
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Condition_MultipleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaAnd
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
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Gte = new DataSetQueryTimePeriod { Period = "", Code = "AY" }
                        },
                    },
                ],
            },
        };

        var result = _validator.TestValidate(query);

        Assert.Equal(2, result.Errors.Count);

        result.ShouldHaveValidationErrorFor("not.and[0].filters.eq");
        result.ShouldHaveValidationErrorFor("not.and[1].timePeriods.gte.period");
    }

    [Fact]
    public void Not_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaNot
            {
                Not = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = "12345"
                    },
                },
            },
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void Not_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaNot
            {
                Not = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = new string('x', 11)
                    },
                },
            },
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("not.not.filters.eq")
            .Only();
    }

    [Fact]
    public void Condition_Not_SingleFacets_Success()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaNot
                    {
                        Not = new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = "12345"
                            },
                        },
                    },
                ],
            }
        };

        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Condition_Not_SingleFacets_Failure()
    {
        var query = new DataSetQueryCriteriaNot
        {
            Not = new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaNot
                    {
                        Not = new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                Eq = new string('x', 11)
                            },
                        },
                    },
                ],
            }
        };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor("not.and[0].not.filters.eq")
            .Only();
    }
}
