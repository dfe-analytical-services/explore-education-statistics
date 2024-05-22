#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage FileHasApiDataSetVersion = new(
        Code: "FileHasApiDataSetVersion",
        Message: "The file has already been used for an API data set version."
    );

    public static readonly LocalizableMessage DataSetVersionCanNotBeDeleted = new(
        Code: "DataSetVersionCanNotBeDeleted",
        Message: $"The data set version is not in a '{DataSetVersionStatus.Draft}' status, so cannot be deleted."
    );
}
