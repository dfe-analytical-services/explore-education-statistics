using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.TableStorageTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public class ReleaseNotifierTests
{
    private static readonly AppSettingsOptions AppSettingsOptions = new()
    {
        BaseUrl = "https://notifier.func/api",
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
        var publication1Id = Guid.NewGuid();

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(Guid.NewGuid().ToString(), "test@test.com", "Publication 1", "publication-1", null)
        });
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntityOld>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPubId = Guid.NewGuid();
        var tableQuerySegmentSupersededPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(supersededPubId.ToString(), "superseded@test.com", "Superseded publication",
                "superseded-publication", null)
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntityOld>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPubId}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPubSubs);

        // other mocks
        var publicationSubscriptionRepository = new Mock<IPublicationSubscriptionRepository>(MockBehavior.Strict);
        publicationSubscriptionRepository.Setup(mock =>
                mock.GetTable(NotifierTableStorage.PublicationSubscriptionsTable))
            .ReturnsAsync(cloudTable.Object);

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

        var function = BuildFunction(
            publicationSubscriptionRepository: publicationSubscriptionRepository.Object,
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
            SupersededPublications = new List<IdTitleViewModel>
            {
                new()
                {
                    Id = supersededPubId,
                    Title = "Superseded publication",
                },
            },
        };

        await function.NotifySubscribers(
            releaseNotificationMessage,
            new TestFunctionContext());

        emailService.Verify(mock =>
            mock.SendEmail("test@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null, null)
                )), Times.Once);

        emailService.Verify(mock =>
            mock.SendEmail("superseded@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "Superseded publication")
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_MultipleSubs()
    {
        var publication1Id = Guid.NewGuid();

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(Guid.NewGuid().ToString(), "test1@test.com", "Publication 1", "publication-1", null),
            new(Guid.NewGuid().ToString(), "test2@test.com", "Publication 1", "publication-1", null),
            new(Guid.NewGuid().ToString(), "test3@test.com", "Publication 1", "publication-1", null),
        });
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntityOld>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        // other mocks
        var publicationSubscriptionRepository = new Mock<IPublicationSubscriptionRepository>(MockBehavior.Strict);
        publicationSubscriptionRepository.Setup(mock =>
                mock.GetTable(NotifierTableStorage.PublicationSubscriptionsTable))
            .ReturnsAsync(cloudTable.Object);

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

        var function = BuildFunction(
            publicationSubscriptionRepository: publicationSubscriptionRepository.Object,
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
            SupersededPublications = new List<IdTitleViewModel>(),
        };

        await function.NotifySubscribers(
            releaseNotificationMessage,
            new TestFunctionContext());

        emailService.Verify(mock =>
            mock.SendEmail("test1@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null, null)
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("test2@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null, null)
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("test3@test.com", "release-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-3",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null, null)
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_MultipleSupersededPublicationSubs()
    {
        var publication1Id = Guid.NewGuid();

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>());
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntityOld>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPubId = Guid.NewGuid();
        var tableQuerySegmentSupersededPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(supersededPubId.ToString(), "superseded1@test.com", "Superseded publication",
                "superseded-publication", null),
            new(supersededPubId.ToString(), "superseded2@test.com", "Superseded publication",
                "superseded-publication", null),
            new(supersededPubId.ToString(), "superseded3@test.com", "Superseded publication",
                "superseded-publication", null),
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntityOld>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPubId}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPubSubs);

        // other mocks
        var publicationSubscriptionRepository = new Mock<IPublicationSubscriptionRepository>(MockBehavior.Strict);
        publicationSubscriptionRepository.Setup(mock =>
                mock.GetTable(NotifierTableStorage.PublicationSubscriptionsTable))
            .ReturnsAsync(cloudTable.Object);

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

        var function = BuildFunction(
            publicationSubscriptionRepository: publicationSubscriptionRepository.Object,
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
            SupersededPublications = new List<IdTitleViewModel>
            {
                new()
                {
                    Id = supersededPubId,
                    Title = "Superseded publication",
                },
            },
        };

        await function.NotifySubscribers(
            releaseNotificationMessage,
            new TestFunctionContext());

        emailService.Verify(mock =>
            mock.SendEmail("superseded1@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "Superseded publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("superseded2@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "Superseded publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("superseded3@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-3",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "Superseded publication")
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_MultipleSupersededPublications()
    {
        var publication1Id = Guid.NewGuid();

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>());
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntityOld>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPub1Id = Guid.NewGuid();
        var tableQuerySegmentSupersededPub1Subs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(supersededPub1Id.ToString(), "superseded1@test.com", "Superseded 1 publication",
                "superseded-1-publication", null),
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntityOld>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPub1Id}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPub1Subs);

        var supersededPub2Id = Guid.NewGuid();
        var tableQuerySegmentSupersededPub2Subs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(supersededPub1Id.ToString(), "superseded2@test.com", "Superseded21 publication",
                "superseded-2-publication", null),
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntityOld>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPub2Id}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPub2Subs);

        // other mocks
        var publicationSubscriptionRepository = new Mock<IPublicationSubscriptionRepository>(MockBehavior.Strict);
        publicationSubscriptionRepository.Setup(mock =>
                mock.GetTable(NotifierTableStorage.PublicationSubscriptionsTable))
            .ReturnsAsync(cloudTable.Object);

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

        var function = BuildFunction(
            publicationSubscriptionRepository: publicationSubscriptionRepository.Object,
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
            SupersededPublications = new List<IdTitleViewModel>
            {
                new()
                {
                    Id = supersededPub1Id,
                    Title = "Superseded 1 publication",
                },
                new()
                {
                    Id = supersededPub2Id,
                    Title = "Superseded 2 publication",
                }
            },
        };

        await function.NotifySubscribers(
            releaseNotificationMessage,
            new TestFunctionContext());

        emailService.Verify(mock =>
            mock.SendEmail("superseded1@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        supersededPub1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "Superseded 1 publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail("superseded2@test.com", "release-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        supersededPub2Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", null,
                        "Superseded 2 publication")
                )), Times.Once);
    }

    [Fact]
    public async Task NotifySubscribers_Amendment()
    {
        var publication1Id = Guid.NewGuid();

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(Guid.NewGuid().ToString(), "test@test.com", "Publication 1", "publication-1", null)
        });
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntityOld>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPubId = Guid.NewGuid();
        var tableQuerySegmentSupersededPubSubs = CreateTableQuerySegment(new List<SubscriptionEntityOld>
        {
            new(supersededPubId.ToString(), "superseded@test.com", "Superseded publication",
                "superseded-publication", null)
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntityOld>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPubId}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPubSubs);

        // other mocks
        var publicationSubscriptionRepository = new Mock<IPublicationSubscriptionRepository>(MockBehavior.Strict);
        publicationSubscriptionRepository.Setup(mock =>
                mock.GetTable(NotifierTableStorage.PublicationSubscriptionsTable))
            .ReturnsAsync(cloudTable.Object);

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

        var function = BuildFunction(
            publicationSubscriptionRepository: publicationSubscriptionRepository.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var releaseNotificationMessage = new ReleaseNotificationMessage
        {
            PublicationId = publication1Id,
            PublicationName = "Publication 1",
            PublicationSlug = "publication-1",
            ReleaseName = "2000",
            ReleaseSlug = "2000",
            Amendment = true,
            UpdateNote = "Update note",
            SupersededPublications = new List<IdTitleViewModel>
            {
                new()
                {
                    Id = supersededPubId,
                    Title = "Superseded publication",
                },
            },
        };

        await function.NotifySubscribers(
            releaseNotificationMessage,
            new TestFunctionContext());

        emailService.Verify(mock =>
            mock.SendEmail("test@test.com", "release-amendment-published-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-1",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note", null)
                )), Times.Once);

        emailService.Verify(mock =>
            mock.SendEmail("superseded@test.com",
                "release-amendment-published-superseded-subscribers-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "unsubscribe-token-2",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded publication")
                )), Times.Once);
    }

    private static bool AssertEmailTemplateValues(Dictionary<string, dynamic> values,
        string unsubToken,
        string pubId, string pubName, string pubSlug,
        string releaseName, string releaseSlug, string? updateNote,
        string? supersededPublicationTitle = null)
    {
        Assert.Equal(pubName, values["publication_name"]);
        Assert.Equal(releaseName, values["release_name"]);
        Assert.Equal($"{AppSettingsOptions.PublicAppUrl}/find-statistics/{pubSlug}/{releaseSlug}",
            values["release_link"]);
        Assert.Equal($"{AppSettingsOptions.PublicAppUrl}/subscriptions/{pubSlug}/confirm-unsubscription/{unsubToken}", values["unsubscribe_link"]);

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
        ITokenService? tokenService = null,
        IEmailService? emailService = null,
        IPublicationSubscriptionRepository? publicationSubscriptionRepository = null)
    {
        return new ReleaseNotifier(
            Mock.Of<ILogger<ReleaseNotifier>>(),
            Options.Create(AppSettingsOptions),
            Options.Create(new GovUkNotifyOptions
            {
                ApiKey = "",
                EmailTemplates = EmailTemplateOptions
            }),
            tokenService ?? Mock.Of<ITokenService>(MockBehavior.Strict),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            publicationSubscriptionRepository ?? Mock.Of<IPublicationSubscriptionRepository>(MockBehavior.Strict));
    }
}
