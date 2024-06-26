using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class ChangeGeneratorExtensions
{
    public static Generator<Change<TState>> DefaultChange<TState>(
        this DataFixture fixture)
        => fixture
            .Generator<Change<TState>>()
            .WithDefaults();

    public static Generator<Change<TState>> WithDefaults<TState>(
        this Generator<Change<TState>> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Change<TState>> WithIdentifier<TState>(
        this Generator<Change<TState>> generator,
        Guid identifier)
        => generator.ForInstance(s => s.SetIdentifier(identifier));

    public static Generator<Change<TState>> WithType<TState>(
        this Generator<Change<TState>> generator,
        ChangeType type)
        => generator.ForInstance(s => s.SetType(type));

    public static Generator<Change<TState>> WithCurrentState<TState>(
        this Generator<Change<TState>> generator,
        TState? currentState)
        => generator.ForInstance(s => s.SetCurrentState(currentState));

    public static Generator<Change<TState>> WithPreviousState<TState>(
        this Generator<Change<TState>> generator,
        TState? previousState)
        => generator.ForInstance(s => s.SetPreviousState(previousState));

    public static InstanceSetters<Change<TState>> SetDefaults<TState>(
        this InstanceSetters<Change<TState>> setters)
        => setters
            .SetDefault(c => c.Identifier)
            .SetDefault(c => c.Type);

    public static InstanceSetters<Change<TState>> SetIdentifier<TState>(
        this InstanceSetters<Change<TState>> setters,
        Guid identifier)
        => setters.Set(c => c.Identifier, identifier);

    public static InstanceSetters<Change<TState>> SetType<TState>(
        this InstanceSetters<Change<TState>> setters,
        ChangeType type)
        => setters.Set(c => c.Type, type);

    public static InstanceSetters<Change<TState>> SetCurrentState<TState>(
        this InstanceSetters<Change<TState>> setters,
        TState? currentState)
        => setters.Set(c => c.CurrentState, currentState);

    public static InstanceSetters<Change<TState>> SetPreviousState<TState>(
        this InstanceSetters<Change<TState>> setters,
        TState? previousState)
        => setters.Set(c => c.PreviousState, previousState);
}
