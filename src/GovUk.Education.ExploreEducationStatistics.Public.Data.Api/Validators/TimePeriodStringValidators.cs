using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class TimePeriodStringValidators
{
    private const string ExpectedFormat = "{period}|{code}";

    public static IRuleBuilderOptionsConditions<T, string> TimePeriodString<T>(
        this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().Custom((value, context) =>
        {
            if (value.IsNullOrWhitespace())
            {
                return;
            }

            if (!HasValidFormat(value))
            {
                context.AddFailure(
                    message: ValidationMessages.TimePeriodFormat,
                    detail: new FormatErrorDetail(Value: value, ExpectedFormat: ExpectedFormat)
                );
                return;
            }

            var validator = new DataSetQueryTimePeriod.Validator();
            var result = validator.Validate(DataSetQueryTimePeriod.Parse(value));

            if (result.IsValid)
            {
                return;
            }

            foreach (var error in result.Errors)
            {
                error.FormattedMessagePlaceholderValues["Property"] = error.PropertyName.ToLowerFirst();
                error.PropertyName = context.PropertyPath;

                context.AddFailure(error);
            }
        });
    }

    // Note this regex only does basic checks on the delimiter to allow
    // other validations to provide more detailed error messages.
    [GeneratedRegex(@"^[^\|]*\|[^\|]*$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);
}
