using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetFilterOptionsGeneratorExtensions
{
    public static Generator<ChangeSetFilterOptions> DefaultChangeSetFilterOptions(this DataFixture fixture, int changes)
        => fixture
            .Generator<ChangeSetFilterOptions>()
            .WithChanges(() => fixture.DefaultChange<FilterOptionChangeState>()
                    .GenerateList(changes));

    public static Generator<ChangeSetFilterOptions> WithDefaults(this Generator<ChangeSetFilterOptions> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetFilterOptions> WithId(
        this Generator<ChangeSetFilterOptions> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetFilterOptions> WithDataSetVersion(
        this Generator<ChangeSetFilterOptions> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetFilterOptions> WithDataSetVersionId(
        this Generator<ChangeSetFilterOptions> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetFilterOptions> WithChanges(
        this Generator<ChangeSetFilterOptions> generator,
        Func<List<Change<FilterOptionChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetFilterOptions> SetDefaults(
        this InstanceSetters<ChangeSetFilterOptions> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetFilterOptions> SetId(
        this InstanceSetters<ChangeSetFilterOptions> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetFilterOptions> SetDataSetVersion(
        this InstanceSetters<ChangeSetFilterOptions> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetFilterOptions> SetDataSetVersionId(
        this InstanceSetters<ChangeSetFilterOptions> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetFilterOptions> SetChanges(
        this InstanceSetters<ChangeSetFilterOptions> setters,
        Func<List<Change<FilterOptionChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
