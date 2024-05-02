using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record ProcessorTriggerRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<ProcessorTriggerRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty().WithMessage("ReleaseFileId must not be empty");
        }
    }
}
