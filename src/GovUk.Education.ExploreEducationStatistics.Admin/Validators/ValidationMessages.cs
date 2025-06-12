#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
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
        Message: "The file cannot be deleted as it is linked to a published API data set."
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

    public static readonly LocalizableMessage ZipFileNameMustEndDotZip = new(
        Code: nameof(ZipFileNameMustEndDotZip),
        Message: "The file provided '{0}' should have a file name ending in '.zip'."
    );

    public static ErrorViewModel GenerateErrorZipFileNameMustEndDotZip(string fullFileName)
    {
        return new ErrorViewModel
        {
            Code = ZipFileNameMustEndDotZip.Code,
            Message = string.Format(ZipFileNameMustEndDotZip.Message, fullFileName),
        };
    }

    public static readonly LocalizableMessage MustBeZipFile = new(
        Code: nameof(MustBeZipFile),
        Message: "The file provided '{0}' must be a ZIP file."
    );

    public static ErrorViewModel GenerateErrorMustBeZipFile(string fullFileName)
    {
        return new ErrorViewModel
        {
            Code = MustBeZipFile.Code,
            Message = string.Format(MustBeZipFile.Message, fullFileName),
        };
    }

    public static readonly LocalizableMessage DataSetFileNamesShouldMatchConvention = new(
        Code: nameof(DataSetFileNamesShouldMatchConvention),
        Message: $"The data file should end {Constants.DataSet.DataFileExtension}, and the meta file should end {Constants.DataSet.MetaFileExtension}."
    );

    public static ErrorViewModel GenerateErrorDataSetFileNamesShouldMatchConvention()
    {
        return new ErrorViewModel
        {
            Code = DataSetFileNamesShouldMatchConvention.Code,
            Message = DataSetFileNamesShouldMatchConvention.Message,
        };
    }

    public static readonly LocalizableMessage MustBeCsvFile = new(
        Code: nameof(MustBeCsvFile),
        Message: "The file provided '{0}' must be a CSV file."
    );

    public static ErrorViewModel GenerateErrorMustBeCsvFile(string fullFileName)
    {
        return new ErrorViewModel
        {
            Code = MustBeCsvFile.Code,
            Message = string.Format(MustBeCsvFile.Message, fullFileName),
        };
    }

    public static readonly LocalizableMessage BulkDataZipMustContainDataSetNamesCsv = new(
        Code: nameof(BulkDataZipMustContainDataSetNamesCsv),
        Message: "For bulk imports, the ZIP must include dataset_names.csv"
    );

    public static ErrorViewModel GenerateErrorBulkDataZipMustContainDataSetNamesCsv()
    {
        return new ErrorViewModel
        {
            Code = BulkDataZipMustContainDataSetNamesCsv.Code,
            Message = BulkDataZipMustContainDataSetNamesCsv.Message,
        };
    }

    public static readonly LocalizableMessage DataSetNamesCsvReaderException = new(
        Code: nameof(DataSetNamesCsvReaderException),
        Message: "Failed to read dataset_names.csv. Exception: {0}"
    );

    public static ErrorViewModel GenerateErrorDataSetIsNotInAnImportableState()
    {
        return new ErrorViewModel
        {
            Code = DataSetIsNotInAnImportableState.Code,
            Message = DataSetIsNotInAnImportableState.Message,
        };
    }

    public static readonly LocalizableMessage DataSetIsNotInAnImportableState = new(
        Code: nameof(DataSetIsNotInAnImportableState),
        Message: "Data set cannot be imported. Ensure it has been screened, and that there are no errors"
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

    public static ErrorViewModel GenerateErrorDataSetNamesCsvIncorrectHeaders()
    {
        return new ErrorViewModel
        {
            Code = DataSetNamesCsvIncorrectHeaders.Code,
            Message = DataSetNamesCsvIncorrectHeaders.Message,
        };
    }

    public static readonly LocalizableMessage FileNotFoundInZip = new(
        Code: nameof(FileNotFoundInZip),
        Message: "Failed to find file '{0}' of type {1} in ZIP file."
    );

    public static ErrorViewModel GenerateErrorFileNotFoundInZip(string fullFileName, FileType type)
    {
        return new ErrorViewModel
        {
            Code = FileNotFoundInZip.Code,
            Message = string.Format(FileNotFoundInZip.Message, fullFileName, type.ToString()),
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

    public static readonly LocalizableMessage DataSetNamesCsvFileNamesShouldBeUnique = new(
        Code: nameof(DataSetNamesCsvFileNamesShouldBeUnique),
        Message: "In dataset_names.csv, all file names should be unique. Duplicate file name: '{0}'."
    );

    public static ErrorViewModel GenerateErrorDataSetNamesCsvFilenamesShouldBeUnique(string duplicate)
    {
        return new ErrorViewModel
        {
            Code = DataSetNamesCsvFileNamesShouldBeUnique.Code,
            Message = string.Format(DataSetNamesCsvFileNamesShouldBeUnique.Message, duplicate),
        };
    }

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
        Message: "Title '{0}' must be {1} characters or less"
    );

    public static ErrorViewModel GenerateErrorDataSetTitleTooLong(
        string title,
        int maxLength)
    {
        return new ErrorViewModel
        {
            Code = DataSetTitleTooLong.Code,
            Message = string.Format(DataSetTitleTooLong.Message, title, maxLength),
        };
    }

    public static readonly LocalizableMessage FileNameLengthInvalid = new(
        Code: nameof(FileNameLengthInvalid),
        Message: "File name '{0}' should be greater than 0 characters in length, and less than {1}."
    );

    public static ErrorViewModel GenerateErrorFilenameTooLong(string fullFileName, int maxLength)
    {
        return new ErrorViewModel
        {
            Code = FileNameLengthInvalid.Code,
            Message = string.Format(FileNameLengthInvalid.Message, fullFileName, maxLength),
        };
    }

    public static readonly LocalizableMessage ZipContainsUnusedFiles = new(
        Code: nameof(ZipContainsUnusedFiles),
        Message: "ZIP file contains unused files: {0}"
    );

    public static ErrorViewModel GenerateErrorZipContainsUnusedFiles(List<string> unusedFileNames)
    {
        return new ErrorViewModel
        {
            Code = ZipContainsUnusedFiles.Code,
            Message = string.Format(ZipContainsUnusedFiles.Message, unusedFileNames.JoinToString(",")),
        };
    }

    public static readonly LocalizableMessage DataAndMetaFilesCannotHaveSameName = new(
        Code: nameof(DataAndMetaFilesCannotHaveSameName),
        Message: "Data file and meta file must use a different file name"
    );

    public static readonly LocalizableMessage FileNameCannotContainSpaces = new(
        Code: nameof(FileNameCannotContainSpaces),
        Message: "File name '{0}' must not use spaces."
    );

    public static readonly LocalizableMessage FileNameCannotContainSpecialCharacters = new(
        Code: nameof(FileNameCannotContainSpecialCharacters),
        Message: "File name '{0}' must not use special characters."
    );

    public static readonly LocalizableMessage FileNameMustEndDotCsv = new(
        Code: nameof(FileNameMustEndDotCsv),
        Message: "File name '{0}' must end in '{1}'."
    );

    public static ErrorViewModel GenerateErrorFilenameMustEndDotCsv(string fileName)
    {
        return new ErrorViewModel
        {
            Code = FileNameMustEndDotCsv.Code,
            Message = string.Format(FileNameMustEndDotCsv.Message, fileName),
        };
    }

    public static readonly LocalizableMessage MetaFileNameMustEndDotMetaDotCsv = new(
        Code: nameof(MetaFileNameMustEndDotMetaDotCsv),
        Message: "Meta file '{0}' must end in '{1}'."
    );

    public static ErrorViewModel GenerateErrorMetaFilenameMustEndDotMetaDotCsv(string fileName)
    {
        return new ErrorViewModel
        {
            Code = MetaFileNameMustEndDotMetaDotCsv.Code,
            Message = string.Format(MetaFileNameMustEndDotMetaDotCsv.Message, fileName),
        };
    }

    public static readonly LocalizableMessage FileNameNotUnique = new(
        Code: nameof(FileNameNotUnique),
        Message: "File name '{0}' of type {1} isn't unique to this release."
    );

    public static ErrorViewModel GenerateErrorInvalidFileTypeForReplacement(FileType type)
    {
        return new ErrorViewModel
        {
            Code = InvalidFileTypeForReplacement.Code,
            Message = string.Format(InvalidFileTypeForReplacement.Message, type.ToString()),
        };
    }

    public static readonly LocalizableMessage InvalidFileTypeForReplacement = new(
        Code: nameof(InvalidFileTypeForReplacement),
        Message: "Replacing file should be of type '{0}'."
    );

    public static ErrorViewModel GenerateErrorFileNameNotUnique(string fileName, FileType type)
    {
        return new ErrorViewModel
        {
            Code = FileNameNotUnique.Code,
            Message = string.Format(FileNameNotUnique.Message, fileName, type.ToString()),
        };
    }

    public static readonly LocalizableMessage FileSizeMustNotBeZero = new(
        Code: nameof(FileSizeMustNotBeZero),
        Message: "File '{0}' either empty or not found."
    );

    public static ErrorViewModel GenerateErrorFileSizeMustNotBeZero(string fileName)
    {
        return new ErrorViewModel
        {
            Code = FileSizeMustNotBeZero.Code,
            Message = string.Format(FileSizeMustNotBeZero.Message, fileName),
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

    public static ErrorViewModel GenerateErrorDataSetTitleShouldNotContainSpecialCharacters(string fileName)
    {
        return new ErrorViewModel
        {
            Code = DataSetTitleShouldNotContainSpecialCharacters.Code,
            Message = string.Format(DataSetTitleShouldNotContainSpecialCharacters.Message, fileName),
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
