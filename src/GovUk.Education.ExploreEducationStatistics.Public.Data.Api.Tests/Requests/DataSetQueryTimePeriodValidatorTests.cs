using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryTimePeriodValidatorTests
{
    private readonly DataSetQueryTimePeriod.Validator _validator = new();

    public static TheoryData<string, string> ValidTimePeriods =>
        new()
        {
            { "2020", "AY" },
            { "2020/2021", "AY" },
            { "2020/2021", "AYQ1" },
            { "2020/2021", "AYQ3" },
            { "2020", "FY" },
            { "2021", "CY" },
            { "2021", "CYQ1" },
            { "2021", "CYQ2" },
            { "2021", "RY" },
            { "2021", "M1" },
            { "2021", "M12" },
            { "2021", "W20" },
            { "2021", "W40" },
            { "2021", "T1" },
            { "2021", "T1T2" },
            { "2021", "T3" },
            { "2021/2022", "T1" },
            { "2021/2022", "T1T2" },
            { "2021/2022", "T3" },
            { "2021", "P1" },
            { "2021", "P2" },
            { "2021/2022", "P1" },
            { "2021/2022", "P2" },
            { "2019", "FYQ4" },
            { "2019/2020", "FYQ4" },
        };

    [Theory]
    [MemberData(nameof(ValidTimePeriods))]
    public void Success(string period, string code)
    {
        var timePeriod = new DataSetQueryTimePeriod { Code = code, Period = period };

        _validator.TestValidate(timePeriod).ShouldNotHaveAnyValidationErrors();
    }

    public static TheoryData<string, string> InvalidTimePeriodYears =>
        new()
        {
            { "Invalid", "CY" },
            { "2022-", "CY" },
            { "-2022", "CY" },
            { "2022Invalid", "CY" },
            { "2022 ", "AY" },
            { " 2022", "AY" },
            { " 2022 ", "AY" },
        };

    [Theory]
    [MemberData(nameof(InvalidTimePeriodYears))]
    public void Failure_InvalidYear(string period, string code)
    {
        var timePeriod = new DataSetQueryTimePeriod { Code = code, Period = period };

        var result = _validator.TestValidate(timePeriod);

        Assert.Single(result.Errors);

        result
            .ShouldHaveValidationErrorFor(t => t.Period)
            .WithErrorCode(ValidationMessages.TimePeriodInvalidYear.Code)
            .WithErrorMessage(ValidationMessages.TimePeriodInvalidYear.Message)
            .WithCustomState<InvalidErrorDetail<string>>(s => s.Value == period)
            .Only();
    }

    public static TheoryData<string, string> InvalidTimePeriodYearRanges =>
        new()
        {
            { "Invalid/2024", "AY" },
            { "2023/Invalid", "AY" },
            { "2023Invalid/2024", "AY" },
            { "2023/2024Invalid", "AY" },
            { "2022/2020", "AY" },
            { "2000/1999", "AY" },
            { "2000/2002", "AY" },
            { "2022-/2023", "FYQ2" },
            { "-2022/2023", "FYQ2" },
            { "2022/2023-", "FYQ2" },
            { "2022/-2023", "FYQ2" },
            { "-2022/-2023", "FYQ2" },
            { "2022 /2023", "FYQ2" },
            { " 2022/2023", "FYQ2" },
            { " 2022 /2023", "FYQ2" },
            { "2022/2023 ", "FYQ2" },
            { "2022/ 2023 ", "FYQ2" },
            { "2022/ 2023", "FYQ2" },
        };

    [Theory]
    [MemberData(nameof(InvalidTimePeriodYearRanges))]
    public void Failure_InvalidYearRange(string period, string code)
    {
        var timePeriod = new DataSetQueryTimePeriod { Code = code, Period = period };

        var result = _validator.TestValidate(timePeriod);

        Assert.Single(result.Errors);

        result
            .ShouldHaveValidationErrorFor(t => t.Period)
            .WithErrorCode(ValidationMessages.TimePeriodInvalidYearRange.Code)
            .WithErrorMessage(ValidationMessages.TimePeriodInvalidYearRange.Message)
            .WithCustomState<InvalidErrorDetail<string>>(s => s.Value == period)
            .Only();
    }

    public static TheoryData<string, string> InvalidTimePeriodCodes =>
        new()
        {
            { "2022", "INVALID" },
            { "2022", "YY" },
            { "2022", "WEEK12" },
            { "2022", "JANUARY" },
        };

    [Theory]
    [MemberData(nameof(InvalidTimePeriodCodes))]
    public void Failure_InvalidCode(string period, string code)
    {
        var timePeriod = new DataSetQueryTimePeriod { Code = code, Period = period };

        var result = _validator.TestValidate(timePeriod);

        result
            .ShouldHaveValidationErrorFor(t => t.Code)
            .WithErrorCode(ValidationMessages.TimePeriodAllowedCode.Code)
            .WithErrorMessage(ValidationMessages.TimePeriodAllowedCode.Message)
            .WithCustomState<TimePeriodAllowedCodeErrorDetail>(s => s.Value == code)
            .WithCustomState<TimePeriodAllowedCodeErrorDetail>(s =>
                s.AllowedCodes.SequenceEqual(TimeIdentifierUtils.Codes.Order())
            )
            .Only();
    }

    public static TheoryData<string, string> InvalidTimePeriodRangeCodes =>
        new()
        {
            { "2022/2023", "INVALID" },
            { "2022/2023", "AY1" },
            { "2022/2023", "ZZ" },
            { "2022/2023", "CY" },
            { "2022/2023", "CYQ1" },
            { "2022/2023", "CYQ3" },
            { "2022/2023", "RY" },
            { "2022/2023", "W12" },
            { "2022/2023", "W40" },
            { "2022/2023", "M1" },
            { "2022/2023", "M12" },
        };

    [Theory]
    [MemberData(nameof(InvalidTimePeriodRangeCodes))]
    public void Failure_InvalidCodeForRange(string period, string code)
    {
        var timePeriod = new DataSetQueryTimePeriod { Code = code, Period = period };

        var result = _validator.TestValidate(timePeriod);

        result
            .ShouldHaveValidationErrorFor(t => t.Code)
            .WithErrorCode(ValidationMessages.TimePeriodAllowedCode.Code)
            .WithErrorMessage(ValidationMessages.TimePeriodAllowedCode.Message)
            .WithCustomState<TimePeriodAllowedCodeErrorDetail>(d => d.Value == code)
            .WithCustomState<TimePeriodAllowedCodeErrorDetail>(d =>
                d.AllowedCodes.All(allowedCode =>
                {
                    var yearFormat = EnumUtil
                        .GetFromEnumValue<TimeIdentifier>(allowedCode)
                        .GetEnumAttribute<TimeIdentifierMetaAttribute>()
                        .YearFormat;

                    var isValidYearFormat =
                        yearFormat is TimePeriodYearFormat.Academic or TimePeriodYearFormat.Fiscal;

                    Assert.True(isValidYearFormat);

                    return isValidYearFormat;
                })
            )
            .Only();
    }
}
