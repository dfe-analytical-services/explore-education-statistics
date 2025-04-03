using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionMappingsCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }
    
    public PatchVersionConfigs? PatchVersionConfig { get; init; }
    
    public class Validator : AbstractValidator<NextDataSetVersionMappingsCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetId)
                .NotEmpty();
            
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
        }
    }
}

public record PatchVersionConfigs(bool IsIncrementingPatchVersion, Guid? OldReleaseFileId)
{
    public Guid? SourceReleaseFileId { get; init; } = OldReleaseFileId;
    
    public bool IsIncrementingPatchVersion { get; set; } = IsIncrementingPatchVersion;
}
