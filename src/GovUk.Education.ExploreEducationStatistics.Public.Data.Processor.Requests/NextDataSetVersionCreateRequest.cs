using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionCreateRequest
{
    public required Guid DataSetId { get; init; }

    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<NextDataSetVersionCreateRequest>
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
