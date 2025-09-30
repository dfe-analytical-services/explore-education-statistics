using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class GeographicLevelMetaChangeGeneratorExtensions
{
    public static Generator<GeographicLevelMetaChange> DefaultGeographicLevelMetaChange(
        this DataFixture fixture
    ) => fixture.Generator<GeographicLevelMetaChange>().WithDefaults();

    public static Generator<GeographicLevelMetaChange> WithDefaults(
        this Generator<GeographicLevelMetaChange> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<GeographicLevelMetaChange> WithDataSetVersion(
        this Generator<GeographicLevelMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<GeographicLevelMetaChange> WithDataSetVersionId(
        this Generator<GeographicLevelMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<GeographicLevelMetaChange> WithCurrentState(
        this Generator<GeographicLevelMetaChange> generator,
        GeographicLevelMeta? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<GeographicLevelMetaChange> WithCurrentState(
        this Generator<GeographicLevelMetaChange> generator,
        Func<GeographicLevelMeta?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<GeographicLevelMetaChange> WithCurrentStateId(
        this Generator<GeographicLevelMetaChange> generator,
        int? currentStateId
    ) => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<GeographicLevelMetaChange> WithPreviousState(
        this Generator<GeographicLevelMetaChange> generator,
        GeographicLevelMeta? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<GeographicLevelMetaChange> WithPreviousState(
        this Generator<GeographicLevelMetaChange> generator,
        Func<GeographicLevelMeta?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<GeographicLevelMetaChange> WithPreviousStateId(
        this Generator<GeographicLevelMetaChange> generator,
        int? previousStateId
    ) => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<GeographicLevelMetaChange> SetDefaults(
        this InstanceSetters<GeographicLevelMetaChange> setters
    ) => setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<GeographicLevelMetaChange> SetDataSetVersion(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) =>
        setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<GeographicLevelMetaChange> SetDataSetVersionId(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<GeographicLevelMetaChange> SetCurrentState(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        GeographicLevelMeta? currentState
    ) => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<GeographicLevelMetaChange> SetCurrentState(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        Func<GeographicLevelMeta?> currentState
    ) =>
        setters
            .Set(c => c.CurrentState, currentState)
            .Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<GeographicLevelMetaChange> SetCurrentStateId(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        int? currentStateId
    ) => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<GeographicLevelMetaChange> SetPreviousState(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        GeographicLevelMeta? currentState
    ) => setters.SetPreviousState(() => currentState);

    public static InstanceSetters<GeographicLevelMetaChange> SetPreviousState(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        Func<GeographicLevelMeta?> previousState
    ) =>
        setters
            .Set(c => c.PreviousState, previousState)
            .Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<GeographicLevelMetaChange> SetPreviousStateId(
        this InstanceSetters<GeographicLevelMetaChange> setters,
        int? previousStateId
    ) => setters.Set(c => c.PreviousStateId, previousStateId);
}
