#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
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
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EmailTemplateServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task SendInviteEmail()
    {
        const string expectedTemplateId = "invite-with-roles-template-id";

        User user = _dataFixture.DefaultUser();

        var publications = _dataFixture
            .DefaultPublication()
            .ForIndex(0, s => s.SetTitle("Title 1"))
            .ForIndex(1, s => s.SetTitle("Title 2"))
            .ForIndex(2, s => s.SetTitle("Title 3"))
            .GenerateList(3);

        var releaseVersions = _dataFixture
            .DefaultReleaseVersion()
            .ForIndex(
                0,
                s =>
                    s.SetRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithYear(2021)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYearQ1)
                            .WithPublication(publications[0])
                    )
            ) // 2021 Q1 Title 1
            .ForIndex(
                1,
                s =>
                    s.SetRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithYear(2022)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYearQ1)
                            .WithPublication(publications[0])
                    )
            ) // 2022 Q1 Title 1
            .ForIndex(
                2,
                s =>
                    s.SetRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithYear(2021)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYearQ1)
                            .WithPublication(publications[1])
                    )
            ) // 2021 Q1 Title 2
            .ForIndex(
                3,
                s =>
                    s.SetRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithYear(2022)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYearQ1)
                            .WithPublication(publications[1])
                    )
            ) // 2022 Q1 Title 2
            .ForIndex(
                4,
                s =>
                    s.SetRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithYear(2021)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYearQ1)
                            .WithPublication(publications[2])
                    )
            ) // 2021 Q1 Title 3
            .ForIndex(
                5,
                s =>
                    s.SetRelease(
                        _dataFixture
                            .DefaultRelease()
                            .WithYear(2022)
                            .WithTimePeriodCoverage(TimeIdentifier.AcademicYearQ1)
                            .WithPublication(publications[2])
                    )
            ) // 2022 Q1 Title 3
            .GenerateList(6);

        var applicableUserPublicationRoles = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .ForIndex(
                0,
                s =>
                    s.SetPublication(publications[1]) // Title 2
                        .SetRole(PublicationRole.Owner)
            )
            .ForIndex(
                1,
                s =>
                    s.SetPublication(publications[1]) // Title 2
                        .SetRole(PublicationRole.Allower)
            )
            .ForIndex(
                2,
                s =>
                    s.SetPublication(publications[0]) // Title 1
                        .SetRole(PublicationRole.Allower)
            )
            .ForIndex(
                3,
                s =>
                    s.SetPublication(publications[0]) // Title 1
                        .SetRole(PublicationRole.Owner)
            )
            .ForIndex(
                4,
                s =>
                    s.SetPublication(publications[2]) // Title 3
                        .SetRole(PublicationRole.Owner)
            )
            .ForIndex(
                5,
                s =>
                    s.SetPublication(publications[2]) // Title 3
                        .SetRole(PublicationRole.Allower)
            )
            .GenerateList(6);

        var applicableUserReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .ForIndex(
                0,
                s =>
                    s.SetReleaseVersion(releaseVersions[5]) // 2022 Q1 Title 3
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                1,
                s =>
                    s.SetReleaseVersion(releaseVersions[4]) // 2021 Q1 Title 3)
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                2,
                s =>
                    s.SetReleaseVersion(releaseVersions[5]) // 2022 Q1 Title 3)
                        .SetRole(ReleaseRole.Approver)
            )
            .ForIndex(
                3,
                s =>
                    s.SetReleaseVersion(releaseVersions[1]) // 2022 Q1 Title 1)
                        .SetRole(ReleaseRole.Approver)
            )
            .ForIndex(
                4,
                s =>
                    s.SetReleaseVersion(releaseVersions[0]) // 2021 Q1 Title 1))
                        .SetRole(ReleaseRole.Approver)
            )
            .ForIndex(
                5,
                s =>
                    s.SetReleaseVersion(releaseVersions[1]) // 2022 Q1 Title 1))
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                6,
                s =>
                    s.SetReleaseVersion(releaseVersions[3]) // 2022 Q1 Title 2))
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                7,
                s =>
                    s.SetReleaseVersion(releaseVersions[2]) // 2021 Q1 Title 2)))
                        .SetRole(ReleaseRole.Contributor)
            )
            .ForIndex(
                8,
                s =>
                    s.SetReleaseVersion(releaseVersions[3]) // 2022 Q1 Title 2)))
                        .SetRole(ReleaseRole.Approver)
            )
            .GenerateList(9);

        // These roles should be filtered out, as the IDs or these aren't passed to the email service
        UserPublicationRole otherUserPublicationRole = _dataFixture.DefaultUserPublicationRole();
        UserReleaseRole otherUserReleaseRole = _dataFixture.DefaultUserReleaseRole();

        UserPublicationRole[] allUserPublicationRoles = [.. applicableUserPublicationRoles, otherUserPublicationRole];
        UserReleaseRole[] allUserReleaseRoles = [.. applicableUserReleaseRoles, otherUserReleaseRole];

        var applicableUserPublicationRoleIds = applicableUserPublicationRoles.Select(r => r.Id).ToHashSet();
        var applicableUserReleaseRoleIds = applicableUserReleaseRoles.Select(r => r.Id).ToHashSet();

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

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, allUserPublicationRoles);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, allUserReleaseRoles);

        var emailService = new Mock<IEmailService>(Strict);
        emailService
            .Setup(mock => mock.SendEmail(user.Email, expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(
            emailService: emailService.Object,
            userPublicationRoleRepository: userPublicationRoleRepository.Object,
            userReleaseRoleRepository: userReleaseRoleRepository.Object
        );

        var result = await service.SendInviteEmail(
            email: user.Email,
            userReleaseRoleIds: applicableUserReleaseRoleIds,
            userPublicationRoleIds: applicableUserPublicationRoleIds
        );

        emailService.Verify(s => s.SendEmail(user.Email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService, userPublicationRoleRepository, userReleaseRoleRepository);

        result.AssertRight();
    }

    [Fact]
    public async Task SendInviteEmail_SomeProvidedRolesDoNotMatchProvidedUserEmail()
    {
        User user = _dataFixture.DefaultUser();

        var userPublicationRoles = _dataFixture
            .DefaultUserPublicationRole()
            .WithPublication(_dataFixture.DefaultPublication())
            // This role is for the correct user email
            .ForIndex(0, s => s.SetUser(user))
            // This role is for a different user email
            .ForIndex(1, s => s.SetUser(_dataFixture.DefaultUser()))
            .GenerateList(2);

        var userReleaseRoles = _dataFixture
            .DefaultUserReleaseRole()
            .WithReleaseVersion(
                _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication()))
            )
            // This role is for the correct user email
            .ForIndex(0, s => s.SetUser(user))
            // This role is for a different user email
            .ForIndex(1, s => s.SetUser(_dataFixture.DefaultUser()))
            .GenerateList(2);

        // Try sending an invite email for all roles, even those that don't match the user's email
        var userPublicationRoleIds = userPublicationRoles.Select(r => r.Id).ToHashSet();
        var userReleaseRoleIds = userReleaseRoles.Select(r => r.Id).ToHashSet();

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userPublicationRoles]);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, [.. userReleaseRoles]);

        var service = SetupEmailTemplateService(
            userPublicationRoleRepository: userPublicationRoleRepository.Object,
            userReleaseRoleRepository: userReleaseRoleRepository.Object
        );

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.SendInviteEmail(
                email: user.Email,
                userReleaseRoleIds: userReleaseRoleIds,
                userPublicationRoleIds: userPublicationRoleIds
            )
        );

        VerifyAllMocks(userPublicationRoleRepository, userReleaseRoleRepository);
    }

    [Fact]
    public async Task SendInviteEmail_PlainInviteWithNoRoles()
    {
        var email = "test@test.com";
        const string expectedTemplateId = "invite-with-roles-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "release role list", "* No release permissions granted" },
            { "publication role list", "* No publication permissions granted" },
        };

        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, []);

        var userReleaseRoleRepository = new Mock<IUserReleaseRoleRepository>(Strict);
        userReleaseRoleRepository.SetupQuery(ResourceRoleFilter.PendingOnly, []);

        var emailService = new Mock<IEmailService>(Strict);
        emailService.Setup(mock => mock.SendEmail(email, expectedTemplateId, expectedValues)).Returns(Unit.Instance);

        var service = SetupEmailTemplateService(
            emailService: emailService.Object,
            userPublicationRoleRepository: userPublicationRoleRepository.Object,
            userReleaseRoleRepository: userReleaseRoleRepository.Object
        );

        var result = await service.SendInviteEmail(email, [], []);

        emailService.Verify(s => s.SendEmail(email, expectedTemplateId, expectedValues), Times.Once);

        VerifyAllMocks(emailService, userPublicationRoleRepository, userReleaseRoleRepository);

        result.AssertRight();
    }

    [Fact]
    public void SendPublicationRoleEmail()
    {
        string publicationTitle = "Publication Title";

        const string expectedTemplateId = "publication-role-template-id";

        var expectedValues = new Dictionary<string, dynamic>
        {
            { "url", "https://admin-uri" },
            { "role", PublicationRole.Owner.ToString() },
            { "publication", publicationTitle },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendPublicationRoleEmail("test@test.com", publicationTitle, PublicationRole.Owner);

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
            { "role", ReleaseRole.Contributor.ToString() },
            { "publication", releaseVersion.Release.Publication.Title },
            { "release", releaseVersion.Release.Title },
        };

        var emailService = new Mock<IEmailService>(Strict);

        emailService
            .Setup(mock => mock.SendEmail("test@test.com", expectedTemplateId, expectedValues))
            .Returns(Unit.Instance);

        var service = SetupEmailTemplateService(emailService: emailService.Object);

        var result = service.SendReleaseRoleEmail("test@test.com", releaseVersion, ReleaseRole.Contributor);

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
        ContentDbContext? contentDbContext = null,
        IPreReleaseService? preReleaseService = null,
        IEmailService? emailService = null,
        IOptions<AppOptions>? appOptions = null,
        IOptions<NotifyOptions>? notifyOptions = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IUserReleaseRoleRepository? userReleaseRoleRepository = null
    )
    {
        contentDbContext ??= DbUtils.InMemoryApplicationDbContext();

        return new(
            contentDbContext,
            preReleaseService ?? Mock.Of<IPreReleaseService>(Strict),
            emailService ?? Mock.Of<IEmailService>(Strict),
            appOptions ?? DefaultAppOptions(),
            notifyOptions ?? DefaultNotifyOptions(),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            userReleaseRoleRepository ?? Mock.Of<IUserReleaseRoleRepository>(Strict)
        );
    }
}
