#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage CannotDeleteApiDataSetReleaseFile = new(
        Code: nameof(CannotDeleteApiDataSetReleaseFile),
        Message: "The file cannot be deleted as it is linked to an API data set."
    );

    public static readonly LocalizableMessage DataSetVersionStatusNotDraft = new(
        Code: nameof(DataSetVersionStatusNotDraft),
        Message: "The data set version is not in draft status."
    );

    public static readonly LocalizableMessage DataSetVersionCannotHaveNotes = new(
        Code: nameof(DataSetVersionCannotHaveNotes),
        Message: "The data set version cannot have guidance notes as it is the first version."
    );

    public static readonly LocalizableMessage DataSetVersionCannotBeUpdated = new(
        Code: nameof(DataSetVersionCannotBeUpdated),
        Message: "The data set version cannot be updated."
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
        Code: nameof(ZipFilenameMustEndDotZip),
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
        Code: nameof(MustBeZipFile),
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

    public static readonly LocalizableMessage DataZipShouldContainTwoFiles = new(
        Code: nameof(DataZipShouldContainTwoFiles),
        Message: "The ZIP file provided should contain two files."
    );

    public static readonly LocalizableMessage MustBeCsvFile = new(
        Code: nameof(MustBeCsvFile),
        Message: "The file provided '{0}' must be a CSV file."
    );

    public static readonly LocalizableMessage DataSetVersionIsInvalid = new(
        Code: nameof(DataSetVersionIsInvalid),
        Message: "The data set version '{0}' must be valid Semantic Version."
    );

    public static ErrorViewModel GenerateErrorMustBeCsvFile(string fullFilename)
    {
        return new ErrorViewModel
        {
            Code = MustBeCsvFile.Code,
            Message = string.Format(MustBeCsvFile.Message, fullFilename),
        };
    }

    public static readonly LocalizableMessage BulkDataZipMustContainDataSetNamesCsv = new(
        Code: nameof(BulkDataZipMustContainDataSetNamesCsv),
        Message: "For bulk imports, the ZIP must include dataset_names.csv"
    );

    public static readonly LocalizableMessage DataSetNamesCsvReaderException = new(
        Code: nameof(DataSetNamesCsvReaderException),
        Message: "Failed to read dataset_names.csv. Exception: {0}"
    );

    public static ErrorViewModel GenerateErrorDataSetNamesCsvReaderException(string exception)
    {
        return new ErrorViewModel
        {
            Code = DataSetNamesCsvReaderException.Code,
            Message = string.Format(DataSetNamesCsvReaderException.Message, exception),
        };
    }

    public static readonly LocalizableMessage DataSetNamesCsvIncorrectHeaders = new(
        Code: nameof(DataSetNamesCsvIncorrectHeaders),
        Message: "dataset_names.csv has incorrect headers. It should have 'file_name' and 'dataset_name' only."
    );

    public static readonly LocalizableMessage FileNotFoundInZip = new(
        Code: nameof(FileNotFoundInZip),
        Message: "Failed to find file '{0}' of type {1} in ZIP file."
    );

    public static ErrorViewModel GenerateErrorFileNotFoundInZip(string fullFilename, FileType type)
    {
        return new ErrorViewModel
        {
            Code = FileNotFoundInZip.Code,
            Message = string.Format(FileNotFoundInZip.Message, fullFilename, type.ToString()),
        };
    }

    public static readonly LocalizableMessage DataSetTitleShouldBeUnique = new(
        Code: nameof(DataSetTitleShouldBeUnique),
        Message: "Data set title should be unique. Duplicate title: '{0}'."
    );

    public static ErrorViewModel GenerateErrorDataSetTitleShouldBeUnique(string duplicate)
    {
        return new ErrorViewModel
        {
            Code = DataSetTitleShouldBeUnique.Code,
            Message = string.Format(DataSetTitleShouldBeUnique.Message, duplicate),
        };
    }

    public static readonly LocalizableMessage DataSetNamesCsvFilenamesShouldBeUnique = new(
        Code: nameof(DataSetNamesCsvFilenamesShouldBeUnique),
        Message: "In dataset_names.csv, all filenames should be unique. Duplicate filename: '{0}'."
    );

    public static ErrorViewModel GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique(string duplicate)
    {
        return new ErrorViewModel
        {
            Code = DataSetNamesCsvFilenamesShouldBeUnique.Code,
            Message = string.Format(DataSetNamesCsvFilenamesShouldBeUnique.Message, duplicate),
        };
    }

    public static readonly LocalizableMessage DataSetNamesCsvFilenamesShouldNotEndDotCsv = new(
        Code: nameof(DataSetNamesCsvFilenamesShouldNotEndDotCsv),
        Message: "Inside dataset_names.csv, file_name cell entries should not end in '.csv' i.e. should be 'filename' not 'filename.csv'. Filename found with extension: '{0}'."
    );

    public static readonly LocalizableMessage FileIsNull = new(
        Code: nameof(FileIsNull),
        Message: "No file provided."
    );

    public static ErrorViewModel GenerateErrorFileIsNull()
    {
        return new ErrorViewModel
        {
            Code = FileIsNull.Code,
            Message = FileIsNull.Message,
        };
    }

    public static readonly LocalizableMessage DataReplacementAlreadyInProgress = new(
        Code: nameof(DataReplacementAlreadyInProgress),
        Message: "Data replacement already in progress"
    );

    public static ErrorViewModel GenerateErrorDataReplacementAlreadyInProgress()
    {
        return new ErrorViewModel
        {
            Code = DataReplacementAlreadyInProgress.Code,
            Message = DataReplacementAlreadyInProgress.Message,
        };
    }

    public static readonly LocalizableMessage DataSetTitleTooLong = new(
        Code: nameof(DataSetTitleTooLong),
        Message: "Title '{0}' must be 120 characters or less"
    );

    public static ErrorViewModel GenerateErrorDataSetTitleTooLong(string title)
    {
        return new ErrorViewModel
        {
            Code = DataSetTitleTooLong.Code,
            Message = string.Format(DataSetTitleTooLong.Message, title),
        };
    }

    public static ErrorViewModel GenerateErrorDataSetNamesCsvFilenamesShouldNotEndDotCsv(string filename)
    {
        return new ErrorViewModel
        {
            Code = DataSetNamesCsvFilenamesShouldNotEndDotCsv.Code,
            Message = string.Format(DataSetNamesCsvFilenamesShouldNotEndDotCsv.Message, filename),
        };
    }

    public static readonly LocalizableMessage FilenameTooLong = new(
        Code: nameof(FilenameTooLong),
        Message: "Filename '{0}' is too long. Should be at most {1} characters."
    );

    public static ErrorViewModel GenerateErrorFilenameTooLong(string fullFileName, int maxLength)
    {
        return new ErrorViewModel
        {
            Code = FilenameTooLong.Code,
            Message = string.Format(FilenameTooLong.Message, fullFileName, maxLength),
        };
    }

    public static readonly LocalizableMessage ZipContainsUnusedFiles = new(
        Code: nameof(ZipContainsUnusedFiles),
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
        Code: nameof(DataAndMetaFilesCannotHaveSameName),
        Message: "Data file and meta file must use a different filename"
    );

    public static readonly LocalizableMessage FilenameCannotContainSpacesOrSpecialCharacters = new(
        Code: nameof(FilenameCannotContainSpacesOrSpecialCharacters),
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
        Code: nameof(FilenameMustEndDotCsv),
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
        Code: nameof(MetaFilenameMustEndDotMetaDotCsv),
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
        Code: nameof(FilenameNotUnique),
        Message: "Filename '{0}' of type {1} isn't unique to this release."
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
        Code: nameof(FileSizeMustNotBeZero),
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

    public static readonly LocalizableMessage DataSetTitleCannotBeEmpty = new(
        Code: nameof(DataSetTitleCannotBeEmpty),
        Message: "Data set title cannot be empty"
    );

    public static readonly LocalizableMessage DataSetTitleShouldNotContainSpecialCharacters = new(
        Code: nameof(DataSetTitleShouldNotContainSpecialCharacters),
        Message: "Data set title '{0}' should not contain special characters"
    );

    public static ErrorViewModel GenerateErrorDataSetTitleShouldNotContainSpecialCharacters(string filename)
    {
        return new ErrorViewModel
        {
            Code = DataSetTitleShouldNotContainSpecialCharacters.Code,
            Message = string.Format(DataSetTitleShouldNotContainSpecialCharacters.Message, filename),
        };
    }

    public static readonly LocalizableMessage CannotReplaceDataSetWithApiDataSet = new(
        Code: nameof(CannotReplaceDataSetWithApiDataSet),
        Message: "Data set with title '{0}' cannot be replaced as it has an API data set."
    );

    public static ErrorViewModel GenerateErrorCannotReplaceDataSetWithApiDataSet(string title)
    {
        return new ErrorViewModel
        {
            Code = CannotReplaceDataSetWithApiDataSet.Code,
            Message = string.Format(CannotReplaceDataSetWithApiDataSet.Message, title),
        };
    }

    public static readonly LocalizableMessage PreviewTokenExpired = new(
        Code: nameof(PreviewTokenExpired),
        Message: "The preview token is expired."
    );
}
