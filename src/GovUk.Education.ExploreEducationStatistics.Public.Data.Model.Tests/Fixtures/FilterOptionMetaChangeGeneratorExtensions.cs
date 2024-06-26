using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterOptionMetaChangeGeneratorExtensions
{
    public static Generator<FilterOptionMetaChange> DefaultFilterOptionMetaChange(this DataFixture fixture)
        => fixture
            .Generator<FilterOptionMetaChange>()
            .WithDefaults();

    public static Generator<FilterOptionMetaChange> WithDefaults(this Generator<FilterOptionMetaChange> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterOptionMetaChange> WithDataSetVersion(
        this Generator<FilterOptionMetaChange> generator,
        Func<DataSetVersion> dataSetVersion)
        => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<FilterOptionMetaChange> WithDataSetVersionId(
        this Generator<FilterOptionMetaChange> generator,
        Guid dataSetVersionId)
        => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<FilterOptionMetaChange> WithPublicId(
        this Generator<FilterOptionMetaChange> generator,
        string publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<FilterOptionMetaChange> WithMeta(
        this Generator<FilterOptionMetaChange> generator,
        FilterMeta meta)
        => generator.ForInstance(s => s.SetMeta(meta));

    public static Generator<FilterOptionMetaChange> WithMetaId(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterMeta> meta)
        => generator.ForInstance(s => s.SetMeta(meta));

    public static Generator<FilterOptionMetaChange> WithMetaId(
        this Generator<FilterOptionMetaChange> generator,
        int metaId)
        => generator.ForInstance(s => s.SetMetaId(metaId));

    public static Generator<FilterOptionMetaChange> WithCurrentState(
        this Generator<FilterOptionMetaChange> generator,
        FilterOptionMeta? currentState)
        => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterOptionMetaChange> WithCurrentState(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterOptionMeta?> currentState)
        => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterOptionMetaChange> WithCurrentStateId(
        this Generator<FilterOptionMetaChange> generator,
        int? currentStateId)
        => generator.ForInstance(s => s.SetCurrentStateId(currentStateId));

    public static Generator<FilterOptionMetaChange> WithPreviousState(
        this Generator<FilterOptionMetaChange> generator,
        FilterOptionMeta? previousState)
        => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterOptionMetaChange> WithPreviousState(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterOptionMeta?> previousState)
        => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterOptionMetaChange> WithPreviousStateId(
        this Generator<FilterOptionMetaChange> generator,
        int? previousStateId)
        => generator.ForInstance(s => s.SetPreviousStateId(previousStateId));

    public static InstanceSetters<FilterOptionMetaChange> SetDefaults(
        this InstanceSetters<FilterOptionMetaChange> setters)
        => setters
            .SetDefault(c => c.DataSetVersionId)
            .SetDefault(c => c.PublicId)
            .SetDefault(c => c.MetaId);

    public static InstanceSetters<FilterOptionMetaChange> SetDataSetVersion(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<DataSetVersion> dataSetVersion)
        => setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<FilterOptionMetaChange> SetDataSetVersionId(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Guid dataSetVersionId)
        => setters.Set(c => c.DataSetVersionId, dataSetVersionId);
    
    public static InstanceSetters<FilterOptionMetaChange> SetPublicId(
        this InstanceSetters<FilterOptionMetaChange> setters,
        string publicId)
        => setters.Set(c => c.PublicId, publicId);

    public static InstanceSetters<FilterOptionMetaChange> SetMeta(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterMeta meta)
        => setters.SetMeta(() => meta);

    public static InstanceSetters<FilterOptionMetaChange> SetMeta(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterMeta> meta)
        => setters
            .Set(c => c.Meta, meta)
            .Set(c => c.MetaId, (_, f) => f.Meta.Id);

    public static InstanceSetters<FilterOptionMetaChange> SetMetaId(
        this InstanceSetters<FilterOptionMetaChange> setters,
        int metaId)
        => setters.Set(c => c.MetaId, metaId);

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterOptionMeta? currentState)
        => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterOptionMeta?> currentState)
        => setters
            .Set(c => c.CurrentState, currentState)
            .Set(c => c.CurrentStateId, (_, f) => f.CurrentState?.Id);

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentStateId(
        this InstanceSetters<FilterOptionMetaChange> setters,
        int? currentStateId)
        => setters.Set(c => c.CurrentStateId, currentStateId);

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterOptionMeta? previousState)
        => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterOptionMeta?> previousState)
        => setters
            .Set(c => c.PreviousState, previousState)
            .Set(c => c.PreviousStateId, (_, f) => f.PreviousState?.Id);

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousStateId(
        this InstanceSetters<FilterOptionMetaChange> setters,
        int? previousStateId)
        => setters.Set(c => c.PreviousStateId, previousStateId);
}
