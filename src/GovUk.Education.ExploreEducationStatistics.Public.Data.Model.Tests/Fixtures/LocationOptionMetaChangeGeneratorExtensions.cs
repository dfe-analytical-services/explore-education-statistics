using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionMetaChangeGeneratorExtensions
{
    public static Generator<LocationOptionMetaChange> DefaultLocationOptionMetaChange(
        this DataFixture fixture
    ) => fixture.Generator<LocationOptionMetaChange>().WithDefaults();

    public static Generator<LocationOptionMetaChange> WithDefaults(
        this Generator<LocationOptionMetaChange> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionMetaChange> WithDataSetVersion(
        this Generator<LocationOptionMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<LocationOptionMetaChange> WithDataSetVersionId(
        this Generator<LocationOptionMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<LocationOptionMetaChange> WithCurrentState(
        this Generator<LocationOptionMetaChange> generator,
        LocationOptionMetaLink? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationOptionMetaChange> WithCurrentState(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationOptionMetaLink?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationOptionMetaChange> WithCurrentState(
        this Generator<LocationOptionMetaChange> generator,
        LocationOptionMetaChange.State? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationOptionMetaChange> WithCurrentState(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationOptionMetaChange.State?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<LocationOptionMetaChange> WithPreviousState(
        this Generator<LocationOptionMetaChange> generator,
        LocationOptionMetaLink? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationOptionMetaChange> WithPreviousState(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationOptionMetaLink?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationOptionMetaChange> WithPreviousState(
        this Generator<LocationOptionMetaChange> generator,
        LocationOptionMetaChange.State? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<LocationOptionMetaChange> WithPreviousState(
        this Generator<LocationOptionMetaChange> generator,
        Func<LocationOptionMetaChange.State?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static InstanceSetters<LocationOptionMetaChange> SetDefaults(
        this InstanceSetters<LocationOptionMetaChange> setters
    ) => setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<LocationOptionMetaChange> SetDataSetVersion(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) =>
        setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<LocationOptionMetaChange> SetDataSetVersionId(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationOptionMetaLink? currentState
    ) => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationOptionMetaLink?> currentState
    )
    {
        var current = currentState();

        return current is not null
            ? setters.SetCurrentState(
                new LocationOptionMetaChange.State
                {
                    MetaId = current.MetaId,
                    OptionId = current.OptionId,
                    PublicId = current.PublicId,
                }
            )
            : setters;
    }

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationOptionMetaChange.State? currentState
    ) => setters.Set(c => c.CurrentState, currentState);

    public static InstanceSetters<LocationOptionMetaChange> SetCurrentState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationOptionMetaChange.State?> currentState
    ) => setters.Set(c => c.CurrentState, currentState);

    public static InstanceSetters<LocationOptionMetaChange> SetPreviousState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationOptionMetaLink? previousState
    ) => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<LocationOptionMetaChange> SetPreviousState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationOptionMetaLink?> previousState
    )
    {
        var previous = previousState();

        return previous is not null
            ? setters.SetPreviousState(
                new LocationOptionMetaChange.State
                {
                    MetaId = previous.MetaId,
                    OptionId = previous.OptionId,
                    PublicId = previous.PublicId,
                }
            )
            : setters;
    }

    public static InstanceSetters<LocationOptionMetaChange> SetPreviousState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        LocationOptionMetaChange.State? previousState
    ) => setters.Set(c => c.PreviousState, previousState);

    public static InstanceSetters<LocationOptionMetaChange> SetPreviousState(
        this InstanceSetters<LocationOptionMetaChange> setters,
        Func<LocationOptionMetaChange.State?> previousState
    ) => setters.Set(c => c.PreviousState, previousState);
}
