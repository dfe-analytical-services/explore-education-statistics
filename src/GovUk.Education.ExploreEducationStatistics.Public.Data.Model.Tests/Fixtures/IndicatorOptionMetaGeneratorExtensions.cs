using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class IndicatorOptionMetaGeneratorExtensions
{
    public static Generator<IndicatorOptionMeta> DefaultIndicatorOptionMeta(this DataFixture fixture)
        => fixture.Generator<IndicatorOptionMeta>().WithDefaults();

    public static Generator<IndicatorOptionMeta> WithDefaults(this Generator<IndicatorOptionMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<IndicatorOptionMeta> WithIdentifier(this Generator<IndicatorOptionMeta> generator, string identifier)
        => generator.ForInstance(s => s.SetIdentifier(identifier));

    public static Generator<IndicatorOptionMeta> WithLabel(this Generator<IndicatorOptionMeta> generator, string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<IndicatorOptionMeta> WithUnit(this Generator<IndicatorOptionMeta> generator, IndicatorUnit? unit)
        => generator.ForInstance(s => s.SetUnit(unit));

    public static Generator<IndicatorOptionMeta> WithDecimalPlaces(
        this Generator<IndicatorOptionMeta> generator,
        byte? decimalPlaces)
        => generator.ForInstance(s => s.SetDecimalPlaces(decimalPlaces));

    public static InstanceSetters<IndicatorOptionMeta> SetDefaults(this InstanceSetters<IndicatorOptionMeta> setters)
        => setters
            .SetDefault(m => m.Identifier)
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Unit)
            .Set(
                m => m.DecimalPlaces,
                (f, im) => im switch
                {
                    { Unit: IndicatorUnit.PercentagePoint or IndicatorUnit.Percent } => f.Random.Byte(0, 3),
                    _ => (byte)0
                }
            );

    public static InstanceSetters<IndicatorOptionMeta> SetIdentifier(
        this InstanceSetters<IndicatorOptionMeta> setters,
        string identifier)
        => setters.Set(m => m.Identifier, identifier);

    public static InstanceSetters<IndicatorOptionMeta> SetLabel(
        this InstanceSetters<IndicatorOptionMeta> setters,
        string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<IndicatorOptionMeta> SetUnit(
        this InstanceSetters<IndicatorOptionMeta> setters,
        IndicatorUnit? unit)
        => setters.Set(m => m.Unit, unit);

    public static InstanceSetters<IndicatorOptionMeta> SetDecimalPlaces(
        this InstanceSetters<IndicatorOptionMeta> setters,
        byte? decimalPlaces)
        => setters.Set(m => m.DecimalPlaces, decimalPlaces);
}
