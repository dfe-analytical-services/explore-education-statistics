using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetFiltersGeneratorExtensions
{
    public static Generator<ChangeSetFilters> DefaultChangeSetFilters(this DataFixture fixture, int changes)
        => fixture
            .Generator<ChangeSetFilters>()
            .WithChanges(() => fixture.DefaultChange<FilterChangeState>()
                .GenerateList(changes));

    public static Generator<ChangeSetFilters> WithDefaults(this Generator<ChangeSetFilters> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetFilters> WithId(
        this Generator<ChangeSetFilters> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetFilters> WithDataSetVersion(
        this Generator<ChangeSetFilters> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetFilters> WithDataSetVersionId(
        this Generator<ChangeSetFilters> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetFilters> WithChanges(
        this Generator<ChangeSetFilters> generator,
        Func<List<Change<FilterChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetFilters> SetDefaults(this InstanceSetters<ChangeSetFilters> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetFilters> SetId(
        this InstanceSetters<ChangeSetFilters> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetFilters> SetDataSetVersion(
        this InstanceSetters<ChangeSetFilters> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetFilters> SetDataSetVersionId(
        this InstanceSetters<ChangeSetFilters> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetFilters> SetChanges(
        this InstanceSetters<ChangeSetFilters> setters,
        Func<List<Change<FilterChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
