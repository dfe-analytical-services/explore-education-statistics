using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The geographic levels criteria to filter results by in a data set query.
/// </summary>
public record DataSetQueryCriteriaGeographicLevels
{
    /// <summary>
    /// Filter the results to be in this geographic level.
    /// </summary>
    /// <example>NAT</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this geographic level.
    /// </summary>
    /// <example>PROV</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these geographic levels.
    /// </summary>
    /// <example>["REG", "LA"]</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public virtual IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these geographic levels.
    /// </summary>
    /// <example>["LAD", "SCH"]</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public virtual IReadOnlyList<string>? NotIn { get; init; }

    public GeographicLevel? ParsedEq()
        => Eq is not null ? EnumUtil.GetFromEnumValue<GeographicLevel>(Eq) : null;

    public GeographicLevel? ParsedNotEq()
        => NotEq is not null ? EnumUtil.GetFromEnumValue<GeographicLevel>(NotEq) : null;

    public IReadOnlyList<GeographicLevel>? ParsedIn()
        => In?.Select(EnumUtil.GetFromEnumValue<GeographicLevel>).ToList();

    public IReadOnlyList<GeographicLevel>? ParsedNotIn()
        => NotIn?.Select(EnumUtil.GetFromEnumValue<GeographicLevel>).ToList();

    public HashSet<GeographicLevel> GetOptions()
    {
        List<GeographicLevel?> options =
        [
            ParsedEq(),
            ParsedNotEq(),
            .. ParsedIn() ?? [],
            .. ParsedNotIn() ?? []
        ];

        return options
            .OfType<GeographicLevel>()
            .ToHashSet();
    }

    public static DataSetQueryCriteriaGeographicLevels Create(
        string comparator,
        IList<GeographicLevel> geographicLevels)
        => Create(comparator, geographicLevels.Select(l => l.GetEnumValue()).ToList());

    public static DataSetQueryCriteriaGeographicLevels Create(
        string comparator,
        IList<string> geographicLevels)
    {
        return comparator switch
        {
            nameof(Eq) => new DataSetQueryCriteriaGeographicLevels
            {
                Eq = geographicLevels.Count > 0 ? geographicLevels[0] : null
            },
            nameof(NotEq) => new DataSetQueryCriteriaGeographicLevels
            {
                NotEq = geographicLevels.Count > 0 ? geographicLevels[0] : null
            },
            nameof(In) => new DataSetQueryCriteriaGeographicLevels
            {
                In = geographicLevels.ToList()
            },
            nameof(NotIn) => new DataSetQueryCriteriaGeographicLevels
            {
                NotIn = geographicLevels.ToList()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };
    }

    public class Validator : AbstractValidator<DataSetQueryCriteriaGeographicLevels>
    {
        public Validator()
        {
            RuleFor(q => q.Eq)
                .AllowedValue(GeographicLevelUtils.OrderedCodes)
                .When(q => q.Eq is not null);

            RuleFor(q => q.NotEq)
                .AllowedValue(GeographicLevelUtils.OrderedCodes)
                .When(q => q.NotEq is not null);

            When(q => q.In is not null, () =>
            {
                RuleFor(q => q.In)
                    .NotEmpty();
                RuleForEach(q => q.In)
                    .AllowedValue(GeographicLevelUtils.OrderedCodes);
            });

            When(q => q.NotIn is not null, () =>
            {
                RuleFor(q => q.NotIn)
                    .NotEmpty();
                RuleForEach(q => q.NotIn)
                    .AllowedValue(GeographicLevelUtils.OrderedCodes);
            });
        }
    }
}
