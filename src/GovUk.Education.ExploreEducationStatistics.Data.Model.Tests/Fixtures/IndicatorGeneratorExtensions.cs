#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class IndicatorGeneratorExtensions
{
    public static Generator<Indicator> DefaultIndicator(this DataFixture fixture) =>
        fixture.Generator<Indicator>().WithDefaults();

    public static Generator<Indicator> WithDefaults(this Generator<Indicator> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<Indicator> WithLabel(this Generator<Indicator> generator, string label) =>
        generator.ForInstance(s => s.SetLabel(label));

    public static Generator<Indicator> WithIndicatorGroup(
        this Generator<Indicator> generator,
        IndicatorGroup indicatorGroup
    ) => generator.ForInstance(s => s.SetIndicatorGroup(indicatorGroup));

    public static InstanceSetters<Indicator> SetDefaults(this InstanceSetters<Indicator> setters) =>
        setters
            .SetDefault(i => i.Id)
            .SetDefault(i => i.Label)
            .SetDefault(i => i.Name)
            .Set(i => i.Name, (_, i) => i.Name.SnakeCase())
            .Set(i => i.Unit, IndicatorUnit.None)
            .Set(i => i.DecimalPlaces, 0);

    public static InstanceSetters<Indicator> SetLabel(this InstanceSetters<Indicator> setters, string label) =>
        setters.Set(f => f.Label, label).Set(f => f.Name, label.SnakeCase());

    public static InstanceSetters<Indicator> SetIndicatorGroup(
        this InstanceSetters<Indicator> setters,
        IndicatorGroup indicatorGroup
    ) =>
        setters
            .Set(
                i => i.IndicatorGroup,
                (_, indicator) =>
                {
                    indicatorGroup.Indicators.Add(indicator);
                    return indicatorGroup;
                }
            )
            .Set(i => i.IndicatorGroupId, indicatorGroup.Id);
}
