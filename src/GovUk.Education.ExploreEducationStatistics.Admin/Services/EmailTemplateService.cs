#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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

        public Either<ActionResult, Unit> SendInviteEmail(string email)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyInviteTemplateId");

            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}"}
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
    }
}
