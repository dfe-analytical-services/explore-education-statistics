using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationMetaChangeGeneratorExtensions
{
    public static Generator<LocationMetaChange> DefaultLocationMetaChange(this DataFixture fixture) =>
        fixture.Generator<LocationMetaChange>().WithDefaults();

    public static Generator<LocationMetaChange> WithDefaults(this Generator<LocationMetaChange> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationMetaChange> WithDataSetVersion(
        this Generator<LocationMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<LocationMetaChange> WithDataSetVersionId(
        this Generator<LocationMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<LocationMetaChange> WithCurrentState(
        this Generator<LocationMetaChange> generator,
        LocationMeta? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationMetaChange> WithCurrentState(
        this Generator<LocationMetaChange> generator,
        Func<LocationMeta?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationMetaChange> WithCurrentStateId(
        this Generator<LocationMetaChange> generator,
        int? currentStateId
    ) => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<LocationMetaChange> WithPreviousState(
        this Generator<LocationMetaChange> generator,
        LocationMeta? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationMetaChange> WithPreviousState(
        this Generator<LocationMetaChange> generator,
        Func<LocationMeta?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationMetaChange> WithPreviousStateId(
        this Generator<LocationMetaChange> generator,
        int? previousStateId
    ) => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<LocationMetaChange> SetDefaults(this InstanceSetters<LocationMetaChange> setters) =>
        setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<LocationMetaChange> SetDataSetVersion(
        this InstanceSetters<LocationMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) => setters.Set(c => c.DataSetVersion, dataSetVersion).Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<LocationMetaChange> SetDataSetVersionId(
        this InstanceSetters<LocationMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<LocationMetaChange> SetCurrentState(
        this InstanceSetters<LocationMetaChange> setters,
        LocationMeta? currentState
    ) => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<LocationMetaChange> SetCurrentState(
        this InstanceSetters<LocationMetaChange> setters,
        Func<LocationMeta?> currentState
    ) => setters.Set(c => c.CurrentState, currentState).Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<LocationMetaChange> SetCurrentStateId(
        this InstanceSetters<LocationMetaChange> setters,
        int? currentStateId
    ) => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<LocationMetaChange> SetPreviousState(
        this InstanceSetters<LocationMetaChange> setters,
        LocationMeta? previousState
    ) => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<LocationMetaChange> SetPreviousState(
        this InstanceSetters<LocationMetaChange> setters,
        Func<LocationMeta?> previousState
    ) => setters.Set(c => c.PreviousState, previousState).Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<LocationMetaChange> SetPreviousStateId(
        this InstanceSetters<LocationMetaChange> setters,
        int? previousStateId
    ) => setters.Set(c => c.PreviousStateId, previousStateId);
}
