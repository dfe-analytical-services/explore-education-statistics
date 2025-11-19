#nullable enable
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Releases.Dtos;

public record ReleaseUpdatesGetRequest
{
    [FromRoute]
    public required Guid ReleaseVersionId { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public class Validator : AbstractValidator<ReleaseUpdatesGetRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseVersionId).NotEmpty();

            RuleFor(request => request.Page).GreaterThanOrEqualTo(1);

            RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        }
    }
}
