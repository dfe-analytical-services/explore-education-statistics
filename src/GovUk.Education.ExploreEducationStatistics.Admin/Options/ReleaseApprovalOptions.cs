#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public record ReleaseApprovalOptions
{
    public const string Section = "ReleaseApproval";

    public string PrepareScheduledReleaseVersionsFunctionCronSchedule { get; init; } = string.Empty;

    public string PublishScheduledReleaseVersionsFunctionCronSchedule { get; init; } = string.Empty;
}
