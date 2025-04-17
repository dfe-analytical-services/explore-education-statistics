using FluentValidation;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionMappingsCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }

    public string? DataSetVersionToPatch { get; init; } = null;

    public class Validator : AbstractValidator<NextDataSetVersionMappingsCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();
            
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
            
            RuleFor(request => request.DataSetVersionToPatch)
                .Must(version => version == null || SemVersion.TryParse(version,
                    SemVersionStyles.OptionalMinorPatch
                    | SemVersionStyles.AllowWhitespace
                    | SemVersionStyles.AllowLowerV
                    ,out _))
                .WithMessage("DataSetVersionToPatch must be a valid semantic version if provided.");
        }
    }
}
