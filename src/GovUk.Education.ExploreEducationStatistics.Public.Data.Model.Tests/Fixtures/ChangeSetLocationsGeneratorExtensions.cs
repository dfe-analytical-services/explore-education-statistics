using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetLocationsGeneratorExtensions
{
    public static Generator<ChangeSetLocations> DefaultChangeSetLocations(this DataFixture fixture, int changes)
        => fixture
            .Generator<ChangeSetLocations>()
            .WithChanges(() => fixture.DefaultChange<LocationChangeState>()
                .GenerateList(changes));

    public static Generator<ChangeSetLocations> WithDefaults(this Generator<ChangeSetLocations> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetLocations> WithId(
        this Generator<ChangeSetLocations> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetLocations> WithDataSetVersion(
        this Generator<ChangeSetLocations> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetLocations> WithDataSetVersionId(
        this Generator<ChangeSetLocations> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetLocations> WithChanges(
        this Generator<ChangeSetLocations> generator,
        Func<List<Change<LocationChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetLocations> SetDefaults(this InstanceSetters<ChangeSetLocations> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetLocations> SetId(
        this InstanceSetters<ChangeSetLocations> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetLocations> SetDataSetVersion(
        this InstanceSetters<ChangeSetLocations> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetLocations> SetDataSetVersionId(
        this InstanceSetters<ChangeSetLocations> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetLocations> SetChanges(
        this InstanceSetters<ChangeSetLocations> setters,
        Func<List<Change<LocationChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
