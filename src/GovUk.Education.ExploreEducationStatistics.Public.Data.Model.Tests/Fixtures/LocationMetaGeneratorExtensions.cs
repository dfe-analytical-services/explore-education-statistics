using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationMetaGeneratorExtensions
{
    public static Generator<LocationMeta> DefaultLocationMeta(this DataFixture fixture)
        => fixture.Generator<LocationMeta>().WithDefaults();

    public static Generator<LocationMeta> DefaultLocationMeta(this DataFixture fixture, int options)
        => fixture.Generator<LocationMeta>()
            .WithDefaults()
            .WithOptions(fixture.DefaultLocationOptionMeta().GenerateList(options));

    public static Generator<LocationMeta> WithDefaults(this Generator<LocationMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationMeta> WithLevel(this Generator<LocationMeta> generator, GeographicLevel level)
        => generator.ForInstance(s => s.SetLevel(level));

    public static Generator<LocationMeta> WithOptions(
        this Generator<LocationMeta> generator,
        IEnumerable<LocationOptionMeta> options)
        => generator.ForInstance(s => s.SetOptions(options));

    public static Generator<LocationMeta> WithOptions(
        this Generator<LocationMeta> generator,
        Func<IEnumerable<LocationOptionMeta>> options)
        => generator.ForInstance(s => s.SetOptions(options));

    public static InstanceSetters<LocationMeta> SetDefaults(this InstanceSetters<LocationMeta> setters)
        => setters
            .Set(
                m => m.Level,
                (_, _, context) =>
                    GeographicLevelUtils.Levels[context.Index % GeographicLevelUtils.Levels.Length]
            );

    public static InstanceSetters<LocationMeta> SetLevel(
        this InstanceSetters<LocationMeta> setters,
        GeographicLevel level)
        => setters.Set(m => m.Level, level);

    public static InstanceSetters<LocationMeta> SetOptions(
        this InstanceSetters<LocationMeta> setters,
        IEnumerable<LocationOptionMeta> options)
        => setters.Set(m => m.Options, options);

    public static InstanceSetters<LocationMeta> SetOptions(
        this InstanceSetters<LocationMeta> setters,
        Func<IEnumerable<LocationOptionMeta>> options)
        => setters.Set(m => m.Options, () => options().ToList());
}
