using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetGetQueryFiltersValidatorTests
{
    private readonly DataSetGetQueryFilters.Validator _validator = new();

    public static readonly TheoryData<string?> ValidFiltersSingle =
        DataSetQueryCriteriaFiltersValidatorTests.ValidFiltersSingle;

    public static readonly TheoryData<string[]> ValidFiltersMultiple =
        DataSetQueryCriteriaFiltersValidatorTests.ValidFiltersMultiple;

    public class EqTests : DataSetGetQueryFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersSingle))]
        public void Success(string? filter)
        {
            var query = new DataSetGetQueryFilters { Eq = filter };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryFilters { Eq = "" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetGetQueryFilters { Eq = "12345678901" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.Eq)
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator)
                .Only();
        }
    }

    public class NotEqTests : DataSetGetQueryFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersSingle))]
        public void Success(string? filter)
        {
            var query = new DataSetGetQueryFilters { NotEq = filter };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryFilters { NotEq = "" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Fact]
        public void Failure_MaxLength()
        {
            var query = new DataSetGetQueryFilters { NotEq = "12345678901" };

            _validator
                .TestValidate(query)
                .ShouldHaveValidationErrorFor(q => q.NotEq)
                .WithErrorCode(FluentValidationKeys.MaximumLengthValidator)
                .Only();
        }
    }

    public class InTests : DataSetGetQueryFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersMultiple))]
        public void Success(params string[] filters)
        {
            var query = new DataSetGetQueryFilters { In = filters };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetGetQueryFilters { In = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryFilters { In = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.In).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_EmptyValues()
        {
            var query = new DataSetGetQueryFilters { In = ["", " ", null!] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.In.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("In[0]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("In[1]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("In[2]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLengths()
        {
            var query = new DataSetGetQueryFilters { In = ["12345678901", "999999999999999"] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.In.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("In[0]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
            result.ShouldHaveValidationErrorFor("In[1]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class NotInTests : DataSetGetQueryFiltersValidatorTests
    {
        [Theory]
        [MemberData(nameof(ValidFiltersMultiple))]
        public void Success(params string[] filters)
        {
            var query = new DataSetGetQueryFilters { NotIn = filters };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Success_Null()
        {
            var query = new DataSetGetQueryFilters { NotIn = null };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_Empty()
        {
            var query = new DataSetGetQueryFilters { NotIn = [] };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(q => q.NotIn).WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_EmptyValues()
        {
            var query = new DataSetGetQueryFilters { NotIn = ["", " ", null!] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.NotIn.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("NotIn[0]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("NotIn[1]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
            result.ShouldHaveValidationErrorFor("NotIn[2]").WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Fact]
        public void Failure_MaxLengths()
        {
            var query = new DataSetGetQueryFilters { NotIn = ["12345678901", "999999999999999"] };

            var result = _validator.TestValidate(query);

            Assert.Equal(query.NotIn.Count, result.Errors.Count);

            result.ShouldHaveValidationErrorFor("NotIn[0]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
            result.ShouldHaveValidationErrorFor("NotIn[1]").WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
        }
    }

    public class EmptyTests : DataSetGetQueryFiltersValidatorTests
    {
        [Fact]
        public void AllEmpty_Failure()
        {
            var query = new DataSetGetQueryFilters
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
            var query = new DataSetGetQueryFilters
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
