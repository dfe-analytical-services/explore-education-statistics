#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
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
                {"url", "https://admin-uri"},
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
            var releaseVersion = new ReleaseVersion
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
                {
                    "url",
                    $"https://admin-uri/publication/{releaseVersion.Publication.Id}/release/{releaseVersion.Id}/summary"
                },
                { "role", Contributor.ToString() },
                { "publication", "Test Publication" },
                { "release", "December 2020" }
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

            var result = service.SendReleaseRoleEmail("test@test.com", releaseVersion, Contributor);

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
        public void SendReleaseHigherReviewEmail()
        {
            const string expectedTemplateId = "notify-release-higher-reviewers-template-id";
            var releaseVersion = new ReleaseVersion
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
                {
                    "url",
                    $"https://admin-uri/publication/{releaseVersion.Publication.Id}/release/{releaseVersion.Id}/summary"
                },
                { "publication", releaseVersion.Publication.Title },
                { "release", releaseVersion.Title },
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

            var result = service.SendReleaseHigherReviewEmail("test@test.com", releaseVersion);

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
        public void SendMethodologyHigherReviewEmail()
        {
            const string expectedTemplateId = "notify-methodology-higher-reviewers-template-id";
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Owning publication title",
                }
            };

            var expectedValues = new Dictionary<string, dynamic>
            {
                {"url", $"https://admin-uri/methodology/{methodologyVersion.Id}/summary"},
                {"methodology", methodologyVersion.Title},
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

            var result = service.SendMethodologyHigherReviewEmail(
                "test@test.com", methodologyVersion.Id, methodologyVersion.Title);

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

        private static IOptions<AppOptions> DefaultAppOptions()
        {
            return new AppOptions { Url = "https://admin-uri" }.ToOptionsWrapper();
        }

        private static IOptions<NotifyOptions> DefaultNotifyOptions()
        {
            return new NotifyOptions
            {
                InviteWithRolesTemplateId = "invite-with-roles-template-id",
                PublicationRoleTemplateId = "publication-role-template-id",
                ReleaseRoleTemplateId = "release-role-template-id",
                ReleaseHigherReviewersTemplateId = "notify-release-higher-reviewers-template-id",
                MethodologyHigherReviewersTemplateId = "notify-methodology-higher-reviewers-template-id"
            }.ToOptionsWrapper();
        }

        private static EmailTemplateService SetupEmailTemplateService(
            IEmailService? emailService = null,
            IOptions<AppOptions>? appOptions = null,
            IOptions<NotifyOptions>? notifyOptions = null)
        {
            return new(
                emailService ?? Mock.Of<IEmailService>(Strict),
                appOptions ?? DefaultAppOptions(),
                notifyOptions ?? DefaultNotifyOptions());
        }
    }
}
