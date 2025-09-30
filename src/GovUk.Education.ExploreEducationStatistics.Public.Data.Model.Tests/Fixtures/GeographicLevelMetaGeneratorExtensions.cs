using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class GeographicLevelMetaGeneratorExtensions
{
    public static Generator<GeographicLevelMeta> DefaultGeographicLevelMeta(
        this DataFixture fixture
    ) => fixture.Generator<GeographicLevelMeta>().WithDefaults();

    public static Generator<GeographicLevelMeta> WithDefaults(
        this Generator<GeographicLevelMeta> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<GeographicLevelMeta> WithDataSetVersion(
        this Generator<GeographicLevelMeta> generator,
        DataSetVersion dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<GeographicLevelMeta> WithLevels(
        this Generator<GeographicLevelMeta> generator,
        IEnumerable<GeographicLevel> levels
    ) => generator.ForInstance(s => s.SetLevels(levels));

    public static InstanceSetters<GeographicLevelMeta> SetDefaults(
        this InstanceSetters<GeographicLevelMeta> setters
    ) => setters.Set(m => m.Levels, f => f.Random.EnumValues<GeographicLevel>(3).ToList());

    public static InstanceSetters<GeographicLevelMeta> SetDataSetVersion(
        this InstanceSetters<GeographicLevelMeta> setters,
        DataSetVersion dataSetVersion
    ) =>
        setters
            .Set(m => m.DataSetVersion, dataSetVersion)
            .Set(m => m.DataSetVersionId, (_, m) => m.DataSetVersion.Id);

    public static InstanceSetters<GeographicLevelMeta> SetLevels(
        this InstanceSetters<GeographicLevelMeta> setters,
        IEnumerable<GeographicLevel> levels
    ) => setters.Set(m => m.Levels, levels.ToList());
}
