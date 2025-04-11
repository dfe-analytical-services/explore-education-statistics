#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using System.IO;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

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
    public required string Title { get; set; }

    public required DataSetFileDto DataFile { get; set; }

    public required DataSetFileDto MetaFile { get; set; }
}

/// <summary>
/// Represents a validated data set extracted from a zip file.
/// </summary>
public record ZippedDataSet : DataSet
{
    public File? ReplacingFile { get; init; }
}
