using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataSetFileGeographicLevelGeneratorExtensions
{
    public static Generator<DataSetFileGeographicLevel> DefaultDataSetFileGeographicLevel(this DataFixture fixture)
        => fixture.Generator<DataSetFileGeographicLevel>().WithDefaults();

    public static Generator<DataSetFileGeographicLevel> WithDefaults(this Generator<DataSetFileGeographicLevel> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<DataSetFileGeographicLevel> SetDefaults(
        this InstanceSetters<DataSetFileGeographicLevel> setters)
        => setters
            .SetDataSetFileVersionId(Guid.NewGuid())
            .SetGeographicLevel(GeographicLevel.Country);

    public static Generator<DataSetFileGeographicLevel> WithDataSetFileVersion(
        this Generator<DataSetFileGeographicLevel> generator,
        File dataSetFileVersion)
        => generator.ForInstance(s => s.SetDataSetFileVersion(dataSetFileVersion));

    public static Generator<DataSetFileGeographicLevel> WithDataSetFileVersionId(
        this Generator<DataSetFileGeographicLevel> generator,
        Guid dataSetFileVersionId)
        => generator.ForInstance(s => s.SetDataSetFileVersionId(dataSetFileVersionId));

    public static Generator<DataSetFileGeographicLevel> WithGeographicLevel(
        this Generator<DataSetFileGeographicLevel> generator,
        GeographicLevel geographicLevel)
        => generator.ForInstance(s => s.SetGeographicLevel(geographicLevel));

    public static InstanceSetters<DataSetFileGeographicLevel> SetDataSetFileVersion(
        this InstanceSetters<DataSetFileGeographicLevel> setters,
        File dataSetFileVersion)
        => setters.Set(f => f.DataSetFileVersion, dataSetFileVersion);

    public static InstanceSetters<DataSetFileGeographicLevel> SetDataSetFileVersionId(
        this InstanceSetters<DataSetFileGeographicLevel> setters,
        Guid dataSetFileVersionId)
        => setters.Set(f => f.DataSetFileVersionId, dataSetFileVersionId);

    public static InstanceSetters<DataSetFileGeographicLevel> SetGeographicLevel(
        this InstanceSetters<DataSetFileGeographicLevel> setters,
        GeographicLevel geographicLevel)
        => setters.Set(f => f.GeographicLevel, geographicLevel);
}
