using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

public record GetReleaseUpdatesRequest
{
    public required string PublicationSlug { get; init; }

    public required string ReleaseSlug { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<GetReleaseUpdatesRequest>
    {
        public Validator()
        {
            RuleFor(x => x.PublicationSlug)
                .NotEmpty();

            RuleFor(x => x.ReleaseSlug)
                .NotEmpty();

            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100);
        }
    }
}
