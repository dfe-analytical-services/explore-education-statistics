using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class Change<TState>
{
    public Guid Identifier { get; set; } = UuidUtils.UuidV7();

    public required ChangeType Type { get; set; }

    public TState? CurrentState { get; set; }

    public TState? PreviousState { get; set; }
}
