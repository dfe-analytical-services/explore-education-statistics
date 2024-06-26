using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeSetIndicatorsGeneratorExtensions
{
    public static Generator<ChangeSetIndicators> DefaultChangeSetIndicators(this DataFixture fixture, int changes)
        => fixture
            .Generator<ChangeSetIndicators>()
            .WithChanges(() => fixture.DefaultChange<IndicatorChangeState>()
                .GenerateList(changes));

    public static Generator<ChangeSetIndicators> WithDefaults(this Generator<ChangeSetIndicators> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<ChangeSetIndicators> WithId(
        this Generator<ChangeSetIndicators> generator,
        Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static Generator<ChangeSetIndicators> WithDataSetVersion(
        this Generator<ChangeSetIndicators> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<ChangeSetIndicators> WithDataSetVersionId(
        this Generator<ChangeSetIndicators> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<ChangeSetIndicators> WithChanges(
        this Generator<ChangeSetIndicators> generator,
        Func<List<Change<IndicatorChangeState>>> changes)
        => generator.ForInstance(s => s.SetChanges(changes));

    public static InstanceSetters<ChangeSetIndicators> SetDefaults(this InstanceSetters<ChangeSetIndicators> setters)
        => setters
            .SetDefault(cs => cs.Id);

    public static InstanceSetters<ChangeSetIndicators> SetId(
        this InstanceSetters<ChangeSetIndicators> setters,
        Guid id)
        => setters.Set(cs => cs.Id, id);

    public static InstanceSetters<ChangeSetIndicators> SetDataSetVersion(
        this InstanceSetters<ChangeSetIndicators> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(cs => cs.DataSetVersion, dataSetVersion())
            .Set(cs => cs.DataSetVersionId, (_, cs) => cs.DataSetVersion.Id);

    public static InstanceSetters<ChangeSetIndicators> SetDataSetVersionId(
        this InstanceSetters<ChangeSetIndicators> setters,
        Guid dataSetVersionId)
        => setters.Set(cs => cs.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<ChangeSetIndicators> SetChanges(
        this InstanceSetters<ChangeSetIndicators> setters,
        Func<List<Change<IndicatorChangeState>>> changes)
        => setters.Set(cs => cs.Changes, changes());
}
