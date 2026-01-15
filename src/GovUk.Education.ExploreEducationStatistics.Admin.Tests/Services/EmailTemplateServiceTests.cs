#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EmailTemplateServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public void SendInviteEmail()
    {
        const string expectedTemplateId = "invite-with-roles-template-id";

        string userEmail = "test@test.com";

        HashSet<(string PublicationTitle, string ReleaseTitle, ReleaseRole Role)> releaseRolesInfo =
        [
            ("Title 3", "Academic year Q1 2022/23", ReleaseRole.Contributor),
            ("Title 3", "Academic year Q1 2021/22", ReleaseRole.Contributor),
            ("Title 3", "Academic year Q1 2022/23", ReleaseRole.Approver),
            ("Title 1", "Academic year Q1 2022/23", ReleaseRole.Approver),
            ("Title 1", "Academic year Q1 2021/22", ReleaseRole.Approver),
            ("Title 1", "Academic year Q1 2022/23", ReleaseRole.Contributor),
            ("Title 2", "Academic year Q1 2022/23", ReleaseRole.Contributor),
            ("Title 2", "Academic year Q1 2021/22", ReleaseRole.Contributor),
            ("Title 2", "Academic year Q1 2022/23", ReleaseRole.Approver),
        ];

        HashSet<(string PublicationTitle, PublicationRole Role)> publicationRolesInfo =
        [
            ("Title 2", PublicationRole.Owner),
            ("Title 2", PublicationRole.Allower),
            ("Title 1", PublicationRole.Allower),
            ("Title 1", PublicationRole.Owner),
            ("Title 3", PublicationRole.Owner),
            ("Title 3", PublicationRole.Allower),
        ];

        // These should be ordered by publication title, and then by role
        var expectedPublicationRoleList = """
            * Title 1 - Owner
            * Title 1 - Approver
            * Title 2 - Owner
            * Title 2 - Approver
            * Title 3 - Owner
            * Title 3 - Approver
            """;

        // These should be ordered by publication title, and then by release title, and then by role
        var expectedReleaseRoleList = """
            * Title 1, Academic year Q1 2021/22 - Approver
            * Title 1, Academic year Q1 2022/23 - Approver
            * Title 1, Academic year Q1 2022/23 - Contributor
            * Title 2, Academic year Q1 2021/22 - Contributor
            * Title 2, Academic year Q1 2022/23 - Approver
            * Title 2, Academic year Q1 2022/23 - Contributor
            * Title 3, Academic year Q1 2021/22 - Contributor
            * Title 3, Academic year Q1 2022/23 - Approver
            * Title 3, Academic year Q1 2022/23 - Contributor
            """;

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "release role list", expectedReleaseRoleList },
            { "publication role list", expectedPublicationRoleList },
        };

        var emailService = new Mock<IEmailService>(Strict);
        emailService
            .Setup(mock => mock.SendEmail(userEmail, expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendInviteEmail(
            email: userEmail,
            releaseRolesInfo: releaseRolesInfo,
            publicationRolesInfo: publicationRolesInfo
        );

        emailService.Verify(s => s.SendEmail(userEmail, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Fact]
    public void SendInviteEmail_PlainInviteWithNoRoles()
    {
        var email = "test@test.com";
        const string expectedTemplateId = "invite-with-roles-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "release role list", "* No release permissions granted" },
            { "publication role list", "* No publication permissions granted" },
        };

        var emailService = new Mock<IEmailService>(Strict);
        emailService.Setup(mock => mock.SendEmail(email, expectedTemplateId, expectedValues)).Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendInviteEmail(email, [], []);

        emailService.Verify(s => s.SendEmail(email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Theory]
    [InlineData(PublicationRole.Owner, "Owner")]
    [InlineData(PublicationRole.Allower, "Approver")]
    public void SendPublicationRoleEmail(PublicationRole role, string expectedRoleText)
    {
        string email = "test@test.com";
        string publicationTitle = "Publication Title";

        const string expectedTemplateId = "publication-role-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "role", expectedRoleText },
            { "publication", publicationTitle },
        };

        var emailService = new Mock<IEmailService>(Strict);
        emailService.Setup(mock => mock.SendEmail(email, expectedTemplateId, expectedValues)).Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendPublicationRoleEmail(email: email, publicationTitle: publicationTitle, role: role);

        emailService.Verify(s => s.SendEmail(email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Fact]
    public void SendReleaseRoleEmail()
    {
        string email = "test@test.com";
        string publicationTitle = "Publication Title";
        string releaseTitle = "Release Title";
        var publicationId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();
        var role = ReleaseRole.Approver;

        const string expectedTemplateId = "release-role-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", $"https://admin-uri/publication/{publicationId}/release/{releaseVersionId}/summary" },
            { "role", role.ToString() },
            { "publication", publicationTitle },
            { "release", releaseTitle },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendReleaseRoleEmail(
            email: email,
            publicationTitle: publicationTitle,
            releaseTitle: releaseTitle,
            publicationId: publicationId,
            releaseVersionId: releaseVersionId,
            role: role
        );

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Fact]
    public void SendContributorInviteEmail()
    {
        string email = "test@test.com";
        string publicationTitle = "Publication Title";

        const string expectedTemplateId = "contributor-template-id";

        // Purposefully creating the release info out of order to ensure sorting is working as expected when generating the formatted release titles for the email.
        HashSet<(int Year, TimeIdentifier TimePeriodCoverage, string Title)> releasesInfo =
        [
            (2021, TimeIdentifier.AcademicYearQ2, "Academic year Q2 2021/22"),
            (2020, TimeIdentifier.AcademicYearQ1, "Academic year Q1 2020/21"),
            (2022, TimeIdentifier.AcademicYearQ2, "Academic year Q2 2022/23"),
            (2020, TimeIdentifier.AcademicYearQ2, "Academic year Q2 2020/21"),
            (2022, TimeIdentifier.AcademicYearQ1, "Academic year Q1 2022/23"),
            (2021, TimeIdentifier.AcademicYearQ1, "Academic year Q1 2021/22"),
        ];

        // These should be ordered by release year and then time period coverage
        var expectedReleaseList = """
            * Academic year Q1 2020/21
            * Academic year Q2 2020/21
            * Academic year Q1 2021/22
            * Academic year Q2 2021/22
            * Academic year Q1 2022/23
            * Academic year Q2 2022/23
            """;

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "publication name", publicationTitle },
            { "release list", expectedReleaseList },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService.Setup(mock => mock.SendEmail(email, expectedTemplateId, expectedValues)).Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendContributorInviteEmail(
            email: email,
            publicationTitle: publicationTitle,
            releasesInfo: releasesInfo
        );

        result.AssertRight();

        emailService.Verify(s => s.SendEmail(email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SendPreReleaseInviteEmail(bool isNewUser)
    {
        string email = "test@test.com";
        string publicationTitle = "Publication Title";
        string releaseTitle = "Release Title";
        var publicationId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();
        var preReleaseWindowStart = new DateTimeOffset(2020, 9, 8, 7, 30, 0, TimeSpan.Zero); // UK time 08:30 on 8th Sept 2020
        var publishScheduled = new DateOnly(2020, 9, 9).GetUkStartOfDayUtc(); // UK time 00:00 on 9th Sept 2020

        const string expectedTemplateId = "prerelease-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "newUser", isNewUser ? "yes" : "no" },
            { "release name", releaseTitle },
            { "publication name", publicationTitle },
            {
                "prerelease link",
                $"https://admin-uri/publication/{publicationId}/release/{releaseVersionId}/prerelease/content"
            },
            { "prerelease day", "Tuesday 08 September 2020" },
            { "prerelease time", "08:30" },
            { "publish day", "Wednesday 09 September 2020" },
            { "publish time", "09:30" },
        };

        var emailService = new Mock<IEmailService>(Strict);
        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendPreReleaseInviteEmail(
            email: email,
            publicationTitle: publicationTitle,
            releaseTitle: releaseTitle,
            isNewUser: isNewUser,
            publicationId: publicationId,
            releaseVersionId: releaseVersionId,
            preReleaseWindowStart: preReleaseWindowStart,
            publishScheduled: publishScheduled
        );

        result.AssertRight();

        emailService.Verify(s => s.SendEmail(email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);
    }

    [Fact]
    public void SendReleaseHigherReviewEmail()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        const string expectedTemplateId = "notify-release-higher-reviewers-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            {
                "url",
                $"https://admin-uri/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/summary"
            },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendReleaseHigherReviewEmail("test@test.com", releaseVersion);

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

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
            Methodology = new Methodology { OwningPublicationTitle = "Owning publication title" },
        };

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", $"https://admin-uri/methodology/{methodologyVersion.Id}/summary" },
            { "methodology", methodologyVersion.Title },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendMethodologyHigherReviewEmail(
            "test@test.com",
            methodologyVersion.Id,
            methodologyVersion.Title
        );

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    private static IOptions<AppOptions> DefaultAppOptions() =>
        new AppOptions { Url = "https://admin-uri" }.ToOptionsWrapper();

    private static IOptions<NotifyOptions> DefaultNotifyOptions() =>
        new NotifyOptions
        {
            InviteWithRolesTemplateId = "invite-with-roles-template-id",
            PublicationRoleTemplateId = "publication-role-template-id",
            ReleaseRoleTemplateId = "release-role-template-id",
            ReleaseHigherReviewersTemplateId = "notify-release-higher-reviewers-template-id",
            MethodologyHigherReviewersTemplateId = "notify-methodology-higher-reviewers-template-id",
            PreReleaseTemplateId = "prerelease-template-id",
            ContributorTemplateId = "contributor-template-id",
        }.ToOptionsWrapper();

    private static EmailTemplateService SetupEmailTemplateService(
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null
    )
    {
        return new(
            emailService ?? Mock.Of<IEmailService>(Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions()
        );
    }
}
