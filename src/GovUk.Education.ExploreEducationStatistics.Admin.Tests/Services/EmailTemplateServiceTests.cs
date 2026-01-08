#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EmailTemplateServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public void SendInviteEmail()
    {
        const string expectedTemplateId = "invite-with-roles-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "release role list", "* No release permissions granted" },
            { "publication role list", "* No publication permissions granted" },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendInviteEmail(
            "test@test.com",
            new List<UserReleaseInvite>(),
            new List<UserPublicationInvite>()
        );

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Fact]
    public void SendPublicationRoleEmail()
    {
        Publication publication = _dataFixture.DefaultPublication();

        const string expectedTemplateId = "publication-role-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "role", Owner.ToString() },
            { "publication", publication.Title },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendPublicationRoleEmail("test@test.com", publication, Owner);

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Fact]
    public void SendReleaseRoleEmail()
    {
        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()));

        const string expectedTemplateId = "release-role-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            {
                "url",
                $"https://admin-uri/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/summary"
            },
            { "role", Contributor.ToString() },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendReleaseRoleEmail("test@test.com", releaseVersion, Contributor);

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);

        result.AssertRight();
    }

    [Fact]
    public async Task SendContributorInviteEmail()
    {
        var email = "test@test.com";

        Publication publication = _dataFixture.DefaultPublication();

        // Purposefully creating some releases out of order to ensure sorting is working as expected when generating the release titles for the email.
        var releases = _dataFixture
            .DefaultRelease(3)
            .WithPublication(publication)
            .ForIndex(0, s => s.SetYear(2021).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ2))
            .ForIndex(1, s => s.SetYear(2020).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ1))
            .ForIndex(2, s => s.SetYear(2022).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ2))
            .ForIndex(3, s => s.SetYear(2020).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ2))
            .ForIndex(4, s => s.SetYear(2022).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ1))
            .ForIndex(5, s => s.SetYear(2021).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ1))
            .ForIndex(6, s => s.SetYear(2023).SetTimePeriodCoverage(TimeIdentifier.AcademicYearQ1))
            .GenerateList(7);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Releases.AddRange(releases);
            await contentDbContext.SaveChangesAsync();
        }

        // Grab some release version Ids from some (but not all) releases
        var releaseVersionIdsForEmail = new HashSet<Guid>
        {
            // Grab a couple from the first release in the list (shouldn't matter how many or which ones)
            releases[0].Versions[0].Id,
            releases[0].Versions[1].Id,
            // Grab just 1 from the remaining releases in the list EXCEPT the last one
            releases[1].Versions[1].Id,
            releases[2].Versions[2].Id,
            releases[3].Versions[0].Id,
            releases[4].Versions[2].Id,
            releases[5].Versions[1].Id,
        };

        // These should be ordered by release year and then time period coverage
        var expectedReleaseList = """
            * Academic year Q1 2020/21
            * Academic year Q2 2020/21
            * Academic year Q1 2021/22
            * Academic year Q2 2021/22
            * Academic year Q1 2022/23
            * Academic year Q2 2022/23
            """;

        const string expectedTemplateId = "contributor-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "publication name", publication.Title },
            { "release list", expectedReleaseList },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService.Setup(mock => mock.SendEmail(email, expectedTemplateId, expectedValues)).Returns(Unit.Instance);

        await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupEmailTemplateService(
                contentDbContext: contentDbContext,
                emailService: emailService.Object
            );

            var result = await service.SendContributorInviteEmail(
                email: email,
                publicationTitle: publication.Title,
                releaseVersionIds: releaseVersionIdsForEmail
            );

            result.AssertRight();
        }

        emailService.Verify(s => s.SendEmail(email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService);
    }

    [Fact]
    public async Task SendContributorInviteEmail_EmptyReleaseVersionIds_Throws()
    {
        var service = SetupEmailTemplateService();

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.SendContributorInviteEmail(
                email: "test@test.com",
                publicationTitle: "publication-title",
                releaseVersionIds: []
            )
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SendPreReleaseInviteEmail(bool isNewUser)
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        // UK time 00:00 on 9th Sept 2020
        var publishedScheduledStartOfDay = new DateOnly(2020, 9, 9).GetUkStartOfDayUtc();

        ReleaseVersion releaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
            .WithPublishScheduled(publishedScheduledStartOfDay);

        await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var preReleaseService = new Mock<IPreReleaseService>(Strict);
        preReleaseService
            .Setup(s => s.GetPreReleaseWindow(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
            .Returns(
                new PreReleaseWindow
                {
                    Start = new DateTimeOffset(2020, 9, 8, 7, 30, 0, TimeSpan.Zero), // UK time 08:30 on 8th Sept 2020
                    ScheduledPublishDate = publishedScheduledStartOfDay,
                }
            );

        const string expectedTemplateId = "prerelease-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "newUser", isNewUser ? "yes" : "no" },
            { "release name", releaseVersion.Release.Title },
            { "publication name", releaseVersion.Release.Publication.Title },
            {
                "prerelease link",
                $"https://admin-uri/publication/{releaseVersion.Release.Publication.Id}/release/{releaseVersion.Id}/prerelease/content"
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

        await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupEmailTemplateService(
                contentDbContext: contentDbContext,
                emailService: emailService.Object,
                preReleaseService: preReleaseService.Object
            );

            var result = await service.SendPreReleaseInviteEmail(
                email: "test@test.com",
                releaseVersionId: releaseVersion.Id,
                isNewUser: isNewUser
            );

            result.AssertRight();
        }

        emailService.Verify(s => s.SendEmail("test@test.com", expectedTemplateId, expectedValues), Times.Once);

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
            MethodologyHigherReviewersTemplateId = "notify-methodology-higher-reviewers-template-id",
            PreReleaseTemplateId = "prerelease-template-id",
            ContributorTemplateId = "contributor-template-id",
        }.ToOptionsWrapper();
    }

    private static EmailTemplateService SetupEmailTemplateService(
        ContentDbContext? contentDbContext = null,
        IPreReleaseService? preReleaseService = null,
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null
    )
    {
        contentDbContext ??= DbUtils.InMemoryApplicationDbContext();

        return new(
            contentDbContext,
            preReleaseService ?? Mock.Of<IPreReleaseService>(Strict),
            emailService ?? Mock.Of<IEmailService>(Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions()
        );
    }
}
