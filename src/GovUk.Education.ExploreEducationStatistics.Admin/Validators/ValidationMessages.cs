using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage HasExistingApiDataSetVersion = new(
        Code: "HasExistingApiDataSetVersion",
        Message: "The API data set version already exists."
    );
}
