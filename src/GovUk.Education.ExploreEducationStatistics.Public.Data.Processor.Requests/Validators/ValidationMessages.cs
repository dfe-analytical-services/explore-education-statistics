using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage FileNotFound = new(
        Code: nameof(FileNotFound),
        Message: "The file could not be found."
    );

    public static readonly LocalizableMessage FileHasApiDataSetVersion = new(
        Code: nameof(FileHasApiDataSetVersion),
        Message: "The file has already been used for an API data set version."
    );

    public static readonly LocalizableMessage FileReleaseVersionNotDraft = new(
        Code: nameof(FileReleaseVersionNotDraft),
        Message: "The file must belong to a release in 'Draft' approval status."
    );

    public static readonly LocalizableMessage FileTypeNotData = new(
        Code: nameof(FileTypeNotData),
        Message: "The file type must be 'Data'."
    );

    public static readonly LocalizableMessage NoMetadataFile = new(
        Code: nameof(NoMetadataFile),
        Message: "The data file must have a corresponding metadata file."
    );

    public static readonly LocalizableMessage DataSetVersionCanNotBeDeleted = new(
        Code: nameof(DataSetVersionCanNotBeDeleted),
        Message: $"The data set version is not in a '{DataSetVersionStatus.Draft}' status, so cannot be deleted."
    );
    
    public static readonly LocalizableMessage DataSetNotFound = new(
        Code: nameof(DataSetNotFound),
        Message: "The data set must exist."
    );

    public static readonly LocalizableMessage DataSetMustHaveLiveDataSetVersion = new(
        Code: nameof(DataSetMustHaveLiveDataSetVersion),
        Message: "The data set must have a live version."
    );

    public static readonly LocalizableMessage DataSetAndReleaseFileMustBeForSamePublication = new(
        Code: nameof(DataSetAndReleaseFileMustBeForSamePublication),
        Message: "The data set and file must belong to the same publication."
    );

    /// <summary>
    /// This rule ensures that we're not creating the next data set version based on a release file that is an
    /// amendment of the release that the currently live data set version   
    /// </summary>
    public static readonly LocalizableMessage ReleaseFileMustBeFromDifferentReleaseToHistoricalVersions = new(
        Code: nameof(ReleaseFileMustBeFromDifferentReleaseToHistoricalVersions),
        Message: "The release file must be from a different release than the currently live data set version."
    );
}
