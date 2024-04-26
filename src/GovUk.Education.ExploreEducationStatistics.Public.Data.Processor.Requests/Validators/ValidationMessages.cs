using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators;

public static class ValidationMessages
{
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
}
