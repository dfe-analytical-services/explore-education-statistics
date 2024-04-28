using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The filter option criteria to filter results by in a data set query.
/// </summary>
public record DataSetQueryCriteriaFilters
{
    /// <summary>
    /// Filter the results to have a filter option matching this ID.
    /// </summary>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching this ID.
    /// </summary>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to have a filter option matching at least one of these IDs.
    /// </summary>
    public virtual IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching any of these IDs.
    /// </summary>
    public virtual IReadOnlyList<string>? NotIn { get; init; }

    public HashSet<string> GetOptions()
    {
        List<string?> filters =
        [
            Eq,
            NotEq,
            ..In ?? [],
            ..NotIn ?? []
        ];

        return filters
            .OfType<string>()
            .ToHashSet();
    }

    public class Validator : AbstractValidator<DataSetQueryCriteriaFilters>
    {
        public Validator()
        {
            RuleFor(q => q.Eq)
                .NotEmpty()
                .MaximumLength(10)
                .When(q => q.Eq is not null);

            RuleFor(q => q.NotEq)
                .NotEmpty()
                .MaximumLength(10)
                .When(q => q.NotEq is not null);

            When(q => q.In is not null, () =>
            {
                RuleFor(q => q.In)
                    .NotEmpty();

                RuleForEach(q => q.In)
                    .NotEmpty()
                    .MaximumLength(10);
            });

            When(q => q.NotIn is not null, () =>
            {
                RuleFor(q => q.NotIn)
                    .NotEmpty();

                RuleForEach(q => q.NotIn)
                    .NotEmpty()
                    .MaximumLength(10);
            });
        }
    }
}
