using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record InitialDataSetVersionCreateRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<InitialDataSetVersionCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
        }
    }
}
