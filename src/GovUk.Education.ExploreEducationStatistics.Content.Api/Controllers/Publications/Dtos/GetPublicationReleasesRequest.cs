#nullable enable
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications.Dtos;

public record GetPublicationReleasesRequest
{
    [FromRoute]
    public required string PublicationSlug { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<GetPublicationReleasesRequest>
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
