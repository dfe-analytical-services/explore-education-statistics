#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
    IOptions<NotifyOptions> notifyOptions)
    : IEmailTemplateService
{
    public Either<ActionResult, Unit> SendInviteEmail(
        string email,
        List<UserReleaseInvite> userReleaseInvites,
        List<UserPublicationInvite> userPublicationInvites)
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.InviteWithRolesTemplateId;

        var releaseRoleList = userReleaseInvites
            .OrderBy(invite => invite.ReleaseVersion.Release.Publication.Title)
            .ThenBy(invite => invite.ReleaseVersion.Release.Title)
            .ThenBy(invite => invite.Role.ToString())
            .Select(invite =>
                $"* {invite.ReleaseVersion.Release.Publication.Title}, {invite.ReleaseVersion.Release.Title} - {invite.Role}")
            .ToList();

        var publicationRoleList = userPublicationInvites
            .OrderBy(invite => invite.Publication.Title)
            .ThenBy(invite => invite.Role)
            .Select(invite => $"* {invite.Publication.Title} - {invite.Role}")
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
        PublicationRole role)
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.PublicationRoleTemplateId;

        var emailValues = new Dictionary<string, dynamic>
        {
            {"url", url},
            {"role", role.ToString()},
            {"publication", publication.Title}
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendReleaseRoleEmail(
        string email,
        ReleaseVersion releaseVersion,
        ReleaseRole role)
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.ReleaseRoleTemplateId;

        var link = role == ReleaseRole.PrereleaseViewer ? "prerelease " : "summary";
        var emailValues = new Dictionary<string, dynamic>
        {
            {
                "url",
                $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/{link}"
            },
            { "role", role.ToString() },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title }
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendReleaseHigherReviewEmail(
        string email,
        ReleaseVersion releaseVersion)
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.ReleaseHigherReviewersTemplateId;

        var emailValues = new Dictionary<string, dynamic>
        {
            { "url", $"{url}/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/summary" },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title }
        };

        return emailService.SendEmail(email, template, emailValues);
    }

    public Either<ActionResult, Unit> SendMethodologyHigherReviewEmail(
        string email,
        Guid methodologyVersionId,
        string methodologyTitle)
    {
        var url = appOptions.Value.Url;
        var template = notifyOptions.Value.MethodologyHigherReviewersTemplateId;

        var emailValues = new Dictionary<string, dynamic>
        {
            {"url", $"{url}/methodology/{methodologyVersionId}/summary"},
            {"methodology", methodologyTitle},
        };

        return emailService.SendEmail(email, template, emailValues);
    }
}
