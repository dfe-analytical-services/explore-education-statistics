using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionMetaChangeGeneratorExtensions
{
    public static Generator<LocationOptionMetaChange> DefaultLocationOptionMetaChange(this DataFixture fixture)
        => fixture
            .Generator<LocationOptionMetaChange>()
            .WithDefaults();

    public static Generator<LocationOptionMetaChange> WithDefaults(this Generator<LocationOptionMetaChange> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionMetaChange> WithDataSetVersion(
        this Generator<LocationOptionMetaChange> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<LocationOptionMetaChange> WithDataSetVersionId(
        this Generator<LocationOptionMetaChange> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));
    
    public static Generator<LocationOptionMetaChange> WithMeta(
        this Generator<LocationOptionMetaChange> generator,
        LocationMeta meta)
        => generator.ForInstance(s => s.SetMeta(meta));
    
    public static Generator<LocationOptionMetaChange> WithMeta(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationMeta> meta)
        => generator.ForInstance(s => s.SetMeta(meta));
    
    public static Generator<LocationOptionMetaChange> WithMetaId(
        this Generator<LocationOptionMetaChange> generator,
        int metaId)
        => generator.ForInstance(s => s.SetMetaId(metaId));

    public static Generator<LocationOptionMetaChange> WithCurrentState(
        this Generator<LocationOptionMetaChange> generator,
        LocationOptionMeta? currentState)
        => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationOptionMetaChange> WithCurrentState(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationOptionMeta?> currentState)
        => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationOptionMetaChange> WithCurrentStateId(
        this Generator<LocationOptionMetaChange> generator,
        int? currentStateId)
        => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<LocationOptionMetaChange> WithPreviousState(
        this Generator<LocationOptionMetaChange> generator,
        LocationOptionMeta? previousState)
        => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationOptionMetaChange> WithPreviousState(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationOptionMeta?> previousState)
        => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationOptionMetaChange> WithPreviousStateId(
        this Generator<LocationOptionMetaChange> generator,
        int? previousStateId)
        => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<LocationOptionMetaChange> SetDefaults(
        this InstanceSetters<LocationOptionMetaChange> setters)
        => setters
            .SetDefault(c => c.DataSetVersionId)
            .SetDefault(c => c.MetaId);

    public static InstanceSetters<LocationOptionMetaChange> SetDataSetVersion(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<LocationOptionMetaChange> SetDataSetVersionId(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Guid dataSetVersionId)
        => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<LocationOptionMetaChange> SetMeta(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationMeta meta)
        => setters.SetMeta(() => meta);

    public static InstanceSetters<LocationOptionMetaChange> SetMeta(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationMeta> meta)
        => setters
            .Set(c => c.Meta, meta)
            .Set(c => c.MetaId, (_, c) => c.Meta.Id);

    public static InstanceSetters<LocationOptionMetaChange> SetMetaId(
        this InstanceSetters<LocationOptionMetaChange> setters,
        int metaId)
        => setters.Set(c => c.MetaId, metaId);

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationOptionMeta? currentState)
        => setters.SetPreviousState(() => currentState);

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationOptionMeta?> currentState)
        => setters
            .Set(c => c.CurrentState, currentState)
            .Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentStateId(
        this InstanceSetters<LocationOptionMetaChange> setters,
        int? currentStateId)
        => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<LocationOptionMetaChange> SetPreviousState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationOptionMeta? previousState)
        => setters.SetPreviousState(() => previousState);
    public static InstanceSetters<LocationOptionMetaChange> SetPreviousState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationOptionMeta?> previousState)
        => setters
            .Set(c => c.PreviousState, previousState)
            .Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<LocationOptionMetaChange> SetPreviousStateId(
        this InstanceSetters<LocationOptionMetaChange> setters,
        int? previousStateId)
        => setters.Set(c => c.PreviousStateId, previousStateId);
}
