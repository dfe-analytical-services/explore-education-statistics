using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

[Collection("Set FluentValidation property name camel case configuration")]
public class DataSetQueryRequestValidatorTests
{
    private readonly DataSetQueryRequest.Validator _validator = new();

    public class CriteriaTests : DataSetQueryRequestValidatorTests
    {
        [Fact]
        public void Facets_Success()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetGetQueryFilters
                    {
                        Eq = "abc"
                    },
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Facets_Failure()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetGetQueryFilters
                    {
                        Eq = ""
                    },
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor("criteria.filters.eq")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Fact]
        public void AndCondition_Success()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetGetQueryFilters
                            {
                                Eq = "abc"
                            },
                        }
                    ]
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void AndCondition_Failure()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaAnd
                {
                    And =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetGetQueryFilters
                            {
                                Eq = ""
                            },
                        }
                    ]
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor("criteria.and[0].filters.eq")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Fact]
        public void OrCondition_Success()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetGetQueryFilters
                            {
                                Eq = "abc"
                            },
                        }
                    ]
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void OrCondition_Failure()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetGetQueryFilters
                            {
                                Eq = ""
                            },
                        }
                    ]
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor("criteria.or[0].filters.eq")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Fact]
        public void NotCondition_Success()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaNot
                {
                    Not = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetGetQueryFilters
                        {
                            Eq = "abc"
                        },
                    }
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void NotCondition_Failure()
        {
            var query = new DataSetQueryRequest
            {
                Criteria = new DataSetQueryCriteriaNot
                {
                    Not = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetGetQueryFilters
                        {
                            Eq = ""
                        },
                    }
                },
                Indicators = ["indicator1", "indicator2"],
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor("criteria.not.filters.eq")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }
    }

    public class IndicatorsTests : DataSetQueryRequestValidatorTests
    {
        [Theory]
        [InlineData("indicator1")]
        [InlineData("indicator1111111111111111111111111111111")]
        [InlineData("indicator1", "indicator2")]
        public void Success(params string[] indicators)
        {
            var query = new DataSetQueryRequest
            {
                Indicators = indicators,
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = []
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Indicators)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_EmptyValues()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["", " ", "  ", null!]
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.Indicators.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("indicators[0]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result.ShouldHaveValidationErrorFor("indicators[1]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result.ShouldHaveValidationErrorFor("indicators[2]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result.ShouldHaveValidationErrorFor("indicators[3]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = [new string('x', 41), new string('x', 42)]
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.Indicators.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("indicators[0]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result.ShouldHaveValidationErrorFor("indicators[1]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }

        [Fact]
        public void Failure_Mixture()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = [new string('x', 101), ""]
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.Indicators.Count, result.Errors.Count);

            result
                .ShouldHaveValidationErrorFor("indicators[0]")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);

            result
                .ShouldHaveValidationErrorFor("indicators[1]")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }

    public class SortsTests : DataSetQueryRequestValidatorTests
    {
        [Fact]
        public void Success()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Sorts =
                [
                    new DataSetQuerySort { Field = "TimePeriod", Direction = "Asc" },
                    new DataSetQuerySort { Field = "GeographicLevel", Direction = "Asc" },
                ],
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Sorts = []
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Sorts)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Fact]
        public void Failure_InvalidSorts()
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Sorts =
                [
                    null!,
                    new DataSetQuerySort { Field = "", Direction = "Asc" },
                    new DataSetQuerySort { Field = "timePeriod", Direction = "" },
                    new DataSetQuerySort { Field = "timePeriod", Direction = "asc" },
                    new DataSetQuerySort { Field = new string('x', 41), Direction = "Asc" },
                ],
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.Sorts.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("sorts[0]")
                .WithErrorCode(FluentValidationKeys.NotNullValidator);

            result.ShouldHaveValidationErrorFor("sorts[1].field")
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);

            result.ShouldHaveValidationErrorFor("sorts[2].direction")
                .WithErrorCode(ValidationMessages.AllowedValue.Code);

            result.ShouldHaveValidationErrorFor("sorts[3].direction")
                .WithErrorCode(ValidationMessages.AllowedValue.Code);

            result.ShouldHaveValidationErrorFor("sorts[4].field")
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class PageTests : DataSetQueryRequestValidatorTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void Success(int page)
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Page = page,
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(-10)]
        public void Failure_LessThanOne(int page)
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                Page = page,
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Page)
                .WithErrorCode(FluentValidationKeys.GreaterThanOrEqualValidator)
                .Only();
        }
    }

    public class PageSizeTests : DataSetQueryRequestValidatorTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void Success(int pageSize)
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                PageSize = pageSize,
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(10001)]
        public void Failure_OutOfBounds(int pageSize)
        {
            var query = new DataSetQueryRequest
            {
                Indicators = ["indicator1", "indicator2"],
                PageSize = pageSize,
            };

            _validator.TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.PageSize)
                .WithErrorCode(FluentValidationKeys.InclusiveBetweenValidator)
                .Only();
        }
    }
}
