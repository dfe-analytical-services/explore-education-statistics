using System.ComponentModel;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

public record PublicationListRequest
{
    /// <summary>
    /// A search term to find matching publications.
    /// </summary>
    /// <example>Pupil absence</example>
    public string? Search { get; init; }

    /// <summary>
    /// The page of results to fetch.
    /// </summary>
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    /// <summary>
    /// The maximum number of results per page.
    /// </summary>
    [DefaultValue(20)]
    public int PageSize { get; init; } = 20;

    public class Validator : AbstractValidator<PublicationListRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Search).MinimumLength(3);
            RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
            RuleFor(request => request.PageSize).InclusiveBetween(1, 40);
        }
    }
}
