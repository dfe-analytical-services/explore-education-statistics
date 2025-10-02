using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryCriteriaFiltersValidatorTests
{
    private readonly DataSetQueryCriteriaFilters.Validator _validator = new();

    public static readonly TheoryData<string?> ValidFiltersSingle = new()
    {
        null,
        "abc",
        "12345",
        "123456789",
        "1234567890",
    };

    public static readonly TheoryData<string[]> ValidFiltersMultiple = new()
    {
        new[] { "abc", "12345", "123456789" },
        new[] { "1234567890", "123", "abcde" },
    };

    public class EqTests : DataSetQueryCriteriaFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersSingle))]
        public void Success(string? filter)
        {
            var query = new DataSetQueryCriteriaFilters { Eq = filter };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryCriteriaFilters { Eq = "" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetQueryCriteriaFilters { Eq = "12345678901" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class NotEqTests : DataSetQueryCriteriaFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersSingle))]
        public void Success(string? filter)
        {
            var query = new DataSetQueryCriteriaFilters { NotEq = filter };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetQueryCriteriaFilters { NotEq = "" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetQueryCriteriaFilters { NotEq = "12345678901" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class InTests : DataSetQueryCriteriaFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersMultiple))]
        public void Success(params string[] filters)
        {
            var query = new DataSetQueryCriteriaFilters { In = filters };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetQueryCriteriaFilters { In = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_EmptyValues()
        {
            var query = new DataSetQueryCriteriaFilters { In = ["", " ", null!] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.In.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("In[0]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("In[1]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("In[2]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLengths()
        {
            var query = new DataSetQueryCriteriaFilters { In = ["12345678901", "999999999999999"] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.In.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("In[0]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
            result.ShouldHaveValidationErrorFor("In[1]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class NotInTests : DataSetQueryCriteriaFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersMultiple))]
        public void Success(params string[] filters)
        {
            var query = new DataSetQueryCriteriaFilters { NotIn = filters };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetQueryCriteriaFilters { NotIn = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_EmptyValues()
        {
            var query = new DataSetQueryCriteriaFilters { NotIn = ["", " ", null!] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.NotIn.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("NotIn[0]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("NotIn[1]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("NotIn[2]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLengths()
        {
            var query = new DataSetQueryCriteriaFilters { NotIn = ["12345678901", "999999999999999"] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.NotIn.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("NotIn[0]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
            result.ShouldHaveValidationErrorFor("NotIn[1]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class EmptyTests : DataSetQueryCriteriaFiltersValidatorTests
    {
        [Fact]
        public void AllEmpty_Failure()
        {
            var query = new DataSetQueryCriteriaFilters
            {
                Eq = "",
                NotEq = "",
                In = [],
                NotIn = [],
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(4, result.Errors.Count);

            result.ShouldHaveValidationErrorFor(q => q.Eq).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotEq).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.In).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void SomeEmpty_Failure()
        {
            var query = new DataSetQueryCriteriaFilters
            {
                Eq = "123",
                NotEq = "",
                In = ["456"],
                NotIn = [],
            };

            var result = _validator.TestValidate(query);

            Assert.Equal(2, result.Errors.Count);

            result.ShouldNotHaveValidationErrorFor(q => q.Eq);
            result.ShouldNotHaveValidationErrorFor(q => q.In);

            result.ShouldHaveValidationErrorFor(q => q.NotEq).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor(q => q.NotIn).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }
    }
}
