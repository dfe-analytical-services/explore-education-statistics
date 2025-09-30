using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage DebugEnabled = new(
        Code: "DebugEnabled",
        Message: "Debug mode is enabled. Do not enable debug mode in a production environment."
    );

    public static readonly LocalizableMessage FiltersNotFound = new(
        Code: "FiltersNotFound",
        Message: "One or more filters could not be found."
    );

    public static readonly LocalizableMessage GeographicLevelsNotFound = new(
        Code: "GeographicLevelsNotFound",
        Message: "One or more geographic levels could not be found."
    );

    public static readonly LocalizableMessage IndicatorsNotFound = new(
        Code: "IndicatorsNotFound",
        Message: "One or more indicators could not be found."
    );

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

    public static readonly LocalizableMessage LocationValueNotEmpty = new(
        Code: "LocationValueNotEmpty",
        Message: "Must be a location with '{Property}' that is not empty."
    );

    public static readonly LocalizableMessage LocationValueMaxLength = new(
        Code: "LocationValueMaxLength",
        Message: "Must be a location with '{Property}' that is {MaxLength} characters or fewer."
    );

    public static readonly LocalizableMessage LocationsNotFound = new(
        Code: "LocationsNotFound",
        Message: "One or more locations could not be found."
    );

    public static readonly LocalizableMessage TimePeriodFormat = new(
        Code: "TimePeriodFormat",
        Message: "Must be a time period in the correct format."
    );

    public static readonly LocalizableMessage TimePeriodInvalidYear = new(
        Code: "TimePeriodInvalidYear",
        Message: "Must be a time period for a valid year."
    );

    public static readonly LocalizableMessage TimePeriodInvalidYearRange = new(
        Code: "TimePeriodInvalidYearRange",
        Message: "Must be a valid time period range where the start year is one year before the end year."
    );

    public static readonly LocalizableMessage TimePeriodAllowedCode = new(
        Code: "TimePeriodAllowedCode",
        Message: "Must be a time period with an allowed code."
    );

    public static readonly LocalizableMessage TimePeriodsNotFound = new(
        Code: "TimePeriodsNotFound",
        Message: "One or more time periods could not be found."
    );

    public static readonly LocalizableMessage QueryNoResults = new(
        Code: "QueryNoResults",
        Message: "The query did not match any results. You may need to refine your criteria."
    );

    public static readonly LocalizableMessage SortFormat = new(
        Code: "SortFormat",
        Message: "Must be a sort in the correct format."
    );

    public static readonly LocalizableMessage SortFieldNotEmpty = new(
        Code: "SortFieldNotEmpty",
        Message: "Must be a sort with a field that is not empty."
    );

    public static readonly LocalizableMessage SortFieldMaxLength = new(
        Code: "SortFieldMaxLength",
        Message: "Must be a sort with a field that is {MaxLength} characters or fewer."
    );

    public static readonly LocalizableMessage SortDirection = new(
        Code: "SortDirection",
        Message: "Must be a sort in an allowed direction."
    );

    public static readonly LocalizableMessage SortFieldsNotFound = new(
        Code: "SortFieldsNotFound",
        Message: "One or more sort fields could not be found."
    );
}
