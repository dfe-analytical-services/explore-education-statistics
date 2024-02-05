using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class IndicatorMetaGeneratorExtensions
{
    public static Generator<IndicatorMeta> DefaultIndicatorMeta(this DataFixture fixture)
        => fixture.Generator<IndicatorMeta>().WithDefaults();

    public static Generator<IndicatorMeta> WithDefaults(this Generator<IndicatorMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<IndicatorMeta> WithIdentifier(this Generator<IndicatorMeta> generator, string identifier)
        => generator.ForInstance(s => s.SetIdentifier(identifier));

    public static Generator<IndicatorMeta> WithLabel(this Generator<IndicatorMeta> generator, string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<IndicatorMeta> WithUnit(this Generator<IndicatorMeta> generator, IndicatorUnit? unit)
        => generator.ForInstance(s => s.SetUnit(unit));

    public static Generator<IndicatorMeta> WithDecimalPlaces(
        this Generator<IndicatorMeta> generator,
        byte? decimalPlaces)
        => generator.ForInstance(s => s.SetDecimalPlaces(decimalPlaces));

    public static InstanceSetters<IndicatorMeta> SetDefaults(this InstanceSetters<IndicatorMeta> setters)
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

    public static InstanceSetters<IndicatorMeta> SetIdentifier(
        this InstanceSetters<IndicatorMeta> setters,
        string identifier)
        => setters.Set(m => m.Identifier, identifier);

    public static InstanceSetters<IndicatorMeta> SetLabel(
        this InstanceSetters<IndicatorMeta> setters,
        string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<IndicatorMeta> SetUnit(
        this InstanceSetters<IndicatorMeta> setters,
        IndicatorUnit? unit)
        => setters.Set(m => m.Unit, unit);

    public static InstanceSetters<IndicatorMeta> SetDecimalPlaces(
        this InstanceSetters<IndicatorMeta> setters,
        byte? decimalPlaces)
        => setters.Set(m => m.DecimalPlaces, decimalPlaces);
}
