#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class ValidationErrorMessages
{
    public static readonly ValidationErrorMessage AllowedValue = new(
        Code: "AllowedValue",
        Single: "Must be one of the allowed values.",
        Plural: "Must only contain allowed values."
    );
}
