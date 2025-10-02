using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class SortStringValidators
{
    private const string ExpectedFormat = "{field}|{direction}";

    public static IRuleBuilderOptionsConditions<T, string> SortString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .Custom(
                (value, context) =>
                {
                    if (value.IsNullOrWhitespace())
                    {
                        return;
                    }

                    if (!HasValidFormat(value))
                    {
                        context.AddFailure(
                            message: ValidationMessages.SortFormat,
                            detail: new FormatErrorDetail(value, ExpectedFormat: ExpectedFormat)
                        );

                        return;
                    }

                    var sort = DataSetQuerySort.Parse(value);

                    var validator = new DataSetQuerySort.Validator();
                    var result = validator.Validate(sort);

                    if (result.IsValid)
                    {
                        return;
                    }

                    foreach (var error in result.Errors)
                    {
                        switch (error.PropertyName.ToUpperFirst())
                        {
                            case nameof(DataSetQuerySort.Field)
                                when error.ErrorCode == FluentValidationKeys.NotEmptyValidator:

                                error.ErrorCode = ValidationMessages.SortFieldNotEmpty.Code;
                                error.ErrorMessage = ValidationMessages.SortFieldNotEmpty.Message;

                                break;

                            case nameof(DataSetQuerySort.Field)
                                when error.ErrorCode == FluentValidationKeys.MaximumLengthValidator:

                                error.ErrorCode = ValidationMessages.SortFieldMaxLength.Code;
                                error.ErrorMessage = context
                                    .MessageFormatter.AppendArgument(
                                        "MaxLength",
                                        error.FormattedMessagePlaceholderValues["MaxLength"]
                                    )
                                    .BuildMessage(ValidationMessages.SortFieldMaxLength.Message);

                                break;

                            case nameof(DataSetQuerySort.Direction)
                                when error.ErrorCode == Common.Validators.ValidationMessages.AllowedValue.Code:

                                error.ErrorCode = ValidationMessages.SortDirection.Code;
                                error.ErrorMessage = ValidationMessages.SortDirection.Message;

                                break;
                        }

                        error.FormattedMessagePlaceholderValues["Property"] = error.PropertyName.ToLowerFirst();
                        error.PropertyName = context.PropertyPath;

                        context.AddFailure(error);
                    }
                }
            );
    }

    // Note this regex only does basic checks on delimiters to allow other
    // validations to provide more specific error messages on what is invalid.
    [GeneratedRegex(@"^[^\|]*(\|[^\|]+)?\|[^\|]*$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);
}
