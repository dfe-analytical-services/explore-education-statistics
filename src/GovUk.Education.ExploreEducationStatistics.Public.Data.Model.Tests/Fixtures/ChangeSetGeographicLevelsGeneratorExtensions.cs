using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetGeographicLevelsGeneratorExtensions
{
    public static Generator<ChangeSetGeographicLevels> DefaultChangeSetGeographicLevels(
        this DataFixture fixture,
        int changes)
        => fixture
            .Generator<ChangeSetGeographicLevels>()
            .WithChanges(() => fixture.DefaultChange<GeographicLevelChangeState>()
                    .GenerateList(changes));

    public static Generator<ChangeSetGeographicLevels> WithDefaults(this Generator<ChangeSetGeographicLevels> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetGeographicLevels> WithId(
        this Generator<ChangeSetGeographicLevels> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetGeographicLevels> WithDataSetVersion(
        this Generator<ChangeSetGeographicLevels> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetGeographicLevels> WithDataSetVersionId(
        this Generator<ChangeSetGeographicLevels> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetGeographicLevels> WithChanges(
        this Generator<ChangeSetGeographicLevels> generator,
        Func<List<Change<GeographicLevelChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetGeographicLevels> SetDefaults(
        this InstanceSetters<ChangeSetGeographicLevels> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetGeographicLevels> SetId(
        this InstanceSetters<ChangeSetGeographicLevels> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetGeographicLevels> SetDataSetVersion(
        this InstanceSetters<ChangeSetGeographicLevels> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetGeographicLevels> SetDataSetVersionId(
        this InstanceSetters<ChangeSetGeographicLevels> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetGeographicLevels> SetChanges(
        this InstanceSetters<ChangeSetGeographicLevels> setters,
        Func<List<Change<GeographicLevelChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
