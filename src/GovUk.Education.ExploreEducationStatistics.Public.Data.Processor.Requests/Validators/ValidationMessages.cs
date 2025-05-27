using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;

public static class ValidationMessages
{
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

    public static readonly LocalizableMessage FileNotInDataSetPublication = new(
        Code: nameof(FileNotInDataSetPublication),
        Message: "The file must belong to the same publication as the data set."
    );

    public static readonly LocalizableMessage FileMustBeInDifferentRelease = new(
        Code: nameof(FileMustBeInDifferentRelease),
        Message: "The file must be in a different release to previous data set versions."
    );

    public static readonly LocalizableMessage DataSetVersionCanNotBeDeleted = new(
        Code: nameof(DataSetVersionCanNotBeDeleted),
        Message:
        "The data set version is not in a draft status, or is currently being processed, so cannot be deleted."
    );

    public static readonly LocalizableMessage DataSetMustHaveNoExistingVersions = new(
        Code: nameof(DataSetMustHaveNoExistingVersions),
        Message: "The data set must have no existing versions when creating the initial version."
    );

    public static readonly LocalizableMessage DataSetNoLiveVersion = new(
        Code: nameof(DataSetNoLiveVersion),
        Message: "The data set must have a live version."
    );

    public static readonly LocalizableMessage OneOrMoreDataSetVersionsCanNotBeDeleted = new(
        Code: nameof(OneOrMoreDataSetVersionsCanNotBeDeleted),
        Message: "One or more data set versions are not in a draft status, " +
                 "or are currently being processed, so cannot be deleted."
    );

    public static readonly LocalizableMessage DataSetVersionNotInMappingStatus = new(
        Code: nameof(DataSetVersionNotInMappingStatus),
        Message: "The data set version must be in the 'Mapping' status."
    );

    public static readonly LocalizableMessage ImportInManualMappingStateNotFound = new(
        Code: nameof(ImportInManualMappingStateNotFound),
        Message: "An import in mapping state could not be found."
    );

    public static readonly LocalizableMessage DataSetVersionMappingNotFound = new(
        Code: nameof(DataSetVersionMappingNotFound),
        Message: "A data set version mapping could not be found."
    );

    public static readonly LocalizableMessage DataSetVersionMappingsNotComplete = new(
        Code: nameof(DataSetVersionMappingsNotComplete),
        Message: "The data set version mappings are not complete."
    );
    
    public static readonly LocalizableMessage NextDataSetVersionNotFound = new(
        Code: nameof(NextDataSetVersionNotFound),
        Message: "The data set version required for creating the next version was not found."
    );
}
