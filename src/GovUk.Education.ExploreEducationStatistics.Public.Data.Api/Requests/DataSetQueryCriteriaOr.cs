using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A condition criteria where at least one sub-criteria must resolve
/// to true for the overall query to match any results.
///
/// This is equivalent to the `OR` operator in SQL.
/// </summary>
public record DataSetQueryCriteriaOr : DataSetQueryCriteria
{
    /// <summary>
    /// The sub-criteria where one must resolve to true.
    /// </summary>
    public required IReadOnlyList<DataSetQueryCriteria> Or { get; init; }

    public class Validator : AbstractValidator<DataSetQueryCriteriaOr>
    {
        public Validator()
        {
            RuleFor(q => q.Or)
                .NotEmpty();

            RuleForEach(q => q.Or)
                .NotNull()
                .SetInheritanceValidator(InheritanceValidator);
        }
    }
}
