#nullable enable

using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage AllowedValue = new(
        Code: "AllowedValue",
        Message: "Must be one of the allowed values."
    );
}
