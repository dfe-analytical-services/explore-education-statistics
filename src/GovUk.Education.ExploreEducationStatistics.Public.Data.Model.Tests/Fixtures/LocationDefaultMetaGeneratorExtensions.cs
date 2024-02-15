using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationDefaultMetaGeneratorExtensions
{
    private static readonly GeographicLevel[] DefaultMetaLevels = EnumUtil.GetEnumValues<GeographicLevel>()
        .Where(
            e => e is not (
                GeographicLevel.LocalAuthority or
                GeographicLevel.Provider or
                GeographicLevel.RscRegion or
                GeographicLevel.School)
        )
        .ToArray();

    public static Generator<LocationDefaultMeta> DefaultLocationMeta(this DataFixture fixture)
        => fixture.Generator<LocationDefaultMeta>()
            .WithDefaults();

    public static Generator<LocationDefaultMeta> DefaultLocationMeta(this DataFixture fixture, int options)
        => fixture.Generator<LocationDefaultMeta>()
            .WithDefaults()
            .WithOptions(fixture.DefaultLocationCodedOptionMeta().WithDefaults().GenerateList(options));

    public static Generator<LocationDefaultMeta> WithDefaults(this Generator<LocationDefaultMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationDefaultMeta> WithLevel<TMeta>(
        this Generator<LocationDefaultMeta> generator,
        GeographicLevel level)
        => generator.ForInstance(s => s.SetLevel(level));

    public static InstanceSetters<LocationDefaultMeta> SetDefaults(this InstanceSetters<LocationDefaultMeta> setters)
        => setters
            .Set(
                m => m.Level,
                (_, _, context) => DefaultMetaLevels[context.Index % DefaultMetaLevels.Length]
            );

    public static InstanceSetters<LocationDefaultMeta> SetLevel(
        this InstanceSetters<LocationDefaultMeta> setters,
        GeographicLevel level)
        => setters.Set(m => m.Level, level);
}
