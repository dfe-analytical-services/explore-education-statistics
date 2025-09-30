using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataSetFileVersionGeographicLevelGeneratorExtensions
{
    public static Generator<DataSetFileVersionGeographicLevel> DefaultDataSetFileVersionGeographicLevel(this DataFixture fixture)
        => fixture.Generator<DataSetFileVersionGeographicLevel>().WithDefaults();

    public static Generator<DataSetFileVersionGeographicLevel> WithDefaults(this Generator<DataSetFileVersionGeographicLevel> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<DataSetFileVersionGeographicLevel> SetDefaults(
        this InstanceSetters<DataSetFileVersionGeographicLevel> setters)
        => setters
            .SetDefault(p => p.DataSetFileVersionId);

    public static Generator<DataSetFileVersionGeographicLevel> WithDataSetFileVersion(
        this Generator<DataSetFileVersionGeographicLevel> generator,
        File dataSetFileVersion)
        => generator.ForInstance(s => s.SetDataSetFileVersion(dataSetFileVersion));

    public static Generator<DataSetFileVersionGeographicLevel> WithDataSetFileVersionId(
        this Generator<DataSetFileVersionGeographicLevel> generator,
        Guid dataSetFileVersionId)
        => generator.ForInstance(s => s.SetDataSetFileVersionId(dataSetFileVersionId));

    public static Generator<DataSetFileVersionGeographicLevel> WithGeographicLevel(
        this Generator<DataSetFileVersionGeographicLevel> generator,
        GeographicLevel geographicLevel)
        => generator.ForInstance(s => s.SetGeographicLevel(geographicLevel));

    public static InstanceSetters<DataSetFileVersionGeographicLevel> SetDataSetFileVersion(
        this InstanceSetters<DataSetFileVersionGeographicLevel> setters,
        File dataSetFileVersion)
        => setters
            .Set(f => f.DataSetFileVersion, dataSetFileVersion)
            .Set(f => f.DataSetFileVersionId, dataSetFileVersion.Id);

    public static InstanceSetters<DataSetFileVersionGeographicLevel> SetDataSetFileVersionId(
        this InstanceSetters<DataSetFileVersionGeographicLevel> setters,
        Guid dataSetFileVersionId)
        => setters.Set(f => f.DataSetFileVersionId, dataSetFileVersionId);

    public static InstanceSetters<DataSetFileVersionGeographicLevel> SetGeographicLevel(
        this InstanceSetters<DataSetFileVersionGeographicLevel> setters,
        GeographicLevel geographicLevel)
        => setters.Set(f => f.GeographicLevel, geographicLevel);
}
