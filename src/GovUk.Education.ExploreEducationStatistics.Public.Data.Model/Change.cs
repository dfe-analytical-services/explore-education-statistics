namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class Change<TState>
{
    public Guid Identifier { get; set; } = Guid.NewGuid();

    public required ChangeType Type { get; set; }

    public TState? CurrentState { get; set; }

    public TState? PreviousState { get; set; }
}
