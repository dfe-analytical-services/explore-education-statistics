using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationMetaGeneratorExtensions
{
    public static Generator<LocationMeta> DefaultLocationMeta(this DataFixture fixture) =>
        fixture.Generator<LocationMeta>().WithDefaults();

    public static Generator<LocationMeta> DefaultLocationMeta<TOptionMeta>(
        this DataFixture fixture,
        int options
    )
        where TOptionMeta : LocationOptionMeta =>
        fixture
            .Generator<LocationMeta>()
            .WithDefaults()
            .WithOptions(() => fixture.Generator<TOptionMeta>().GenerateList(options));

    public static Generator<LocationMeta> DefaultLocationMeta(
        this DataFixture fixture,
        int options
    ) =>
        fixture
            .Generator<LocationMeta>()
            .WithDefaults()
            .WithOptions(() => fixture.DefaultLocationCodedOptionMeta().GenerateList(options));

    public static Generator<LocationMeta> WithDefaults(this Generator<LocationMeta> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationMeta> WithDataSetVersion(
        this Generator<LocationMeta> generator,
        DataSetVersion dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<LocationMeta> WithDataSetVersionId(
        this Generator<LocationMeta> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<LocationMeta> WithLevel(
        this Generator<LocationMeta> generator,
        GeographicLevel level
    ) => generator.ForInstance(s => s.SetLevel(level));

    public static Generator<LocationMeta> WithOptions(
        this Generator<LocationMeta> generator,
        IEnumerable<LocationOptionMeta> options
    ) => generator.ForInstance(s => s.SetOptions(options));

    public static Generator<LocationMeta> WithOptions(
        this Generator<LocationMeta> generator,
        Func<IEnumerable<LocationOptionMeta>> options
    ) => generator.ForInstance(s => s.SetOptions(options));

    public static Generator<LocationMeta> WithOptionLinks(
        this Generator<LocationMeta> generator,
        Func<IEnumerable<LocationOptionMetaLink>> links
    ) => generator.ForInstance(s => s.SetOptionLinks(links));

    public static InstanceSetters<LocationMeta> SetDefaults(
        this InstanceSetters<LocationMeta> setters
    ) =>
        setters.Set(
            m => m.Level,
            (_, _, context) =>
                GeographicLevelUtils.Levels[context.Index % GeographicLevelUtils.Levels.Count]
        );

    public static InstanceSetters<LocationMeta> SetDataSetVersion(
        this InstanceSetters<LocationMeta> setters,
        DataSetVersion dataSetVersion
    ) => setters.Set(m => m.DataSetVersion, dataSetVersion).SetDataSetVersionId(dataSetVersion.Id);

    public static InstanceSetters<LocationMeta> SetDataSetVersionId(
        this InstanceSetters<LocationMeta> setters,
        Guid dataSetVersionId
    ) => setters.Set(m => m.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<LocationMeta> SetLevel(
        this InstanceSetters<LocationMeta> setters,
        GeographicLevel level
    ) => setters.Set(m => m.Level, level);

    public static InstanceSetters<LocationMeta> SetOptions(
        this InstanceSetters<LocationMeta> setters,
        IEnumerable<LocationOptionMeta> options
    ) => setters.SetOptions(() => options);

    public static InstanceSetters<LocationMeta> SetOptions(
        this InstanceSetters<LocationMeta> setters,
        Func<IEnumerable<LocationOptionMeta>> options
    ) =>
        setters.Set(
            (_, m, context) =>
            {
                m.Options = options().ToList();

                if (context.Fixture is not null)
                {
                    m.OptionLinks = m
                        .Options.Select(o =>
                            context.Fixture.DefaultLocationOptionMetaLink().WithOption(o).Generate()
                        )
                        .ToList();
                }
            }
        );

    public static InstanceSetters<LocationMeta> SetOptionLinks(
        this InstanceSetters<LocationMeta> setters,
        Func<IEnumerable<LocationOptionMetaLink>> links
    ) => setters.Set(m => m.OptionLinks, () => links().ToList());
}
