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
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "123abc", 
            Slug = "test-publication-slug", 
            Email= "test@test.com", 
            Title = "Test Publication Title"
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title",
                        "https://localhost:3000/subscriptions/test-publication-slug/confirm-subscription/activation-code-1", null)
                )), Times.Once);
    }
    
    [Fact]
    public async Task DoesNotSendEmailAgainIfSubIsPending()
    {
        // Arrange (data)
        await fixture.AddTestSubscription(NotifierPendingSubscriptionsTableName,
            new("123abc", "test@test.com", "Test Publication Title", "test-publication-slug", DateTime.UtcNow));
        
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "123abc", 
            Slug = "test-publication-slug", 
            Email= "test@test.com", 
            Title = "Test Publication Title"
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title",
                        "https://localhost:3000/subscriptions/test-publication-slug/confirm-subscription/activation-code-1", null)
                )), Times.Never);
    }
    
    [Fact]
    public async Task SendsConfirmationEmailIfUserAlreadySubscribed()
    {
        // Arrange (data)
        await fixture.AddTestSubscription(NotifierSubscriptionsTableName,
            new("123abc-2", "test2@test.com", "Test Publication Title 2", "test-publication-slug-2", DateTime.UtcNow.AddDays(-4)));
        
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
            .Returns("activation-code-1");
        
        var emailService = new Mock<IEmailService>(MockBehavior.Strict);
        emailService.Setup(mock =>
            mock.SendEmail(notificationClient, "test2@test.com", "subscription-confirmation-id",
                It.IsAny<Dictionary<string, dynamic>>()));
        
        var notifierFunction = BuildFunction(
            storageTableService: storageTableService,
            notificationClientProvider: notificationClientProvider.Object,
            tokenService: tokenService.Object,
            emailService: emailService.Object);
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "123abc-2", 
            Slug = "test-publication-slug-2", 
            Email= "test2@test.com", 
            Title = "Test Publication Title 2"
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var okResult = Assert.IsType<OkResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test2@test.com", "subscription-confirmation-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title 2", null, "https://localhost:3000/subscriptions/test-publication-slug-2/confirm-unsubscription/activation-code-1")
                )), Times.Once);
    }
    
    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Id_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "", 
            Slug = "test-publication-slug", 
            Email= "test@test.com", 
            Title = "Test Publication Title"
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Id));
    }
    
    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Title_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "123abc", 
            Slug = "test-publication-slug", 
            Email= "test@test.com", 
            Title = ""
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Title));
    }
    
    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Email_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "123abc", 
            Slug = "test-publication-slug", 
            Email= "", 
            Title = "Test Publication Title"
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Email));
    }
    
    [Fact]
    public async Task RequestPendingSubscription_ReturnsValidationProblem_When_Slug_Is_Blank()
    {
        // Arrange (mocks)
        var storageTableService = new StorageTableService(Options.Create(new AppSettingOptions()
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
        
        var request = new NewPendingSubscriptionRequest()
        {
            Id = "123abc", 
            Slug = "", 
            Email= "test@test.com", 
            Title = "Test Publication Title"
        };
        
        // Act
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext(), new CancellationToken());

        // Assert
        var validationProblem = result.AssertBadRequestWithValidationProblem();
        validationProblem.AssertHasNotEmptyError(nameof(NewPendingSubscriptionRequest.Slug));
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
        INotificationClientProvider? notificationClientProvider = null)
    {
        return new SubscriptionManager(
            Mock.Of<ILogger<SubscriptionManager>>(),
            Options.Create(new AppSettingOptions()
            {
                PublicAppUrl = "https://localhost:3000"
            }),
            Options.Create(new GovUkNotifyOptions
            {
                ApiKey = "",
                EmailTemplates = EmailTemplateOptions
            }),
            tokenService ?? Mock.Of<ITokenService>(MockBehavior.Strict),
            emailService ?? Mock.Of<IEmailService>(MockBehavior.Strict),
            storageTableService ?? Mock.Of<IStorageTableService>(MockBehavior.Strict),
            notificationClientProvider ?? Mock.Of<INotificationClientProvider>(MockBehavior.Strict),
            new NewPendingSubscriptionRequest.Validator());
    }
}
