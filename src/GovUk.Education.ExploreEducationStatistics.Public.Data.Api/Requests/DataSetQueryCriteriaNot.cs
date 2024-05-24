using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A condition criteria where its sub-criteria must resolve
/// to *false* for the overall query to match any results.
///
/// This is equivalent to the `NOT` operator in SQL.
/// </summary>
public record DataSetQueryCriteriaNot : DataSetQueryCriteria
{
    /// <summary>
    /// The sub-criteria which must resolve to false.
    /// </summary>
    public required DataSetQueryCriteria Not { get; init; }

    public class Validator : AbstractValidator<DataSetQueryCriteriaNot>
    {
        public Validator()
        {
            RuleFor(q => q.Not)
                .NotNull()
                .SetInheritanceValidator(InheritanceValidator);
        }
    }
}
