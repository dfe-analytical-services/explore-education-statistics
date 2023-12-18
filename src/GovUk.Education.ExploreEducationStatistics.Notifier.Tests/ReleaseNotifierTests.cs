using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.TableStorageTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Utils.NotifierTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Utils.ConfigKeys;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using IConfigurationProvider = GovUk.Education.ExploreEducationStatistics.Notifier.Services.IConfigurationProvider;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests;

public class ReleaseNotifierTests
{
    [Fact]
    public async Task ReleaseNotifierFunc()
    {
        var publication1Id = Guid.NewGuid();

        // generate in-memory config and use it in test
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { ReleaseEmailTemplateIdName, "release-template-id" },
                { ReleaseEmailSupersededSubscribersTemplateIdName, "release-superseded-template-id" },
                { ReleaseAmendmentEmailTemplateIdName, "should-not-be-used" },
                { ReleaseAmendmentSupersededSubscribersEmailTemplateIdName, "should-not-be-used" },
                { BaseUrlName, "http://base.url/"},
                { WebApplicationBaseUrlName, "http://web.base.url/" },
                { TokenSecretKeyName, "token-secret" },
                { NotifyApiKeyName, "notify-api-key" },
                { StorageConnectionName, "storage-connection-string"}
            }).Build();

        var configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Strict);
        configurationProvider.Setup(mock =>
                mock.Get(It.IsAny<ExecutionContext>()))
            .Returns(configuration);

        // generate NotificationClient and use it in test
        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get("notify-api-key"))
            .Returns(notificationClient);

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(Guid.NewGuid().ToString(), "test@test.com", "Publication 1", "publication-1", null)
        });
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntity>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPubId = Guid.NewGuid();
        var tableQuerySegmentSupersededPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(supersededPubId.ToString(), "superseded@test.com", "Superseded publication",
                "superseded-publication", null)
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntity>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPubId}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPubSubs);

        // other mocks
        var storageTableService = new Mock<IStorageTableService>(MockBehavior.Strict);
        storageTableService.Setup(mock =>
            mock.GetTable("storage-connection-string", SubscriptionsTblName))
            .ReturnsAsync(cloudTable.Object);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "test@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "release-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded@test.com", "release-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var function = BuildFunction(
            configurationProvider: configurationProvider.Object,
            storageTableService: storageTableService.Object,
            notificationClientProvider: notificationClientProvider.Object,
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
            UpdateNote = "Update note",
            SupersededPublicationIds = new List<Guid>{ supersededPubId },
            SupersededPublicationTitles = new List<string>{ "Superseded publication" },
        };
        var executionContext = Mock.Of<ExecutionContext>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger>(MockBehavior.Loose);

        await function.ReleaseNotifierFunc(
            releaseNotificationMessage,
            logger,
            executionContext);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "release-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-1",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note", null)
                )), Times.Once);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded@test.com", "release-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-2",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded publication")
                )), Times.Once);
    }

    [Fact]
    public async Task ReleaseNotifierFunc_MultipleSubs()
    {
        var publication1Id = Guid.NewGuid();

        // generate in-memory config and use it in test
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { ReleaseEmailTemplateIdName, "release-template-id" },
                { ReleaseEmailSupersededSubscribersTemplateIdName, "release-superseded-template-id" },
                { ReleaseAmendmentEmailTemplateIdName, "should-not-be-used" },
                { ReleaseAmendmentSupersededSubscribersEmailTemplateIdName, "should-not-be-used" },
                { BaseUrlName, "http://base.url/"},
                { WebApplicationBaseUrlName, "http://web.base.url/" },
                { TokenSecretKeyName, "token-secret" },
                { NotifyApiKeyName, "notify-api-key" },
                { StorageConnectionName, "storage-connection-string"}
            }).Build();

        var configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Strict);
        configurationProvider.Setup(mock =>
                mock.Get(It.IsAny<ExecutionContext>()))
            .Returns(configuration);

        // generate NotificationClient and use it in test
        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get("notify-api-key"))
            .Returns(notificationClient);

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(Guid.NewGuid().ToString(), "test1@test.com", "Publication 1", "publication-1", null),
            new(Guid.NewGuid().ToString(), "test2@test.com", "Publication 1", "publication-1", null),
            new(Guid.NewGuid().ToString(), "test3@test.com", "Publication 1", "publication-1", null),
        });
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntity>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        // other mocks
        var storageTableService = new Mock<IStorageTableService>(MockBehavior.Strict);
        storageTableService.Setup(mock =>
            mock.GetTable("storage-connection-string", SubscriptionsTblName))
            .ReturnsAsync(cloudTable.Object);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "test1@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "test2@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "test3@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-3");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test1@test.com", "release-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test2@test.com", "release-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test3@test.com", "release-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var function = BuildFunction(
            configurationProvider: configurationProvider.Object,
            storageTableService: storageTableService.Object,
            notificationClientProvider: notificationClientProvider.Object,
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
            UpdateNote = "Update note",
            SupersededPublicationIds = new List<Guid>(),
            SupersededPublicationTitles = new List<string>(),
        };
        var executionContext = Mock.Of<ExecutionContext>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger>(MockBehavior.Loose);

        await function.ReleaseNotifierFunc(
            releaseNotificationMessage,
            logger,
            executionContext);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test1@test.com", "release-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-1",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note", null)
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test2@test.com", "release-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-2",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note", null)
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test3@test.com", "release-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-3",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note", null)
                )), Times.Once);
    }

    [Fact]
    public async Task ReleaseNotifierFunc_MultipleSupersededPublicationSubs()
    {
        var publication1Id = Guid.NewGuid();

        // generate in-memory config and use it in test
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { ReleaseEmailTemplateIdName, "release-template-id" },
                { ReleaseEmailSupersededSubscribersTemplateIdName, "release-superseded-template-id" },
                { ReleaseAmendmentEmailTemplateIdName, "should-not-be-used" },
                { ReleaseAmendmentSupersededSubscribersEmailTemplateIdName, "should-not-be-used" },
                { BaseUrlName, "http://base.url/"},
                { WebApplicationBaseUrlName, "http://web.base.url/" },
                { TokenSecretKeyName, "token-secret" },
                { NotifyApiKeyName, "notify-api-key" },
                { StorageConnectionName, "storage-connection-string"}
            }).Build();

        var configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Strict);
        configurationProvider.Setup(mock =>
                mock.Get(It.IsAny<ExecutionContext>()))
            .Returns(configuration);

        // generate NotificationClient and use it in test
        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get("notify-api-key"))
            .Returns(notificationClient);

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>());
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntity>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPubId = Guid.NewGuid();
        var tableQuerySegmentSupersededPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(supersededPubId.ToString(), "superseded1@test.com", "Superseded publication",
                "superseded-publication", null),
            new(supersededPubId.ToString(), "superseded2@test.com", "Superseded publication",
                "superseded-publication", null),
            new(supersededPubId.ToString(), "superseded3@test.com", "Superseded publication",
                "superseded-publication", null),
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntity>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPubId}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPubSubs);

        // other mocks
        var storageTableService = new Mock<IStorageTableService>(MockBehavior.Strict);
        storageTableService.Setup(mock =>
            mock.GetTable("storage-connection-string", SubscriptionsTblName))
            .ReturnsAsync(cloudTable.Object);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded1@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded2@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded3@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-3");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded1@test.com", "release-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded2@test.com", "release-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded3@test.com", "release-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var function = BuildFunction(
            configurationProvider: configurationProvider.Object,
            storageTableService: storageTableService.Object,
            notificationClientProvider: notificationClientProvider.Object,
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
            UpdateNote = "Update note",
            SupersededPublicationIds = new List<Guid>{ supersededPubId },
            SupersededPublicationTitles = new List<string>{ "Superseded publication" },
        };
        var executionContext = Mock.Of<ExecutionContext>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger>(MockBehavior.Loose);

        await function.ReleaseNotifierFunc(
            releaseNotificationMessage,
            logger,
            executionContext);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded1@test.com", "release-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-1",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded2@test.com", "release-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-2",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded3@test.com", "release-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-3",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded publication")
                )), Times.Once);
    }

    [Fact]
    public async Task ReleaseNotifierFunc_MultipleSupersededPublications()
    {
        var publication1Id = Guid.NewGuid();

        // generate in-memory config and use it in test
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { ReleaseEmailTemplateIdName, "release-template-id" },
                { ReleaseEmailSupersededSubscribersTemplateIdName, "release-superseded-template-id" },
                { ReleaseAmendmentEmailTemplateIdName, "should-not-be-used" },
                { ReleaseAmendmentSupersededSubscribersEmailTemplateIdName, "should-not-be-used" },
                { BaseUrlName, "http://base.url/"},
                { WebApplicationBaseUrlName, "http://web.base.url/" },
                { TokenSecretKeyName, "token-secret" },
                { NotifyApiKeyName, "notify-api-key" },
                { StorageConnectionName, "storage-connection-string"}
            }).Build();

        var configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Strict);
        configurationProvider.Setup(mock =>
                mock.Get(It.IsAny<ExecutionContext>()))
            .Returns(configuration);

        // generate NotificationClient and use it in test
        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get("notify-api-key"))
            .Returns(notificationClient);

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>());
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntity>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPub1Id = Guid.NewGuid();
        var tableQuerySegmentSupersededPub1Subs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(supersededPub1Id.ToString(), "superseded1@test.com", "Superseded 1 publication",
                "superseded-1-publication", null),
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntity>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPub1Id}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPub1Subs);

        var supersededPub2Id = Guid.NewGuid();
        var tableQuerySegmentSupersededPub2Subs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(supersededPub1Id.ToString(), "superseded2@test.com", "Superseded21 publication",
                "superseded-2-publication", null),
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntity>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPub2Id}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPub2Subs);

        // other mocks
        var storageTableService = new Mock<IStorageTableService>(MockBehavior.Strict);
        storageTableService.Setup(mock =>
            mock.GetTable("storage-connection-string", SubscriptionsTblName))
            .ReturnsAsync(cloudTable.Object);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded1@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded2@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded1@test.com", "release-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded2@test.com", "release-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var function = BuildFunction(
            configurationProvider: configurationProvider.Object,
            storageTableService: storageTableService.Object,
            notificationClientProvider: notificationClientProvider.Object,
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
            UpdateNote = "Update note",
            SupersededPublicationIds = new List<Guid>{ supersededPub1Id, supersededPub2Id },
            SupersededPublicationTitles = new List<string>{ "Superseded 1 publication", "Superseded 2 publication" },
        };
        var executionContext = Mock.Of<ExecutionContext>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger>(MockBehavior.Loose);

        await function.ReleaseNotifierFunc(
            releaseNotificationMessage,
            logger,
            executionContext);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded1@test.com", "release-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-1",
                        supersededPub1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded 1 publication")
                )), Times.Once);
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded2@test.com", "release-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-2",
                        supersededPub2Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded 2 publication")
                )), Times.Once);
    }

    [Fact]
    public async Task ReleaseNotifierFunc_Amendment()
    {
        var publication1Id = Guid.NewGuid();

        // generate in-memory config and use it in test
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { ReleaseEmailTemplateIdName, "should-not-be-used" },
                { ReleaseEmailSupersededSubscribersTemplateIdName, "should-not-be-used" },
                { ReleaseAmendmentEmailTemplateIdName, "amendment-template-id" },
                { ReleaseAmendmentSupersededSubscribersEmailTemplateIdName, "amendment-superseded-template-id" },
                { BaseUrlName, "http://base.url/"},
                { WebApplicationBaseUrlName, "http://web.base.url/" },
                { TokenSecretKeyName, "token-secret" },
                { NotifyApiKeyName, "notify-api-key" },
                { StorageConnectionName, "storage-connection-string"}
            }).Build();

        var configurationProvider = new Mock<IConfigurationProvider>(MockBehavior.Strict);
        configurationProvider.Setup(mock =>
                mock.Get(It.IsAny<ExecutionContext>()))
            .Returns(configuration);

        // generate NotificationClient and use it in test
        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get("notify-api-key"))
            .Returns(notificationClient);

        // generate Azure Storage Table and return results
        var cloudTable = MockCloudTable();

        var tableQuerySegmentPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(Guid.NewGuid().ToString(), "test@test.com", "Publication 1", "publication-1", null)
        });
        cloudTable.Setup(mock =>
                mock.ExecuteQuerySegmentedAsync(
                    It.Is<TableQuery<SubscriptionEntity>>(
                        tq =>
                            tq.FilterString == $"PartitionKey eq '{publication1Id}'"),
                    It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentPubSubs);

        var supersededPubId = Guid.NewGuid();
        var tableQuerySegmentSupersededPubSubs = CreateTableQuerySegment(new List<SubscriptionEntity>
        {
            new(supersededPubId.ToString(), "superseded@test.com", "Superseded publication",
                "superseded-publication", null)
        });
        cloudTable.Setup(mock =>
            mock.ExecuteQuerySegmentedAsync(It.Is<TableQuery<SubscriptionEntity>>(tq =>
                    tq.FilterString == $"PartitionKey eq '{supersededPubId}'"),
                It.IsAny<TableContinuationToken>()))
            .ReturnsAsync(tableQuerySegmentSupersededPubSubs);

        // other mocks
        var storageTableService = new Mock<IStorageTableService>(MockBehavior.Strict);
        storageTableService.Setup(mock =>
            mock.GetTable("storage-connection-string", SubscriptionsTblName))
            .ReturnsAsync(cloudTable.Object);

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "test@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-1");
        tokenService.Setup(mock =>
                mock.GenerateToken("token-secret", "superseded@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscribe-token-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "amendment-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "superseded@test.com", "amendment-superseded-template-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var function = BuildFunction(
            configurationProvider: configurationProvider.Object,
            storageTableService: storageTableService.Object,
            notificationClientProvider: notificationClientProvider.Object,
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
            SupersededPublicationIds = new List<Guid>{ supersededPubId },
            SupersededPublicationTitles = new List<string>{ "Superseded publication" },
        };
        var executionContext = Mock.Of<ExecutionContext>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger>(MockBehavior.Loose);

        await function.ReleaseNotifierFunc(
            releaseNotificationMessage,
            logger,
            executionContext);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "amendment-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-1",
                        publication1Id.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note", null)
                )), Times.Once);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "superseded@test.com", "amendment-superseded-template-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "http://web.base.url/", "http://base.url/", "unsubscribe-token-2",
                        supersededPubId.ToString(), "Publication 1", "publication-1",
                        "2000", "2000", "Update note",
                        "Superseded publication")
                )), Times.Once);
    }

    private static bool AssertEmailTemplateValues(Dictionary<string,dynamic> values,
        string webAppBaseUrl, string baseUrl, string unsubToken,
        string pubId, string pubName, string pubSlug,
        string releaseName, string releaseSlug, string updateNote,
        string? supersededPublicationTitle = null)
    {
        Assert.Equal(pubName, values["publication_name"]);
        Assert.Equal(releaseName, values["release_name"]);
        Assert.Equal($"{webAppBaseUrl}find-statistics/{pubSlug}/{releaseSlug}", values["release_link"]);
        Assert.Equal(updateNote, values["update_note"]);
        Assert.Equal($"{baseUrl}{pubId}/unsubscribe/{unsubToken}", values["unsubscribe_link"]);
        if (supersededPublicationTitle != null)
        {
            Assert.Equal(supersededPublicationTitle, values["superseded_publication_title"]);
        }

        return true;
    }

    private static Functions.ReleaseNotifier BuildFunction(
        ITokenService? tokenService = null,
        IEmailService? emailService = null,
        IStorageTableService? storageTableService = null,
        IConfigurationProvider? configurationProvider = null,
        INotificationClientProvider? notificationClientProvider = null)
    {
        return new Functions.ReleaseNotifier(
            tokenService ?? Mock.Of<ITokenService>(MockBehavior.Strict),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            storageTableService ?? Mock.Of<IStorageTableService>(MockBehavior.Strict),
            configurationProvider ?? Mock.Of<IConfigurationProvider>(MockBehavior.Strict),
            notificationClientProvider ?? Mock.Of<INotificationClientProvider>(MockBehavior.Strict));
    }
}
