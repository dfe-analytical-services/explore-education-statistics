using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases.Dtos;

public record GetPaginatedReleasesForPublicationRequest
{
    [FromRoute]
    public required string PublicationSlug { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<GetPaginatedReleasesForPublicationRequest>
    {
        public Validator()
        {
            RuleFor(request => request.PublicationSlug)
                .NotEmpty();

            RuleFor(request => request.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100);
        }
    }
}
