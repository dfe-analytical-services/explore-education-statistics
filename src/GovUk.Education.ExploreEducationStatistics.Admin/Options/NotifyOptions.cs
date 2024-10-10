namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class NotifyOptions
{
    public const string Section = "Notify";

    public string ApiKey { get; set; } = string.Empty;

    public string InviteWithRolesTemplateId { get; set; } = string.Empty;

    public string PublicationRoleTemplateId { get; set; } = string.Empty;

    public string ReleaseRoleTemplateId { get; set; } = string.Empty;

    public string PreReleaseTemplateId { get; set; } = string.Empty;

    public string ContributorTemplateId { get; set; } = string.Empty;

    public string ReleaseHigherReviewersTemplateId { get; set; } = string.Empty;

    public string MethodologyHigherReviewersTemplateId { get; set; } = string.Empty;
}
