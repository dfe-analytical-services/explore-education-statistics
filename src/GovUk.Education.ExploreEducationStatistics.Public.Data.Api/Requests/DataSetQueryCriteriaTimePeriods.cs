using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The time period criteria to filter results by in a data set query.
/// </summary>
public record DataSetQueryCriteriaTimePeriods
{
    /// <summary>
    /// Filter the results to be in this time period.
    /// </summary>
    public DataSetQueryTimePeriod? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this time period.
    /// </summary>
    public DataSetQueryTimePeriod? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these time periods.
    /// </summary>
    public IReadOnlyList<DataSetQueryTimePeriod>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these time periods.
    /// </summary>
    public IReadOnlyList<DataSetQueryTimePeriod>? NotIn { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Gt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically greater than or equal to the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Gte { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Lt { get; init; }

    /// <summary>
    /// Filter the results to be in a time period that is
    /// chronologically less than or equal to the one specified.
    /// </summary>
    public DataSetQueryTimePeriod? Lte { get; init; }

    public HashSet<DataSetQueryTimePeriod> GetOptions()
    {
        List<DataSetQueryTimePeriod?> timePeriods =
        [
            Eq,
            NotEq,
            Gt,
            Gte,
            Lt,
            Lte,
            .. In ?? [],
            .. NotIn ?? [],
        ];

        return timePeriods.OfType<DataSetQueryTimePeriod>().ToHashSet();
    }

    public static DataSetQueryCriteriaTimePeriods Create(
        string comparator,
        IList<DataSetQueryTimePeriod> timePeriods
    )
    {
        return comparator switch
        {
            nameof(Eq) => new DataSetQueryCriteriaTimePeriods
            {
                Eq = timePeriods.Count > 0 ? timePeriods[0] : null,
            },
            nameof(NotEq) => new DataSetQueryCriteriaTimePeriods
            {
                NotEq = timePeriods.Count > 0 ? timePeriods[0] : null,
            },
            nameof(In) => new DataSetQueryCriteriaTimePeriods { In = timePeriods.ToList() },
            nameof(NotIn) => new DataSetQueryCriteriaTimePeriods { NotIn = timePeriods.ToList() },
            nameof(Gt) => new DataSetQueryCriteriaTimePeriods
            {
                Gt = timePeriods.Count > 0 ? timePeriods[0] : null,
            },
            nameof(Gte) => new DataSetQueryCriteriaTimePeriods
            {
                Gte = timePeriods.Count > 0 ? timePeriods[0] : null,
            },
            nameof(Lt) => new DataSetQueryCriteriaTimePeriods
            {
                Lt = timePeriods.Count > 0 ? timePeriods[0] : null,
            },
            nameof(Lte) => new DataSetQueryCriteriaTimePeriods
            {
                Lte = timePeriods.Count > 0 ? timePeriods[0] : null,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null),
        };
    }

    public class Validator : AbstractValidator<DataSetQueryCriteriaTimePeriods>
    {
        public Validator()
        {
            RuleFor(request => request.Eq)
                .SetValidator(new DataSetQueryTimePeriod.Validator()!)
                .When(request => request.Eq is not null);

            RuleFor(request => request.NotEq)!
                .SetValidator(new DataSetQueryTimePeriod.Validator()!)
                .When(request => request.NotEq is not null);

            When(
                q => q.In is not null,
                () =>
                {
                    RuleFor(request => request.In).NotEmpty();
                    RuleForEach(request => request.In)
                        .SetValidator(new DataSetQueryTimePeriod.Validator());
                }
            );

            When(
                q => q.NotIn is not null,
                () =>
                {
                    RuleFor(request => request.NotIn).NotEmpty();
                    RuleForEach(request => request.NotIn)
                        .SetValidator(new DataSetQueryTimePeriod.Validator());
                }
            );

            RuleFor(request => request.Gt)
                .SetValidator(new DataSetQueryTimePeriod.Validator()!)
                .When(request => request.Gt is not null);

            RuleFor(request => request.Gte)
                .SetValidator(new DataSetQueryTimePeriod.Validator()!)
                .When(request => request.Gte is not null);

            RuleFor(request => request.Lt)
                .SetValidator(new DataSetQueryTimePeriod.Validator()!)
                .When(request => request.Lt is not null);

            RuleFor(request => request.Lte)
                .SetValidator(new DataSetQueryTimePeriod.Validator()!)
                .When(request => request.Lte is not null);
        }
    }
}
