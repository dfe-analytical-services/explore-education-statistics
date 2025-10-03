#nullable enable
using FluentValidation.Resources;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Common.Config;

public class FluentValidationLanguageManager : LanguageManager
{
    public FluentValidationLanguageManager()
    {
        foreach (var translation in Translations)
        {
            AddTranslation("en", translation.Key, translation.Message);
            AddTranslation("en-GB", translation.Key, translation.Message);
            AddTranslation("en-US", translation.Key, translation.Message);
        }
    }

    private static IEnumerable<Translation> Translations =>
        new List<Translation>
        {
            new(Key: FluentValidationKeys.EmailValidator, Message: "Must be a valid email address."),
            new(
                Key: FluentValidationKeys.GreaterThanOrEqualValidator,
                Message: "Must be greater than or equal to '{ComparisonValue}'."
            ),
            new(Key: FluentValidationKeys.GreaterThanValidator, Message: "Must be greater than '{ComparisonValue}'."),
            new(
                Key: FluentValidationKeys.LengthValidator,
                Message: "Must be between {MinLength} and {MaxLength} characters (was {TotalLength})."
            ),
            new(
                Key: FluentValidationKeys.MinimumLengthValidator,
                Message: "Must be at least {MinLength} characters (was {TotalLength})."
            ),
            new(
                Key: FluentValidationKeys.MaximumLengthValidator,
                Message: "Must be {MaxLength} characters or fewer (was {TotalLength})."
            ),
            new(
                Key: FluentValidationKeys.LessThanOrEqualValidator,
                Message: "Must be less than or equal to '{ComparisonValue}'."
            ),
            new(Key: FluentValidationKeys.LessThanValidator, Message: "Must be less than '{ComparisonValue}'."),
            new(Key: FluentValidationKeys.NotEmptyValidator, Message: "Must not be empty."),
            new(Key: FluentValidationKeys.NotEqualValidator, Message: "Must not be equal to '{ComparisonValue}'."),
            new(Key: FluentValidationKeys.NotNullValidator, Message: "Must not be empty."),
            new(Key: FluentValidationKeys.PredicateValidator, Message: "Must meet required condition."),
            new(Key: FluentValidationKeys.AsyncPredicateValidator, Message: "Must meet required condition."),
            new(Key: FluentValidationKeys.RegularExpressionValidator, Message: "Must be in correct format."),
            new(Key: FluentValidationKeys.EqualValidator, Message: "Must be equal to '{ComparisonValue}'."),
            new(
                Key: FluentValidationKeys.ExactLengthValidator,
                Message: "Must be {MaxLength} characters in length (was {TotalLength})."
            ),
            new(
                Key: FluentValidationKeys.InclusiveBetweenValidator,
                Message: "Must be between {From} and {To} (inclusive)."
            ),
            new(
                Key: FluentValidationKeys.ExclusiveBetweenValidator,
                Message: "Must be between {From} and {To} (exclusive)."
            ),
            new(Key: FluentValidationKeys.CreditCardValidator, Message: "Must be a valid credit card number."),
            new(
                Key: FluentValidationKeys.ScalePrecisionValidator,
                Message: "Must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale)} decimals were found."
            ),
            new(Key: FluentValidationKeys.EmptyValidator, Message: "Must be empty."),
            new(Key: FluentValidationKeys.NullValidator, Message: "Must be empty."),
            new(Key: FluentValidationKeys.EnumValidator, Message: "Must be one of an allowed range of values."),
        };

    public record Translation(string Key, string Message);
}
