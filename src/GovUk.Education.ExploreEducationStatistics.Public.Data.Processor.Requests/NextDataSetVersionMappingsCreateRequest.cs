using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionMappingsCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }

    public DataSetVersionNumber? DataSetVersionToPatch { get; init; } = null;
    
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
