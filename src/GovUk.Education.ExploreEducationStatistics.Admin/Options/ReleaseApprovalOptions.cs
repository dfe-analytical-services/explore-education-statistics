#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public record ReleaseApprovalOptions
{
    public const string Section = "ReleaseApproval";

    public string StageScheduledReleasesFunctionCronSchedule { get; init; } = string.Empty;

    public string PublishScheduledReleasesFunctionCronSchedule { get; init; } = string.Empty;
}
