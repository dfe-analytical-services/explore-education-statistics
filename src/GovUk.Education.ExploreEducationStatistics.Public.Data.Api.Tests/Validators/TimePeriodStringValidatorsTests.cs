using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

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

        public static readonly TheoryData<string, string> ValidTimePeriods =
            DataSetQueryTimePeriodValidatorTests.ValidTimePeriods;

        [Theory]
        [MemberData(nameof(ValidTimePeriods))]
        public void Success(string period, string code)
        {
            var testObj = new TestClass { TimePeriod = $"{period}|{code}" };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Failure_Empty(string? timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod! };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("2022")]
        [InlineData("20222")]
        [InlineData("2022/2023")]
        [InlineData("2022|AY|blah")]
        [InlineData("2022|AY|")]
        [InlineData("|2022|AY")]
        [InlineData("|2022|AY|")]
        public void Failure_InvalidFormat(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(ValidationMessages.TimePeriodFormat.Code)
                .WithErrorMessage(ValidationMessages.TimePeriodFormat.Message)
                .WithCustomState<FormatErrorDetail>(d => d.Value == timePeriod)
                .WithCustomState<FormatErrorDetail>(d => d.ExpectedFormat == "{period}|{code}")
                .Only();
        }

        public static readonly TheoryData<string, string> InvalidTimePeriodYears =
            DataSetQueryTimePeriodValidatorTests.InvalidTimePeriodYears;

        [Theory]
        [MemberData(nameof(InvalidTimePeriodYears))]
        public void Failure_InvalidYear(string period, string code)
        {
            var testObj = new TestClass { TimePeriod = $"{period}|{code}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(ValidationMessages.TimePeriodInvalidYear.Code)
                .WithErrorMessage(ValidationMessages.TimePeriodInvalidYear.Message)
                .WithMessageArgument("Property", "period")
                .WithCustomState<InvalidErrorDetail<string>>(d => d.Value == period)
                .Only();
        }

        public static readonly TheoryData<string, string> InvalidTimePeriodYearRanges =
            DataSetQueryTimePeriodValidatorTests.InvalidTimePeriodYearRanges;

        [Theory]
        [MemberData(nameof(InvalidTimePeriodYearRanges))]
        public void Failure_InvalidYearRange(string period, string code)
        {
            var testObj = new TestClass { TimePeriod = $"{period}|{code}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(ValidationMessages.TimePeriodInvalidYearRange.Code)
                .WithErrorMessage(ValidationMessages.TimePeriodInvalidYearRange.Message)
                .WithMessageArgument("Property", "period")
                .WithCustomState<InvalidErrorDetail<string>>(d => d.Value == period)
                .Only();
        }

        public static readonly TheoryData<string, string> InvalidTimePeriodCodes =
            DataSetQueryTimePeriodValidatorTests.InvalidTimePeriodCodes;

        [Theory]
        [MemberData(nameof(InvalidTimePeriodCodes))]
        public void Failure_InvalidCode(string period, string code)
        {
            var testObj = new TestClass { TimePeriod = $"{period}|{code}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(ValidationMessages.TimePeriodAllowedCode.Code)
                .WithErrorMessage(ValidationMessages.TimePeriodAllowedCode.Message)
                .WithMessageArgument("Property", "code")
                .WithCustomState<TimePeriodAllowedCodeErrorDetail>(d => d.Value == code)
                .WithCustomState<TimePeriodAllowedCodeErrorDetail>(d =>
                    d.AllowedCodes.SequenceEqual(TimeIdentifierUtils.Codes.Order()))
                .Only();
        }

        public static readonly TheoryData<string, string> InvalidTimePeriodRangeCodes =
            DataSetQueryTimePeriodValidatorTests.InvalidTimePeriodRangeCodes;

        [Theory]
        [MemberData(nameof(InvalidTimePeriodRangeCodes))]
        public void Failure_InvalidCodeForRange(string period, string code)
        {
            var testObj = new TestClass { TimePeriod = $"{period}|{code}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.TimePeriod)
                .WithErrorCode(ValidationMessages.TimePeriodAllowedCode.Code)
                .WithErrorMessage(ValidationMessages.TimePeriodAllowedCode.Message)
                .WithMessageArgument("Property", "code")
                .WithCustomState<TimePeriodAllowedCodeErrorDetail>(d => d.Value == code)
                .WithCustomState<TimePeriodAllowedCodeErrorDetail>(d => d.AllowedCodes.All(allowedCode =>
                {
                    var yearFormat = EnumUtil
                        .GetFromEnumValue<TimeIdentifier>(allowedCode)
                        .GetEnumAttribute<TimeIdentifierMetaAttribute>()
                        .YearFormat;

                    var isValidYearFormat = yearFormat is TimePeriodYearFormat.Academic or TimePeriodYearFormat.Fiscal;

                    Assert.True(isValidYearFormat);

                    return isValidYearFormat;
                }))
                .Only();
        }
    }
}
