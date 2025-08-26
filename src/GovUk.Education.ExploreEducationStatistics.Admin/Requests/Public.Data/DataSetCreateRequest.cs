#nullable enable
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

public record DataSetCreateRequest
{
    public required Guid ReleaseFileId { get; init; }

    public class Validator : AbstractValidator<DataSetCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseFileId)
                .NotEmpty();
        }
    }
}
