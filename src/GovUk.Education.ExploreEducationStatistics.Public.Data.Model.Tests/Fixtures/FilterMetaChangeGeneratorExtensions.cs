using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterMetaChangeGeneratorExtensions
{
    public static Generator<FilterMetaChange> DefaultFilterMetaChange(this DataFixture fixture) =>
        fixture.Generator<FilterMetaChange>().WithDefaults();

    public static Generator<FilterMetaChange> WithDefaults(this Generator<FilterMetaChange> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterMetaChange> WithDataSetVersion(
        this Generator<FilterMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<FilterMetaChange> WithDataSetVersionId(
        this Generator<FilterMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<FilterMetaChange> WithCurrentState(
        this Generator<FilterMetaChange> generator,
        FilterMeta? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterMetaChange> WithCurrentState(
        this Generator<FilterMetaChange> generator,
        Func<FilterMeta?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterMetaChange> WithCurrentStateId(
        this Generator<FilterMetaChange> generator,
        int? currentStateId
    ) => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<FilterMetaChange> WithPreviousState(
        this Generator<FilterMetaChange> generator,
        FilterMeta? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterMetaChange> WithPreviousState(
        this Generator<FilterMetaChange> generator,
        Func<FilterMeta?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterMetaChange> WithPreviousStateId(
        this Generator<FilterMetaChange> generator,
        int? previousStateId
    ) => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<FilterMetaChange> SetDefaults(this InstanceSetters<FilterMetaChange> setters) =>
        setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<FilterMetaChange> SetDataSetVersion(
        this InstanceSetters<FilterMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) => setters.Set(c => c.DataSetVersion, dataSetVersion).Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<FilterMetaChange> SetDataSetVersionId(
        this InstanceSetters<FilterMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<FilterMetaChange> SetCurrentState(
        this InstanceSetters<FilterMetaChange> setters,
        FilterMeta? currentState
    ) => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<FilterMetaChange> SetCurrentState(
        this InstanceSetters<FilterMetaChange> setters,
        Func<FilterMeta?> currentState
    ) => setters.Set(c => c.CurrentState, currentState).Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<FilterMetaChange> SetCurrentStateId(
        this InstanceSetters<FilterMetaChange> setters,
        int? currentStateId
    ) => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<FilterMetaChange> SetPreviousState(
        this InstanceSetters<FilterMetaChange> setters,
        FilterMeta? previousState
    ) => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<FilterMetaChange> SetPreviousState(
        this InstanceSetters<FilterMetaChange> setters,
        Func<FilterMeta?> previousState
    ) => setters.Set(c => c.PreviousState, previousState).Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<FilterMetaChange> SetPreviousStateId(
        this InstanceSetters<FilterMetaChange> setters,
        int? previousStateId
    ) => setters.Set(c => c.PreviousStateId, previousStateId);
}
