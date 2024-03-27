using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Validators;

public class SortStringValidatorsTests
{
     public class SortStringTests : SortStringValidatorsTests
    {
        private class TestClass
        {
            public required string Sort { get; init; }
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Sort).SortString();
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("timePeriod|Asc")]
        [InlineData("timePeriod|Desc")]
        [InlineData("geographicLevel|Asc")]
        [InlineData("geographicLevel|Desc")]
        [InlineData("locations|REG|Desc")]
        [InlineData("locations|LA|Desc")]
        [InlineData("characteristic|Asc")]
        [InlineData("characteristic|Desc")]
        [InlineData("school_type|Asc")]
        [InlineData("school_type|Desc")]
        [InlineData("ssa_tier_1|Asc")]
        [InlineData("ssa_tier_2|Desc")]
        public void Success(string sort)
        {
            var testObj = new TestClass { Sort = sort };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Invalid")]
        [InlineData("Asc")]
        [InlineData("|Asc")]
        [InlineData(" |Asc")]
        [InlineData("timePeriod:Asc")]
        [InlineData("timePeriod|")]
        [InlineData(" timePeriod|Asc")]
        [InlineData("timePeriod| ")]
        [InlineData("timePeriod| Asc")]
        [InlineData(" timePeriod|Asc ")]
        [InlineData("timePeriod |Asc ")]
        [InlineData("timePeriod|Asc1")]
        [InlineData("locations||Asc")]
        [InlineData("locations| |Asc")]
        [InlineData("locations| |")]
        [InlineData("school-type|Asc")]
        [InlineData("school_type| Asc")]
        [InlineData("school_type|Asc ")]
        public void Failure_InvalidFormat(string sort)
        {
            var testObj = new TestClass { Sort = sort };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Sort), error.PropertyName);
            Assert.Equal(ValidationMessages.SortFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.SortFormat.Message, error.ErrorMessage);

            var state = Assert.IsType<FormatErrorDetail>(error.CustomState);

            Assert.Equal(sort, state.Value);
        }

        [Theory]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa|Asc")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa|Desc")]
        public void Failure_InvalidFieldLength(string sort)
        {
            var testObj = new TestClass { Sort = sort };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Sort), error.PropertyName);
            Assert.Equal(ValidationMessages.SortMaxFieldLength.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.SortMaxFieldLength.Message, error.ErrorMessage);

            var state = Assert.IsType<SortStringValidators.MaxFieldLengthErrorDetail>(error.CustomState);

            Assert.Equal(sort, $"{state.Value.Field}|{state.Value.Direction}");
            Assert.Equal(40, state.MaxFieldLength);
        }

        [Theory]
        [InlineData("timePeriod|Invalid")]
        [InlineData("timePeriod|asc")]
        [InlineData("timePeriod|desc")]
        public void Failure_InvalidDirection(string sort)
        {
            var testObj = new TestClass { Sort = sort };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Sort), error.PropertyName);
            Assert.Equal(ValidationMessages.SortDirection.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.SortDirection.Message, error.ErrorMessage);

            var state = Assert.IsType<SortStringValidators.DirectionErrorDetail>(error.CustomState);

            Assert.Equal(sort, $"{state.Value.Field}|{state.Value.Direction}");
            Assert.Equal([SortDirection.Asc.ToString(), SortDirection.Desc.ToString()], state.AllowedDirections);
        }
    }
}
