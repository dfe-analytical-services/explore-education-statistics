using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class IndicatorMetaGeneratorExtensions
{
    public static Generator<IndicatorMeta> DefaultIndicatorMeta(this DataFixture fixture)
        => fixture.Generator<IndicatorMeta>().WithDefaults();

    public static Generator<IndicatorMeta> DefaultIndicatorMeta(this DataFixture fixture, int options)
        => fixture.Generator<IndicatorMeta>()
            .WithDefaults()
            .WithOptions(fixture.DefaultIndicatorOptionMeta().Generate(options));

    public static Generator<IndicatorMeta> WithDefaults(this Generator<IndicatorMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<IndicatorMeta> WithDataSetVersion(
        this Generator<IndicatorMeta> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<IndicatorMeta> WithOptions(
        this Generator<IndicatorMeta> generator,
        IEnumerable<IndicatorOptionMeta> options)
        => generator.ForInstance(s => s.SetOptions(options));

    public static InstanceSetters<IndicatorMeta> SetDefaults(this InstanceSetters<IndicatorMeta> setters)
        => setters
            .SetDefault(m => m.Id);

    public static InstanceSetters<IndicatorMeta> SetDataSetVersion(
        this InstanceSetters<IndicatorMeta> setters,
        DataSetVersion dataSetVersion)
        => setters
            .Set(m => m.DataSetVersion, dataSetVersion)
            .Set(m => m.DataSetVersionId, dataSetVersion.Id);

    public static InstanceSetters<IndicatorMeta> SetOptions(
        this InstanceSetters<IndicatorMeta> setters,
        IEnumerable<IndicatorOptionMeta> options)
        => setters.Set(m => m.Options, options);
}
