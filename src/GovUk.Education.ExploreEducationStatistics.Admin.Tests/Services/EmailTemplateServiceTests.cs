using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class EmailTemplateServiceTests
    {
        [Fact]
        public void SendInviteEmail()
        {
            const string expectedTemplateId = "invite-template-id";

            var expectedValues = new Dictionary<string, dynamic>
            {
                {"url", "https://admin-uri"}
            };

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            emailService.Setup(mock =>
                mock.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ));

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            service.SendInviteEmail("test@test.com");

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);
        }

        [Fact]
        public void SendPublicationRoleEmail()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Test Publication"
            };

            const string expectedTemplateId = "publication-role-template-id";

            var expectedValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://admin-uri"},
                {"role", Owner.ToString()},
                {"publication", "Test Publication"},
            };

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            emailService.Setup(mock =>
                mock.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ));

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            service.SendPublicationRoleEmail("test@test.com", publication, Owner);

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);
        }

        [Fact]
        public void SendReleaseRoleEmail()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Publication"
                },
                ReleaseName = "2020",
                TimePeriodCoverage = December
            };

            const string expectedTemplateId = "release-role-template-id";

            var expectedValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://admin-uri/publication/{release.Publication.Id}/release/{release.Id}/summary"},
                {"role", Contributor.ToString()},
                {"publication", "Test Publication"},
                {"release", "December 2020"}
            };

            var emailService = new Mock<IEmailService>(MockBehavior.Strict);

            emailService.Setup(mock =>
                mock.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ));

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            service.SendReleaseRoleEmail("test@test.com", release, Contributor);

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);
        }

        private static Mock<IConfiguration> ConfigurationMock()
        {
            var notifyInviteTemplateSection = new Mock<IConfigurationSection>();
            var publicationRoleTemplateSection = new Mock<IConfigurationSection>();
            var releaseRoleTemplateSection = new Mock<IConfigurationSection>();
            var adminUriSection = new Mock<IConfigurationSection>();
            var configuration = new Mock<IConfiguration>();

            notifyInviteTemplateSection.Setup(m => m.Value)
                .Returns("invite-template-id");

            publicationRoleTemplateSection.Setup(m => m.Value)
                .Returns("publication-role-template-id");

            releaseRoleTemplateSection.Setup(m => m.Value)
                .Returns("release-role-template-id");

            adminUriSection.Setup(m => m.Value)
                .Returns("admin-uri");

            configuration
                .Setup(m => m.GetSection("NotifyInviteTemplateId"))
                .Returns(notifyInviteTemplateSection.Object);

            configuration
                .Setup(m => m.GetSection("NotifyPublicationRoleTemplateId"))
                .Returns(publicationRoleTemplateSection.Object);

            configuration
                .Setup(m => m.GetSection("NotifyReleaseRoleTemplateId"))
                .Returns(releaseRoleTemplateSection.Object);

            configuration
                .Setup(m => m.GetSection("AdminUri"))
                .Returns(adminUriSection.Object);

            return configuration;
        }

        private static EmailTemplateService SetupEmailTemplateService(
            IEmailService emailService = null,
            IConfiguration configuration = null)
        {
            return new EmailTemplateService(
                emailService ?? new Mock<IEmailService>().Object,
                configuration ?? ConfigurationMock().Object);
        }
    }
}
