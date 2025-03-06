using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public class ReleaseNotifierTests
{
    private static readonly AppOptions AppOptions = new()
    {
        Url = "https://notifier.func/api",
        PublicAppUrl = "https://public.app"
    };

    private static readonly GovUkNotifyOptions.EmailTemplateOptions EmailTemplateOptions = new()
    {
        ReleaseAmendmentPublishedId = "release-amendment-published-id",
        ReleaseAmendmentPublishedSupersededSubscribersId = "release-amendment-published-superseded-subscribers-id",
        ReleasePublishedId = "release-published-id",
        ReleasePublishedSupersededSubscribersId = "release-published-superseded-subscribers-id",
        SubscriptionConfirmationId = "subscription-confirmation-id",
        SubscriptionVerificationId = "subscription-verification-id"
    };

    [Fact]
    public async Task NotifySubscribers()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>(MockBehavior.Strict);

        var publication1Id = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(publication1Id))
            .ReturnsAsync([ "test@test.com" ]);

        var supersededPubId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(supersededPubId))
            .ReturnsAsync([ "superseded@test.com" ]);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail("test@test.com", "release-published-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("superseded@test.com", "release-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            context.Publications.Add(new Publication
            {
                Id = supersededPubId,
                Title = "Superseded publication",
                Slug = "superseded-publication",
                SupercededById = publication1Id,
            });
            await context.SaveChangesAsync();
        }

        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var function = BuildFunction(
                contentDbContext: context,
                subscriptionRepository: subscriptionRepository.Object,
                tokenService: tokenService.Object,
                emailService: emailService.Object);

            var releaseNotificationMessage = new ReleaseNotificationMessage
            {
                PublicationId = publication1Id,
                PublicationName = "Publication 1",
                PublicationSlug = "publication-1",
                ReleaseName = "2000",
                ReleaseSlug = "2000",
                Amendment = false,
                UpdateNote = string.Empty,
            };

            await function.NotifySubscribers(
                releaseNotificationMessage,
                new TestFunctionContext());
        }

        emailService.Verify(mock =>
            mock.SendEmail("test@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "publication-1", null)
                )), Times.Once);

        emailService.Verify(mock =>
            mock.SendEmail("superseded@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "superseded-publication",
                        "Superseded publication")
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_MultipleSubs()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>(MockBehavior.Strict);

        var publicationId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(publicationId))
            .ReturnsAsync([ "test1@test.com", "test2@test.com", "test3@test.com" ]);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test1@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("test2@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");
        tokenService.Setup(mock =>
                mock.GenerateToken("test3@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-3");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail("test1@test.com", "release-published-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("test2@test.com", "release-published-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("test3@test.com", "release-published-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        await using var context = ContentDbUtils.InMemoryContentDbContext(Guid.NewGuid().ToString());

        var function = BuildFunction(
            contentDbContext: context,
            subscriptionRepository: subscriptionRepository.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var releaseNotificationMessage = new ReleaseNotificationMessage
        {
            PublicationId = publicationId,
            PublicationName = "Publication 1",
            PublicationSlug = "publication-1",
            ReleaseName = "2000",
            ReleaseSlug = "2000",
            Amendment = false,
            UpdateNote = string.Empty,
        };

        await function.NotifySubscribers(
            releaseNotificationMessage,
            new TestFunctionContext());

        emailService.Verify(mock =>
            mock.SendEmail("test1@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "publication-1", null)
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("test2@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "publication-1", null)
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("test3@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-3",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "publication-1", null)
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_MultipleSupersededPublicationSubs()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>(MockBehavior.Strict);

        var publicationId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(publicationId))
            .ReturnsAsync([]);

        var supersededPubId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(supersededPubId))
            .ReturnsAsync([ "superseded1@test.com", "superseded2@test.com", "superseded3@test.com" ]);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded1@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded2@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded3@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-3");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail("superseded1@test.com", "release-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("superseded2@test.com", "release-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("superseded3@test.com", "release-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            context.Publications.Add(new Publication
            {
                Id = supersededPubId,
                Title = "Superseded publication",
                Slug = "superseded-publication",
                SupercededById = publicationId,
            });
            await context.SaveChangesAsync();
        }

        var releaseNotificationMessage = new ReleaseNotificationMessage
        {
            PublicationId = publicationId,
            PublicationName = "Publication 1",
            PublicationSlug = "publication-1",
            ReleaseName = "2000",
            ReleaseSlug = "2000",
            Amendment = false,
            UpdateNote = string.Empty,
        };

        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var function = BuildFunction(
                contentDbContext: context,
                subscriptionRepository: subscriptionRepository.Object,
                tokenService: tokenService.Object,
                emailService: emailService.Object);

            await function.NotifySubscribers(
                releaseNotificationMessage,
                new TestFunctionContext());
        }

        emailService.Verify(mock =>
            mock.SendEmail("superseded1@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "superseded-publication",
                        "Superseded publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("superseded2@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "superseded-publication",
                        "Superseded publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("superseded3@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-3",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "superseded-publication",
                        "Superseded publication")
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_MultipleSupersededPublications()
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>(MockBehavior.Strict);

        var publicationId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(publicationId))
            .ReturnsAsync([]);

        var supersededPub1Id = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(supersededPub1Id))
            .ReturnsAsync([ "superseded1@test.com" ]);

        var supersededPub2Id = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(supersededPub2Id))
            .ReturnsAsync([ "superseded2@test.com" ]);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded1@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded2@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail("superseded1@test.com", "release-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("superseded2@test.com", "release-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            context.Publications.AddRange(
                new Publication
                {
                    Id = supersededPub1Id,
                    Title = "Superseded 1 publication",
                    Slug = "superseded-1-publication",
                    SupercededById = publicationId,
                },
                new Publication
                {
                    Id = supersededPub2Id,
                    Title = "Superseded 2 publication",
                    Slug = "superseded-2-publication",
                    SupercededById = publicationId,
                });
            await context.SaveChangesAsync();
        }

        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var function = BuildFunction(
                contentDbContext: context,
                subscriptionRepository: subscriptionRepository.Object,
                tokenService: tokenService.Object,
                emailService: emailService.Object);

            var releaseNotificationMessage = new ReleaseNotificationMessage
            {
                PublicationId = publicationId,
                PublicationName = "Publication 1",
                PublicationSlug = "publication-1",
                ReleaseName = "2000",
                ReleaseSlug = "2000",
                Amendment = false,
                UpdateNote = string.Empty,
            };

            await function.NotifySubscribers(
                releaseNotificationMessage,
                new TestFunctionContext());
        }

        emailService.Verify(mock =>
            mock.SendEmail("superseded1@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "superseded-1-publication",
                        "Superseded 1 publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("superseded2@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "superseded-2-publication",
                        "Superseded 2 publication")
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_Amendment() // If an amendment we use a different email template
    {
        var subscriptionRepository = new Mock<ISubscriptionRepository>(MockBehavior.Strict);

        var publicationId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(publicationId))
            .ReturnsAsync([ "test@test.com" ]);

        var supersededPubId = Guid.NewGuid();
        subscriptionRepository.Setup(mock => mock.GetSubscriberEmails(supersededPubId))
            .ReturnsAsync([ "superseded@test.com" ]);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("superseded@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail("test@test.com", "release-amendment-published-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail("superseded@test.com",
                "release-amendment-published-superseded-subscribers-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            context.Publications.Add(new Publication
            {
                Id = supersededPubId,
                Title = "Superseded publication",
                Slug = "superseded-publication",
                SupercededById = publicationId,
            });
            await context.SaveChangesAsync();
        }

        await using (var context = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var function = BuildFunction(
                contentDbContext: context,
                subscriptionRepository: subscriptionRepository.Object,
                tokenService: tokenService.Object,
                emailService: emailService.Object);

            var releaseNotificationMessage = new ReleaseNotificationMessage
            {
                PublicationId = publicationId,
                PublicationName = "Publication 1",
                PublicationSlug = "publication-1",
                ReleaseName = "2000",
                ReleaseSlug = "2000",
                Amendment = true,
                UpdateNote = "Update note",
            };

            await function.NotifySubscribers(
                releaseNotificationMessage,
                new TestFunctionContext());
        }

        emailService.Verify(mock =>
            mock.SendEmail("test@test.com", "release-amendment-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "publication-1", null)
                )), Times.Once);

        emailService.Verify(mock =>
            mock.SendEmail("superseded@test.com",
                "release-amendment-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "superseded-publication",
                        "Superseded publication")
                )), Times.Once);
    }

    private static bool AssertEmailTemplateValues(Dictionary<string, dynamic> values,
        string unsubToken,
        string pubName, string pubSlug,
        string releaseName, string releaseSlug, string? updateNote,
        string unsubPubSlug,
        string? supersededPublicationTitle = null)
    {
        Assert.Equal(pubName, values["publication_name"]);
        Assert.Equal(releaseName, values["release_name"]);
        Assert.Equal($"{AppOptions.PublicAppUrl}/find-statistics/{pubSlug}/{releaseSlug}",
            values["release_link"]);
        Assert.Equal($"{AppOptions.PublicAppUrl}/subscriptions/{unsubPubSlug}/confirm-unsubscription/{unsubToken}", values["unsubscribe_link"]);

        if (updateNote != null)
        {
            Assert.Equal(updateNote, values["update_note"]);
        }
        else
        {
            Assert.False(values.ContainsKey("update_note"));
        }

        if (supersededPublicationTitle != null)
        {
            Assert.Equal(supersededPublicationTitle, values["superseded_publication_title"]);
        }
        else
        {
            Assert.False(values.ContainsKey("superseded_publication_title"));
        }

        return true;
    }

    private static ReleaseNotifier BuildFunction(
        ContentDbContext? contentDbContext = null,
        ITokenService? tokenService = null,
        IEmailService? emailService = null,
        ISubscriptionRepository? subscriptionRepository = null)
    {
        return new ReleaseNotifier(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            Mock.Of<ILogger<ReleaseNotifier>>(),
            AppOptions.ToOptionsWrapper(),
            new GovUkNotifyOptions
            {
                ApiKey = "",
                EmailTemplates = EmailTemplateOptions
            }.ToOptionsWrapper(),
            tokenService ?? Mock.Of<ITokenService>(MockBehavior.Strict),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            subscriptionRepository ?? Mock.Of<ISubscriptionRepository>(MockBehavior.Strict));
    }
}
