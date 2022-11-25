#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class EmailTemplateServiceTests
    {
        [Fact]
        public void SendInviteEmail()
        {
            const string expectedTemplateId = "invite-with-roles-template-id";

            var expectedValues = new Dictionary<string, dynamic>
            {
                {"url", "https://admin-uri"},
                {"release role list", "* No release permissions granted"},
                {"publication role list" , "* No publication permissions granted"},
            };

            var emailService = new Mock<IEmailService>(Strict);

            emailService.Setup(mock =>
                    mock.SendEmail(
                        "test@test.com",
                        expectedTemplateId,
                        expectedValues
                    ))
                .Returns(Unit.Instance);

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            var result = service.SendInviteEmail(
                "test@test.com",
                new List<UserReleaseInvite>(),
                new List<UserPublicationInvite>());

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);

            result.AssertRight();
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

            var emailService = new Mock<IEmailService>(Strict);

            emailService.Setup(mock =>
                    mock.SendEmail(
                        "test@test.com",
                        expectedTemplateId,
                        expectedValues
                    ))
                .Returns(Unit.Instance);

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            var result = service.SendPublicationRoleEmail("test@test.com", publication, Owner);

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);

            result.AssertRight();
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

            var emailService = new Mock<IEmailService>(Strict);

            emailService.Setup(mock =>
                    mock.SendEmail(
                        "test@test.com",
                        expectedTemplateId,
                        expectedValues
                    ))
                .Returns(Unit.Instance);

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            var result = service.SendReleaseRoleEmail("test@test.com", release, Contributor);

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);

            result.AssertRight();
        }

        [Fact]
        public void SendHigherReviewEmail()
        {
            const string expectedTemplateId = "notify-higher-reviewers-template-id";
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
            
            var expectedValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://admin-uri/publication/{release.Publication.Id}/release/{release.Id}/summary"},
                {"publication", release.Publication.Title},
                {"release", release.Title},
            };

            var emailService = new Mock<IEmailService>(Strict);

            emailService.Setup(mock =>
                    mock.SendEmail(
                        "test@test.com",
                        expectedTemplateId,
                        expectedValues
                    ))
                .Returns(Unit.Instance);

            var service = SetupEmailTemplateService(emailService: emailService.Object);

            var result = service.SendHigherReviewEmail("test@test.com", release);

            emailService.Verify(
                s => s.SendEmail(
                    "test@test.com",
                    expectedTemplateId,
                    expectedValues
                ), Times.Once
            );

            VerifyAllMocks(emailService);

            result.AssertRight();
        }

        private static Mock<IConfiguration> ConfigurationMock()
        {
            return CreateMockConfiguration(
                TupleOf("NotifyInviteWithRolesTemplateId", "invite-with-roles-template-id"),
                TupleOf("NotifyPublicationRoleTemplateId", "publication-role-template-id"),
                TupleOf("NotifyReleaseRoleTemplateId", "release-role-template-id"),
                TupleOf("NotifyHigherReviewersTemplateId", "notify-higher-reviewers-template-id"),
                TupleOf("AdminUri", "admin-uri"));
        }

        private static EmailTemplateService SetupEmailTemplateService(
            IEmailService? emailService = null,
            IConfiguration? configuration = null)
        {
            return new (
                emailService ?? Mock.Of<IEmailService>(Strict),
                configuration ?? ConfigurationMock().Object);
        }
    }
}
