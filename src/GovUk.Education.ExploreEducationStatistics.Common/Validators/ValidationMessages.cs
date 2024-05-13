#nullable enable

using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage AllowedValue = new(
        Code: "AllowedValue",
        Message: "Must be one of the allowed values."
    );

    public static readonly LocalizableMessage InvalidInput = new(
        Code: "InvalidInput",
        Message: "The input is not valid. Check that it is in the expected format."
    );

    public static readonly LocalizableMessage NotEmptyBody = new(
        Code: "NotEmptyBody",
        Message: "The request body must not be empty."
    );

    public static readonly LocalizableMessage RequiredField = new(
        Code: "RequiredField",
        Message: "The field is required."
    );
}
