#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

/// <summary>
/// Orders the contents of mapping plans alphabetically (in natural order) by label.
/// <para>
/// Mapping plans are stored as jsonb, which does not preserve dictionary key order,
/// so ordering is applied when reading plans for display. New dictionaries are built
/// in sorted insertion order (which is the order they serialise in), and new plan
/// instances are returned so that tracked entities are never mutated.
/// </para>
/// </summary>
public static class MappingPlanOrdering
{
    public static LocationMappingPlan OrderByLabel(this LocationMappingPlan plan)
    {
        return new LocationMappingPlan
        {
            Levels = plan
                .Levels.NaturalOrderBy(level => level.Key.GetEnumLabel())
                .ToDictionary(
                    level => level.Key,
                    level =>
                        level.Value with
                        {
                            Mappings = OrderByLabel(level.Value.Mappings, mapping => mapping.Source.Label),
                            Candidates = OrderByLabel(level.Value.Candidates, candidate => candidate.Label),
                        }
                ),
        };
    }

    public static FilterMappingPlan OrderByLabel(this FilterMappingPlan plan)
    {
        return plan with
        {
            Mappings = OrderByLabel(
                plan.Mappings,
                filter => filter.Source.Label,
                filter =>
                    filter with
                    {
                        OptionMappings = OrderByLabel(filter.OptionMappings, option => option.Source.Label),
                    }
            ),
            Candidates = OrderByLabel(
                plan.Candidates,
                filter => filter.Label,
                filter => filter with { Options = OrderByLabel(filter.Options, option => option.Label) }
            ),
        };
    }

    public static IndicatorMappingPlan OrderByLabel(this IndicatorMappingPlan plan)
    {
        return plan with
        {
            Mappings = OrderByLabel(plan.Mappings, mapping => mapping.Source.Label),
            Candidates = OrderByLabel(plan.Candidates, candidate => candidate.Label),
        };
    }

    /// <summary>
    /// Orders by label, using the key as a tiebreak as labels are not
    /// necessarily unique (e.g. locations with the same name but different codes).
    /// Values can optionally be transformed (e.g. to order their own children).
    /// </summary>
    private static Dictionary<string, TValue> OrderByLabel<TValue>(
        Dictionary<string, TValue> dictionary,
        Func<TValue, string> labelSelector,
        Func<TValue, TValue>? valueSelector = null
    )
    {
        return dictionary
            .NaturalOrderBy(entry => labelSelector(entry.Value))
            .NaturalThenBy(entry => entry.Key)
            .ToDictionary(
                entry => entry.Key,
                entry => valueSelector is null ? entry.Value : valueSelector(entry.Value)
            );
    }
}
