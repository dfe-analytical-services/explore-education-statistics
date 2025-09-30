using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A condition criteria where at least one sub-criteria must resolve
/// to true for the overall query to match any results.
///
/// This is equivalent to the `OR` operator in SQL.
/// </summary>
public record DataSetQueryCriteriaOr : IDataSetQueryCriteria
{
    /// <summary>
    /// The sub-criteria where one must resolve to true.
    /// </summary>
    /// <example>
    /// [
    ///     {
    ///         "filters": {
    ///             "eq": "pVAkV"
    ///         }
    ///     },
    ///     {
    ///         "locations": {
    ///             "eq": {
    ///                 "level": "LA",
    ///                 "code": "E08000019"
    ///             }
    ///         }
    ///     }
    /// ]
    /// </example>
    public required IReadOnlyList<IDataSetQueryCriteria> Or { get; init; }

    public class Validator : AbstractValidator<DataSetQueryCriteriaOr>
    {
        public Validator()
        {
            RuleFor(q => q.Or).NotEmpty();

            RuleForEach(q => q.Or)
                .NotNull()
                .SetInheritanceValidator(IDataSetQueryCriteria.InheritanceValidator);
        }
    }
}
