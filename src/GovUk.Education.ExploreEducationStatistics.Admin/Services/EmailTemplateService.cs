#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EmailTemplateService(
    IEmailService emailService,
    IOptions<AppOptions> appOptions,
    IOptions<NotifyOptions> notifyOptions
) : IEmailTemplateService
{
    public Either<ActionResult, Unit> SendInviteEmail(
        string email,
        HashSet<(string PublicationTitle, string ReleaseTitle)> prereleaseRolesInfo,
        HashSet<(string PublicationTitle, PublicationRole Role)> publicationRolesInfo
    )
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.InviteWithRolesTemplateId;

        var prereleaseRoleList = prereleaseRolesInfo
            .OrderBy(rri => rri.PublicationTitle)
            .ThenBy(rri => rri.ReleaseTitle)
            .Select(rri => $"* {rri.PublicationTitle}, {rri.ReleaseTitle}")
            .ToList();

        // The transformation step here is necessary to ensure that the email still uses the name 'Approver' for the
        // temporarily named 'Allower' role, as this is the name used in the UI. This will be removed in the future;
        // likely in STEP 9 (EES-6196) of the permissions rework approach (NO TICKET HAS BEEN CREATED FOR THIS YET).
        var publicationRoleList = publicationRolesInfo
            .OrderBy(pri => pri.PublicationTitle)
            .ThenBy(pri => pri.Role)
            .Select(pri => $"* {pri.PublicationTitle} - {TransformPublicationRole(pri.Role)}")
            .ToList();

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", url },
            {
                "pre-release role list",
                prereleaseRoleList.IsNullOrEmpty()
                    ? "* No pre-release permissions granted"
                    : prereleaseRoleList.JoinToString("\n")
            },
            {
                "publication role list",
                publicationRoleList.IsNullOrEmpty()
                    ? "* No publication permissions granted"
                    : publicationRoleList.JoinToString("\n")
            },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendPublicationRoleEmail(
        string email,
        string publicationTitle,
        PublicationRole role
    )
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.PublicationRoleTemplateId;

        // This transformation is necessary to ensure that the email still uses the name 'Approver' for the
        // temporarily named 'Allower' role, as this is the name used in the UI. This will be removed in the future;
        // likely in STEP 9 (EES-6196) of the permissions rework approach (NO TICKET HAS BEEN CREATED FOR THIS YET).
        var transformedRole = TransformPublicationRole(role);

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", url },
            { "role", transformedRole },
            { "publication", publicationTitle },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendPreReleaseInviteEmail(
        string email,
        string publicationTitle,
        string releaseTitle,
        bool isNewUser,
        Guid publicationId,
        Guid releaseVersionId,
        DateTimeOffset preReleaseWindowStart,
        DateTimeOffset publishScheduled
    )
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.PreReleaseTemplateId;

        var prereleaseUrl = $"{url}/publication/{publicationId}/release/{releaseVersionId}/prerelease/content";

        var preReleaseWindowStartUkTimeZone = preReleaseWindowStart.ConvertToUkTimeZone();
        var publishScheduledUkTimeZone = publishScheduled.ConvertToUkTimeZone();

        // TODO EES-828 This time should depend on the Publisher schedule
        var publishScheduledTime = new TimeSpan(9, 30, 0);

        var preReleaseDay = FormatDayForEmail(preReleaseWindowStartUkTimeZone);
        var preReleaseTime = FormatTimeForEmail(preReleaseWindowStartUkTimeZone);
        var publishDay = FormatDayForEmail(publishScheduledUkTimeZone);
        var publishTime = FormatTimeForEmail(publishScheduledTime);

        var emailValues = new Dictionary<string, dynamic>
        {
            { "newUser", isNewUser ? "yes" : "no" },
            { "release name", releaseTitle },
            { "publication name", publicationTitle },
            { "prerelease link", prereleaseUrl },
            { "prerelease day", preReleaseDay },
            { "prerelease time", preReleaseTime },
            { "publish day", publishDay },
            { "publish time", publishTime },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendReleaseHigherReviewEmail(string email, ReleaseVersion releaseVersion)
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.ReleaseHigherReviewersTemplateId;

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/summary" },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendMethodologyHigherReviewEmail(
        string email,
        Guid methodologyVersionId,
        string methodologyTitle
    )
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.MethodologyHigherReviewersTemplateId;

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", $"{url}/methodology/{methodologyVersionId}/summary" },
            { "methodology", methodologyTitle },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    private static string TransformPublicationRole(PublicationRole role)
    {
        // This transformation is necessary to ensure that the email still uses the name 'Approver' for the
        // temporarily named 'Allower' role, as this is the name used in the UI. This will be removed in the future;
        // likely in STEP 9 (EES-6196) of the permissions rework approach (NO TICKET HAS BEEN CREATED FOR THIS YET).
        return role switch
        {
            PublicationRole.Owner => role.ToString(),
            PublicationRole.Allower => "Approver",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
    }

    private static string FormatTimeForEmail(DateTimeOffset dateTime)
    {
        return dateTime.ToString("HH:mm");
    }

    private static string FormatTimeForEmail(TimeSpan timeSpan)
    {
        return timeSpan.ToString(@"hh\:mm");
    }

    private static string FormatDayForEmail(DateTimeOffset dateTime)
    {
        return dateTime.ToString("dddd dd MMMM yyyy");
    }
}
