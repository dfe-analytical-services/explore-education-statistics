using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A condition criteria where one or more sub-criteria must all resolve
/// to true for the overall query to match any results.
///
/// This is equivalent to the `AND` operator in SQL.
/// </summary>
public record DataSetQueryCriteriaAnd : IDataSetQueryCriteria
{
    /// <summary>
    /// The sub-criteria which all must resolve to true.
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
    public required IReadOnlyList<IDataSetQueryCriteria> And { get; init; }

    public class Validator : AbstractValidator<DataSetQueryCriteriaAnd>
    {
        public Validator()
        {
            RuleFor(q => q.And).NotEmpty();

            RuleForEach(q => q.And).NotNull().SetInheritanceValidator(IDataSetQueryCriteria.InheritanceValidator);
        }
    }
}
