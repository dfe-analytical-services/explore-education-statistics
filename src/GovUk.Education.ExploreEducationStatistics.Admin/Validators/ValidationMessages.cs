#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage CannotDeleteApiDataSetReleaseFile = new(
        Code: nameof(CannotDeleteApiDataSetReleaseFile),
        Message: "The file cannot be deleted as it is linked to an API data set."
    );

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

    public static readonly LocalizableMessage BulkDataZipContainsDuplicateDatasetNames = new(
        Code: "BulkDataZipContainsDuplicateDatasetNames",
        Message: "All new data sets should have unique names. Duplicated name: '{0}'."
    );

    public static ErrorViewModel GenerateErrorBulkDataZipContainsDuplicateDatasetNames(string duplicate)
    {
        return new ErrorViewModel
        {
            Code = BulkDataZipContainsDuplicateDatasetNames.Code,
            Message = string.Format(BulkDataZipContainsDuplicateDatasetNames.Message, duplicate),
        };
    }

    public static readonly LocalizableMessage FileNameTooLong = new(
        Code: "FileNameTooLong",
        Message: "Filename '{0}' is too long. Should be at most {1} characters."
    );

    public static ErrorViewModel GenerateErrorFileNameTooLong(string fullFileName, int maxLength)
    {
        return new ErrorViewModel
        {
            Code = FileNameTooLong.Code,
            Message = string.Format(FileNameTooLong.Message, fullFileName, maxLength),
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
            Code = FileNameTooLong.Code,
            Message = string.Format(FileNameTooLong.Message, unusedFilenames.JoinToString(",")),
        };
    }
}
