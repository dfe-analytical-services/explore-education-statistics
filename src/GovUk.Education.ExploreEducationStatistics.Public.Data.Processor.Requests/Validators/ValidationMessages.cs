using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage FileNotFound = new(
        Code: "FileNotFound",
        Message: "The file could not be found."
    );

    public static readonly LocalizableMessage FileHasApiDataSetVersion = new(
        Code: "FileHasApiDataSetVersion",
        Message: "The file has already been used for an API data set version."
    );

    public static readonly LocalizableMessage FileReleaseVersionNotDraft = new(
        Code: "FileReleaseVersionNotDraft",
        Message: "The file must belong to a release in 'Draft' approval status."
    );

    public static readonly LocalizableMessage FileTypeNotData = new(
        Code: "FileTypeNotData",
        Message: "The file type must be 'Data'."
    );

    public static readonly LocalizableMessage NoMetadataFile = new(
        Code: "NoMetadataFile",
        Message: "The data file must have a corresponding metadata file."
    );


    public static readonly LocalizableMessage DataSetVersionCanNotBeDeleted = new(
        Code: "DataSetVersionCanNotBeDeleted",
        Message: $"The data set version is not in a '{DataSetVersionStatus.Draft}' status, so cannot be deleted."
    );
}
