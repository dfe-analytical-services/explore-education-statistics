using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage AllowedValue = new(
        Code: "AllowedValue",
        Message: "Must be one of the allowed values."
    );

    public static readonly LocalizableMessage InvalidValue = new(
        Code: "InvalidValue",
        Message: "Must be a valid value. Check that the type and format are correct."
    );

    public static readonly LocalizableMessage NotEmptyBody = new(
        Code: "NotEmptyBody",
        Message: "The request body must not be empty."
    );

    public static readonly LocalizableMessage RequiredValue = new(
        Code: "RequiredValue",
        Message: "A value is required for this field."
    );

    public static readonly LocalizableMessage UnknownField = new(
        Code: "UnknownField",
        Message: "The field was not expected in the request and should be removed."
    );
}
