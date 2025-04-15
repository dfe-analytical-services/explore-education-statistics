#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

/// <summary>
/// Represents an unvalidated data set.
/// </summary>
public record DataSetDto()
{
    public Guid ReleaseVersionId { get; init; }

    public string Title { get; init; } = string.Empty;

    public required DataSetFileDto DataFile { get; init; }

    public required DataSetFileDto MetaFile { get; init; }

    public File? ReplacingFile { get; init; }

    public class Validator : AbstractValidator<DataSetDto>
    {
        public Validator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(dto => dto.ReleaseVersionId)
                .NotEmpty();

            RuleFor(dto => dto.Title)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong, "{PropertyValue}", "{MaxLength}");

            RuleFor(dto => dto.DataFile)
                .Must(file => FileNameValidators.MeetsLengthRequirements(file.FileName))
                    .WithMessage(ValidationMessages.FilenameTooLong, "{PropertyValue}", FileNameValidators.MaxFileNameSize.ToString())
                .Must(file => FileNameValidators.DoesNotContainSpaces(file.FileName))
                    .When(dto => dto.DataFile is not null)
                    .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{PropertyValue}")
                .Must(file => FileNameValidators.DoesNotContainSpecialChars(file.FileName))
                    .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{PropertyValue}")
                .Must(file => file.FileName.ToLower().EndsWith(".csv"))
                    .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{PropertyValue}")
                .Must((dto, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", dto.DataFile.FileName);
                    return file.FileStream.Length > 0;
                })
                    .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{FileName}");

            RuleFor(dto => dto.MetaFile)
                .Must(file => FileNameValidators.MeetsLengthRequirements(file.FileName))
                    .WithMessage(ValidationMessages.FilenameTooLong, "{PropertyValue}", FileNameValidators.MaxFileNameSize.ToString())
                .Must(file => FileNameValidators.DoesNotContainSpaces(file.FileName))
                    .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{PropertyValue}")
                .Must(file => FileNameValidators.DoesNotContainSpecialChars(file.FileName))
                    .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{PropertyValue}")
                .Must(file => file.FileName.ToLower().EndsWith(".csv"))
                    .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{PropertyValue}")
                .Must((dto, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", dto.MetaFile.FileName);
                    return file.FileStream.Length > 0;
                })
                    .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{FileName}");

            // TODO: Extract duplicated DataSetFileDto validation (FileName/FileStream)
            // DataSetFileDto.Validate() is now only used for the dataset_names.csv file
        }
    }
}

/// <summary>
/// Represents an data set file.
/// </summary>
public record DataSetFileDto
{
    public required string FileName { get; init; }

    /// <summary>
    /// Get the size of a data set file in bytes.
    /// </summary>
    /// <remarks>
    /// Although streams contain a Length property, this value is unavailable once the stream has been disposed of.
    /// </remarks>
    public required long FileSize { get; init; }

    public required MemoryStream FileStream { get; init; }

    public class Validator : AbstractValidator<DataSetFileDto>
    {
        public Validator()
        {
            RuleFor(dto => dto.FileName)
                .Must(FileNameValidators.MeetsLengthRequirements)
                    .WithMessage(ValidationMessages.FilenameTooLong, "{PropertyValue}", FileNameValidators.MaxFileNameSize.ToString())
                .Must(FileNameValidators.DoesNotContainSpaces)
                    .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{PropertyValue}")
                .Must(FileNameValidators.DoesNotContainSpecialChars)
                    .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{PropertyValue}")
                .Must(fileName => fileName.ToLower().EndsWith(".csv"))
                    .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{PropertyValue}");

            RuleFor(dto => dto.FileStream)
                .Must((dto, fileStream, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                    return fileStream.Length > 0;
                })
                    .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{FileName}");
            // TODO: See why GetFileType keeps returning null. Replace raw MIME string with const/enum
            //.Must(f => f.GetFileType().Mime == "text/csv")
            //    .WithMessage(ValidationMessages.ZipFilenameMustEndDotZip, "{PropertyName}");
        }
    }
}

/// <summary>
/// Represents a validated data set.
/// </summary>
public record DataSet
{
    public required string Title { get; init; }

    public required DataSetFileDto DataFile { get; init; }

    public required DataSetFileDto MetaFile { get; init; }

    public File? ReplacingFile { get; init; }
}

public record DataSetIndex()
{
    public required Guid ReleaseVersionId { get; init; }

    public List<DataSetIndexItem> DataSetIndexItems { get; init; } = [];
}

public record DataSetIndexItem
{
    public required string DataSetTitle { get; init; }

    public required string DataFileName { get; init; }

    public required string MetaFileName { get; init; }

    public File? ReplacingFile { get; init; }
}
