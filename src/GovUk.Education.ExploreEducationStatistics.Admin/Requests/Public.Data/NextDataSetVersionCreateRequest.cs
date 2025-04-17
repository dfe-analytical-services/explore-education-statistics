#nullable enable
using System;
using FluentValidation;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record NextDataSetVersionCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }
    
    public string? DataSetVersionToPatch { get; init; } = null;

    public class Validator : AbstractValidator<NextDataSetVersionCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();

            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
            
            RuleFor(request => request.DataSetVersionToPatch)
                .Must(version => version == null || SemVersion.TryParse(version.ToString(),
                    SemVersionStyles.OptionalMinorPatch
                    | SemVersionStyles.AllowWhitespace
                    | SemVersionStyles.AllowLowerV
                    ,out _))
                .WithMessage("DataSetVersionToPatch must be a valid semantic version if provided.");
        }
    }
}
