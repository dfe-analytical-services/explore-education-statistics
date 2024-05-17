using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A condition criteria where one or more sub-criteria must all resolve
/// to true for the overall query to match any results.
///
/// This is equivalent to the `AND` operator in SQL.
/// </summary>
public record DataSetQueryCriteriaAnd : DataSetQueryCriteria
{
    /// <summary>
    /// The sub-criteria which all must resolve to true.
    /// </summary>
    public required IReadOnlyList<DataSetQueryCriteria> And { get; init; }

    public class Validator : AbstractValidator<DataSetQueryCriteriaAnd>
    {
        public Validator()
        {
            RuleFor(q => q.And)
                .NotEmpty();

            RuleForEach(q => q.And)
                .NotNull()
                .SetInheritanceValidator(InheritanceValidator);
        }
    }
}
