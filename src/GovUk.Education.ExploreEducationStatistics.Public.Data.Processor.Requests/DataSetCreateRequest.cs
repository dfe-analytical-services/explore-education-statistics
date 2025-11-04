using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record DataSetCreateRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<DataSetCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseFileId).NotEmpty();
        }
    }
}
