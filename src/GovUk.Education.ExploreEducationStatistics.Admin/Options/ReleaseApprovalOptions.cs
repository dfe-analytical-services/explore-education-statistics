#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public record ReleaseApprovalOptions
{
    public const string Section = "ReleaseApproval";

    public string PublishReleasesCronSchedule { get; init; } = string.Empty;
    
    public string PublishReleaseContentCronSchedule { get; init; } = string.Empty;
}
