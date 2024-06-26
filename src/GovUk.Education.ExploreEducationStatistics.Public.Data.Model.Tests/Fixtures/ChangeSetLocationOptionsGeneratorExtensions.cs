using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetLocationOptionsGeneratorExtensions
{
    public static Generator<ChangeSetLocationOptions> DefaultChangeSetLocationOptions(
        this DataFixture fixture,
        int changes)
        => fixture
            .Generator<ChangeSetLocationOptions>()
            .WithChanges(() => fixture.DefaultChange<LocationOptionChangeState>()
                    .GenerateList(changes));

    public static Generator<ChangeSetLocationOptions> WithDefaults(this Generator<ChangeSetLocationOptions> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetLocationOptions> WithId(
        this Generator<ChangeSetLocationOptions> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetLocationOptions> WithDataSetVersion(
        this Generator<ChangeSetLocationOptions> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetLocationOptions> WithDataSetVersionId(
        this Generator<ChangeSetLocationOptions> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetLocationOptions> WithChanges(
        this Generator<ChangeSetLocationOptions> generator,
        Func<List<Change<LocationOptionChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetLocationOptions> SetDefaults(
        this InstanceSetters<ChangeSetLocationOptions> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetLocationOptions> SetId(
        this InstanceSetters<ChangeSetLocationOptions> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetLocationOptions> SetDataSetVersion(
        this InstanceSetters<ChangeSetLocationOptions> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetLocationOptions> SetDataSetVersionId(
        this InstanceSetters<ChangeSetLocationOptions> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetLocationOptions> SetChanges(
        this InstanceSetters<ChangeSetLocationOptions> setters,
        Func<List<Change<LocationOptionChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
