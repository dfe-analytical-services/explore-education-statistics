using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class IndicatorMetaChangeGeneratorExtensions
{
    public static Generator<IndicatorMetaChange> DefaultIndicatorMetaChange(
        this DataFixture fixture
    ) => fixture.Generator<IndicatorMetaChange>().WithDefaults();

    public static Generator<IndicatorMetaChange> WithDefaults(
        this Generator<IndicatorMetaChange> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<IndicatorMetaChange> WithDataSetVersion(
        this Generator<IndicatorMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<IndicatorMetaChange> WithDataSetVersionId(
        this Generator<IndicatorMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<IndicatorMetaChange> WithCurrentState(
        this Generator<IndicatorMetaChange> generator,
        IndicatorMeta? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<IndicatorMetaChange> WithCurrentState(
        this Generator<IndicatorMetaChange> generator,
        Func<IndicatorMeta?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<IndicatorMetaChange> WithCurrentStateId(
        this Generator<IndicatorMetaChange> generator,
        int? currentStateId
    ) => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<IndicatorMetaChange> WithPreviousState(
        this Generator<IndicatorMetaChange> generator,
        IndicatorMeta? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<IndicatorMetaChange> WithPreviousState(
        this Generator<IndicatorMetaChange> generator,
        Func<IndicatorMeta?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<IndicatorMetaChange> WithPreviousStateId(
        this Generator<IndicatorMetaChange> generator,
        int? previousStateId
    ) => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<IndicatorMetaChange> SetDefaults(
        this InstanceSetters<IndicatorMetaChange> setters
    ) => setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<IndicatorMetaChange> SetDataSetVersion(
        this InstanceSetters<IndicatorMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) =>
        setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<IndicatorMetaChange> SetDataSetVersionId(
        this InstanceSetters<IndicatorMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<IndicatorMetaChange> SetCurrentState(
        this InstanceSetters<IndicatorMetaChange> setters,
        IndicatorMeta? currentState
    ) => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<IndicatorMetaChange> SetCurrentState(
        this InstanceSetters<IndicatorMetaChange> setters,
        Func<IndicatorMeta?> currentState
    ) =>
        setters
            .Set(c => c.CurrentState, currentState)
            .Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<IndicatorMetaChange> SetCurrentStateId(
        this InstanceSetters<IndicatorMetaChange> setters,
        int? currentStateId
    ) => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<IndicatorMetaChange> SetPreviousState(
        this InstanceSetters<IndicatorMetaChange> setters,
        IndicatorMeta? previousState
    ) => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<IndicatorMetaChange> SetPreviousState(
        this InstanceSetters<IndicatorMetaChange> setters,
        Func<IndicatorMeta?> previousState
    ) =>
        setters
            .Set(c => c.PreviousState, previousState)
            .Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<IndicatorMetaChange> SetPreviousStateId(
        this InstanceSetters<IndicatorMetaChange> setters,
        int? previousStateId
    ) => setters.Set(c => c.PreviousStateId, previousStateId);
}
