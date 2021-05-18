using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

        public void SendInviteEmail(string email)
        {
            var uri = _configuration.GetValue<string>("AdminUri");
            var template = _configuration.GetValue<string>("NotifyInviteTemplateId");

            var emailValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://{uri}"}
            };

            _emailService.SendEmail(email, template, emailValues);
        }

        public void SendPublicationRoleEmail(string email, Publication publication, PublicationRole role)
        {
            // TODO EES-2311
        }

        public void SendReleaseRoleEmail(string email, Release release, ReleaseRole role)
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

            _emailService.SendEmail(email, template, emailValues);
        }
    }
}
