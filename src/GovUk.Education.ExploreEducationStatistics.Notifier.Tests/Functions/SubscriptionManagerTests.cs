using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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
        var result = await notifierFunction.RequestPendingSubscriptionFunc(request, new TestFunctionContext());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
        
        emailService.Verify(mock =>
            mock.SendEmail(notificationClient, "test@test.com", "subscription-verification-id",
                It.Is<Dictionary<string, dynamic>>(d =>
                    AssertEmailTemplateValues(d, "Test Publication Title",
                        "https://localhost:3000/subscriptions/test-publication-slug/confirm-subscription/activation-code-1")
                )), Times.Once);
    }

    private static bool AssertEmailTemplateValues(Dictionary<string, dynamic> values,
        string publicationName,
        string? verificationLink)
    {
        Assert.Equal(publicationName, values["publication_name"]);

        if (verificationLink != null)
        {
            Assert.Equal(verificationLink, values["verification_link"]);
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
            notificationClientProvider ?? Mock.Of<INotificationClientProvider>(MockBehavior.Strict));
    }
}
