using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
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

        [Theory]
        [InlineData("2020|AY")]
        [InlineData("2020/2021|AY")]
        [InlineData("2020/2021|AYQ1")]
        [InlineData("2020/2021|AYQ3")]
        [InlineData("2020|FY")]
        [InlineData("2021|CY")]
        [InlineData("2021|CYQ1")]
        [InlineData("2021|CYQ2")]
        [InlineData("2021|RY")]
        [InlineData("2021|M1")]
        [InlineData("2021|M12")]
        [InlineData("2021|W20")]
        [InlineData("2021|W40")]
        [InlineData("2021|T1")]
        [InlineData("2021|T1T2")]
        [InlineData("2021|T3")]
        [InlineData("2021/2022|T1")]
        [InlineData("2021/2022|T1T2")]
        [InlineData("2021/2022|T3")]
        [InlineData("2021|P1")]
        [InlineData("2021|P2")]
        [InlineData("2021/2022|P1")]
        [InlineData("2021/2022|P2")]
        [InlineData("2019|FYQ4")]
        [InlineData("2019/2020|FYQ4")]
        public void Success(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Failure_Empty(string? timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod! };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(FluentValidationKeys.NotEmptyValidator, error.ErrorCode);
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("2022")]
        [InlineData("2022/2023")]
        [InlineData("20222")]
        [InlineData("20222|AY")]
        [InlineData("2022 | AY")]
        [InlineData("2022| AY")]
        [InlineData("2022 |AY")]
        [InlineData(" 2022|AY ")]
        [InlineData(" 2022 | AY ")]
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
            Assert.Equal(ValidationMessages.TimePeriodFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.TimePeriodFormat.Message, error.ErrorMessage);

            var state = Assert.IsType<FormatErrorDetail>(error.CustomState);

            Assert.Equal(timePeriod, state.Value);
            Assert.Equal("{period}|{code}", state.ExpectedFormat);
        }

        [Theory]
        [InlineData("2022/2020|AY")]
        [InlineData("2000/1999|AY")]
        [InlineData("2000/2002|AY")]
        public void Failure_InvalidYearRange(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(ValidationMessages.TimePeriodYearRange.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.TimePeriodYearRange.Message, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.RangeErrorDetail>(error.CustomState);

            Assert.Equal(timePeriod, $"{state.Value.Period}|{state.Value.Code}");
        }

        [Theory]
        [InlineData("2022|INVALID")]
        [InlineData("2022|YY")]
        [InlineData("2022|WEEK12")]
        [InlineData("2022|JANUARY")]
        public void Failure_InvalidCode(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(ValidationMessages.TimePeriodAllowedCode.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.TimePeriodAllowedCode.Message, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.AllowedCodeErrorDetail>(error.CustomState);

            Assert.Equal(timePeriod, $"{state.Value.Period}|{state.Value.Code}");
            Assert.Equal(TimeIdentifierUtils.DataCodes.Order(), state.AllowedCodes);
        }

        [Theory]
        [InlineData("2022/2023|INVALID")]
        [InlineData("2022/2023|AY1")]
        [InlineData("2022/2023|ZZ")]
        [InlineData("2022/2023|CY")]
        [InlineData("2022/2023|CYQ1")]
        [InlineData("2022/2023|CYQ3")]
        [InlineData("2022/2023|RY")]
        [InlineData("2022/2023|W12")]
        [InlineData("2022/2023|W40")]
        [InlineData("2022/2023|M1")]
        [InlineData("2022/2023|M12")]
        public void Failure_InvalidCodeForRange(string timePeriod)
        {
            var testObj = new TestClass { TimePeriod = timePeriod };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.TimePeriod), error.PropertyName);
            Assert.Equal(ValidationMessages.TimePeriodAllowedCode.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.TimePeriodAllowedCode.Message, error.ErrorMessage);

            var state = Assert.IsType<TimePeriodStringValidators.AllowedCodeErrorDetail>(error.CustomState);

            Assert.Equal(timePeriod, $"{state.Value.Period}|{state.Value.Code}");

            Assert.All(state.AllowedCodes, code =>
            {
                var yearFormat = EnumUtil
                    .GetFromEnumValue<TimeIdentifier>(code)
                    .GetEnumAttribute<TimeIdentifierMetaAttribute>()
                    .YearFormat;

                Assert.True(yearFormat is TimePeriodYearFormat.Academic or TimePeriodYearFormat.Fiscal);
            });
        }
    }
}
