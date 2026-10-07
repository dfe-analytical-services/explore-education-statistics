namespace GovUk.Education.ExploreEducationStatistics.Publisher.Options;

public class AzureFrontDoorOptions
{
    public const string Section = "AzureFrontDoor";

    public bool CachePurgeEnabled { get; init; }

    public string EndpointResourceId { get; init; } = string.Empty;

    public string ContentApiHostName { get; init; } = string.Empty;
}
