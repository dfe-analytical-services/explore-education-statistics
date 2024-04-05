using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class SortStringValidators
{
    private const string ExpectedFormat = "{field}|{order}";
    private const int MaxFieldLength = 40;

    private static readonly HashSet<string> AllowedDirections = [
        SortDirection.Asc.ToString(),
        SortDirection.Desc.ToString()
    ];

    public static IRuleBuilderOptionsConditions<T, string> SortString<T>(this IRuleBuilder<T, string> rule)
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
                    message: ValidationMessages.SortFormat,
                    detail: new FormatErrorDetail(value, ExpectedFormat: ExpectedFormat)
                );

                return;
            }

            var sort = ParsedSort.Parse(value);

            if (!HasValidFieldLength(sort))
            {
                context.AddFailure(
                    message: ValidationMessages.SortMaxFieldLength,
                    detail: new MaxFieldLengthErrorDetail(sort, MaxFieldLength),
                    new Dictionary<string, object>
                    {
                        { "MaxFieldLength", MaxFieldLength }
                    }
                );
            }

            if (!HasValidDirection(sort))
            {
                context.AddFailure(
                    message: ValidationMessages.SortDirection,
                    detail: new DirectionErrorDetail(sort, AllowedDirections)
                );
            }
        });
    }

    [GeneratedRegex(@"^[\w_]+(\|\w+)?\|[A-Za-z]+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);

    private static bool HasValidFieldLength(ParsedSort sort) => sort.Field.Length < MaxFieldLength;

    private static bool HasValidDirection(ParsedSort sort) => AllowedDirections.Contains(sort.Direction);

    public record DirectionErrorDetail(ParsedSort Value, IEnumerable<string> AllowedDirections)
        : InvalidErrorDetail<ParsedSort>(Value);

    public record MaxFieldLengthErrorDetail(ParsedSort Value, int MaxFieldLength)
        : InvalidErrorDetail<ParsedSort>(Value);

    public record ParsedSort
    {
        public string Field { get; init; }

        public string Direction { get; init; }

        public static ParsedSort Parse(string value)
        {
            var directionDelimiter = value.LastIndexOf('|');

            return new ParsedSort
            {
                Field = value[..directionDelimiter],
                Direction = value[(directionDelimiter + 1)..],
            };
        }
    }
}
