#nullable enable
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications.Dtos;

public record GetPublicationReleasesRequest
{
    [FromRoute]
    public required string PublicationSlug { get; init; }

    /// <summary>
    /// The page of results to fetch. Required when <see cref="PageSize"/> is also specified.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// The maximum number of results per page. When omitted, all results are returned in a single page.
    /// </summary>
    public int? PageSize { get; init; }

    public class Validator : AbstractValidator<GetPublicationReleasesRequest>
    {
        public Validator()
        {
            RuleFor(request => request.PublicationSlug).NotEmpty();

            RuleFor(request => request.Page).GreaterThanOrEqualTo(1).When(request => request.Page.HasValue);

            RuleFor(request => request.PageSize).InclusiveBetween(1, 100).When(request => request.PageSize.HasValue);

            RuleFor(request => request.Page)
                .NotNull()
                .WithMessage($"'{nameof(Page)}' must also be provided when '{nameof(PageSize)}' is set.")
                .When(request => request.PageSize.HasValue);

            RuleFor(request => request.PageSize)
                .NotNull()
                .WithMessage($"'{nameof(PageSize)}' must also be provided when '{nameof(Page)}' is set.")
                .When(request => request.Page.HasValue);
        }
    }
}
