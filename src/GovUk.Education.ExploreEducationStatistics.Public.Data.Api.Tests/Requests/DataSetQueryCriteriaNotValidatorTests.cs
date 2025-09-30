using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryCriteriaNotValidatorTests
{
    private readonly DataSetQueryCriteriaNot.Validator _validator = new();

    [Fact]
    public void Null_Failure()
    {
        var query = new DataSetQueryCriteriaNot { Not = null! };

        _validator
            .TestValidate(query)
            .ShouldHaveValidationErrorFor("Not")
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
                Filters = new DataSetQueryCriteriaFilters { Eq = "12345" },
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
                Filters = new DataSetQueryCriteriaFilters { Eq = "" },
            },
        };

        _validator.TestValidate(query).ShouldHaveValidationErrorFor("Not.Filters.Eq").Only();
    }

    [Fact]
    public void Condition_Empty_Failure()
    {
        var query = new DataSetQueryCriteriaNot { Not = new DataSetQueryCriteriaOr { Or = [] } };

        _validator.TestValidate(query).ShouldHaveValidationErrorFor("Not.Or").Only();
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
                        Filters = new DataSetQueryCriteriaFilters { Eq = "12345" },
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
                        Filters = new DataSetQueryCriteriaFilters { Eq = new string('x', 11) },
                    },
                ],
            },
        };

        _validator.TestValidate(query).ShouldHaveValidationErrorFor("Not.Or[0].Filters.Eq").Only();
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
                        Filters = new DataSetQueryCriteriaFilters { Eq = "12345" },
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Gte = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
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
                        Filters = new DataSetQueryCriteriaFilters { Eq = new string('x', 11) },
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Gte = new DataSetQueryTimePeriod { Period = "", Code = "AY" },
                        },
                    },
                ],
            },
        };

        var result = _validator.TestValidate(query);

        Assert.Equal(2, result.Errors.Count);

        result.ShouldHaveValidationErrorFor("Not.And[0].Filters.Eq");
        result.ShouldHaveValidationErrorFor("Not.And[1].TimePeriods.Gte.Period");
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
                    Filters = new DataSetQueryCriteriaFilters { Eq = "12345" },
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
                    Filters = new DataSetQueryCriteriaFilters { Eq = new string('x', 11) },
                },
            },
        };

        _validator.TestValidate(query).ShouldHaveValidationErrorFor("Not.Not.Filters.Eq").Only();
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
                            Filters = new DataSetQueryCriteriaFilters { Eq = "12345" },
                        },
                    },
                ],
            },
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
                            Filters = new DataSetQueryCriteriaFilters { Eq = new string('x', 11) },
                        },
                    },
                ],
            },
        };

        _validator
            .TestValidate(query)
            .ShouldHaveValidationErrorFor("Not.And[0].Not.Filters.Eq")
            .Only();
    }
}
