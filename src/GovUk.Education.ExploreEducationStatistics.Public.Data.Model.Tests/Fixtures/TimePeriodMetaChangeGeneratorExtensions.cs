using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class TimePeriodMetaChangeGeneratorExtensions
{
    public static Generator<TimePeriodMetaChange> DefaultTimePeriodMetaChange(
        this DataFixture fixture
    ) => fixture.Generator<TimePeriodMetaChange>().WithDefaults();

    public static Generator<TimePeriodMetaChange> WithDefaults(
        this Generator<TimePeriodMetaChange> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<TimePeriodMetaChange> WithDataSetVersion(
        this Generator<TimePeriodMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<TimePeriodMetaChange> WithDataSetVersionId(
        this Generator<TimePeriodMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<TimePeriodMetaChange> WithCurrentState(
        this Generator<TimePeriodMetaChange> generator,
        TimePeriodMeta? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<TimePeriodMetaChange> WithCurrentState(
        this Generator<TimePeriodMetaChange> generator,
        Func<TimePeriodMeta?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<TimePeriodMetaChange> WithCurrentStateId(
        this Generator<TimePeriodMetaChange> generator,
        int? currentStateId
    ) => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<TimePeriodMetaChange> WithPreviousState(
        this Generator<TimePeriodMetaChange> generator,
        TimePeriodMeta? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<TimePeriodMetaChange> WithPreviousState(
        this Generator<TimePeriodMetaChange> generator,
        Func<TimePeriodMeta?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<TimePeriodMetaChange> WithPreviousStateId(
        this Generator<TimePeriodMetaChange> generator,
        int? previousStateId
    ) => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<TimePeriodMetaChange> SetDefaults(
        this InstanceSetters<TimePeriodMetaChange> setters
    ) => setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<TimePeriodMetaChange> SetDataSetVersion(
        this InstanceSetters<TimePeriodMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) =>
        setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<TimePeriodMetaChange> SetDataSetVersionId(
        this InstanceSetters<TimePeriodMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<TimePeriodMetaChange> SetCurrentState(
        this InstanceSetters<TimePeriodMetaChange> setters,
        TimePeriodMeta? previousState
    ) => setters.SetCurrentState(() => previousState);

    public static InstanceSetters<TimePeriodMetaChange> SetCurrentState(
        this InstanceSetters<TimePeriodMetaChange> setters,
        Func<TimePeriodMeta?> currentState
    ) =>
        setters
            .Set(c => c.CurrentState, currentState)
            .Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<TimePeriodMetaChange> SetCurrentStateId(
        this InstanceSetters<TimePeriodMetaChange> setters,
        int? currentStateId
    ) => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<TimePeriodMetaChange> SetPreviousState(
        this InstanceSetters<TimePeriodMetaChange> setters,
        TimePeriodMeta? previousState
    ) => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<TimePeriodMetaChange> SetPreviousState(
        this InstanceSetters<TimePeriodMetaChange> setters,
        Func<TimePeriodMeta?> previousState
    ) =>
        setters
            .Set(c => c.PreviousState, previousState)
            .Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<TimePeriodMetaChange> SetPreviousStateId(
        this InstanceSetters<TimePeriodMetaChange> setters,
        int? previousStateId
    ) => setters.Set(c => c.PreviousStateId, previousStateId);
}
