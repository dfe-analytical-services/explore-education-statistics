using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class TimePeriodMetaGeneratorExtensions
{
    public static Generator<TimePeriodMeta> DefaultTimePeriodMeta(this DataFixture fixture)
        => fixture.Generator<TimePeriodMeta>().WithDefaults();

    public static Generator<TimePeriodMeta> DefaultTimePeriodMeta(this DataFixture fixture, int options)
        => fixture.Generator<TimePeriodMeta>()
            .WithDefaults()
            .WithOptions(fixture.DefaultTimePeriodOptionMeta().Generate(options));

    public static Generator<TimePeriodMeta> WithDefaults(this Generator<TimePeriodMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<TimePeriodMeta> WithDataSetVersion(
        this Generator<TimePeriodMeta> generator,
        DataSetVersion dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<TimePeriodMeta> WithOptions(
        this Generator<TimePeriodMeta> generator,
        IEnumerable<TimePeriodOptionMeta> options)
        => generator.ForInstance(s => s.SetOptions(options));

    public static InstanceSetters<TimePeriodMeta> SetDefaults(this InstanceSetters<TimePeriodMeta> setters)
        => setters
            .SetDefault(m => m.Id);

    public static InstanceSetters<TimePeriodMeta> SetDataSetVersion(
        this InstanceSetters<TimePeriodMeta> setters,
        DataSetVersion dataSetVersion)
        => setters
            .Set(m => m.DataSetVersion, dataSetVersion)
            .Set(m => m.DataSetVersionId, dataSetVersion.Id);

    public static InstanceSetters<TimePeriodMeta> SetOptions(
        this InstanceSetters<TimePeriodMeta> setters,
        IEnumerable<TimePeriodOptionMeta> options)
        => setters.Set(m => m.Options, options);
}
