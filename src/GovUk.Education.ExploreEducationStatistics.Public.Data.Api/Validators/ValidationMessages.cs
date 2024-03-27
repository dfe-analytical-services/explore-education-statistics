using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage LocationFormat = new(
        Code: "LocationFormat",
        Message: "Must be a location in the correct format."
    );

    public static readonly LocalizableMessage LocationAllowedLevel = new(
        Code: "LocationAllowedLevel",
        Message: "Must be a location with an allowed level."
    );

    public static readonly LocalizableMessage LocationAllowedProperty = new(
        Code: "LocationAllowedProperty",
        Message: "Must be a location with an allowed identifying property."
    );

    public static readonly LocalizableMessage LocationMaxValueLength = new(
        Code: "LocationMaxLengthValue",
        Message: "Must be a location with an identifying value that is {MaxLength} characters or fewer."
    );

    public static readonly LocalizableMessage TimePeriodFormat = new(
        Code: "TimePeriodFormat",
        Message: "Must be a time period in the correct format."
    );

    public static readonly LocalizableMessage TimePeriodYearRange = new(
        Code: "TimePeriodYearRange",
        Message: "Must be a valid time period range where the start year is one year before the end year."
    );

    public static readonly LocalizableMessage TimePeriodAllowedCode = new(
        Code: "TimePeriodAllowedCode",
        Message: "Must be a time period with an allowed code."
    );

    public static readonly LocalizableMessage SortFormat = new(
        Code: "SortFormat",
        Message: "Must be a sort in the correct format."
    );

    public static readonly LocalizableMessage SortMaxFieldLength = new(
        Code: "SortMaxFieldLength",
        Message: "Must be a sort with a field that is {MaxFieldLength} characters or fewer."
    );

    public static readonly LocalizableMessage SortDirection = new(
        Code: "SortDirection",
        Message: "Must be a sort with an allowed direction."
    );
}
