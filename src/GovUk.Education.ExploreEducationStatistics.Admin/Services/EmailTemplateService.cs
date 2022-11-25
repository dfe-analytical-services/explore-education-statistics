#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public EmailTemplateService(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }

        public Either<ActionResult, Unit> SendInviteEmail(
            string email,
            List<UserReleaseInvite> userReleaseInvites,
            List<UserPublicationInvite> userPublicationInvites)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyInviteWithRolesTemplateId");

            var releaseRoleList = userReleaseInvites
                .OrderBy(invite => invite.Release.Publication.Title)
                .ThenBy(invite => invite.Release.Title)
                .ThenBy(invite => invite.Role.ToString())
                .Select(invite =>
                    $"* {invite.Release.Publication.Title}, {invite.Release.Title} - {invite.Role.ToString()}")
                .ToList();

            var publicationRoleList = userPublicationInvites
                .OrderBy(invite => invite.Publication.Title)
                .ThenBy(invite => invite.Role)
                .Select(invite => $"* {invite.Publication.Title} - {invite.Role.ToString()}")
                .ToList();

            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}"},
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

            return _emailService.SendEmail(email, template, emailValues);
        }
        
        public Either<ActionResult, Unit> SendPublicationRoleEmail(
            string email,
            Publication publication,
            PublicationRole role)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyPublicationRoleTemplateId");

            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}"},
                {"role", role.ToString()},
                {"publication", publication.Title}
            };

            return _emailService.SendEmail(email, template, emailValues);
        }

        public Either<ActionResult, Unit> SendReleaseRoleEmail(
            string email,
            Release release,
            ReleaseRole role)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyReleaseRoleTemplateId");

            var link = role == ReleaseRole.PrereleaseViewer ? "prerelease " : "summary";
            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}/publication/{release.Publication.Id}/release/{release.Id}/{link}"},
                {"role", role.ToString()},
                {"publication", release.Publication.Title},
                {"release", release.Title}
            };

            return _emailService.SendEmail(email, template, emailValues);
        }
        
        public Either<ActionResult, Unit> SendHigherReviewEmail(string email, Release release)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyHigherReviewersTemplateId");
            
            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}/publication/{release.Publication.Id}/release/{release.Id}/summary"},
                {"publication", release.Publication.Title},
                {"release", release.Title},
            };
            
            return _emailService.SendEmail(email, template, emailValues);
        }
    }
}
