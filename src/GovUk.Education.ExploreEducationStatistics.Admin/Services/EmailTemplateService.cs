#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EmailTemplateService(
    ContentDbContext contentDbContext,
    IPreReleaseService preReleaseService,
    IEmailService emailService,
    IOptions<AppOptions> appOptions,
    IOptions<NotifyOptions> notifyOptions,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserReleaseRoleRepository userReleaseRoleRepository
) : IEmailTemplateService
{
    public async Task<Either<ActionResult, Unit>> SendInviteEmail(
        string email,
        HashSet<Guid> userReleaseRoleIds,
        HashSet<Guid> userPublicationRoleIds
    )
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.InviteWithRolesTemplateId;

        List<UserReleaseRole> userReleaseRoles = await GetUserReleaseRoles(userReleaseRoleIds);
        List<UserPublicationRole> userPublicationRoles = await GetUserPublicationRoles(userPublicationRoleIds);

        var releaseRoleList = userReleaseRoles
            .OrderBy(invite => invite.ReleaseVersion.Release.Publication.Title)
            .ThenBy(invite => invite.ReleaseVersion.Release.Title)
            .ThenBy(invite => invite.Role.ToString())
            .Select(invite =>
                $"* {invite.ReleaseVersion.Release.Publication.Title}, {invite.ReleaseVersion.Release.Title} - {invite.Role}"
            )
            .ToList();

        // The transformation step here is necessary to ensure that the email still uses the name 'Approver' for the
        // temporarily named 'Allower' role, as this is the name used in the UI. This will be removed in the future;
        // likely in STEP 9 (EES-6196) of the permissions rework approach (NO TICKET HAS BEEN CREATED FOR THIS YET).
        var publicationRoleList = userPublicationRoles
            .OrderBy(invite => invite.Publication.Title)
            .ThenBy(invite => invite.Role)
            .Select(invite => $"* {invite.Publication.Title} - {TransformPublicationRole(invite.Role)}")
            .ToList();

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", url },
            {
                "release role list",
                releaseRoleList.IsNullOrEmpty()
                    ? "* No release permissions granted"
                    : releaseRoleList.JoinToString("\n")
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
        Publication publication,
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
            { "publication", publication.Title },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendReleaseRoleEmail(
        string email,
        ReleaseVersion releaseVersion,
        ReleaseRole role
    )
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.ReleaseRoleTemplateId;

        var link = role == ReleaseRole.PrereleaseViewer ? "prerelease " : "summary";
        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/{link}" },
            { "role", role.ToString() },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public async Task<Either<ActionResult, Unit>> SendContributorInviteEmail(
        string email,
        string publicationTitle,
        HashSet<Guid> releaseVersionIds
    )
    {
        if (releaseVersionIds.IsNullOrEmpty())
        {
            throw new ArgumentException("List of release versions cannot be empty");
        }

        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.ContributorTemplateId;

        var releases = await contentDbContext
            .Releases.Where(r => r.Versions.Any(rv => releaseVersionIds.Contains(rv.Id)))
            .ToListAsync();

        var releaseTitles = releases
            .OrderBy(r => r.Year)
            .ThenBy(r => r.TimePeriodCoverage)
            .Select(r => $"* {r.Title}")
            .JoinToString('\n');

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", url },
            { "publication name", publicationTitle },
            { "release list", releaseTitles },
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public async Task<Either<ActionResult, Unit>> SendPreReleaseInviteEmail(
        string email,
        Guid releaseVersionId,
        bool isNewUser
    )
    {
        return await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId)
            .OnSuccess(releaseVersion =>
            {
                var url = appOptions.Value.Url;
                var template = notifyOptions.Value.PreReleaseTemplateId;

                var prereleaseUrl =
                    $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/prerelease/content";

                var preReleaseWindow = preReleaseService.GetPreReleaseWindow(releaseVersion);
                var preReleaseWindowStart = preReleaseWindow.Start.ConvertToUkTimeZone();
                var publishScheduled = releaseVersion.PublishScheduled!.Value.ConvertToUkTimeZone();

                // TODO EES-828 This time should depend on the Publisher schedule
                var publishScheduledTime = new TimeSpan(9, 30, 0);

                var preReleaseDay = FormatDayForEmail(preReleaseWindowStart);
                var preReleaseTime = FormatTimeForEmail(preReleaseWindowStart);
                var publishDay = FormatDayForEmail(publishScheduled);
                var publishTime = FormatTimeForEmail(publishScheduledTime);

                var emailValues = new Dictionary<string, dynamic>
                {
                    { "newUser", isNewUser ? "yes" : "no" },
                    { "release name", releaseVersion.Release.Title },
                    { "publication name", releaseVersion.Release.Publication.Title },
                    { "prerelease link", prereleaseUrl },
                    { "prerelease day", preReleaseDay },
                    { "prerelease time", preReleaseTime },
                    { "publish day", publishDay },
                    { "publish time", publishTime },
                };

                return emailService.SendEmail(email, template, emailValues);
            });
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

    private async Task<List<UserReleaseRole>> GetUserReleaseRoles(HashSet<Guid> userReleaseRoleIds)
    {
        return await userReleaseRoleRepository
            .Query(ResourceRoleFilter.PendingOnly)
            .AsNoTracking()
            .Where(urr => userReleaseRoleIds.Contains(urr.Id))
            .Include(urr => urr.ReleaseVersion)
                .ThenInclude(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
            .ToListAsync();
    }

    private async Task<List<UserPublicationRole>> GetUserPublicationRoles(HashSet<Guid> userPublicationRoleIds)
    {
        return await userPublicationRoleRepository
            .Query(ResourceRoleFilter.PendingOnly)
            .AsNoTracking()
            .Where(upr => userPublicationRoleIds.Contains(upr.Id))
            .Include(upr => upr.Publication)
            .ToListAsync();
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
