using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static class ValidationErrorMessages
{
    public static readonly ValidationErrorMessage LocationFormat = new(
        Code: "LocationFormat",
        Single: "Must be a location in the correct format.",
        Plural: "Must only contain locations in the correct format."
    );

    public static readonly ValidationErrorMessage LocationAllowedLevel = new(
        Code: "LocationAllowedLevel",
        Single: "Must be a location with an allowed level.",
        Plural: "Must only contain locations with allowed levels."
    );

    public static readonly ValidationErrorMessage LocationAllowedProperty = new(
        Code: "LocationAllowedProperty",
        Single: "Must be a location with an allowed identifying property.",
        Plural: "Must only contain locations with allowed identifying properties."
    );

    public static readonly ValidationErrorMessage LocationMaxValueLength = new(
        Code: "LocationMaxLengthValue",
        Single: "Must be a location with an identifying value that is {MaxLength} characters or fewer.",
        Plural: "Must only contain locations with identifying values that are {MaxLength} characters or fewer."
    );

    public static readonly ValidationErrorMessage TimePeriodFormat = new(
        Code: "TimePeriodFormat",
        Single: "Must be a time period in the correct format.",
        Plural: "Must only contain time periods in the correct format."
    );

    public static readonly ValidationErrorMessage TimePeriodRange = new(
        Code: "TimePeriodRange",
        Single: "Must be a valid time period range where the start period is before the end period.",
        Plural: "Must only contain time period ranges where the start period is before the end period."
    );

    public static readonly ValidationErrorMessage TimePeriodAllowedCode = new(
        Code: "TimePeriodAllowedCode",
        Single: "Must be a time period with an allowed code.",
        Plural: "Must only contain time periods with allowed codes."
    );
}


