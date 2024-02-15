using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationMetaGeneratorExtensions
{
    public static Generator<TMeta> DefaultLocationMeta<TMeta, TOptionMeta>(this DataFixture fixture, int options)
        where TMeta : LocationMeta, ILocationMetaWithOptions<TOptionMeta>
        where TOptionMeta : LocationOptionMetaBase
        => fixture.Generator<TMeta>()
            .WithOptions(fixture.Generator<TOptionMeta>().Generate(options));

    public static Generator<TMeta> WithDataSetVersion<TMeta>(
        this Generator<TMeta> generator,
        DataSetVersion dataSetVersion) where TMeta : LocationMeta
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<TMeta> WithOptions<TMeta, TOptionMeta>(
        this Generator<TMeta> generator,
        IEnumerable<TOptionMeta> options)
        where TMeta : LocationMeta, ILocationMetaWithOptions<TOptionMeta>
        where TOptionMeta : LocationOptionMetaBase
        => generator.ForInstance(s => s.SetOptions(options));

    public static Generator<TMeta> WithOptions<TMeta, TOptionMeta>(
        this Generator<TMeta> generator,
        Func<IEnumerable<TOptionMeta>> options)
        where TMeta : LocationMeta, ILocationMetaWithOptions<TOptionMeta>
        where TOptionMeta : LocationOptionMetaBase
        => generator.ForInstance(s => s.SetOptions(options));

    public static InstanceSetters<TMeta> SetDataSetVersion<TMeta>(
        this InstanceSetters<TMeta> setters,
        DataSetVersion dataSetVersion) where TMeta : LocationMeta
        => setters
            .Set(m => m.DataSetVersion, dataSetVersion)
            .Set(m => m.DataSetVersionId, dataSetVersion.Id);

    public static InstanceSetters<TMeta> SetOptions<TMeta, TOptionMeta>(
        this InstanceSetters<TMeta> setters,
        IEnumerable<TOptionMeta> options)
        where TMeta : LocationMeta, ILocationMetaWithOptions<TOptionMeta>
        where TOptionMeta : LocationOptionMetaBase
        => setters.Set(m => m.Options, options);

    public static InstanceSetters<TMeta> SetOptions<TMeta, TOptionMeta>(
        this InstanceSetters<TMeta> setters,
        Func<IEnumerable<TOptionMeta>> options)
        where TMeta : LocationMeta, ILocationMetaWithOptions<TOptionMeta>
        where TOptionMeta : LocationOptionMetaBase
        => setters.Set(m => m.Options, () => options().ToList());
}
