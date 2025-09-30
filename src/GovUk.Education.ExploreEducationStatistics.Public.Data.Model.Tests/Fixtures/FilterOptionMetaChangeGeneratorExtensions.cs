using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterOptionMetaChangeGeneratorExtensions
{
    public static Generator<FilterOptionMetaChange> DefaultFilterOptionMetaChange(
        this DataFixture fixture
    ) => fixture.Generator<FilterOptionMetaChange>().WithDefaults();

    public static Generator<FilterOptionMetaChange> WithDefaults(
        this Generator<FilterOptionMetaChange> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterOptionMetaChange> WithDataSetVersion(
        this Generator<FilterOptionMetaChange> generator,
        Func<DataSetVersion> dataSetVersion
    ) => generator.ForInstance(s => s.SetDataSetVersion(dataSetVersion));

    public static Generator<FilterOptionMetaChange> WithDataSetVersionId(
        this Generator<FilterOptionMetaChange> generator,
        Guid dataSetVersionId
    ) => generator.ForInstance(s => s.SetDataSetVersionId(dataSetVersionId));

    public static Generator<FilterOptionMetaChange> WithCurrentState(
        this Generator<FilterOptionMetaChange> generator,
        FilterOptionMetaLink? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterOptionMetaChange> WithCurrentState(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterOptionMetaLink?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterOptionMetaChange> WithCurrentState(
        this Generator<FilterOptionMetaChange> generator,
        FilterOptionMetaChange.State? currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterOptionMetaChange> WithCurrentState(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterOptionMetaChange.State?> currentState
    ) => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<FilterOptionMetaChange> WithPreviousState(
        this Generator<FilterOptionMetaChange> generator,
        FilterOptionMetaLink? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterOptionMetaChange> WithPreviousState(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterOptionMetaLink?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterOptionMetaChange> WithPreviousState(
        this Generator<FilterOptionMetaChange> generator,
        FilterOptionMetaChange.State? previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static Generator<FilterOptionMetaChange> WithPreviousState(
        this Generator<FilterOptionMetaChange> generator,
        Func<FilterOptionMetaChange.State?> previousState
    ) => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static InstanceSetters<FilterOptionMetaChange> SetDefaults(
        this InstanceSetters<FilterOptionMetaChange> setters
    ) => setters.SetDefault(c => c.DataSetVersionId);

    public static InstanceSetters<FilterOptionMetaChange> SetDataSetVersion(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<DataSetVersion> dataSetVersion
    ) =>
        setters
            .Set(c => c.DataSetVersion, dataSetVersion)
            .Set(c => c.DataSetVersionId, (_, f) => f.DataSetVersion.Id);

    public static InstanceSetters<FilterOptionMetaChange> SetDataSetVersionId(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Guid dataSetVersionId
    ) => setters.Set(c => c.DataSetVersionId, dataSetVersionId);

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterOptionMetaLink? currentState
    ) => setters.SetCurrentState(() => currentState);

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterOptionMetaLink?> currentState
    )
    {
        var current = currentState();

        return current is not null
            ? setters.SetCurrentState(
                new FilterOptionMetaChange.State
                {
                    MetaId = current.MetaId,
                    OptionId = current.OptionId,
                    PublicId = current.PublicId,
                }
            )
            : setters;
    }

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterOptionMetaChange.State? currentState
    ) => setters.Set(c => c.CurrentState, currentState);

    public static InstanceSetters<FilterOptionMetaChange> SetCurrentState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterOptionMetaChange.State?> currentState
    ) => setters.Set(c => c.CurrentState, currentState);

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterOptionMetaLink? previousState
    ) => setters.SetPreviousState(() => previousState);

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterOptionMetaLink?> previousState
    )
    {
        var previous = previousState();

        return previous is not null
            ? setters.SetPreviousState(
                new FilterOptionMetaChange.State
                {
                    MetaId = previous.MetaId,
                    OptionId = previous.OptionId,
                    PublicId = previous.PublicId,
                }
            )
            : setters;
    }

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        FilterOptionMetaChange.State? previousState
    ) => setters.Set(c => c.PreviousState, previousState);

    public static InstanceSetters<FilterOptionMetaChange> SetPreviousState(
        this InstanceSetters<FilterOptionMetaChange> setters,
        Func<FilterOptionMetaChange.State?> previousState
    ) => setters.Set(c => c.PreviousState, previousState);
}
