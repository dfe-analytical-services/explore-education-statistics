using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public record NextDataSetVersionCompleteImportRequest
{
    public required Guid DataSetVersionId { get; init; }

    public class Validator : AbstractValidator<NextDataSetVersionCompleteImportRequest>
    {
        public Validator()
        {
            RuleFor(request => request.DataSetVersionId).NotEmpty();
        }
    }
}
