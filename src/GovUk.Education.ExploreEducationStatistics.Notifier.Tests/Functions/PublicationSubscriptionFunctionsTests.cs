using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public class PublicationSubscriptionFunctionsTests(NotifierFunctionsIntegrationTestFixture fixture)
    : NotifierFunctionsIntegrationTest(fixture)
{
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
    public async Task SendsSubscriptionVerificationEmail()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test1@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test1@test.com",
                "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "test-id-1",
            Slug = "test-publication-slug-1",
            Email = "test1@test.com",
            Title = "Test Publication Title 1"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        Assert.IsType<OkObjectResult>(result);

        emailService.Verify(mock =>
                mock.SendEmail(
                    "test1@test.com",
                    "subscription-verification-id",
                    It.Is<Dictionary<string, dynamic>>(d =>
                        AssertEmailTemplateValues(d,
                            "Test Publication Title 1",
                            "https://localhost:3000/subscriptions/test-publication-slug-1/confirm-subscription/activation-code-1",
                            null)
                    )),
            Times.Once);
    }

    [Fact]
    public async Task DoesNotSendEmailAgainIfSubIsPending()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (data)
        await notifierTableStorageService.CreateEntity(
            NotifierTableStorage.PublicationPendingSubscriptionsTable,
            new SubscriptionEntity
            {
                PartitionKey = "test-id-2",
                RowKey = "test2@test.com",
                Slug = "test-publication-slug-2",
                Title = "Test Publication Title 2",
            });

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test2@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test2@test.com",
                "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "test-id-2",
            Slug = "test-publication-slug-2",
            Email = "test2@test.com",
            Title = "Test Publication Title 2"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);

        emailService.Verify(mock =>
                mock.SendEmail(
                    "test2@test.com",
                    "subscription-verification-id",
                    It.Is<Dictionary<string, dynamic>>(d =>
                        AssertEmailTemplateValues(d,
                            "Test Publication Title 2",
                            "https://localhost:3000/subscriptions/test-publication-slug-2/confirm-subscription/activation-code-2",
                            null)
                    )),
            Times.Never);
    }

    [Fact]
    public async Task SendsConfirmationEmailIfUserAlreadySubscribed()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (data)
        await notifierTableStorageService.CreateEntity(
            NotifierTableStorage.PublicationSubscriptionsTable,
            new SubscriptionEntity
            {
                PartitionKey = "test-id-3",
                RowKey = "test3@test.com",
                Slug = "test-publication-slug-3",
                Title = "Test Publication Title 3",
                DateTimeCreated = DateTime.UtcNow.AddDays(-4),
            });

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test3@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-3");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test3@test.com",
                "subscription-confirmation-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "test-id-3",
            Slug = "test-publication-slug-3",
            Email = "test3@test.com",
            Title = "Test Publication Title 3"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);

        emailService.Verify(mock =>
                mock.SendEmail(
                    "test3@test.com",
                    "subscription-confirmation-id",
                    It.Is<Dictionary<string, dynamic>>(d =>
                        AssertEmailTemplateValues(d,
                            "Test Publication Title 3",
                            null,
                            "https://localhost:3000/subscriptions/test-publication-slug-3/confirm-unsubscription/activation-code-3")
                    )),
            Times.Once);
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Id_Is_Blank()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test@test.com",
                "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "",
            Slug = "test-publication-slug",
            Email = "test@test.com",
            Title = "Test Publication Title"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(PendingPublicationSubscriptionCreateRequest.Id).ToLowerFirst());
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Title_Is_Blank()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test@test.com",
                "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "123abc",
            Slug = "test-publication-slug",
            Email = "test@test.com",
            Title = ""
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(PendingPublicationSubscriptionCreateRequest.Title).ToLowerFirst());
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Email_Is_Blank()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test@test.com",
                "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "123abc",
            Slug = "test-publication-slug",
            Email = "",
            Title = "Test Publication Title"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(PendingPublicationSubscriptionCreateRequest.Email).ToLowerFirst());
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Slug_Is_Blank()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test@test.com",
                "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new PendingPublicationSubscriptionCreateRequest
        {
            Id = "123abc",
            Slug = "",
            Email = "test@test.com",
            Title = "Test Publication Title"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscription(request,
                new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(PendingPublicationSubscriptionCreateRequest.Slug).ToLowerFirst());
    }


    [Fact]
    public async Task SendsSubscriptionConfirmationEmail()
    {
                var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (data)
        var publicationId = Guid.NewGuid();
        await notifierTableStorageService.CreateEntity(
            NotifierTableStorage.PublicationPendingSubscriptionsTable,
            new SubscriptionEntity
            {
                PartitionKey = publicationId.ToString(),
                RowKey = "test4@test.com",
                Slug = "test-publication-slug-4",
                Title = "Test Publication Title 4",
                DateTimeCreated = DateTime.UtcNow.AddDays(-4)
            });

        // Arrange (mocks)
        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GetEmailFromToken("verification-code-4"))
            .Returns("test4@test.com");
        tokenService.Setup(mock =>
                mock.GenerateToken("test4@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscription-code-4");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(
                "test4@test.com",
                "subscription-confirmation-id",
                It.IsAny<Dictionary<string, dynamic>>()));


        var notifierFunction = BuildFunction(
            notifierTableStorageService: notifierTableStorageService,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        // Act
        var result =
            await notifierFunction.VerifySubscription(new TestFunctionContext(),
                publicationId,
                "verification-code-4");

        // Assert
        Assert.IsType<OkObjectResult>(result);

        emailService.Verify(mock =>
                mock.SendEmail(
                    "test4@test.com",
                    "subscription-confirmation-id",
                    It.Is<Dictionary<string, dynamic>>(d =>
                        AssertEmailTemplateValues(d,
                            "Test Publication Title 4",
                            null,
                            "https://localhost:3000/subscriptions/test-publication-slug-4/confirm-unsubscription/unsubscription-code-4")
                    )),
            Times.Once);
    }

    [Fact]
    public async Task Unsubscribes()
    {
        var notifierTableStorageService = new NotifierTableStorageService(
            new AppOptions { NotifierStorageConnectionString = StorageConnectionString() }.ToOptionsWrapper()
        );

        // Arrange (data)
        var publicationId = Guid.NewGuid();
        await notifierTableStorageService.CreateEntity(
            NotifierTableStorage.PublicationSubscriptionsTable,
            new SubscriptionEntity
            {
                PartitionKey = publicationId.ToString(),
                RowKey = "test5@test.com",
                Slug = "test-publication-slug-5",
                Title = "Test Publication Title 5",
                DateTimeCreated = DateTime.UtcNow.AddDays(-4)
            });

        var supersededPublicationId = Guid.NewGuid();
        await notifierTableStorageService.CreateEntity(
            NotifierTableStorage.PublicationSubscriptionsTable,
            new SubscriptionEntity
            {
                PartitionKey = supersededPublicationId.ToString(),
                RowKey = "test5@test.com",
                Slug = "test-superseded-publication-slug",
                Title = "Test Superseded Publication Title",
                DateTimeCreated = DateTime.UtcNow.AddDays(-4)
            });

        // Arrange (mocks)
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(new Publication
            {
                Id = supersededPublicationId,
                SupersededById = publicationId,
            });
            await contentDbContext.SaveChangesAsync();
        }

        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GetEmailFromToken("unsubscription-code-5"))
            .Returns("test5@test.com");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
        {
            var notifierFunction = BuildFunction(
                contentDbContext: contentDbContext,
                notifierTableStorageService: notifierTableStorageService,
                tokenService: tokenService.Object,
                emailService: emailService.Object);

            // Act
            var result =
                await notifierFunction.Unsubscribe(new TestFunctionContext(),
                    publicationId.ToString(),
                    "unsubscription-code-5");

            var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
            var subscription = Assert.IsAssignableFrom<SubscriptionStateDto>(okResult.Value);
            Assert.Equal(SubscriptionStatus.NotSubscribed, subscription.Status);
            Assert.Equal("test-publication-slug-5", subscription.Slug);
            Assert.Equal("Test Publication Title 5", subscription.Title);

            var publication = await notifierTableStorageService.GetEntityIfExists<SubscriptionEntity>(
                NotifierTableStorage.PublicationSubscriptionsTable,
                publicationId.ToString(),
                "test5@test.com");
            Assert.Null(publication);

            var supersededPublication = await notifierTableStorageService.GetEntityIfExists<SubscriptionEntity>(
                NotifierTableStorage.PublicationSubscriptionsTable,
                supersededPublicationId.ToString(),
                "test5@test.com");
            Assert.Null(supersededPublication);
        }
    }

    private static bool AssertEmailTemplateValues(
        Dictionary<string, dynamic> values,
        string publicationName,
        string? verificationLink,
        string? unsubscribeLink)
    {
        Assert.Equal(publicationName, values["publication_name"]);

        if (verificationLink != null)
        {
            Assert.Equal(verificationLink, values["verification_link"]);
        }

        if (unsubscribeLink != null)
        {
            Assert.Equal(unsubscribeLink, values["unsubscribe_link"]);
        }

        return true;
    }

    private PublicationSubscriptionFunctions BuildFunction(
        ContentDbContext? contentDbContext = null,
        ITokenService? tokenService = null,
        IEmailService? emailService = null,
        INotifierTableStorageService? notifierTableStorageService = null) =>
        new(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            logger: Mock.Of<ILogger<PublicationSubscriptionFunctions>>(),
            appOptions: new AppOptions { PublicAppUrl = "https://localhost:3000" }.ToOptionsWrapper(),
            govUkNotifyOptions: new GovUkNotifyOptions
            {
                ApiKey = "",
                EmailTemplates = EmailTemplateOptions
            }.ToOptionsWrapper(),
            tokenService: tokenService ?? Mock.Of<ITokenService>(MockBehavior.Strict),
            emailService: emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            requestValidator: new PendingPublicationSubscriptionCreateRequest.Validator(),
            notifierTableStorageService: notifierTableStorageService
                                         ?? Mock.Of<INotifierTableStorageService>(MockBehavior.Strict),
            subscriptionRepository: notifierTableStorageService != null
                ? new SubscriptionRepository(notifierTableStorageService)
                : Mock.Of<ISubscriptionRepository>(MockBehavior.Strict));
}
