#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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

    public static readonly LocalizableMessage ZipFilenameMustEndDotZip = new(
        Code: "ZipFilenameMustEndDotZip",
        Message: "The file provided '{0}' should have a filename ending in '.zip'."
    );

    public static ErrorViewModel GenerateErrorZipFilenameMustEndDotZip(string fullFilename)
    {
        return new ErrorViewModel
        {
            Code = ZipFilenameMustEndDotZip.Code,
            Message = string.Format(ZipFilenameMustEndDotZip.Message, fullFilename),
        };
    }

    public static readonly LocalizableMessage MustBeZipFile = new(
        Code: "MustBeZipFile",
        Message: "The file provided '{0}' must be a ZIP file."
    );

    public static ErrorViewModel GenerateErrorMustBeZipFile(string fullFilename)
    {
        return new ErrorViewModel
        {
            Code = MustBeZipFile.Code,
            Message = string.Format(MustBeZipFile.Message, fullFilename),
        };
    }

    public static readonly LocalizableMessage MustBeCsvFile = new(
        Code: "MustBeCsvFile",
        Message: "The file provided '{0}' must be a CSV file."
    );

    public static ErrorViewModel GenerateErrorMustBeCsvFile(string fullFilename)
    {
        return new ErrorViewModel
        {
            Code = MustBeCsvFile.Code,
            Message = string.Format(MustBeCsvFile.Message, fullFilename),
        };
    }

    public static readonly LocalizableMessage BulkDataZipMustContainDatasetNamesCsv = new(
        Code: "BulkDataZipMustContainDatasetNamesCsv",
        Message: "For bulk imports, the ZIP must include a dataset_names.csv"
    );

    public static readonly LocalizableMessage DatasetNamesCsvReaderException = new(
        Code: "DatasetNamesCsvReaderException",
        Message: "Failed to read dataset_names.csv. Exception: {0}"
    );

    public static ErrorViewModel GenerateErrorDatasetNamesCsvReaderException(string exception)
    {
        return new ErrorViewModel
        {
            Code = DatasetNamesCsvReaderException.Code,
            Message = string.Format(DatasetNamesCsvReaderException.Message, exception),
        };
    }
    public static readonly LocalizableMessage DatasetNamesCsvIncorrectHeaders = new(
        Code: "DatasetNamesCsvIncorrectHeaders",
        Message: "dataset_names.csv has incorrect headers. It should have 'file_name' and 'dataset_name' only."
    );

    public static readonly LocalizableMessage DataFileNotFoundInZip = new(
        Code: "DataFileNotFound",
        Message: "Failed to find data file '{0}' in ZIP file."
    );

    public static ErrorViewModel GenerateErrorDataFileNotFoundInZip(string fullFilename)
    {
        return new ErrorViewModel
        {
            Code = DataFileNotFoundInZip.Code,
            Message = string.Format(DataFileNotFoundInZip.Message, fullFilename),
        };
    }

    public static readonly LocalizableMessage MetaFileNotFoundInZip = new(
        Code: "MetaFileNotFound",
        Message: "Failed to find meta file '{0}' in ZIP file."
    );

    public static ErrorViewModel GenerateErrorMetaFileNotFoundInZip(string fullFilename)
    {
        return new ErrorViewModel
        {
            Code = MetaFileNotFoundInZip.Code,
            Message = string.Format(MetaFileNotFoundInZip.Message, fullFilename),
        };
    }

    public static readonly LocalizableMessage BulkDataZipShouldContainDataSets = new(
        Code: "BulkDataZipShouldContainDataSets",
        Message: "No data sets were included in the ZIP file."
    );

    public static readonly LocalizableMessage DataSetFileNamesShouldBeUnique = new(
        Code: "DataSetFileNamesShouldBeUnique",
        Message: "All new data sets should have unique names. Duplicate name: '{0}'."
    );

    public static ErrorViewModel GenerateErrorDataSetFileNamesShouldBeUnique(string duplicate)
    {
        return new ErrorViewModel
        {
            Code = DataSetFileNamesShouldBeUnique.Code,
            Message = string.Format(DataSetFileNamesShouldBeUnique.Message, duplicate),
        };
    }

    public static readonly LocalizableMessage FilenameTooLong = new(
        Code: "FilenameTooLong",
        Message: "Filename '{0}' is too long. Should be at most {1} characters."
    );

    public static ErrorViewModel GenerateErrorFileNameTooLong(string fullFileName, int maxLength)
    {
        return new ErrorViewModel
        {
            Code = FilenameTooLong.Code,
            Message = string.Format(FilenameTooLong.Message, fullFileName, maxLength),
        };
    }

    public static readonly LocalizableMessage ZipContainsUnusedFiles = new(
        Code: "ZipContainsUnusedFiles",
        Message: "ZIP file contains unused files: {0}"
    );

    public static ErrorViewModel GenerateErrorZipContainsUnusedFiles(List<string> unusedFilenames)
    {
        return new ErrorViewModel
        {
            Code = ZipContainsUnusedFiles.Code,
            Message = string.Format(ZipContainsUnusedFiles.Message, unusedFilenames.JoinToString(",")),
        };
    }

    public static readonly LocalizableMessage DataAndMetaFilesCannotHaveSameName = new(
        Code: "DataAndMetaFilesCannotHaveSameName",
        Message: "Data file and meta file must use a different filename"
    );

    public static readonly LocalizableMessage FilenameCannotContainSpacesOrSpecialCharacters = new(
        Code: "FilenameCannotContainSpacesOrSpecialCharacters",
        Message: "Filename '{0}' must not use spaces or special characters."
    );

    public static ErrorViewModel GenerateErrorFilenameCannotContainSpacesOrSpecialCharacters(string filename)
    {
        return new ErrorViewModel
        {
            Code = FilenameCannotContainSpacesOrSpecialCharacters.Code,
            Message = string.Format(FilenameCannotContainSpacesOrSpecialCharacters.Message, filename),
        };
    }

    public static readonly LocalizableMessage FilenameMustEndDotCsv = new(
        Code: "FilenameMustEndDotCsv",
        Message: "Filename '{0}' must end in '.csv'."
    );

    public static ErrorViewModel GenerateErrorFilenameMustEndDotCsv(string filename)
    {
        return new ErrorViewModel
        {
            Code = FilenameMustEndDotCsv.Code,
            Message = string.Format(FilenameMustEndDotCsv.Message, filename),
        };
    }

    public static readonly LocalizableMessage MetaFilenameMustEndDotMetaDotCsv = new(
        Code: "MetaFilenameMustEndDotMetaDotCsv",
        Message: "Meta file '{0}' must end in '.meta.csv'."
    );

    public static ErrorViewModel GenerateErrorMetaFilenameMustEndDotMetaDotCsv(string filename)
    {
        return new ErrorViewModel
        {
            Code = MetaFilenameMustEndDotMetaDotCsv.Code,
            Message = string.Format(MetaFilenameMustEndDotMetaDotCsv.Message, filename),
        };
    }

    public static readonly LocalizableMessage FilenameNotUnique = new(
        Code: "FilenameNotUnique",
        Message: "Filename '{0}' of FileType {1} isn't unique to this release."
    );

    public static ErrorViewModel GenerateErrorFilenameNotUnique(string filename, FileType type)
    {
        return new ErrorViewModel
        {
            Code = FilenameNotUnique.Code,
            Message = string.Format(FilenameNotUnique.Message, filename, type.ToString()),
        };
    }

    public static readonly LocalizableMessage FileSizeMustNotBeZero = new(
        Code: "FileSizeMustNotBeZero",
        Message: "File '{0}' must not be of 0 size."
    );

    public static ErrorViewModel GenerateErrorFileSizeMustNotBeZero(string filename)
    {
        return new ErrorViewModel
        {
            Code = FileSizeMustNotBeZero.Code,
            Message = string.Format(FileSizeMustNotBeZero.Message, filename),
        };
    }
    
    public static readonly LocalizableMessage DataSetFileNameCannotBeEmpty = new(
        Code: "DataSetFileNameCannotBeEmpty",
        Message: "Data set name cannot be empty"
    );

    public static readonly LocalizableMessage DataSetFileNameShouldNotContainSpecialCharacters = new(
        Code: "DataSetFileNameShouldNotContainSpecialCharacters",
        Message: "Data set name '{0}' should not contain special characters"
    );

    public static ErrorViewModel GenerateErrorDataSetFileNameShouldNotContainSpecialCharacters(string filename)
    {
        return new ErrorViewModel
        {
            Code = DataSetFileNameShouldNotContainSpecialCharacters.Code,
            Message = string.Format(DataSetFileNameShouldNotContainSpecialCharacters.Message, filename),
        };
    }
    
}
