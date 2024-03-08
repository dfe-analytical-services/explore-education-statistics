using System.Text.RegularExpressions;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static partial class SortStringValidators
{
    private const string ExpectedFormat = "{field}|{order}";
    private const int MaxFieldLength = 40;

    private static readonly HashSet<string> AllowedSorts = [
        SortDirection.Asc.ToString(),
        SortDirection.Desc.ToString()
    ];

    public static IRuleBuilderOptionsConditions<T, string> SortString<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.Custom((value, context) =>
        {
            if (!HasValidFormat(value))
            {
                context.AddFailure(
                    message: ValidationMessages.SortFormat,
                    detail: new FormatErrorDetail(value, ExpectedFormat: ExpectedFormat)
                );

                return;
            }

            var sort = new ParsedSort(value);

            if (!HasValidFieldLength(sort))
            {
                context.AddFailure(
                    message: ValidationMessages.SortMaxFieldLength,
                    detail: new MaxFieldLengthErrorDetail(sort.String),
                    new Dictionary<string, object>
                    {
                        { "MaxLength", MaxFieldLength }
                    }
                );
            }

            if (!HasValidDirection(sort))
            {
                context.AddFailure(
                    message: ValidationMessages.SortDirection,
                    detail: new DirectionErrorDetail(sort.String)
                );
            }
        });
    }

    [GeneratedRegex(@"^[\w_]+\|[A-Za-z]+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 200)]
    private static partial Regex FormatRegexGenerated();

    private static readonly Regex FormatRegex = FormatRegexGenerated();

    private static bool HasValidFormat(string value) => FormatRegex.IsMatch(value);

    private static bool HasValidFieldLength(ParsedSort sort) => sort.Field.Length < MaxFieldLength;

    private static bool HasValidDirection(ParsedSort sort) => AllowedSorts.Contains(sort.Direction);

    public record DirectionErrorDetail(string Value) : InvalidErrorDetail<string>(Value)
    {
        public IReadOnlySet<string> Allowed => AllowedSorts;
    }

    public record MaxFieldLengthErrorDetail(string Value) : InvalidErrorDetail<string>(Value)
    {
        public int MaxLength => MaxFieldLength;
    }

    private record ParsedSort
    {
        public string String { get; init; }
        
        public string Field { get; init; }
        
        public string Direction { get; init; }

        public ParsedSort(string value)
        {
            var parts = value.Split('|');

            String = value;
            Field = parts[0];
            Direction = parts[1];
        }
    }
}
