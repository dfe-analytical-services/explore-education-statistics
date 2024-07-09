using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record DataSetVersionProcessRequest
{
    public required Guid DataSetVersionId { get; init; }

    public class Validator : AbstractValidator<DataSetVersionProcessRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetVersionId)
                .NotEmpty();
        }
    }
}
