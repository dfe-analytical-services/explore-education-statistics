using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetTimePeriodsGeneratorExtensions
{
    public static Generator<ChangeSetTimePeriods> DefaultChangeSetTimePeriods(this DataFixture fixture, int changes)
        => fixture
            .Generator<ChangeSetTimePeriods>()
            .WithChanges(
                () => fixture.DefaultChange<TimePeriodChangeState>()
                    .GenerateList(changes)
            );

    public static Generator<ChangeSetTimePeriods> WithDefaults(this Generator<ChangeSetTimePeriods> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetTimePeriods> WithId(
        this Generator<ChangeSetTimePeriods> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetTimePeriods> WithDataSetVersion(
        this Generator<ChangeSetTimePeriods> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetTimePeriods> WithDataSetVersionId(
        this Generator<ChangeSetTimePeriods> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetTimePeriods> WithChanges(
        this Generator<ChangeSetTimePeriods> generator,
        Func<List<Change<TimePeriodChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetTimePeriods> SetDefaults(this InstanceSetters<ChangeSetTimePeriods> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetTimePeriods> SetId(
        this InstanceSetters<ChangeSetTimePeriods> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetTimePeriods> SetDataSetVersion(
        this InstanceSetters<ChangeSetTimePeriods> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetTimePeriods> SetDataSetVersionId(
        this InstanceSetters<ChangeSetTimePeriods> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetTimePeriods> SetChanges(
        this InstanceSetters<ChangeSetTimePeriods> setters,
        Func<List<Change<TimePeriodChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
