#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using System.IO;

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
        private const int MaxFilenameSize = 150;

        public Validator() // TODO: Check if any duplications from the equivalent request model IFormFile validator can be be removed
        {
            RuleFor(dto => dto.FileName)
                .MaximumLength(MaxFilenameSize)
                    .WithMessage(ValidationMessages.FilenameTooLong, "{PropertyName}", MaxFilenameSize.ToString())
                .Must(fileName => fileName.ToLower().EndsWith(".csv"))
                    .WithMessage(ValidationMessages.FilenameMustEndDotCsv, "{PropertyName}");

            RuleFor(dto => dto.FileSize)
                .Equal(dto => dto.FileStream.Length)
                .GreaterThan(0)
                    .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{PropertyName}");

            RuleFor(dto => dto.FileStream)
                .Must(f => f.Length > 0)
                    .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{PropertyName}");
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
    public Content.Model.File? ReplacingFile { get; init; }
}
