using FluentValidation;
using FluentValidation.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The location option criteria to filter results by in a data set query.
///
/// The results can be matched by either the location option's ID or a code.
/// Note the following differences:
///
/// - IDs only match a **single location**
/// - Codes may match **multiple locations**
///
/// Whilst codes are generally unique to a single location, they can be
/// used for multiple locations. This may match more results than you
/// expect so it's recommended to use IDs where possible.
/// </summary>
public record DataSetQueryCriteriaLocations
{
    /// <summary>
    /// Filter the results to be in this location.
    /// </summary>
    public IDataSetQueryLocation? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this location.
    /// </summary>
    public IDataSetQueryLocation? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these locations.
    /// </summary>
    public IReadOnlyList<IDataSetQueryLocation>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these locations.
    /// </summary>
    public IReadOnlyList<IDataSetQueryLocation>? NotIn { get; init; }

    public HashSet<IDataSetQueryLocation> GetOptions()
    {
        List<IDataSetQueryLocation?> locations = [Eq, NotEq, .. In ?? [], .. NotIn ?? []];

        return locations.OfType<IDataSetQueryLocation>().ToHashSet();
    }

    public static DataSetQueryCriteriaLocations Create(string comparator, IList<IDataSetQueryLocation> locations)
    {
        return comparator switch
        {
            nameof(Eq) => new DataSetQueryCriteriaLocations { Eq = locations.Count > 0 ? locations[0] : null },
            nameof(NotEq) => new DataSetQueryCriteriaLocations { NotEq = locations.Count > 0 ? locations[0] : null },
            nameof(In) => new DataSetQueryCriteriaLocations { In = locations.ToList() },
            nameof(NotIn) => new DataSetQueryCriteriaLocations { NotIn = locations.ToList() },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null),
        };
    }

    public class Validator : AbstractValidator<DataSetQueryCriteriaLocations>
    {
        public Validator()
        {
            RuleFor(request => request.Eq)
                .SetInheritanceValidator(InheritanceValidator!)
                .When(request => request.Eq is not null);

            RuleFor(request => request.NotEq)
                .SetInheritanceValidator(InheritanceValidator!)
                .When(request => request.NotEq is not null);

            When(
                q => q.In is not null,
                () =>
                {
                    RuleFor(request => request.In).NotEmpty();
                    RuleForEach(request => request.In).SetInheritanceValidator(InheritanceValidator);
                }
            );

            When(
                q => q.NotIn is not null,
                () =>
                {
                    RuleFor(request => request.NotIn).NotEmpty();
                    RuleForEach(request => request.NotIn).SetInheritanceValidator(InheritanceValidator);
                }
            );
        }

        private static void InheritanceValidator(
            PolymorphicValidator<DataSetQueryCriteriaLocations, IDataSetQueryLocation> validator
        )
        {
            validator.Add(new DataSetQueryLocationId.Validator());
            validator.Add(new DataSetQueryLocationCode.Validator());
            validator.Add(new DataSetQueryLocationLocalAuthorityCode.Validator());
            validator.Add(new DataSetQueryLocationLocalAuthorityOldCode.Validator());
            validator.Add(new DataSetQueryLocationProviderUkprn.Validator());
            validator.Add(new DataSetQueryLocationSchoolLaEstab.Validator());
            validator.Add(new DataSetQueryLocationSchoolUrn.Validator());
        }
    }
}
