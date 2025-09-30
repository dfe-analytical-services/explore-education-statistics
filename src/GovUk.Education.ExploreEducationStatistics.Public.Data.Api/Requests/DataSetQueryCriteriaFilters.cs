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
    /// <example>pVAkV</example>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching this ID.
    /// </summary>
    /// <example>wUzft</example>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to have a filter option matching at least one of these IDs.
    /// </summary>
    /// <example>["q1g3J", "ufp2K"]</example>
    public virtual IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not have a filter option matching any of these IDs.
    /// </summary>
    /// <example>["ksrK9", "s1J8a"]</example>
    public virtual IReadOnlyList<string>? NotIn { get; init; }

    public HashSet<string> GetOptions()
    {
        List<string?> filters =
        [
            Eq,
            NotEq,
            .. In ?? [],
            .. NotIn ?? []
        ];

        return filters
            .OfType<string>()
            .ToHashSet();
    }

    public static DataSetQueryCriteriaFilters Create(
        string comparator,
        IList<string> optionIds)
    {
        return comparator switch
        {
            nameof(Eq) => new DataSetQueryCriteriaFilters
            {
                Eq = optionIds.Count > 0 ? optionIds[0] : null
            },
            nameof(NotEq) => new DataSetQueryCriteriaFilters
            {
                NotEq = optionIds.Count > 0 ? optionIds[0] : null
            },
            nameof(In) => new DataSetQueryCriteriaFilters
            {
                In = optionIds.ToList()
            },
            nameof(NotIn) => new DataSetQueryCriteriaFilters
            {
                NotIn = optionIds.ToList()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };
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
