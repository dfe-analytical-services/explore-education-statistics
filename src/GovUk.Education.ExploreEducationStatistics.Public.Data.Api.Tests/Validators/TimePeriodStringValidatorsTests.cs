using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using ValidationErrorMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Validators;

public class TimePeriodStringValidatorsTests
{
   public class TimePeriodStringTests : TimePeriodStringValidatorsTests
    {
        private class TestClass
        {
            public required string TimePeriod { get; init; }
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.TimePeriod).TimePeriodString();
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("2020|AY")]
        [InlineData("2020/2021|AY")]
        [InlineData("2020|FY")]
        [InlineData("2021|M1")]
        [InlineData("2021|W40")]
        [InlineData("2021|T3")]
        [InlineData("2021/2022|T3")]
        [InlineData("2019|FYQ4")]
        [InlineData("2019/2020|FYQ4")]
        public void Success(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Failure_NotEmpty()
        {
            var testObj = new TestClass { TimePeriod = "" };

            _validator.TestValidate(testObj)
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("2022")]
        [InlineData("2022/2023")]
        [InlineData("20222")]
        [InlineData("20222|AY")]
        [InlineData("2022|ay")]
        [InlineData("2022/2023|ay")]
        [InlineData("2022/20233|AY")]
        [InlineData("2022/202|AY")]
        [InlineData("202/2023|AY")]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.TimePeriodFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.TimePeriodFormat.Single, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.FormatErrorDetail>(error.CustomState);

            Assert.Equal([timePeriod], state.Invalid);
        }

        [Theory]
        [InlineData("2022/2020|AY")]
        [InlineData("2000/1999|AY")]
        public void Failure_InvalidRange(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.TimePeriodRange.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.TimePeriodRange.Single, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.RangeErrorDetail>(error.CustomState);

            Assert.Equal([timePeriod], state.Invalid);
        }

        [Theory]
        [InlineData("2022|INVALID")]
        [InlineData("2022|YY")]
        [InlineData("2022/2023|ZZ")]
        [InlineData("2022/2023|AY1")]
        [InlineData("2022|WEEK12")]
        [InlineData("2022|JANUARY")]
        public void Failure_InvalidCode(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.TimePeriodAllowedCode.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.TimePeriodAllowedCode.Single, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.AllowedCodeErrorDetail>(error.CustomState);

            Assert.Equal([timePeriod], state.Invalid);
            Assert.Equal(TimeIdentifierUtils.DataCodes.Order(), state.Allowed);
        }
    }

    public class OnlyTimePeriodStringsTests : TimePeriodStringValidatorsTests
    {
        private class TestClass
        {
            public required IEnumerable<string> TimePeriods { get; init; }
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.TimePeriods).OnlyTimePeriodStrings();
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("2020|AY", "2020/2021|AY", "2020|FY")]
        [InlineData("2021|M1", "2021|W40", "2021|T3")]
        [InlineData("2021/2022|T3", "2019|FYQ4", "2019/2020|FYQ4")]
        public void Success(params string[] timePeriods)
        {
            var testObj = new TestClass { TimePeriods = timePeriods };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("Invalid", "2022", "2022/2023")]
        [InlineData("20222", "20222|AY", "2022|ay")]
        [InlineData("2022/2023|ay", "2022/20233|AY", "2022/202|AY", "202/2023|AY")]
        public void Failure_InvalidFormats(params string[] timePeriods)
        {
            var testObj = new TestClass { TimePeriods = timePeriods };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriods), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.TimePeriodFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.TimePeriodFormat.Plural, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.FormatErrorDetail>(error.CustomState);

            Assert.Equal(timePeriods, state.Invalid);
        }


        [Theory]
        [InlineData("2022/2020|AY")]
        [InlineData("2000/1999|AY")]
        public void Failure_InvalidRanges(params string[] timePeriods)
        {
            var testObj = new TestClass { TimePeriods = timePeriods };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriods), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.TimePeriodRange.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.TimePeriodRange.Plural, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.RangeErrorDetail>(error.CustomState);

            Assert.Equal(timePeriods, state.Invalid);
        }

        [Theory]
        [InlineData("2022|INVALID")]
        [InlineData("2022|YY")]
        [InlineData("2022/2023|ZZ")]
        [InlineData("2022/2023|AY1")]
        [InlineData("2022|WEEK12")]
        [InlineData("2022|JANUARY")]
        public void Failure_InvalidCodes(params string[] timePeriods)
        {
            var testObj = new TestClass { TimePeriods = timePeriods };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriods), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.TimePeriodAllowedCode.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.TimePeriodAllowedCode.Plural, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.AllowedCodeErrorDetail>(error.CustomState);

            Assert.Equal(timePeriods, state.Invalid);
            Assert.Equal(TimeIdentifierUtils.DataCodes.Order(), state.Allowed);
        }
    }
}
