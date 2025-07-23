using System;
using System.IO;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record FileDto
{
    public required string FileName { get; init; }

    /// <summary>
    /// Get the size of a file in bytes.
    /// </summary>
    /// <remarks>
    /// Although streams contain a <c>Length</c> property, this value is unavailable once the stream has been disposed of.
    /// </remarks>
    public required long FileSize { get; init; }

    public required Func<Stream> FileStreamProvider { get; init; }

    public class Validator : AbstractValidator<FileDto>
    {
        public Validator()
        {
            RuleFor(dto => dto)
                .MustBeValidFile();
        }
    }
}
