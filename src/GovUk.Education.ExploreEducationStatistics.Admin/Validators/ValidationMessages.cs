#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage FileHasApiDataSetVersion = new(
        Code: "FileHasApiDataSetVersion",
        Message: "The file has already been used for an API data set version."
    );
}
