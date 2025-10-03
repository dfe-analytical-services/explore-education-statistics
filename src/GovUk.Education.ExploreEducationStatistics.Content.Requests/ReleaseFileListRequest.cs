using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Content.Requests;

public record ReleaseFileListRequest
{
    public required IReadOnlyList<Guid> Ids { get; init; }

    public class Validator : AbstractValidator<ReleaseFileListRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Ids).NotEmpty();
        }
    }
}
