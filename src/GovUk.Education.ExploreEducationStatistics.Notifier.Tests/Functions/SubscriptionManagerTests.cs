using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Functions;
using GovUk.Education.ExploreEducationStatistics.Notifier.Requests;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Utils.NotifierTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Tests.Functions;

public class SubscriptionManagerFunctionTests(NotifierFunctionsIntegrationTestFixture fixture)
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
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test1@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test1@test.com", "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "test-id-1",
            Slug = "test-publication-slug-1",
            Email = "test1@test.com",
            Title = "Test Publication Title 1"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test1@test.com", "subscription-verification-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title 1",
                        "https://localhost:3000/subscriptions/test-publication-slug-1/confirm-subscription/activation-code-1",
                        null)
                )), Times.Once);
    }

    [Fact]
    public async Task DoesNotSendEmailAgainIfSubIsPending()
    {
        // Arrange (data)
        await fixture.AddTestSubscription(NotifierPendingSubscriptionsTableName,
            new SubscriptionEntity("test-id-2", "test2@test.com", "Test Publication Title 2", "test-publication-slug-2",
                DateTime.UtcNow));

        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test2@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-2");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test2@test.com", "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "test-id-2",
            Slug = "test-publication-slug-2",
            Email = "test2@test.com",
            Title = "Test Publication Title 2"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test2@test.com", "subscription-verification-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title 2",
                        "https://localhost:3000/subscriptions/test-publication-slug-2/confirm-subscription/activation-code-2",
                        null)
                )), Times.Never);
    }

    [Fact]
    public async Task SendsConfirmationEmailIfUserAlreadySubscribed()
    {
        // Arrange (data)
        await fixture.AddTestSubscription(NotifierSubscriptionsTableName,
            new SubscriptionEntity("test-id-3", "test3@test.com", "Test Publication Title 3", "test-publication-slug-3",
                DateTime.UtcNow.AddDays(-4)));

        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test3@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-3");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test3@test.com", "subscription-confirmation-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "test-id-3",
            Slug = "test-publication-slug-3",
            Email = "test3@test.com",
            Title = "Test Publication Title 3"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test3@test.com", "subscription-confirmation-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title 3", null,
                        "https://localhost:3000/subscriptions/test-publication-slug-3/confirm-unsubscription/activation-code-3")
                )), Times.Once);
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Id_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "", Slug = "test-publication-slug", Email = "test@test.com", Title = "Test Publication Title"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Id));
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Title_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "123abc", Slug = "test-publication-slug", Email = "test@test.com", Title = ""
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Title));
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Email_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "123abc", Slug = "test-publication-slug", Email = "", Title = "Test Publication Title"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Email));
    }

    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Slug_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GenerateToken("test@test.com", It.IsAny<DateTime>()))
            .Returns("activation-code-1");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.IsAny<Dictionary<string, dynamic>>()));

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        var request = new NewPendingSubscriptionRequest
        {
            Id = "123abc", Slug = "", Email = "test@test.com", Title = "Test Publication Title"
        };

        // Act
        var result =
            await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(),
                new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Slug));
    }


    [Fact]
    public async Task SendsSubscriptionConfirmationEmail()
    {
        // Arrange (data)
        await fixture.AddTestSubscription(NotifierPendingSubscriptionsTableName,
            new SubscriptionEntity("test-id-4", "test4@test.com", "Test Publication Title 4", "test-publication-slug-4",
                DateTime.UtcNow.AddDays(-4)));


        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GetEmailFromToken("verification-code-4"))
            .Returns("test4@test.com");
        tokenService.Setup(mock =>
                mock.GenerateToken("test4@test.com", It.IsAny<DateTime>()))
            .Returns("unsubscription-code-4");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test4@test.com", "subscription-confirmation-id",
                It.IsAny<Dictionary<string, dynamic>>()));


        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        // Act
        var result =
            await notifierFunction.VerifySubscriptionFunc(new TestFunctionContext(), "test-id-4",
                "verification-code-4");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);

        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test4@test.com", "subscription-confirmation-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title 4",
                        null,
                        "https://localhost:3000/subscriptions/test-publication-slug-4/confirm-unsubscription/unsubscription-code-4")
                )), Times.Once);
    }

    [Fact]
    public async Task Unsubscribes()
    {
        // Arrange (data)
        await fixture.AddTestSubscription(NotifierSubscriptionsTableName,
            new SubscriptionEntity("test-id-5", "test5@test.com", "Test Publication Title 5", "test-publication-slug-5",
                DateTime.UtcNow.AddDays(-4)));


        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions
        {
            TableStorageConnectionString = fixture.TableStorageConnectionString()
        }));

        var notificationClient = MockNotificationClient(HttpStatusCode.OK, "response content");
        var notificationClientProvider = new Mock<INotificationClientProvider>(MockBehavior.Strict);
        notificationClientProvider.Setup(mock =>
                mock.Get())
            .Returns(notificationClient);


        var tokenService = new Mock<ITokenService>(MockBehavior.Strict);
        tokenService.Setup(mock =>
                mock.GetEmailFromToken("unsubscription-code-5"))
            .Returns("test5@test.com");

        var emailService = new Mock<IEmailService>(MockBehavior.Strict);

        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);

        // Act
        var result =
            await notifierFunction.PublicationUnsubscribeFunc(new TestFunctionContext(), "test-id-5",
                "unsubscription-code-5");

        var okResult = Assert.IsAssignableFrom<OkObjectResult>(result);
        var subscription = Assert.IsAssignableFrom<SubscriptionStateDto>(okResult.Value);
        Assert.Equal(SubscriptionStatus.NotSubscribed, subscription.Status);
        Assert.Equal("test-publication-slug-5", subscription.Slug);
        Assert.Equal("Test Publication Title 5", subscription.Title);
    }

    private static bool AssertEmailTemplateValues(Dictionary<string, dynamic> values,
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

    private static SubscriptionManager BuildFunction(
        ITokenService? tokenService = null,
        IEmailService? emailService = null,
        IStorageTableService? storageTableService = null,
        INotificationClientProvider? notificationClientProvider = null) =>
        new(
            Mock.Of<ILogger<SubscriptionManager>>(),
            Options.Create(new AppSettingOptions { PublicAppUrl = "https://localhost:3000" }),
            Options.Create(new GovUkNotifyOptions { ApiKey = "", EmailTemplates = EmailTemplateOptions }),
            tokenService ?? Mock.Of<ITokenService>(MockBehavior.Strict),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            storageTableService ?? Mock.Of<IStorageTableService>(MockBehavior.Strict),
            notificationClientProvider ?? Mock.Of<INotificationClientProvider>(MockBehavior.Strict),
            new NewPendingSubscriptionRequest.Validator());
}
