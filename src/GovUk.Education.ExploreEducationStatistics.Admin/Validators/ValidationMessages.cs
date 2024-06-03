#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage CannotDeleteApiDataSetReleaseFile = new(
        Code: nameof(CannotDeleteApiDataSetReleaseFile),
        Message: "The file cannot be deleted as it is linked to an API data set."
    );

    public static readonly LocalizableMessage DataSetVersionMappingSourcePathDoesNotExist = new(
        Code: nameof(DataSetVersionMappingSourcePathDoesNotExist),
        Message: "The source mapping does not exist."
    );

    public static readonly LocalizableMessage DataSetVersionMappingCandidatePathDoesNotExist = new(
        Code: nameof(DataSetVersionMappingCandidatePathDoesNotExist),
        Message: "The candidate does not exist."
    );

    public static readonly LocalizableMessage ManualMappingTypeInvalid = new(
        Code: nameof(ManualMappingTypeInvalid),
        Message: $"Type must be one of the following values: {MappingType.ManualMapped}, {MappingType.ManualNone}"
    );

    public static readonly LocalizableMessage CandidateKeyMustBeSpecifiedWithMappedMappingType = new(
        Code: nameof(CandidateKeyMustBeSpecifiedWithMappedMappingType),
        Message: $"Value must be specified if type is {nameof(MappingType.ManualMapped)}"
    );

    public static readonly LocalizableMessage CandidateKeyMustBeEmptyWithNoneMappingType = new(
        Code: nameof(CandidateKeyMustBeEmptyWithNoneMappingType),
        Message: $"Value must be empty if type is {nameof(MappingType.ManualNone)}"
    );
    
    public static readonly LocalizableMessage OwningFilterNotMapped = new(
        Code: nameof(OwningFilterNotMapped),
        Message: "The filter that owns this filter option has not been mapped."
    );

    public static readonly LocalizableMessage MustBeZipFile = new(
        Code: "MustBeZipFile",
        Message: "The file provided must be a ZIP file."
    );

    public static readonly LocalizableMessage BulkDataZipMustContainDatasetNamesCsv = new(
        Code: "BulkDataZipMustContainDatasetNamesCsv",
        Message: "For bulk imports, the ZIP must include a dataset_names.csv"
    );

    public static readonly LocalizableMessage DatasetNamesCsvReaderException = new(
        Code: "DatasetNamesCsvReaderException",
        Message: "Failed to read dataset_names.csv. Is it correctly saved as a CSV file?"
    );

    public static readonly LocalizableMessage DatasetNamesCsvIncorrectHeaders = new(
        Code: "DatasetNamesCsvIncorrectHeaders",
        Message: "dataset_names.csv has incorrect headers. It should have 'file_name' and 'dataset_name' only."
    );

    public static readonly LocalizableMessage DataFileNotFoundInZip = new(
        Code: "DataFileNotFound",
        Message: "Failed to find data file in ZIP file"
    );

    public static readonly LocalizableMessage MetaFileNotFoundInZip = new(
        Code: "MetaFileNotFound",
        Message: "Failed to find meta file in ZIP file"
    );

    public static readonly LocalizableMessage BulkDataZipShouldContainDataSets = new(
        Code: "BulkDataZipShouldContainDataSets",
        Message: "No data sets were included in the ZIP file."
    );
}
